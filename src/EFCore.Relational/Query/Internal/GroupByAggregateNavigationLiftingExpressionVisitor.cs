// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Keep both copies aligned. Uses only public EFCore APIs so it can run via
// IQueryTranslationPreprocessorFactory replacement without a custom EFCore build.
//
// Strategy (empirically validated): rewrite
//     src.GroupBy(k).Select(g => g.Sum(y => y.Nav.Prop))
// into the explicit-join form the relational fast path already translates into a
// single JOIN + GROUP BY:
//     src.GroupJoin(Navs, y => y.Fk, n => n.Pk, (y, ns) => pair)
//        .SelectMany(p => p.Inners.DefaultIfEmpty(), (p, n) => new Capture(p.Outer, n))
//        .GroupBy(t => k(t.Outer))
//        .Select(g => g.Sum(t => t.Inner.Prop))
// The GroupBy element-selector overload does NOT work for this (verified: navigation
// expansion defers it as a pending selector and still correlates per group).

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Rewrites <c>source.GroupBy(key).Select(g => ...aggregates...)</c> so that reference-navigation
///     member paths used inside aggregate selectors become explicit left joins on the parent
///     (pre-GroupBy) source, instead of a correlated subquery per group. See dotnet/efcore#27933.
/// </summary>
internal class GroupByAggregateNavigationLiftingExpressionVisitor : ExpressionVisitor
{
    private static readonly string[] SelectorAggregateMethodNames =
        [nameof(Enumerable.Sum), nameof(Enumerable.Min), nameof(Enumerable.Max), nameof(Enumerable.Average)];

    private static readonly string[] PredicateAggregateMethodNames =
        [nameof(Enumerable.Count), nameof(Enumerable.LongCount)];

    private static readonly MethodInfo DefaultIfEmptyMethod =
        typeof(Enumerable).GetMethods().Single(m => m.Name == nameof(Enumerable.DefaultIfEmpty) && m.GetParameters().Length == 1);

    private readonly IModel _model;

    public GroupByAggregateNavigationLiftingExpressionVisitor(IModel model)
    {
        _model = model;
    }

    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        if (IsQueryableMethod(methodCallExpression, QueryableMethods.Select)
            && methodCallExpression.Arguments[0] is MethodCallExpression groupByCall
            && IsQueryableMethod(groupByCall, QueryableMethods.GroupByWithKeySelector))
        {
            var visitedSource = Visit(groupByCall.Arguments[0]);
            var keySelector = UnwrapLambda(groupByCall.Arguments[1])!;
            var resultSelector = UnwrapLambda(methodCallExpression.Arguments[1])!;

            var rewritten = TryRewrite(visitedSource, keySelector, resultSelector);
            if (rewritten != null)
            {
                return rewritten;
            }

            if (!ReferenceEquals(visitedSource, groupByCall.Arguments[0]))
            {
                return methodCallExpression.Update(
                    null,
                    [groupByCall.Update(null, [visitedSource, groupByCall.Arguments[1]]), methodCallExpression.Arguments[1]]);
            }

            return methodCallExpression;
        }

        return base.VisitMethodCall(methodCallExpression);
    }

    private Expression? TryRewrite(Expression source, LambdaExpression keySelector, LambdaExpression resultSelector)
    {
        var elementType = keySelector.Parameters[0].Type;
        var entityType = FindEntityType(elementType);

        // Shape 2: src.Select(proj).GroupBy(k).Select(aggs) — inline the one-level projection so
        // navigation paths hidden inside the projection become visible to the capture analysis.
        LambdaExpression? projection = null;
        Expression effectiveSource = source;
        if (entityType == null
            && source is MethodCallExpression sourceSelect
            && IsQueryableMethod(sourceSelect, QueryableMethods.Select)
            && UnwrapLambda(sourceSelect.Arguments[1]) is LambdaExpression projLambda
            && (projLambda.Body is MemberInitExpression || projLambda.Body is NewExpression { Members: not null })
            && FindEntityType(projLambda.Parameters[0].Type) is IEntityType projEntityType)
        {
            projection = projLambda;
            entityType = projEntityType;
            elementType = projLambda.Parameters[0].Type;
            effectiveSource = sourceSelect.Arguments[0];
        }

        if (entityType == null)
        {
            return null;
        }

        // The synthesized join root is a plain entity query root; it cannot replicate
        // non-standard root semantics of the source (TemporalAsOf must propagate AS OF to
        // joined navigations, FromSql roots are opaque, ...). Bail out when the source uses
        // any operator outside Queryable/Enumerable/EF-core extensions — conservative, but
        // correctness first (a temporal source joined to a plain root returns wrong rows).
        var sourceSafety = new SourceSafetyScanner();
        sourceSafety.Visit(effectiveSource);
        if (sourceSafety.HasUnsupportedOperator)
        {
            return null;
        }

        // The grouping may only be used as g.Key or as the source of a whitelisted aggregate.
        var scanner = new GroupingUsageScanner(resultSelector.Parameters[0]);
        scanner.Visit(resultSelector.Body);
        if (scanner.HasIllegalUsage || scanner.Aggregates.Count == 0)
        {
            return null;
        }

        // Inline the projection into key selector and aggregate selectors, if present.
        var aggregates = new List<AggregateCall>();
        var effectiveKeySelector = keySelector;
        if (projection != null)
        {
            var inlinedKey = InlineProjection(projection, keySelector);
            if (inlinedKey == null)
            {
                return null;
            }

            effectiveKeySelector = inlinedKey;
            foreach (var aggregate in scanner.Aggregates)
            {
                if (aggregate.Selector == null)
                {
                    aggregates.Add(aggregate);
                    continue;
                }

                var inlined = InlineProjection(projection, aggregate.Selector);
                if (inlined == null)
                {
                    return null;
                }

                aggregates.Add(aggregate with { Selector = inlined });
            }
        }
        else
        {
            aggregates.AddRange(scanner.Aggregates);
        }

        // Collect distinct reference-navigation scalar paths used inside aggregate selectors.
        var capturePaths = new List<NavigationPath>();
        foreach (var aggregate in aggregates)
        {
            if (aggregate.Selector == null)
            {
                continue;
            }

            var collector = new NavigationPathCollector(entityType, aggregate.Selector.Parameters[0]);
            collector.Visit(aggregate.Selector.Body);
            if (collector.HasUnsupportedUsage)
            {
                return null;
            }

            foreach (var path in collector.Paths)
            {
                if (!capturePaths.Any(existing => PathEquals(existing.Members, path.Members)))
                {
                    capturePaths.Add(path);
                }
            }
        }

        if (capturePaths.Count == 0)
        {
            return null;
        }

        // Build the join layers: one left join per distinct navigation prefix, in prefix order
        // (a 2-level path Subsidiary.CurrentSettings.X needs the Subsidiary hop before the
        // CurrentSettings hop; paths sharing a prefix share the hop — join dedup).
        var layers = new List<JoinLayer>();
        foreach (var path in capturePaths)
        {
            var currentType = entityType;
            for (var length = 1; length <= path.NavigationCount; length++)
            {
                var navigation = currentType.FindNavigation(path.Members[length - 1].Name)!;
                if (!layers.Any(l => PathEquals(l.NavigationPrefix, path.Members.Take(length).ToArray())))
                {
                    // v1 restrictions: single-column FK with CLR properties on both sides, same key type.
                    var fk = navigation.ForeignKey;
                    if (fk.Properties.Count != 1
                        || !navigation.IsOnDependent
                        || fk.Properties[0].PropertyInfo == null
                        || fk.PrincipalKey.Properties[0].PropertyInfo == null
                        || fk.Properties[0].ClrType != fk.PrincipalKey.Properties[0].ClrType)
                    {
                        return null;
                    }

                    layers.Add(new JoinLayer(
                        path.Members.Take(length).ToArray(),
                        navigation,
                        ParentLayerIndex: layers.FindIndex(l => PathEquals(l.NavigationPrefix, path.Members.Take(length - 1).ToArray()))));
                }

                currentType = navigation.TargetEntityType;
            }
        }

        // Synthesize the join chain: element type evolves T0=entity, Ti+1 = Capture<Ti, TargetClr>.
        var joinedSource = effectiveSource;
        var currentElementType = elementType;
        for (var i = 0; i < layers.Count; i++)
        {
            var layer = layers[i];
            var navigation = layer.Navigation;
            var targetClr = navigation.TargetEntityType.ClrType;
            var fkProperty = navigation.ForeignKey.Properties[0].PropertyInfo!;
            var pkProperty = navigation.ForeignKey.PrincipalKey.Properties[0].PropertyInfo!;
            var keyType = fkProperty.PropertyType;

            // Outer key: o => <owner-hop accessor>.Fk
            var outerParameter = Expression.Parameter(currentElementType, "o");
            var ownerAccessor = BuildHopAccessor(outerParameter, i, layer.ParentLayerIndex);
            var outerKey = Expression.Lambda(Expression.MakeMemberAccess(ownerAccessor, fkProperty), outerParameter);

            // Inner: query root for the principal entity, keyed by its principal key property.
            var innerRoot = new EntityQueryRootExpression((IEntityType)navigation.TargetEntityType);
            var innerParameter = Expression.Parameter(targetClr, "n");
            var innerKey = Expression.Lambda(Expression.MakeMemberAccess(innerParameter, pkProperty), innerParameter);

            // GroupJoin result: Capture<TOuter, IEnumerable<TInner>>
            var enumerableInnerType = typeof(IEnumerable<>).MakeGenericType(targetClr);
            var groupJoinPairType = TransparentIdentifierFactory.Create(currentElementType, enumerableInnerType);
            var innersParameter = Expression.Parameter(enumerableInnerType, "ns");
            var groupJoinResult = Expression.Lambda(
                NewCapture(groupJoinPairType, outerParameter, innersParameter), outerParameter, innersParameter);

            var groupJoin = Expression.Call(
                QueryableMethods.GroupJoin.MakeGenericMethod(currentElementType, targetClr, keyType, groupJoinPairType),
                joinedSource,
                innerRoot,
                Expression.Quote(outerKey),
                Expression.Quote(innerKey),
                Expression.Quote(groupJoinResult));

            // SelectMany(p => p.Inner.DefaultIfEmpty(), (p, n) => new Capture(p.Outer, n))
            var nextElementType = TransparentIdentifierFactory.Create(currentElementType, targetClr);
            var pairParameter = Expression.Parameter(groupJoinPairType, "p");
            var collectionSelector = Expression.Lambda(
                Expression.Call(
                    DefaultIfEmptyMethod.MakeGenericMethod(targetClr),
                    Expression.Field(pairParameter, groupJoinPairType.GetField("Inner")!)),
                pairParameter);
            var joinedParameter = Expression.Parameter(targetClr, "n");
            var selectManyResult = Expression.Lambda(
                NewCapture(
                    nextElementType,
                    Expression.Field(pairParameter, groupJoinPairType.GetField("Outer")!),
                    joinedParameter),
                pairParameter,
                joinedParameter);

            joinedSource = Expression.Call(
                QueryableMethods.SelectManyWithCollectionSelector.MakeGenericMethod(
                    groupJoinPairType, targetClr, nextElementType),
                groupJoin,
                Expression.Quote(collectionSelector),
                Expression.Quote(selectManyResult));

            currentElementType = nextElementType;
        }

        // Key selector: t => key(elementAccessor(t))
        var finalParameter = Expression.Parameter(currentElementType, "t");
        var elementAccessor = BuildHopAccessor(finalParameter, layers.Count, -1);
        var newKeyBody = new ParameterReplacer(effectiveKeySelector.Parameters[0], elementAccessor)
            .Visit(effectiveKeySelector.Body);
        var newKeySelector = Expression.Lambda(newKeyBody, finalParameter);

        // Rebuild aggregates over the joined element type.
        var keyType2 = effectiveKeySelector.ReturnType;
        var newGroupingParameter = Expression.Parameter(
            typeof(IGrouping<,>).MakeGenericType(keyType2, currentElementType), "g");
        var originalGroupingElementType = resultSelector.Parameters[0].Type.GetGenericArguments()[1];

        var aggregateReplacements = new Dictionary<Expression, Expression>(ReferenceEqualityComparer.Instance);
        foreach (var aggregate in aggregates)
        {
            LambdaExpression? newSelector = null;
            if (aggregate.Selector != null)
            {
                var lambdaParameter = Expression.Parameter(currentElementType, "t");
                var remapper = new AggregateSelectorRemapper(
                    aggregate.Selector.Parameters[0], lambdaParameter, capturePaths, layers);
                newSelector = Expression.Lambda(remapper.Visit(aggregate.Selector.Body), lambdaParameter);
            }

            aggregateReplacements[aggregate.Call] = RebuildAggregate(
                aggregate, newGroupingParameter, originalGroupingElementType, currentElementType, newSelector);
        }

        var resultRebinder = new ResultSelectorRebinder(
            resultSelector.Parameters[0], newGroupingParameter, aggregateReplacements);
        var newResultBody = resultRebinder.Visit(resultSelector.Body);
        var newResultSelector = Expression.Lambda(newResultBody, newGroupingParameter);

        var newGroupBy = Expression.Call(
            QueryableMethods.GroupByWithKeySelector.MakeGenericMethod(currentElementType, keyType2),
            joinedSource,
            Expression.Quote(newKeySelector));

        return Expression.Call(
            QueryableMethods.Select.MakeGenericMethod(newGroupingParameter.Type, newResultSelector.ReturnType),
            newGroupBy,
            Expression.Quote(newResultSelector));
    }

    private IEntityType? FindEntityType(Type clrType)
        => _model.FindEntityType(clrType)
            ?? _model.GetEntityTypes().FirstOrDefault(
                et => !et.IsOwned() && clrType != typeof(object) && clrType.IsAssignableFrom(et.ClrType));

    private static bool IsQueryableMethod(MethodCallExpression methodCallExpression, MethodInfo genericMethodDefinition)
        => methodCallExpression.Method.IsGenericMethod
            && methodCallExpression.Method.GetGenericMethodDefinition() == genericMethodDefinition;

    private static LambdaExpression? UnwrapLambda(Expression expression)
        => expression switch
        {
            UnaryExpression { NodeType: ExpressionType.Quote, Operand: LambdaExpression lambda } => lambda,
            LambdaExpression lambda => lambda,
            _ => null
        };

    private static bool PathEquals(MemberInfo[] left, MemberInfo[] right)
        => left.Length == right.Length && left.Zip(right).All(pair => Equals(pair.First, pair.Second));

    private static Expression NewCapture(Type pairType, Expression outer, Expression inner)
        => Expression.New(
            pairType.GetConstructors().Single(),
            [outer, inner],
            [pairType.GetField("Outer")!,
             pairType.GetField("Inner")!]);

    /// <summary>
    ///     Accessor for a hop within the nested capture chain, seen from a parameter whose type
    ///     includes <paramref name="depth" /> layers. <paramref name="layerIndex" /> -1 = the root element
    ///     (t.Outer^depth); otherwise layer i's joined entity (t.Outer^(depth-1-i).Inner).
    /// </summary>
    private static Expression BuildHopAccessor(Expression parameter, int depth, int layerIndex)
    {
        var accessor = parameter;
        var hops = layerIndex < 0 ? depth : depth - 1 - layerIndex;
        for (var i = 0; i < hops; i++)
        {
            accessor = Expression.Field(
                accessor, accessor.Type.GetField("Outer")!);
        }

        return layerIndex < 0
            ? accessor
            : Expression.Field(accessor, accessor.Type.GetField("Inner")!);
    }

    private static MethodCallExpression RebuildAggregate(
        AggregateCall aggregate,
        ParameterExpression newGroupingParameter,
        Type originalElementType,
        Type newElementType,
        LambdaExpression? newSelector)
    {
        var method = aggregate.Call.Method;
        var newMethod = method.GetGenericMethodDefinition().MakeGenericMethod(
            method.GetGenericArguments().Select(t => t == originalElementType ? newElementType : t).ToArray());

        Expression newSource = newGroupingParameter;
        if (aggregate.SourceAsQueryable != null)
        {
            newSource = Expression.Call(
                QueryableMethods.AsQueryable.MakeGenericMethod(newElementType), newGroupingParameter);
        }

        var arguments = new List<Expression> { newSource };
        if (newSelector != null)
        {
            arguments.Add(method.DeclaringType == typeof(Queryable) ? Expression.Quote(newSelector) : newSelector);
        }

        return Expression.Call(newMethod, arguments);
    }

    private static LambdaExpression? InlineProjection(LambdaExpression projection, LambdaExpression lambda)
    {
        var bindings = new Dictionary<MemberInfo, Expression>();
        switch (projection.Body)
        {
            case MemberInitExpression memberInit:
                if (memberInit.Bindings.Any(b => b is not MemberAssignment))
                {
                    return null;
                }

                foreach (var binding in memberInit.Bindings.Cast<MemberAssignment>())
                {
                    bindings[binding.Member] = binding.Expression;
                }

                break;

            case NewExpression { Members: not null } newExpression:
                for (var i = 0; i < newExpression.Members.Count; i++)
                {
                    bindings[newExpression.Members[i]] = newExpression.Arguments[i];
                }

                break;

            default:
                return null;
        }

        var entityParameter = Expression.Parameter(projection.Parameters[0].Type, lambda.Parameters[0].Name);
        var inliner = new ProjectionInliner(lambda.Parameters[0], projection.Parameters[0], entityParameter, bindings);
        var inlinedBody = inliner.Visit(lambda.Body);

        return inliner.Failed ? null : Expression.Lambda(inlinedBody, entityParameter);
    }

    /// <summary>
    ///     Flags queryable operators the rewrite cannot reason about: anything not declared on
    ///     Queryable, Enumerable or the EF-core extensions (e.g. TemporalAsOf, FromSqlRaw live in
    ///     provider/relational assemblies and change root semantics the synthesized join root
    ///     would not share).
    /// </summary>
    private sealed class SourceSafetyScanner : ExpressionVisitor
    {
        public bool HasUnsupportedOperator { get; private set; }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            // Non-plain query roots (TemporalAsOfQueryRootExpression, FromSqlQueryRootExpression, ...)
            // carry semantics the synthesized plain entity root would not share. The funcletizer has
            // already evaluated e.g. TemporalAsOf(...) into its root by the time this runs.
            if (extensionExpression is QueryRootExpression
                && extensionExpression.GetType() != typeof(EntityQueryRootExpression))
            {
                HasUnsupportedOperator = true;
                return extensionExpression;
            }

            return base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var declaringType = methodCallExpression.Method.DeclaringType;
            if (methodCallExpression.Method.IsStatic
                && typeof(IQueryable).IsAssignableFrom(methodCallExpression.Type)
                && declaringType != typeof(Queryable)
                && declaringType != typeof(Enumerable)
                && declaringType?.FullName != "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions")
            {
                HasUnsupportedOperator = true;
                return methodCallExpression;
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }

    private sealed record AggregateCall(MethodCallExpression Call, LambdaExpression? Selector, Expression? SourceAsQueryable);

    private sealed record NavigationPath(MemberInfo[] Members, int NavigationCount);

    private sealed record JoinLayer(MemberInfo[] NavigationPrefix, INavigation Navigation, int ParentLayerIndex);

    private sealed class GroupingUsageScanner(ParameterExpression groupingParameter) : ExpressionVisitor
    {
        public List<AggregateCall> Aggregates { get; } = [];
        public bool HasIllegalUsage { get; private set; }

        [return: NotNullIfNotNull(nameof(expression))]
        public override Expression? Visit(Expression? expression)
        {
            if (expression == groupingParameter)
            {
                HasIllegalUsage = true;
            }

            return base.Visit(expression);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            if (memberExpression.Expression == groupingParameter
                && memberExpression.Member.Name == nameof(IGrouping<object, object>.Key))
            {
                return memberExpression;
            }

            return base.VisitMember(memberExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (TryMatchAggregate(methodCallExpression, out var aggregate))
            {
                Aggregates.Add(aggregate);
                if (aggregate.Selector != null)
                {
                    base.Visit(aggregate.Selector.Body);
                }

                return methodCallExpression;
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private bool TryMatchAggregate(
            MethodCallExpression methodCallExpression, [NotNullWhen(true)] out AggregateCall? aggregate)
        {
            aggregate = null;
            var method = methodCallExpression.Method;
            if (!method.IsStatic
                || (method.DeclaringType != typeof(Enumerable) && method.DeclaringType != typeof(Queryable))
                || !method.IsGenericMethod
                || methodCallExpression.Arguments.Count is < 1 or > 2)
            {
                return false;
            }

            Expression? sourceAsQueryable = null;
            var sourceArgument = methodCallExpression.Arguments[0];
            if (sourceArgument is MethodCallExpression asQueryableCall
                && IsQueryableMethod(asQueryableCall, QueryableMethods.AsQueryable)
                && asQueryableCall.Arguments[0] == groupingParameter)
            {
                sourceAsQueryable = asQueryableCall;
            }
            else if (sourceArgument != groupingParameter)
            {
                return false;
            }

            var isSelectorAggregate = SelectorAggregateMethodNames.Contains(method.Name);
            var isPredicateAggregate = PredicateAggregateMethodNames.Contains(method.Name);
            if (!isSelectorAggregate && !isPredicateAggregate)
            {
                return false;
            }

            LambdaExpression? selector = null;
            if (methodCallExpression.Arguments.Count == 2)
            {
                selector = UnwrapLambda(methodCallExpression.Arguments[1]);
                if (selector == null)
                {
                    return false;
                }
            }
            else if (isSelectorAggregate)
            {
                return false;
            }

            aggregate = new AggregateCall(methodCallExpression, selector, sourceAsQueryable);
            return true;
        }
    }

    private sealed class NavigationPathCollector(IEntityType entityType, ParameterExpression parameter)
        : ExpressionVisitor
    {
        public List<NavigationPath> Paths { get; } = [];
        public bool HasUnsupportedUsage { get; private set; }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var chain = new List<MemberInfo>();
            var current = (Expression?)memberExpression;
            while (current is MemberExpression member)
            {
                chain.Insert(0, member.Member);
                current = member.Expression;
            }

            if (current != parameter)
            {
                return base.VisitMember(memberExpression);
            }

            var currentType = entityType;
            var navigationCount = 0;
            foreach (var member in chain)
            {
                var navigation = currentType.FindNavigation(member.Name);
                if (navigation == null)
                {
                    break;
                }

                if (navigation.IsCollection || navigation.TargetEntityType.IsOwned())
                {
                    return memberExpression;
                }

                currentType = navigation.TargetEntityType;
                navigationCount++;
            }

            if (navigationCount == 0)
            {
                return memberExpression;
            }

            if (navigationCount == chain.Count)
            {
                HasUnsupportedUsage = true;
                return memberExpression;
            }

            var path = new NavigationPath(chain.ToArray(), navigationCount);
            if (!Paths.Any(existing => PathEquals(existing.Members, path.Members)))
            {
                Paths.Add(path);
            }

            return memberExpression;
        }
    }

    private sealed class AggregateSelectorRemapper(
        ParameterExpression originalParameter,
        ParameterExpression newParameter,
        List<NavigationPath> capturePaths,
        List<JoinLayer> layers)
        : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var chain = new List<MemberInfo>();
            var current = (Expression?)memberExpression;
            while (current is MemberExpression member)
            {
                chain.Insert(0, member.Member);
                current = member.Expression;
            }

            if (current == originalParameter)
            {
                foreach (var path in capturePaths)
                {
                    if (PathEquals(path.Members, chain.ToArray()))
                    {
                        // Hop for the full navigation prefix, then the scalar tail.
                        var prefix = path.Members.Take(path.NavigationCount).ToArray();
                        var layerIndex = layers.FindIndex(l => PathEquals(l.NavigationPrefix, prefix));
                        var accessor = BuildHopAccessor(newParameter, layers.Count, layerIndex);
                        return path.Members.Skip(path.NavigationCount).Aggregate(accessor, Expression.MakeMemberAccess);
                    }
                }
            }

            return base.VisitMember(memberExpression);
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
            => parameterExpression == originalParameter
                ? BuildHopAccessor(newParameter, layers.Count, -1)
                : base.VisitParameter(parameterExpression);
    }

    private sealed class ResultSelectorRebinder(
        ParameterExpression originalParameter,
        ParameterExpression newParameter,
        Dictionary<Expression, Expression> aggregateReplacements)
        : ExpressionVisitor
    {
        [return: NotNullIfNotNull(nameof(expression))]
        public override Expression? Visit(Expression? expression)
            => expression != null && aggregateReplacements.TryGetValue(expression, out var replacement)
                ? replacement
                : base.Visit(expression);

        protected override Expression VisitMember(MemberExpression memberExpression)
            => memberExpression.Expression == originalParameter
                && memberExpression.Member.Name == nameof(IGrouping<object, object>.Key)
                    ? Expression.MakeMemberAccess(
                        newParameter, newParameter.Type.GetProperty(nameof(IGrouping<object, object>.Key))!)
                    : base.VisitMember(memberExpression);
    }

    private sealed class ProjectionInliner(
        ParameterExpression modelParameter,
        ParameterExpression projectionParameter,
        ParameterExpression entityParameter,
        Dictionary<MemberInfo, Expression> bindings)
        : ExpressionVisitor
    {
        public bool Failed { get; private set; }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            if (memberExpression.Expression == modelParameter)
            {
                var binding = bindings.Keys.FirstOrDefault(m => m.Name == memberExpression.Member.Name);
                if (binding == null)
                {
                    Failed = true;
                    return memberExpression;
                }

                return new ParameterReplacer(projectionParameter, entityParameter).Visit(bindings[binding]);
            }

            return base.VisitMember(memberExpression);
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            if (parameterExpression == modelParameter)
            {
                Failed = true;
            }

            return base.VisitParameter(parameterExpression);
        }
    }

    private sealed class ParameterReplacer(ParameterExpression from, Expression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression parameterExpression)
            => parameterExpression == from ? to : base.VisitParameter(parameterExpression);
    }
}
