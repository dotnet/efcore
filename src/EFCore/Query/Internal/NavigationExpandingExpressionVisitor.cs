// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public partial class NavigationExpandingExpressionVisitor : ExpressionVisitor
    {
        private static readonly PropertyInfo _queryContextContextPropertyInfo
            = typeof(QueryContext)
                .GetTypeInfo()
                .GetDeclaredProperty(nameof(QueryContext.Context));

        private static readonly Dictionary<MethodInfo, MethodInfo> _predicateLessMethodInfo = new Dictionary<MethodInfo, MethodInfo>
        {
            { QueryableMethods.FirstWithPredicate, QueryableMethods.FirstWithoutPredicate },
            { QueryableMethods.FirstOrDefaultWithPredicate, QueryableMethods.FirstOrDefaultWithoutPredicate },
            { QueryableMethods.SingleWithPredicate, QueryableMethods.SingleWithoutPredicate },
            { QueryableMethods.SingleOrDefaultWithPredicate, QueryableMethods.SingleOrDefaultWithoutPredicate },
            { QueryableMethods.LastWithPredicate, QueryableMethods.LastWithoutPredicate },
            { QueryableMethods.LastOrDefaultWithPredicate, QueryableMethods.LastOrDefaultWithoutPredicate }
        };

        private static readonly List<MethodInfo> _supportedFilteredIncludeOperations = new List<MethodInfo>
        {
            QueryableMethods.Where,
            QueryableMethods.OrderBy,
            QueryableMethods.OrderByDescending,
            QueryableMethods.ThenBy,
            QueryableMethods.ThenByDescending,
            QueryableMethods.Skip,
            QueryableMethods.Take,
            QueryableMethods.AsQueryable
        };

        private readonly QueryTranslationPreprocessor _queryTranslationPreprocessor;
        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly PendingSelectorExpandingExpressionVisitor _pendingSelectorExpandingExpressionVisitor;
        private readonly SubqueryMemberPushdownExpressionVisitor _subqueryMemberPushdownExpressionVisitor;
        private readonly NullCheckRemovingExpressionVisitor _nullCheckRemovingExpressionVisitor;
        private readonly ReducingExpressionVisitor _reducingExpressionVisitor;
        private readonly EntityReferenceOptionalMarkingExpressionVisitor _entityReferenceOptionalMarkingExpressionVisitor;
        private readonly RemoveRedundantNavigationComparisonExpressionVisitor _removeRedundantNavigationComparisonExpressionVisitor;
        private readonly HashSet<string> _parameterNames = new HashSet<string>();
        private readonly ParameterExtractingExpressionVisitor _parameterExtractingExpressionVisitor;
        private readonly HashSet<IEntityType> _nonCyclicAutoIncludeEntityTypes;

        private readonly Dictionary<IEntityType, LambdaExpression> _parameterizedQueryFilterPredicateCache
            = new Dictionary<IEntityType, LambdaExpression>();

        private readonly Parameters _parameters = new Parameters();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NavigationExpandingExpressionVisitor(
            [NotNull] QueryTranslationPreprocessor queryTranslationPreprocessor,
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] IEvaluatableExpressionFilter evaluatableExpressionFilter)
        {
            _queryTranslationPreprocessor = queryTranslationPreprocessor;
            _queryCompilationContext = queryCompilationContext;
            _pendingSelectorExpandingExpressionVisitor = new PendingSelectorExpandingExpressionVisitor(this);
            _subqueryMemberPushdownExpressionVisitor = new SubqueryMemberPushdownExpressionVisitor(queryCompilationContext.Model);
            _nullCheckRemovingExpressionVisitor = new NullCheckRemovingExpressionVisitor();
            _reducingExpressionVisitor = new ReducingExpressionVisitor();
            _entityReferenceOptionalMarkingExpressionVisitor = new EntityReferenceOptionalMarkingExpressionVisitor();
            _removeRedundantNavigationComparisonExpressionVisitor = new RemoveRedundantNavigationComparisonExpressionVisitor(
                queryCompilationContext.Logger);
            _parameterExtractingExpressionVisitor = new ParameterExtractingExpressionVisitor(
                evaluatableExpressionFilter,
                _parameters,
                _queryCompilationContext.ContextType,
                _queryCompilationContext.Model,
                _queryCompilationContext.Logger,
                parameterize: false,
                generateContextAccessors: true);

            if (!_queryCompilationContext.IgnoreAutoIncludes)
            {
                _nonCyclicAutoIncludeEntityTypes = new HashSet<IEntityType>();
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression Expand([NotNull] Expression query)
        {
            var result = Visit(query);
            result = new PendingSelectorExpandingExpressionVisitor(this, applyIncludes: true).Visit(result);
            result = Reduce(result);

            var dbContextOnQueryContextPropertyAccess =
                Expression.Convert(
                    Expression.Property(
                        QueryCompilationContext.QueryContextParameter,
                        _queryContextContextPropertyInfo),
                    _queryCompilationContext.ContextType);

            foreach (var parameterValue in _parameters.ParameterValues)
            {
                var lambda = (LambdaExpression)parameterValue.Value;
                var remappedLambdaBody = ReplacingExpressionVisitor.Replace(
                    lambda.Parameters[0],
                    dbContextOnQueryContextPropertyAccess,
                    lambda.Body);

                _queryCompilationContext.RegisterRuntimeParameter(
                    parameterValue.Key,
                    Expression.Lambda(
                        remappedLambdaBody.Type.IsValueType
                            ? Expression.Convert(remappedLambdaBody, typeof(object))
                            : remappedLambdaBody,
                        QueryCompilationContext.QueryContextParameter));
            }

            return result;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            switch (extensionExpression)
            {
                case QueryRootExpression queryRootExpression:
                    var entityType = queryRootExpression.EntityType;
#pragma warning disable CS0618 // Type or member is obsolete
                    var definingQuery = entityType.GetDefiningQuery();
#pragma warning restore CS0618 // Type or member is obsolete
                    NavigationExpansionExpression navigationExpansionExpression;
                    if (definingQuery != null
                        // Apply defining query only when it is not custom query root
                        && queryRootExpression.GetType() == typeof(QueryRootExpression))
                    {
                        var processedDefiningQueryBody = _parameterExtractingExpressionVisitor.ExtractParameters(definingQuery.Body);
                        processedDefiningQueryBody = _queryTranslationPreprocessor.NormalizeQueryableMethod(processedDefiningQueryBody);
                        processedDefiningQueryBody = _nullCheckRemovingExpressionVisitor.Visit(processedDefiningQueryBody);
                        processedDefiningQueryBody =
                            new SelfReferenceEntityQueryableRewritingExpressionVisitor(this, entityType).Visit(processedDefiningQueryBody);

                        processedDefiningQueryBody = Visit(processedDefiningQueryBody);
                        processedDefiningQueryBody = _pendingSelectorExpandingExpressionVisitor.Visit(processedDefiningQueryBody);
                        processedDefiningQueryBody = Reduce(processedDefiningQueryBody);
                        navigationExpansionExpression = CreateNavigationExpansionExpression(processedDefiningQueryBody, entityType);
                    }
                    else
                    {
                        navigationExpansionExpression = CreateNavigationExpansionExpression(queryRootExpression, entityType);
                    }

                    return ApplyQueryFilter(entityType, navigationExpansionExpression);

                case NavigationExpansionExpression _:
                case OwnedNavigationReference _:
                    return extensionExpression;

                default:
                    return base.VisitExtension(extensionExpression);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            var innerExpression = Visit(memberExpression.Expression);

            // Convert ICollection<T>.Count to Count<T>()
            if (memberExpression.Expression != null
                && memberExpression.Member.Name == nameof(ICollection<int>.Count)
                && memberExpression.Expression.Type.GetInterfaces().Append(memberExpression.Expression.Type)
                    .Any(e => e.IsGenericType && e.GetGenericTypeDefinition() == typeof(ICollection<>)))
            {
                var innerQueryable = UnwrapCollectionMaterialization(innerExpression);

                if (innerQueryable.Type.TryGetElementType(typeof(IQueryable<>)) != null)
                {
                    return Visit(
                        Expression.Call(
                            QueryableMethods.CountWithoutPredicate.MakeGenericMethod(innerQueryable.Type.TryGetSequenceType()),
                            innerQueryable));
                }
            }

            var updatedExpression = (Expression)memberExpression.Update(innerExpression);
            if (innerExpression is NavigationExpansionExpression navigationExpansionExpression
                && navigationExpansionExpression.CardinalityReducingGenericMethodInfo != null)
            {
                // This is FirstOrDefault.Member
                // due to SubqueryMemberPushdown, this may be collection navigation which was not pushed down
                navigationExpansionExpression =
                    (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(navigationExpansionExpression);
                var expandedExpression = new ExpandingExpressionVisitor(this, navigationExpansionExpression).Visit(updatedExpression);
                if (expandedExpression != updatedExpression)
                {
                    updatedExpression = Visit(expandedExpression);
                }
            }

            return updatedExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            var method = methodCallExpression.Method;
            if (method.DeclaringType == typeof(Queryable)
                || method.DeclaringType == typeof(QueryableExtensions)
                || method.DeclaringType == typeof(EntityFrameworkQueryableExtensions))
            {
                var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;
                var firstArgument = Visit(methodCallExpression.Arguments[0]);
                if (firstArgument is NavigationExpansionExpression source)
                {
                    if (source.PendingOrderings.Any()
                        && genericMethod != QueryableMethods.ThenBy
                        && genericMethod != QueryableMethods.ThenByDescending)
                    {
                        ApplyPendingOrderings(source);
                    }

                    switch (method.Name)
                    {
                        case nameof(Queryable.AsQueryable)
                            when genericMethod == QueryableMethods.AsQueryable:
                            return source;

                        case nameof(Queryable.Any)
                            when genericMethod == QueryableMethods.AnyWithoutPredicate:

                        case nameof(Queryable.Count)
                            when genericMethod == QueryableMethods.CountWithoutPredicate:

                        case nameof(Queryable.LongCount)
                            when genericMethod == QueryableMethods.LongCountWithoutPredicate:
                            return ProcessAllAnyCountLongCount(
                                source,
                                genericMethod,
                                predicate: null);

                        case nameof(Queryable.All)
                            when genericMethod == QueryableMethods.All:
                        case nameof(Queryable.Any)
                            when genericMethod == QueryableMethods.AnyWithPredicate:
                        case nameof(Queryable.Count)
                            when genericMethod == QueryableMethods.CountWithPredicate:
                        case nameof(Queryable.LongCount)
                            when genericMethod == QueryableMethods.LongCountWithPredicate:
                            return ProcessAllAnyCountLongCount(
                                source,
                                genericMethod,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());

                        case nameof(Queryable.Average)
                            when QueryableMethods.IsAverageWithoutSelector(method):
                        case nameof(Queryable.Max)
                            when genericMethod == QueryableMethods.MaxWithoutSelector:
                        case nameof(Queryable.Min)
                            when genericMethod == QueryableMethods.MinWithoutSelector:
                        case nameof(Queryable.Sum)
                            when QueryableMethods.IsSumWithoutSelector(method):
                            return ProcessAverageMaxMinSum(
                                source,
                                genericMethod ?? method,
                                selector: null);

                        case nameof(Queryable.Average)
                            when QueryableMethods.IsAverageWithSelector(method):
                        case nameof(Queryable.Sum)
                            when QueryableMethods.IsSumWithSelector(method):
                        case nameof(Queryable.Max)
                            when genericMethod == QueryableMethods.MaxWithSelector:
                        case nameof(Queryable.Min)
                            when genericMethod == QueryableMethods.MinWithSelector:
                            return ProcessAverageMaxMinSum(
                                source,
                                genericMethod ?? method,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());

                        case nameof(Queryable.Distinct)
                            when genericMethod == QueryableMethods.Distinct:
                            return ProcessDistinct(source, genericMethod);

                        case nameof(Queryable.Skip)
                            when genericMethod == QueryableMethods.Skip:
                        case nameof(Queryable.Take)
                            when genericMethod == QueryableMethods.Take:
                            return ProcessSkipTake(
                                source,
                                genericMethod,
                                methodCallExpression.Arguments[1]);

                        case nameof(Queryable.Contains)
                            when genericMethod == QueryableMethods.Contains:
                            return ProcessContains(
                                source,
                                methodCallExpression.Arguments[1]);

                        case nameof(Queryable.First)
                            when genericMethod == QueryableMethods.FirstWithoutPredicate:
                        case nameof(Queryable.FirstOrDefault)
                            when genericMethod == QueryableMethods.FirstOrDefaultWithoutPredicate:
                        case nameof(Queryable.Single)
                            when genericMethod == QueryableMethods.SingleWithoutPredicate:
                        case nameof(Queryable.SingleOrDefault)
                            when genericMethod == QueryableMethods.SingleOrDefaultWithoutPredicate:
                        case nameof(Queryable.Last)
                            when genericMethod == QueryableMethods.LastWithoutPredicate:
                        case nameof(Queryable.LastOrDefault)
                            when genericMethod == QueryableMethods.LastOrDefaultWithoutPredicate:
                            return ProcessFirstSingleLastOrDefault(
                                source,
                                genericMethod,
                                predicate: null,
                                methodCallExpression.Type);

                        case nameof(Queryable.First)
                            when genericMethod == QueryableMethods.FirstWithPredicate:
                        case nameof(Queryable.FirstOrDefault)
                            when genericMethod == QueryableMethods.FirstOrDefaultWithPredicate:
                        case nameof(Queryable.Single)
                            when genericMethod == QueryableMethods.SingleWithPredicate:
                        case nameof(Queryable.SingleOrDefault)
                            when genericMethod == QueryableMethods.SingleOrDefaultWithPredicate:
                        case nameof(Queryable.Last)
                            when genericMethod == QueryableMethods.LastWithPredicate:
                        case nameof(Queryable.LastOrDefault)
                            when genericMethod == QueryableMethods.LastOrDefaultWithPredicate:
                            return ProcessFirstSingleLastOrDefault(
                                source,
                                genericMethod,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                methodCallExpression.Type);

                        case nameof(Queryable.Join)
                            when genericMethod == QueryableMethods.Join:
                        {
                            var secondArgument = Visit(methodCallExpression.Arguments[1]);
                            if (secondArgument is NavigationExpansionExpression innerSource)
                            {
                                return ProcessJoin(
                                    source,
                                    innerSource,
                                    methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                                    methodCallExpression.Arguments[3].UnwrapLambdaFromQuote(),
                                    methodCallExpression.Arguments[4].UnwrapLambdaFromQuote());
                            }

                            goto default;
                        }

                        case nameof(QueryableExtensions.LeftJoin)
                            when genericMethod == QueryableExtensions.LeftJoinMethodInfo:
                        {
                            var secondArgument = Visit(methodCallExpression.Arguments[1]);
                            if (secondArgument is NavigationExpansionExpression innerSource)
                            {
                                return ProcessLeftJoin(
                                    source,
                                    innerSource,
                                    methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                                    methodCallExpression.Arguments[3].UnwrapLambdaFromQuote(),
                                    methodCallExpression.Arguments[4].UnwrapLambdaFromQuote());
                            }

                            goto default;
                        }

                        case nameof(Queryable.SelectMany)
                            when genericMethod == QueryableMethods.SelectManyWithoutCollectionSelector:
                            return ProcessSelectMany(
                                source,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                null);

                        case nameof(Queryable.SelectMany)
                            when genericMethod == QueryableMethods.SelectManyWithCollectionSelector:
                            return ProcessSelectMany(
                                source,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                methodCallExpression.Arguments[2].UnwrapLambdaFromQuote());

                        case nameof(Queryable.Concat)
                            when genericMethod == QueryableMethods.Concat:
                        case nameof(Queryable.Except)
                            when genericMethod == QueryableMethods.Except:
                        case nameof(Queryable.Intersect)
                            when genericMethod == QueryableMethods.Intersect:
                        case nameof(Queryable.Union)
                            when genericMethod == QueryableMethods.Union:
                        {
                            var secondArgument = Visit(methodCallExpression.Arguments[1]);
                            if (secondArgument is NavigationExpansionExpression innerSource)
                            {
                                return ProcessSetOperation(source, genericMethod, innerSource);
                            }

                            goto default;
                        }

                        case nameof(Queryable.Cast)
                            when genericMethod == QueryableMethods.Cast:
                        case nameof(Queryable.OfType)
                            when genericMethod == QueryableMethods.OfType:
                            return ProcessCastOfType(
                                source,
                                genericMethod,
                                methodCallExpression.Type.TryGetSequenceType());

                        case nameof(EntityFrameworkQueryableExtensions.Include):
                        case nameof(EntityFrameworkQueryableExtensions.ThenInclude):
                            return ProcessInclude(
                                source,
                                methodCallExpression.Arguments[1],
                                string.Equals(
                                    method.Name,
                                    nameof(EntityFrameworkQueryableExtensions.ThenInclude)));

                        case nameof(Queryable.GroupBy)
                            when genericMethod == QueryableMethods.GroupByWithKeySelector:
                            return ProcessGroupBy(
                                source,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                null,
                                null);

                        case nameof(Queryable.GroupBy)
                            when genericMethod == QueryableMethods.GroupByWithKeyElementSelector:
                            return ProcessGroupBy(
                                source,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                                null);

                        case nameof(Queryable.GroupBy)
                            when genericMethod == QueryableMethods.GroupByWithKeyElementResultSelector:
                            return ProcessGroupBy(
                                source,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                                methodCallExpression.Arguments[3].UnwrapLambdaFromQuote());

                        case nameof(Queryable.GroupBy)
                            when genericMethod == QueryableMethods.GroupByWithKeyResultSelector:
                            return ProcessGroupBy(
                                source,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                null,
                                methodCallExpression.Arguments[2].UnwrapLambdaFromQuote());

                        case nameof(Queryable.OrderBy)
                            when genericMethod == QueryableMethods.OrderBy:
                        case nameof(Queryable.OrderByDescending)
                            when genericMethod == QueryableMethods.OrderByDescending:
                            return ProcessOrderByThenBy(
                                source,
                                genericMethod,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                thenBy: false);

                        case nameof(Queryable.ThenBy)
                            when genericMethod == QueryableMethods.ThenBy:
                        case nameof(Queryable.ThenByDescending)
                            when genericMethod == QueryableMethods.ThenByDescending:
                            return ProcessOrderByThenBy(
                                source,
                                genericMethod,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                thenBy: true);

                        case nameof(Queryable.Reverse)
                            when genericMethod == QueryableMethods.Reverse:
                            return ProcessReverse(source);

                        case nameof(Queryable.Select)
                            when genericMethod == QueryableMethods.Select:
                            return ProcessSelect(
                                source,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());

                        case nameof(Queryable.Where)
                            when genericMethod == QueryableMethods.Where:
                            return ProcessWhere(
                                source,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());

                        case nameof(Queryable.DefaultIfEmpty)
                            when genericMethod == QueryableMethods.DefaultIfEmptyWithoutArgument:
                            return ProcessDefaultIfEmpty(source);

                        default:
                            // Aggregate overloads
                            // GroupJoin overloads
                            // Zip
                            // SequenceEqual overloads
                            // ElementAt
                            // ElementAtOrDefault
                            // SkipWhile
                            // TakeWhile
                            // DefaultIfEmpty with argument
                            // Index based lambda overloads of Where, SkipWhile, TakeWhile, Select, SelectMany
                            // IEqualityComparer overloads of Distinct, Contains, Join, Except, Intersect, Union, OrderBy, ThenBy, OrderByDescending, ThenByDescending, GroupBy
                            throw new InvalidOperationException(
                                CoreStrings.TranslationFailed(
                                    _reducingExpressionVisitor.Visit(methodCallExpression).Print()));
                    }
                }

                if (genericMethod == QueryableMethods.AsQueryable)
                {
                    return UnwrapCollectionMaterialization(firstArgument);
                }

                if (firstArgument.Type.TryGetElementType(typeof(IQueryable<>)) == null)
                {
                    // firstArgument was not an queryable
                    var visitedArguments = new[] { firstArgument }
                        .Concat(methodCallExpression.Arguments.Skip(1).Select(Visit));

                    return ConvertToEnumerable(method, visitedArguments);
                }

                throw new InvalidOperationException(CoreStrings.TranslationFailed(methodCallExpression.Print()));
            }

            // Remove MaterializeCollectionNavigationExpression when applying ToList/ToArray
            if (method.IsGenericMethod
                && (method.GetGenericMethodDefinition() == EnumerableMethods.ToList
                    || method.GetGenericMethodDefinition() == EnumerableMethods.ToArray))
            {
                return methodCallExpression.Update(
                    null, new[] { UnwrapCollectionMaterialization(Visit(methodCallExpression.Arguments[0])) });
            }

            return ProcessUnknownMethod(methodCallExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            var operand = Visit(unaryExpression.Operand);
            // Convert Array.Length to Count()
            if (unaryExpression.Operand.Type.IsArray
                && unaryExpression.NodeType == ExpressionType.ArrayLength)
            {
                var innerQueryable = UnwrapCollectionMaterialization(operand);
                // Only if inner is queryable as array properties could also have Length access
                if (innerQueryable.Type.TryGetElementType(typeof(IQueryable<>)) is Type elementType)
                {
                    return Visit(Expression.Call(QueryableMethods.CountWithoutPredicate.MakeGenericMethod(elementType), innerQueryable));
                }
            }

            return unaryExpression.Update(operand);
        }

        private Expression ProcessAllAnyCountLongCount(
            NavigationExpansionExpression source,
            MethodInfo genericMethod,
            LambdaExpression predicate)
        {
            if (predicate != null)
            {
                predicate = ProcessLambdaExpression(source, predicate);

                return Expression.Call(
                    genericMethod.MakeGenericMethod(source.SourceElementType), source.Source, Expression.Quote(predicate));
            }

            return Expression.Call(genericMethod.MakeGenericMethod(source.SourceElementType), source.Source);
        }

        private Expression ProcessAverageMaxMinSum(NavigationExpansionExpression source, MethodInfo method, LambdaExpression selector)
        {
            if (selector != null)
            {
                source = ProcessSelect(source, selector);
                source = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(source);

                var selectorLambda = GenerateLambda(source.PendingSelector, source.CurrentParameter);
                if (method.GetGenericArguments().Length == 2)
                {
                    // Min/Max with selector has 2 generic parameters
                    method = method.MakeGenericMethod(source.SourceElementType, selectorLambda.ReturnType);
                }
                else
                {
                    method = method.MakeGenericMethod(source.SourceElementType);
                }

                return Expression.Call(method, source.Source, selectorLambda);
            }

            source = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(source);
            var queryable = Reduce(source);

            if (method.GetGenericArguments().Length == 1)
            {
                // Min/Max without selector has 1 generic parameters
                method = method.MakeGenericMethod(queryable.Type.TryGetSequenceType());
            }

            return Expression.Call(method, queryable);
        }

        private NavigationExpansionExpression ProcessCastOfType(
            NavigationExpansionExpression source,
            MethodInfo genericMethod,
            Type castType)
        {
            if (castType.IsAssignableFrom(source.PendingSelector.Type)
                || castType == typeof(object))
            {
                // Casting to base/implementing interface is redundant
                return source;
            }

            source = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(source);
            var newStructure = SnapshotExpression(source.PendingSelector);
            var queryable = Reduce(source);

            var result = Expression.Call(genericMethod.MakeGenericMethod(castType), queryable);

            if (newStructure is EntityReference entityReference
                && entityReference.EntityType.GetAllBaseTypes().Concat(entityReference.EntityType.GetDerivedTypesInclusive())
                    .FirstOrDefault(et => et.ClrType == castType) is IEntityType castEntityType)
            {
                var newEntityReference = new EntityReference(castEntityType);
                if (entityReference.IsOptional)
                {
                    newEntityReference.MarkAsOptional();
                }

                newEntityReference.IncludePaths.Merge(entityReference.IncludePaths);

                // Prune includes for sibling types
                var siblingNavigations = newEntityReference.IncludePaths.Keys
                    .Where(
                        n => !castEntityType.IsAssignableFrom(n.DeclaringEntityType)
                            && !n.DeclaringEntityType.IsAssignableFrom(castEntityType)).ToList();

                foreach (var navigation in siblingNavigations)
                {
                    newEntityReference.IncludePaths.Remove(navigation);
                }

                newStructure = newEntityReference;
            }
            else
            {
                newStructure = Expression.Default(castType);
            }

            var navigationTree = new NavigationTreeExpression(newStructure);
            var parameterName = GetParameterName("e");

            return new NavigationExpansionExpression(result, navigationTree, navigationTree, parameterName);
        }

        private Expression ProcessContains(NavigationExpansionExpression source, Expression item)
        {
            source = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(source);
            var queryable = Reduce(source);

            return Expression.Call(QueryableMethods.Contains.MakeGenericMethod(queryable.Type.TryGetSequenceType()), queryable, item);
        }

        private NavigationExpansionExpression ProcessDefaultIfEmpty(NavigationExpansionExpression source)
        {
            source.UpdateSource(
                Expression.Call(
                    QueryableMethods.DefaultIfEmptyWithoutArgument.MakeGenericMethod(source.SourceElementType),
                    source.Source));

            var pendingSelector = source.PendingSelector;
            _entityReferenceOptionalMarkingExpressionVisitor.Visit(pendingSelector);
            if (!pendingSelector.Type.IsNullableType())
            {
                pendingSelector = Expression.Coalesce(
                    Expression.Convert(pendingSelector, pendingSelector.Type.MakeNullable()),
                    pendingSelector.Type.GetDefaultValueConstant());
            }

            source.ApplySelector(pendingSelector);

            return source;
        }

        private NavigationExpansionExpression ProcessDistinct(NavigationExpansionExpression source, MethodInfo genericMethod)
        {
            source = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(source);
            var newStructure = SnapshotExpression(source.PendingSelector);
            var queryable = Reduce(source);

            var result = Expression.Call(genericMethod.MakeGenericMethod(queryable.Type.TryGetSequenceType()), queryable);

            var navigationTree = new NavigationTreeExpression(newStructure);
            var parameterName = GetParameterName("e");

            return new NavigationExpansionExpression(result, navigationTree, navigationTree, parameterName);
        }

        private NavigationExpansionExpression ProcessSkipTake(
            NavigationExpansionExpression source,
            MethodInfo genericMethod,
            Expression count)
        {
            source.UpdateSource(Expression.Call(genericMethod.MakeGenericMethod(source.SourceElementType), source.Source, count));

            return source;
        }

        private NavigationExpansionExpression ProcessFirstSingleLastOrDefault(
            NavigationExpansionExpression source,
            MethodInfo genericMethod,
            LambdaExpression predicate,
            Type returnType)
        {
            if (predicate != null)
            {
                source = ProcessWhere(source, predicate);
                genericMethod = _predicateLessMethodInfo[genericMethod];
            }

            if (source.PendingSelector.Type != returnType)
            {
                source.ApplySelector(Expression.Convert(source.PendingSelector, returnType));
            }

            source.ConvertToSingleResult(genericMethod);

            return source;
        }

        private NavigationExpansionExpression ProcessGroupBy(
            NavigationExpansionExpression source,
            LambdaExpression keySelector,
            LambdaExpression elementSelector,
            LambdaExpression resultSelector)
        {
            var keySelectorBody = ExpandNavigationsForSource(source, RemapLambdaExpression(source, keySelector));
            // Need to generate lambda after processing element/result selector
            Expression result;
            if (elementSelector != null)
            {
                source = ProcessSelect(source, elementSelector);
            }

            source = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(source);
            // TODO: Flow include in future
            //source = (NavigationExpansionExpression)new IncludeApplyingExpressionVisitor(
            //    this, _queryCompilationContext.IsTracking).Visit(source);
            keySelector = GenerateLambda(keySelectorBody, source.CurrentParameter);
            elementSelector = GenerateLambda(source.PendingSelector, source.CurrentParameter);
            result = resultSelector == null
                ? Expression.Call(
                    QueryableMethods.GroupByWithKeyElementSelector.MakeGenericMethod(
                        source.CurrentParameter.Type, keySelector.ReturnType, elementSelector.ReturnType),
                    source.Source,
                    Expression.Quote(keySelector),
                    Expression.Quote(elementSelector))
                : Expression.Call(
                    QueryableMethods.GroupByWithKeyElementResultSelector.MakeGenericMethod(
                        source.CurrentParameter.Type, keySelector.ReturnType, elementSelector.ReturnType, resultSelector.ReturnType),
                    source.Source,
                    Expression.Quote(keySelector),
                    Expression.Quote(elementSelector),
                    Expression.Quote(Visit(resultSelector)));

            var navigationTree = new NavigationTreeExpression(Expression.Default(result.Type.TryGetSequenceType()));
            var parameterName = GetParameterName("e");

            return new NavigationExpansionExpression(result, navigationTree, navigationTree, parameterName);
        }

        private NavigationExpansionExpression ProcessInclude(NavigationExpansionExpression source, Expression expression, bool thenInclude)
        {
            if (source.PendingSelector is NavigationTreeExpression navigationTree
                && navigationTree.Value is EntityReference entityReference)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (entityReference.EntityType.GetDefiningQuery() != null)
                {
                    throw new InvalidOperationException(
#pragma warning disable CS0612 // Type or member is obsolete
                        CoreStrings.IncludeOnEntityWithDefiningQueryNotSupported(expression, entityReference.EntityType.DisplayName()));
#pragma warning restore CS0612 // Type or member is obsolete
                }
#pragma warning restore CS0618 // Type or member is obsolete

                if (expression is ConstantExpression includeConstant
                    && includeConstant.Value is string navigationChain)
                {
                    var navigationPaths = navigationChain.Split(new[] { "." }, StringSplitOptions.None);
                    var includeTreeNodes = new Queue<IncludeTreeNode>();
                    includeTreeNodes.Enqueue(entityReference.IncludePaths);
                    foreach (var navigationName in navigationPaths)
                    {
                        var nodesToProcess = includeTreeNodes.Count;
                        while (nodesToProcess-- > 0)
                        {
                            var currentNode = includeTreeNodes.Dequeue();
                            foreach (var navigation in FindNavigations(currentNode.EntityType, navigationName))
                            {
                                var addedNode = currentNode.AddNavigation(navigation);

                                // This is to add eager Loaded navigations when owner type is included.
                                PopulateEagerLoadedNavigations(addedNode);
                                includeTreeNodes.Enqueue(addedNode);
                            }
                        }

                        if (includeTreeNodes.Count == 0)
                        {
                            _queryCompilationContext.Logger.InvalidIncludePathError(navigationChain, navigationName);
                        }
                    }
                }
                else
                {
                    var currentIncludeTreeNode = thenInclude
                        ? entityReference.LastIncludeTreeNode
                        : entityReference.IncludePaths;
                    var includeLambda = expression.UnwrapLambdaFromQuote();

                    var (result, filterExpression) = ExtractIncludeFilter(includeLambda.Body, includeLambda.Body);
                    var lastIncludeTree = PopulateIncludeTree(currentIncludeTreeNode, result);
                    if (filterExpression != null)
                    {
                        if (lastIncludeTree.FilterExpression != null
                            && !ExpressionEqualityComparer.Instance.Equals(filterExpression, lastIncludeTree.FilterExpression))
                        {
                            throw new InvalidOperationException(
                                CoreStrings.MultipleFilteredIncludesOnSameNavigation(
                                    FormatFilter(filterExpression.Body).Print(),
                                    FormatFilter(lastIncludeTree.FilterExpression.Body).Print()));
                        }

                        lastIncludeTree.ApplyFilter(filterExpression);
                    }

                    entityReference.SetLastInclude(lastIncludeTree);
                }

                return source;
            }

            throw new InvalidOperationException(CoreStrings.IncludeOnNonEntity(expression.Print()));

            static (Expression result, LambdaExpression filterExpression) ExtractIncludeFilter(
                Expression currentExpression,
                Expression includeExpression)
            {
                if (currentExpression is MemberExpression)
                {
                    return (currentExpression, default);
                }

                if (currentExpression is MethodCallExpression methodCallExpression)
                {
                    if (!methodCallExpression.Method.IsGenericMethod
                        || !_supportedFilteredIncludeOperations.Contains(methodCallExpression.Method.GetGenericMethodDefinition()))
                    {
                        throw new InvalidOperationException(CoreStrings.InvalidIncludeExpression(includeExpression));
                    }

                    var (result, filterExpression) = ExtractIncludeFilter(methodCallExpression.Arguments[0], includeExpression);
                    if (filterExpression == null)
                    {
                        var prm = Expression.Parameter(result.Type);
                        filterExpression = Expression.Lambda(prm, prm);
                    }

                    var arguments = new List<Expression> { filterExpression.Body };
                    arguments.AddRange(methodCallExpression.Arguments.Skip(1));
                    filterExpression = Expression.Lambda(
                        methodCallExpression.Update(methodCallExpression.Object, arguments),
                        filterExpression.Parameters);

                    return (result, filterExpression);
                }

                throw new InvalidOperationException(CoreStrings.InvalidIncludeExpression(includeExpression));
            }

            static Expression FormatFilter(Expression expression)
            {
                if (expression is MethodCallExpression methodCallExpression
                    && methodCallExpression.Method.IsGenericMethod
                    && _supportedFilteredIncludeOperations.Contains(methodCallExpression.Method.GetGenericMethodDefinition()))
                {
                    if (methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.AsQueryable)
                    {
                        return Expression.Parameter(expression.Type, "navigation");
                    }

                    var arguments = new List<Expression>();
                    var source = FormatFilter(methodCallExpression.Arguments[0]);
                    arguments.Add(source);
                    arguments.AddRange(methodCallExpression.Arguments.Skip(1));

                    return methodCallExpression.Update(methodCallExpression.Object, arguments);
                }

                return expression;
            }
        }

        private NavigationExpansionExpression ProcessJoin(
            NavigationExpansionExpression outerSource,
            NavigationExpansionExpression innerSource,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            LambdaExpression resultSelector)
        {
            if (innerSource.PendingOrderings.Any())
            {
                ApplyPendingOrderings(innerSource);
            }

            (outerKeySelector, innerKeySelector) = ProcessJoinConditions(outerSource, innerSource, outerKeySelector, innerKeySelector);

            var transparentIdentifierType = TransparentIdentifierFactory.Create(
                outerSource.SourceElementType, innerSource.SourceElementType);

            var transparentIdentifierOuterMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
            var transparentIdentifierInnerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");

            var newResultSelector = Expression.Lambda(
                Expression.New(
                    transparentIdentifierType.GetConstructors().Single(),
                    new[] { outerSource.CurrentParameter, innerSource.CurrentParameter },
                    transparentIdentifierOuterMemberInfo,
                    transparentIdentifierInnerMemberInfo),
                outerSource.CurrentParameter,
                innerSource.CurrentParameter);

            var source = Expression.Call(
                QueryableMethods.Join.MakeGenericMethod(
                    outerSource.SourceElementType, innerSource.SourceElementType, outerKeySelector.ReturnType,
                    newResultSelector.ReturnType),
                outerSource.Source,
                innerSource.Source,
                Expression.Quote(outerKeySelector),
                Expression.Quote(innerKeySelector),
                Expression.Quote(newResultSelector));

            var currentTree = new NavigationTreeNode(outerSource.CurrentTree, innerSource.CurrentTree);
            var pendingSelector = new ReplacingExpressionVisitor(
                    new Expression[] { resultSelector.Parameters[0], resultSelector.Parameters[1] },
                    new[] { outerSource.PendingSelector, innerSource.PendingSelector })
                .Visit(resultSelector.Body);
            var parameterName = GetParameterName("ti");

            return new NavigationExpansionExpression(source, currentTree, pendingSelector, parameterName);
        }

        private NavigationExpansionExpression ProcessLeftJoin(
            NavigationExpansionExpression outerSource,
            NavigationExpansionExpression innerSource,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            LambdaExpression resultSelector)
        {
            if (innerSource.PendingOrderings.Any())
            {
                ApplyPendingOrderings(innerSource);
            }

            (outerKeySelector, innerKeySelector) = ProcessJoinConditions(outerSource, innerSource, outerKeySelector, innerKeySelector);

            var transparentIdentifierType = TransparentIdentifierFactory.Create(
                outerSource.SourceElementType, innerSource.SourceElementType);

            var transparentIdentifierOuterMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
            var transparentIdentifierInnerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");

            var newResultSelector = Expression.Lambda(
                Expression.New(
                    transparentIdentifierType.GetConstructors().Single(),
                    new[] { outerSource.CurrentParameter, innerSource.CurrentParameter },
                    transparentIdentifierOuterMemberInfo,
                    transparentIdentifierInnerMemberInfo),
                outerSource.CurrentParameter,
                innerSource.CurrentParameter);

            var source = Expression.Call(
                QueryableExtensions.LeftJoinMethodInfo.MakeGenericMethod(
                    outerSource.SourceElementType, innerSource.SourceElementType, outerKeySelector.ReturnType,
                    newResultSelector.ReturnType),
                outerSource.Source,
                innerSource.Source,
                Expression.Quote(outerKeySelector),
                Expression.Quote(innerKeySelector),
                Expression.Quote(newResultSelector));

            var innerPendingSelector = innerSource.PendingSelector;
            innerPendingSelector = _entityReferenceOptionalMarkingExpressionVisitor.Visit(innerPendingSelector);

            var currentTree = new NavigationTreeNode(outerSource.CurrentTree, innerSource.CurrentTree);
            var pendingSelector = new ReplacingExpressionVisitor(
                    new Expression[] { resultSelector.Parameters[0], resultSelector.Parameters[1] },
                    new[] { outerSource.PendingSelector, innerPendingSelector })
                .Visit(resultSelector.Body);
            var parameterName = GetParameterName("ti");

            return new NavigationExpansionExpression(source, currentTree, pendingSelector, parameterName);
        }

        private NavigationExpansionExpression ProcessOrderByThenBy(
            NavigationExpansionExpression source,
            MethodInfo genericMethod,
            LambdaExpression keySelector,
            bool thenBy)
        {
            var lambdaBody = ReplacingExpressionVisitor.Replace(
                keySelector.Parameters[0],
                source.PendingSelector,
                keySelector.Body);

            lambdaBody = new ExpandingExpressionVisitor(this, source).Visit(lambdaBody);
            lambdaBody = _subqueryMemberPushdownExpressionVisitor.Visit(lambdaBody);

            if (thenBy)
            {
                source.AppendPendingOrdering(genericMethod, lambdaBody);
            }
            else
            {
                source.AddPendingOrdering(genericMethod, lambdaBody);
            }

            return source;
        }

        private Expression ProcessReverse(NavigationExpansionExpression source)
        {
            source.UpdateSource(
                Expression.Call(
                    QueryableMethods.Reverse.MakeGenericMethod(source.SourceElementType),
                    source.Source));

            return source;
        }

        private NavigationExpansionExpression ProcessSelect(NavigationExpansionExpression source, LambdaExpression selector)
        {
            // This is to apply aggregate operator on GroupBy right away rather than deferring
            if (source.SourceElementType.IsGenericType
                && source.SourceElementType.GetGenericTypeDefinition() == typeof(IGrouping<,>)
                && !(selector.ReturnType.IsGenericType
                    && selector.ReturnType.GetGenericTypeDefinition() == typeof(IGrouping<,>)))
            {
                var selectorLambda = ProcessLambdaExpression(source, selector);
                var newSource = Expression.Call(
                    QueryableMethods.Select.MakeGenericMethod(source.SourceElementType, selectorLambda.ReturnType),
                    source.Source,
                    Expression.Quote(selectorLambda));

                var navigationTree = new NavigationTreeExpression(Expression.Default(selectorLambda.ReturnType));
                var parameterName = GetParameterName("e");

                return new NavigationExpansionExpression(newSource, navigationTree, navigationTree, parameterName);
            }

            var selectorBody = ReplacingExpressionVisitor.Replace(
                selector.Parameters[0],
                source.PendingSelector,
                selector.Body);

            source.ApplySelector(selectorBody);

            return source;
        }

        private NavigationExpansionExpression ProcessSelectMany(
            NavigationExpansionExpression source,
            LambdaExpression collectionSelector,
            LambdaExpression resultSelector)
        {
            var collectionSelectorBody = ExpandNavigationsForSource(source, RemapLambdaExpression(source, collectionSelector));
            if (collectionSelectorBody is MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression)
            {
                collectionSelectorBody = materializeCollectionNavigationExpression.Subquery;
            }

            if (collectionSelectorBody is NavigationExpansionExpression collectionSource)
            {
                collectionSource = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(collectionSource);
                var innerTree = new NavigationTreeExpression(SnapshotExpression(collectionSource.PendingSelector));
                collectionSelector = GenerateLambda(collectionSource, source.CurrentParameter);
                var collectionElementType = collectionSelector.ReturnType.TryGetSequenceType();

                // Collection selector body is IQueryable, we need to adjust the type to IEnumerable, to match the SelectMany signature
                // therefore the delegate type is specified explicitly
                var collectionSelectorLambdaType = typeof(Func<,>).MakeGenericType(
                    source.SourceElementType,
                    typeof(IEnumerable<>).MakeGenericType(collectionElementType));

                collectionSelector = Expression.Lambda(
                    collectionSelectorLambdaType,
                    collectionSelector.Body,
                    collectionSelector.Parameters[0]);

                var transparentIdentifierType = TransparentIdentifierFactory.Create(
                    source.SourceElementType, collectionElementType);
                var transparentIdentifierOuterMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
                var transparentIdentifierInnerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");
                var collectionElementParameter = Expression.Parameter(collectionElementType, "c");

                var newResultSelector = Expression.Lambda(
                    Expression.New(
                        transparentIdentifierType.GetConstructors().Single(),
                        new[] { source.CurrentParameter, collectionElementParameter }, transparentIdentifierOuterMemberInfo,
                        transparentIdentifierInnerMemberInfo),
                    source.CurrentParameter,
                    collectionElementParameter);

                var newSource = Expression.Call(
                    QueryableMethods.SelectManyWithCollectionSelector.MakeGenericMethod(
                        source.SourceElementType, collectionElementType, newResultSelector.ReturnType),
                    source.Source,
                    Expression.Quote(collectionSelector),
                    Expression.Quote(newResultSelector));

                var currentTree = new NavigationTreeNode(source.CurrentTree, innerTree);
                var pendingSelector = resultSelector == null
                    ? innerTree
                    : new ReplacingExpressionVisitor(
                            new Expression[] { resultSelector.Parameters[0], resultSelector.Parameters[1] },
                            new[] { source.PendingSelector, innerTree })
                        .Visit(resultSelector.Body);
                var parameterName = GetParameterName("ti");

                return new NavigationExpansionExpression(newSource, currentTree, pendingSelector, parameterName);
            }

            // TODO: Improve this exception message
            throw new InvalidOperationException(CoreStrings.TranslationFailed(collectionSelector.Print()));
        }

        private NavigationExpansionExpression ProcessSetOperation(
            NavigationExpansionExpression outerSource,
            MethodInfo genericMethod,
            NavigationExpansionExpression innerSource)
        {
            outerSource = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(outerSource);
            var outerTreeStructure = SnapshotExpression(outerSource.PendingSelector);

            innerSource = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(innerSource);
            var innerTreeStructure = SnapshotExpression(innerSource.PendingSelector);

            if (!CompareIncludes(outerTreeStructure, innerTreeStructure))
            {
                throw new InvalidOperationException(CoreStrings.SetOperationWithDifferentIncludesInOperands);
            }

            var outerQueryable = Reduce(outerSource);
            var innerQueryable = Reduce(innerSource);

            var outerType = outerQueryable.Type.TryGetSequenceType();
            var innerType = innerQueryable.Type.TryGetSequenceType();

            var result = Expression.Call(
                genericMethod.MakeGenericMethod(outerType.IsAssignableFrom(innerType) ? outerType : innerType),
                outerQueryable,
                innerQueryable);
            var navigationTree = new NavigationTreeExpression(
                outerType.IsAssignableFrom(innerType) ? outerTreeStructure : innerTreeStructure);
            var parameterName = GetParameterName("e");

            return new NavigationExpansionExpression(result, navigationTree, navigationTree, parameterName);
        }

        private Expression ProcessUnknownMethod(MethodCallExpression methodCallExpression)
        {
            var queryableElementType = methodCallExpression.Type.TryGetElementType(typeof(IQueryable<>));
            if (queryableElementType != null
                && methodCallExpression.Object == null
                && methodCallExpression.Arguments.All(a => a.GetLambdaOrNull() == null)
                && methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericArguments().Length == 1
                && methodCallExpression.Method.GetGenericArguments()[0] == queryableElementType
                && methodCallExpression.Arguments.Count > 0
                && methodCallExpression.Arguments.Skip(1).All(e => e.Type.TryGetElementType(typeof(IQueryable<>)) == null))
            {
                var firstArgumet = Visit(methodCallExpression.Arguments[0]);
                if (firstArgumet is NavigationExpansionExpression source
                    && source.Type == methodCallExpression.Type)
                {
                    source = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(source);
                    var newStructure = SnapshotExpression(source.PendingSelector);
                    var queryable = Reduce(source);

                    var result = Expression.Call(
                        methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(queryableElementType),
                        new[] { queryable }.Concat(methodCallExpression.Arguments.Skip(1).Select(e => Visit(e))));

                    var navigationTree = new NavigationTreeExpression(newStructure);
                    var parameterName = GetParameterName("e");

                    return new NavigationExpansionExpression(result, navigationTree, navigationTree, parameterName);
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private NavigationExpansionExpression ProcessWhere(NavigationExpansionExpression source, LambdaExpression predicate)
        {
            predicate = ProcessLambdaExpression(source, predicate);

            source.UpdateSource(
                Expression.Call(
                    QueryableMethods.Where.MakeGenericMethod(source.SourceElementType),
                    source.Source,
                    Expression.Quote(predicate)));

            return source;
        }

        private void ApplyPendingOrderings(NavigationExpansionExpression source)
        {
            if (source.PendingOrderings.Any())
            {
                foreach (var (orderingMethod, keySelector) in source.PendingOrderings)
                {
                    var lambdaBody = Visit(keySelector);
                    lambdaBody = _pendingSelectorExpandingExpressionVisitor.Visit(lambdaBody);

                    if (lambdaBody is NavigationTreeExpression navigationTreeExpression
                        && navigationTreeExpression.Value is EntityReference entityReference)
                    {
                        var primaryKeyProperties = entityReference.EntityType.FindPrimaryKey()?.Properties;
                        if (primaryKeyProperties != null)
                        {
                            for (var i = 0; i < primaryKeyProperties.Count; i++)
                            {
                                var genericMethod = i > 0
                                    ? GetThenByMethod(orderingMethod)
                                    : orderingMethod;

                                var keyPropertyLambda = GenerateLambda(
                                    navigationTreeExpression.CreateEFPropertyExpression(
                                        primaryKeyProperties[i], entityReference.IsOptional),
                                    source.CurrentParameter);

                                source.UpdateSource(
                                    Expression.Call(
                                        genericMethod.MakeGenericMethod(source.SourceElementType, keyPropertyLambda.ReturnType),
                                        source.Source,
                                        keyPropertyLambda));
                            }

                            continue;
                        }
                    }

                    if (lambdaBody is NavigationExpansionExpression navigationExpansionExpression
                        && navigationExpansionExpression.CardinalityReducingGenericMethodInfo != null
                        && navigationExpansionExpression.PendingSelector is NavigationTreeExpression subqueryNavigationTreeExpression
                        && subqueryNavigationTreeExpression.Value is EntityReference subqueryEntityReference)
                    {
                        var primaryKeyProperties = subqueryEntityReference.EntityType.FindPrimaryKey()?.Properties;
                        if (primaryKeyProperties != null)
                        {
                            for (var i = 0; i < primaryKeyProperties.Count; i++)
                            {
                                var genericMethod = i > 0
                                    ? GetThenByMethod(orderingMethod)
                                    : orderingMethod;

                                var keyPropertyLambda = GenerateLambda(
                                    navigationExpansionExpression.CreateEFPropertyExpression(
                                        primaryKeyProperties[i], subqueryEntityReference.IsOptional),
                                    source.CurrentParameter);

                                source.UpdateSource(
                                    Expression.Call(
                                        genericMethod.MakeGenericMethod(source.SourceElementType, keyPropertyLambda.ReturnType),
                                        source.Source,
                                        keyPropertyLambda));
                            }

                            continue;
                        }
                    }

                    var keySelectorLambda = GenerateLambda(lambdaBody, source.CurrentParameter);

                    source.UpdateSource(
                        Expression.Call(
                            orderingMethod.MakeGenericMethod(source.SourceElementType, keySelectorLambda.ReturnType),
                            source.Source,
                            keySelectorLambda));
                }

                source.ClearPendingOrderings();
            }

            static MethodInfo GetThenByMethod(MethodInfo currentGenericMethod)
                => currentGenericMethod == QueryableMethods.OrderBy
                    ? QueryableMethods.ThenBy
                    : currentGenericMethod == QueryableMethods.OrderByDescending
                        ? QueryableMethods.ThenByDescending
                        : currentGenericMethod;
        }

        private (LambdaExpression, LambdaExpression) ProcessJoinConditions(
            NavigationExpansionExpression outerSource,
            NavigationExpansionExpression innerSource,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector)
        {
            var outerKeyLambda = RemapLambdaExpression(outerSource, outerKeySelector);
            var innerKeyLambda = RemapLambdaExpression(innerSource, innerKeySelector);

            var keyComparison = (BinaryExpression)_removeRedundantNavigationComparisonExpressionVisitor
                .Visit(Expression.Equal(outerKeyLambda, innerKeyLambda));

            outerKeySelector = GenerateLambda(ExpandNavigationsForSource(outerSource, keyComparison.Left), outerSource.CurrentParameter);
            innerKeySelector = GenerateLambda(ExpandNavigationsForSource(innerSource, keyComparison.Right), innerSource.CurrentParameter);

            if (outerKeySelector.ReturnType != innerKeySelector.ReturnType)
            {
                var baseType = outerKeySelector.ReturnType.IsAssignableFrom(innerKeySelector.ReturnType)
                    ? outerKeySelector.ReturnType
                    : innerKeySelector.ReturnType;

                outerKeySelector = ChangeReturnType(outerKeySelector, baseType);
                innerKeySelector = ChangeReturnType(innerKeySelector, baseType);
            }

            return (outerKeySelector, innerKeySelector);

            static LambdaExpression ChangeReturnType(LambdaExpression lambdaExpression, Type type)
            {
                var delegateType = typeof(Func<,>).MakeGenericType(lambdaExpression.Parameters[0].Type, type);
                return Expression.Lambda(delegateType, lambdaExpression.Body, lambdaExpression.Parameters);
            }
        }

        private Expression ApplyQueryFilter(IEntityType entityType, NavigationExpansionExpression navigationExpansionExpression)
        {
            if (!_queryCompilationContext.IgnoreQueryFilters)
            {
                var sequenceType = navigationExpansionExpression.Type.GetSequenceType();
                var rootEntityType = entityType.GetRootType();
                var queryFilter = rootEntityType.GetQueryFilter();
                if (queryFilter != null)
                {
                    if (!_parameterizedQueryFilterPredicateCache.TryGetValue(rootEntityType, out var filterPredicate))
                    {
                        filterPredicate = queryFilter;
                        filterPredicate = (LambdaExpression)_parameterExtractingExpressionVisitor.ExtractParameters(filterPredicate);
                        filterPredicate = (LambdaExpression)_queryTranslationPreprocessor.NormalizeQueryableMethod(filterPredicate);

                        // We need to do entity equality, but that requires a full method call on a query root to properly flow the
                        // entity information through. Construct a MethodCall wrapper for the predicate with the proper query root.
                        var filterWrapper = Expression.Call(
                            QueryableMethods.Where.MakeGenericMethod(rootEntityType.ClrType),
                            new QueryRootExpression(rootEntityType),
                            filterPredicate);
                        filterPredicate = filterWrapper.Arguments[1].UnwrapLambdaFromQuote();

                        _parameterizedQueryFilterPredicateCache[rootEntityType] = filterPredicate;
                    }

                    filterPredicate =
                        (LambdaExpression)new SelfReferenceEntityQueryableRewritingExpressionVisitor(this, entityType).Visit(
                            filterPredicate);

                    // if we are constructing EntityQueryable of a derived type, we need to re-map filter predicate to the correct derived type
                    var filterPredicateParameter = filterPredicate.Parameters[0];
                    if (filterPredicateParameter.Type != sequenceType)
                    {
                        var newFilterPredicateParameter = Expression.Parameter(sequenceType, filterPredicateParameter.Name);
                        var newFilterPredicateBody = ReplacingExpressionVisitor.Replace(
                            filterPredicateParameter, newFilterPredicateParameter, filterPredicate.Body);
                        filterPredicate = Expression.Lambda(newFilterPredicateBody, newFilterPredicateParameter);
                    }

                    var filteredResult = Expression.Call(
                        QueryableMethods.Where.MakeGenericMethod(sequenceType),
                        navigationExpansionExpression,
                        filterPredicate);

                    return Visit(filteredResult);
                }
            }

            return navigationExpansionExpression;
        }

        private bool CompareIncludes(Expression outer, Expression inner)
        {
            if (outer is EntityReference outerEntityReference
                && inner is EntityReference innerEntityReference)
            {
                return outerEntityReference.IncludePaths.Equals(innerEntityReference.IncludePaths);
            }

            if (outer is NewExpression outerNewExpression
                && inner is NewExpression innerNewExpression)
            {
                if (outerNewExpression.Arguments.Count != innerNewExpression.Arguments.Count)
                {
                    return false;
                }

                for (var i = 0; i < outerNewExpression.Arguments.Count; i++)
                {
                    if (!CompareIncludes(outerNewExpression.Arguments[i], innerNewExpression.Arguments[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return outer is DefaultExpression outerDefaultExpression
                && inner is DefaultExpression innerDefaultExpression
                && outerDefaultExpression.Type == innerDefaultExpression.Type;
        }

        private MethodCallExpression ConvertToEnumerable(MethodInfo queryableMethod, IEnumerable<Expression> arguments)
        {
            var genericTypeArguments = queryableMethod.IsGenericMethod
                ? queryableMethod.GetGenericArguments()
                : null;
            var enumerableArguments = arguments.Select(
                    arg => arg is UnaryExpression unaryExpression
                        && unaryExpression.NodeType == ExpressionType.Quote
                        && unaryExpression.Operand is LambdaExpression
                            ? unaryExpression.Operand
                            : arg)
                .ToList();

            if (queryableMethod.Name == nameof(Enumerable.Min))
            {
                if (genericTypeArguments.Length == 1)
                {
                    var resultType = genericTypeArguments[0];
                    var enumerableMethod = EnumerableMethods.GetMinWithoutSelector(resultType);

                    if (!IsNumericType(resultType))
                    {
                        enumerableMethod = enumerableMethod.MakeGenericMethod(resultType);
                    }

                    return Expression.Call(enumerableMethod, enumerableArguments);
                }

                if (genericTypeArguments.Length == 2)
                {
                    var resultType = genericTypeArguments[1];
                    var enumerableMethod = EnumerableMethods.GetMinWithSelector(resultType);

                    enumerableMethod = IsNumericType(resultType)
                        ? enumerableMethod.MakeGenericMethod(genericTypeArguments[0])
                        : enumerableMethod.MakeGenericMethod(genericTypeArguments);

                    return Expression.Call(enumerableMethod, enumerableArguments);
                }
            }

            if (queryableMethod.Name == nameof(Enumerable.Max))
            {
                if (genericTypeArguments.Length == 1)
                {
                    var resultType = genericTypeArguments[0];
                    var enumerableMethod = EnumerableMethods.GetMaxWithoutSelector(resultType);

                    if (!IsNumericType(resultType))
                    {
                        enumerableMethod = enumerableMethod.MakeGenericMethod(resultType);
                    }

                    return Expression.Call(enumerableMethod, enumerableArguments);
                }

                if (genericTypeArguments.Length == 2)
                {
                    var resultType = genericTypeArguments[1];
                    var enumerableMethod = EnumerableMethods.GetMaxWithSelector(resultType);

                    enumerableMethod = IsNumericType(resultType)
                        ? enumerableMethod.MakeGenericMethod(genericTypeArguments[0])
                        : enumerableMethod.MakeGenericMethod(genericTypeArguments);

                    return Expression.Call(enumerableMethod, enumerableArguments);
                }
            }

            foreach (var method in typeof(Enumerable).GetTypeInfo().GetDeclaredMethods(queryableMethod.Name))
            {
                var enumerableMethod = method;
                if (enumerableMethod.IsGenericMethod)
                {
                    if (genericTypeArguments != null
                        && enumerableMethod.GetGenericArguments().Length == genericTypeArguments.Length)
                    {
                        enumerableMethod = enumerableMethod.MakeGenericMethod(genericTypeArguments);
                    }
                    else
                    {
                        continue;
                    }
                }

                var enumerableMethodParameters = enumerableMethod.GetParameters();
                if (enumerableMethodParameters.Length != enumerableArguments.Count)
                {
                    continue;
                }

                var validMapping = true;
                for (var i = 0; i < enumerableMethodParameters.Length; i++)
                {
                    if (!enumerableMethodParameters[i].ParameterType.IsAssignableFrom(enumerableArguments[i].Type))
                    {
                        validMapping = false;
                        break;
                    }
                }

                if (validMapping)
                {
                    return Expression.Call(enumerableMethod, enumerableArguments);
                }
            }

            throw new InvalidOperationException(CoreStrings.CannotConvertQueryableToEnumerableMethod);

            static bool IsNumericType(Type type)
            {
                type = type.UnwrapNullableType();

                return type == typeof(int)
                    || type == typeof(long)
                    || type == typeof(float)
                    || type == typeof(double)
                    || type == typeof(decimal);
            }
        }

        private NavigationExpansionExpression CreateNavigationExpansionExpression(
            [NotNull] Expression sourceExpression,
            [NotNull] IEntityType entityType)
        {
            var entityReference = new EntityReference(entityType);
            PopulateEagerLoadedNavigations(entityReference.IncludePaths);

            var currentTree = new NavigationTreeExpression(entityReference);
            var parameterName = GetParameterName(entityType.ShortName()[0].ToString().ToLower());

            return new NavigationExpansionExpression(sourceExpression, currentTree, currentTree, parameterName);
        }

        private NavigationExpansionExpression CreateNavigationExpansionExpression(
            Expression sourceExpression,
            OwnedNavigationReference ownedNavigationReference)
        {
            var parameterName = GetParameterName("o");
            var entityReference = ownedNavigationReference.EntityReference;
            var currentTree = new NavigationTreeExpression(entityReference);

            return new NavigationExpansionExpression(sourceExpression, currentTree, currentTree, parameterName);
        }

        private Expression ExpandNavigationsForSource(NavigationExpansionExpression source, Expression expression)
        {
            expression = _removeRedundantNavigationComparisonExpressionVisitor.Visit(expression);
            expression = new ExpandingExpressionVisitor(this, source).Visit(expression);
            expression = _subqueryMemberPushdownExpressionVisitor.Visit(expression);
            expression = Visit(expression);
            expression = _pendingSelectorExpandingExpressionVisitor.Visit(expression);

            return expression;
        }

        private Expression RemapLambdaExpression(NavigationExpansionExpression source, LambdaExpression lambdaExpression)
            => ReplacingExpressionVisitor.Replace(lambdaExpression.Parameters[0], source.PendingSelector, lambdaExpression.Body);

        private LambdaExpression ProcessLambdaExpression(NavigationExpansionExpression source, LambdaExpression lambdaExpression)
            => GenerateLambda(ExpandNavigationsForSource(source, RemapLambdaExpression(source, lambdaExpression)), source.CurrentParameter);

        private static IEnumerable<INavigationBase> FindNavigations(IEntityType entityType, string navigationName)
        {
            var navigation = entityType.FindNavigation(navigationName);
            if (navigation != null)
            {
                yield return navigation;
            }
            else
            {
                foreach (var derivedNavigation in entityType.GetDerivedTypes()
                    .Select(et => et.FindDeclaredNavigation(navigationName)).Where(n => n != null))
                {
                    yield return derivedNavigation;
                }
            }

            var skipNavigation = entityType.FindSkipNavigation(navigationName);
            if (skipNavigation != null)
            {
                yield return skipNavigation;
            }
            else
            {
                foreach (var derivedSkipNavigation in entityType.GetDerivedTypes()
                    .Select(et => et.FindDeclaredSkipNavigation(navigationName)).Where(n => n != null))
                {
                    yield return derivedSkipNavigation;
                }
            }
        }

        private LambdaExpression GenerateLambda(Expression body, ParameterExpression currentParameter)
            => Expression.Lambda(Reduce(body), currentParameter);

        private Expression UnwrapCollectionMaterialization(Expression expression)
        {
            while (expression is MethodCallExpression innerMethodCall
                && innerMethodCall.Method.IsGenericMethod
                && innerMethodCall.Method.GetGenericMethodDefinition() is MethodInfo innerMethod
                && (innerMethod == EnumerableMethods.AsEnumerable
                    || innerMethod == EnumerableMethods.ToList
                    || innerMethod == EnumerableMethods.ToArray))
            {
                expression = innerMethodCall.Arguments[0];
            }

            if (expression is MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression)
            {
                expression = materializeCollectionNavigationExpression.Subquery;
            }

            return expression is OwnedNavigationReference ownedNavigationReference
                && ownedNavigationReference.Navigation.IsCollection
                    ? CreateNavigationExpansionExpression(
                        Expression.Call(
                            QueryableMethods.AsQueryable.MakeGenericMethod(ownedNavigationReference.Type.TryGetSequenceType()),
                            ownedNavigationReference),
                        ownedNavigationReference)
                    : expression;
        }

        private string GetParameterName(string prefix)
        {
            var uniqueName = prefix;
            var index = 0;
            while (_parameterNames.Contains(uniqueName))
            {
                uniqueName = $"{prefix}{index++}";
            }

            _parameterNames.Add(uniqueName);
            return uniqueName;
        }

        private void PopulateEagerLoadedNavigations(IncludeTreeNode includeTreeNode)
        {
            var entityType = includeTreeNode.EntityType;

            if (!_queryCompilationContext.IgnoreAutoIncludes
                && !_nonCyclicAutoIncludeEntityTypes.Contains(entityType))
            {
                VerifyNoAutoIncludeCycles(entityType, new HashSet<IEntityType>(), new List<INavigationBase>());
            }

            var outboundNavigations = GetOutgoingEagerLoadedNavigations(entityType);

            if (_queryCompilationContext.IgnoreAutoIncludes)
            {
                outboundNavigations = outboundNavigations.Where(n => n is INavigation navigation && navigation.ForeignKey.IsOwnership);
            }

            foreach (var navigation in outboundNavigations)
            {
                includeTreeNode.AddNavigation(navigation);
            }
        }

        private void VerifyNoAutoIncludeCycles(
            IEntityType entityType, HashSet<IEntityType> visitedEntityTypes, List<INavigationBase> navigationChain)
        {
            if (_nonCyclicAutoIncludeEntityTypes.Contains(entityType))
            {
                return;
            }

            if (!visitedEntityTypes.Add(entityType))
            {
                throw new InvalidOperationException(CoreStrings.AutoIncludeNavigationCycle(
                    navigationChain.Select(e => $"'{e.DeclaringEntityType.ShortName()}.{e.Name}'").Join()));
            }

            var autoIncludedNavigations = GetOutgoingEagerLoadedNavigations(entityType)
                .Where(n => !(n is INavigation navigation && navigation.ForeignKey.IsOwnership));

            foreach (var navigationBase in autoIncludedNavigations)
            {
                if (navigationChain.Count > 0
                    && navigationChain[^1].Inverse == navigationBase
                    && navigationBase is INavigation)
                {
                    continue;
                }

                navigationChain.Add(navigationBase);
                VerifyNoAutoIncludeCycles(navigationBase.TargetEntityType, visitedEntityTypes, navigationChain);
                navigationChain.Remove(navigationBase);
            }

            _nonCyclicAutoIncludeEntityTypes.Add(entityType);
            visitedEntityTypes.Remove(entityType);
        }

        private static IEnumerable<INavigationBase> GetOutgoingEagerLoadedNavigations(IEntityType entityType)
            => entityType.GetNavigations()
                .Cast<INavigationBase>()
                .Concat(entityType.GetSkipNavigations())
                .Concat(entityType.GetDerivedNavigations())
                .Concat(entityType.GetDerivedSkipNavigations())
                .Where(n => n.IsEagerLoaded);

        private IncludeTreeNode PopulateIncludeTree(IncludeTreeNode includeTreeNode, Expression expression)
        {
            switch (expression)
            {
                case ParameterExpression _:
                    return includeTreeNode;

                case MemberExpression memberExpression:
                    var innerExpression = memberExpression.Expression.UnwrapTypeConversion(out var convertedType);
                    var innerIncludeTreeNode = PopulateIncludeTree(includeTreeNode, innerExpression);
                    var entityType = innerIncludeTreeNode.EntityType;
                    if (convertedType != null)
                    {
                        entityType = entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                            .FirstOrDefault(et => et.ClrType == convertedType);
                        if (entityType == null)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.InvalidTypeConversationWithInclude(expression, convertedType.ShortDisplayName()));
                        }
                    }

                    var navigation = entityType.FindNavigation(memberExpression.Member);
                    if (navigation != null)
                    {
                        var addedNode = innerIncludeTreeNode.AddNavigation(navigation);

                        // This is to add eager Loaded navigations when owner type is included.
                        PopulateEagerLoadedNavigations(addedNode);

                        return addedNode;
                    }

                    var skipNavigation = entityType.FindSkipNavigation(memberExpression.Member);
                    if (skipNavigation != null)
                    {
                        var addedNode = innerIncludeTreeNode.AddNavigation(skipNavigation);

                        // This is to add eager Loaded navigations when owner type is included.
                        PopulateEagerLoadedNavigations(addedNode);

                        return addedNode;
                    }

                    break;
            }

            throw new InvalidOperationException(CoreStrings.InvalidIncludeExpression(expression));
        }

        private Expression Reduce(Expression source)
            => _reducingExpressionVisitor.Visit(source);

        private Expression SnapshotExpression(Expression selector)
        {
            switch (selector)
            {
                case EntityReference entityReference:
                    return entityReference.Snapshot();

                case NavigationTreeExpression navigationTreeExpression:
                    return SnapshotExpression(navigationTreeExpression.Value);

                case NewExpression newExpression:
                {
                    var allDefault = true;
                    var arguments = new Expression[newExpression.Arguments.Count];
                    for (var i = 0; i < newExpression.Arguments.Count; i++)
                    {
                        arguments[i] = SnapshotExpression(newExpression.Arguments[i]);
                        allDefault &= arguments[i].NodeType == ExpressionType.Default;
                    }

                    return allDefault
                        ? Expression.Default(newExpression.Type)
                        : (Expression)newExpression.Update(arguments);
                }

                case OwnedNavigationReference ownedNavigationReference:
                    return ownedNavigationReference.EntityReference.Snapshot();

                default:
                    return Expression.Default(selector.Type);
            }
        }

        private static EntityReference UnwrapEntityReference(Expression expression)
        {
            switch (expression)
            {
                case EntityReference entityReference:
                    return entityReference;

                case NavigationTreeExpression navigationTreeExpression:
                    return UnwrapEntityReference(navigationTreeExpression.Value);

                case NavigationExpansionExpression navigationExpansionExpression
                    when navigationExpansionExpression.CardinalityReducingGenericMethodInfo != null:
                    return UnwrapEntityReference(navigationExpansionExpression.PendingSelector);

                case OwnedNavigationReference ownedNavigationReference:
                    return ownedNavigationReference.EntityReference;

                default:
                    return null;
            }
        }

        private sealed class Parameters : IParameterValues
        {
            private readonly IDictionary<string, object> _parameterValues = new Dictionary<string, object>();

            public IReadOnlyDictionary<string, object> ParameterValues
                => (IReadOnlyDictionary<string, object>)_parameterValues;

            public void AddParameter(string name, object value)
            {
                _parameterValues.Add(name, value);
            }
        }
    }
}
