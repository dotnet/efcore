// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalProjectionBindingExpressionVisitor : ExpressionVisitor
{
    private static readonly MethodInfo GetParameterValueMethodInfo
        = typeof(RelationalProjectionBindingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(GetParameterValue))!;

    private readonly RelationalQueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
    private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
    private readonly IncludeFindingExpressionVisitor _includeFindingExpressionVisitor;

    private SelectExpression _selectExpression;

    private bool _indexBasedBinding;
    private bool _rootIsTransparentIdentifier;
    private Dictionary<StructuralTypeProjectionExpression, ProjectionBindingExpression>? _projectionBindingCache;
    private List<Expression>? _clientProjections;

    private readonly Dictionary<ProjectionMember, Expression> _projectionMapping = new();
    private readonly Stack<ProjectionMember> _projectionMembers = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalProjectionBindingExpressionVisitor(
        RelationalQueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
        RelationalSqlTranslatingExpressionVisitor sqlTranslatingExpressionVisitor)
    {
        _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
        _sqlTranslator = sqlTranslatingExpressionVisitor;
        _includeFindingExpressionVisitor = new IncludeFindingExpressionVisitor();
        _selectExpression = null!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Translate(SelectExpression selectExpression, Expression expression)
    {
        _selectExpression = selectExpression;
        _indexBasedBinding = false;

        // #30915: a projection whose root is a TransparentIdentifier New is an intermediate join-result projection that EF Core
        // will compose further (e.g. a subsequent Where/Join member-accesses its parts). The recorded non-entity object can appear
        // nested inside it; gating it there would replace the bare New that downstream member-folding relies on, breaking
        // translation. The marker only needs to gate the *final* user projection (which is never a TransparentIdentifier), so
        // suppress gating whenever this projection root is a TransparentIdentifier.
        _rootIsTransparentIdentifier = IsTransparentIdentifierProjection(expression);

        _projectionMembers.Push(new ProjectionMember());

        var result = Visit(expression);

        if (result == QueryCompilationContext.NotTranslatedExpression)
        {
            _indexBasedBinding = true;
            _projectionBindingCache = new Dictionary<StructuralTypeProjectionExpression, ProjectionBindingExpression>();
            _projectionMapping.Clear();
            _clientProjections = [];

            result = Visit(expression);

            _selectExpression.ReplaceProjection(_clientProjections);
            _clientProjections.Clear();
        }
        else
        {
            _selectExpression.ReplaceProjection(_projectionMapping);
            _projectionMapping.Clear();
        }

        _selectExpression = null!;
        _projectionMembers.Clear();

        result = MatchTypes(result, expression.Type);

        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull(nameof(expression))]
    public override Expression? Visit(Expression? expression)
    {
        switch (expression)
        {
            case NewExpression or MemberInitExpression or StructuralTypeShaperExpression or IncludeExpression:
                return base.Visit(expression);

            case null:
                return null;

            case not null when _indexBasedBinding:
            {
                switch (expression)
                {
                    case ConstantExpression:
                        return expression;

                    case QueryParameterExpression queryParameterExpression:
                        return Expression.Call(
                            GetParameterValueMethodInfo.MakeGenericMethod(queryParameterExpression.Type),
                            QueryCompilationContext.QueryContextParameter,
                            Expression.Constant(queryParameterExpression.Name));

                    case ParameterExpression parameterExpression:
                        throw new InvalidOperationException(CoreStrings.TranslationFailed(parameterExpression.Print()));

                    case ProjectionBindingExpression projectionBindingExpression:
                        return _selectExpression.GetProjection(projectionBindingExpression) switch
                        {
                            StructuralTypeProjectionExpression projection => AddClientProjection(projection, typeof(ValueBuffer)),
                            SqlExpression mappedSqlExpression => AddClientProjection(mappedSqlExpression, expression.Type.MakeNullable()),
                            _ => throw new InvalidOperationException(CoreStrings.TranslationFailed(projectionBindingExpression.Print()))
                        };

                    case MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression:
                        if (materializeCollectionNavigationExpression.Navigation.TargetEntityType.IsMappedToJson())
                        {
                            var subquery = materializeCollectionNavigationExpression.Subquery;
                            if (subquery is MethodCallExpression { Method.IsGenericMethod: true } methodCallSubquery)
                            {
                                // strip .Select(x => x) and .AsQueryable() from the JsonCollectionResultExpression
                                if (methodCallSubquery.Method.GetGenericMethodDefinition() == QueryableMethods.Select
                                    && methodCallSubquery.Arguments[0] is MethodCallExpression selectSourceMethod)
                                {
                                    methodCallSubquery = selectSourceMethod;
                                }

                                if (methodCallSubquery.Method.IsGenericMethod
                                    && methodCallSubquery.Method.GetGenericMethodDefinition() == QueryableMethods.AsQueryable)
                                {
                                    subquery = methodCallSubquery.Arguments[0];
                                }
                            }

                            if (subquery is JsonQueryExpression jsonQueryExpression)
                            {
                                Check.DebugAssert(
                                    jsonQueryExpression.IsCollection,
                                    "JsonQueryExpression inside materialize collection should always be a collection.");

                                _clientProjections!.Add(jsonQueryExpression);

                                return new CollectionResultExpression(
                                    new ProjectionBindingExpression(
                                        _selectExpression, _clientProjections!.Count - 1, jsonQueryExpression.Type),
                                    materializeCollectionNavigationExpression.Navigation,
                                    materializeCollectionNavigationExpression.Navigation.ClrType.GetSequenceType());
                            }
                        }

                        _clientProjections!.Add(
                            _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(
                                materializeCollectionNavigationExpression.Subquery)!);

                        return new CollectionResultExpression(
                            // expression.Type will be CLR type of the navigation here so that is fine.
                            new ProjectionBindingExpression(_selectExpression, _clientProjections.Count - 1, expression.Type),
                            materializeCollectionNavigationExpression.Navigation,
                            materializeCollectionNavigationExpression.Navigation.ClrType.GetSequenceType());
                }

                switch (_sqlTranslator.TranslateProjection(expression))
                {
                    case SqlExpression sqlExpression:
                        return AddClientProjection(sqlExpression, expression.Type.MakeNullable());

                    // This handles the case of a complex type being projected out of a Select.
                    case RelationalStructuralTypeShaperExpression { StructuralType: IComplexType } shaper:
                        return base.Visit(shaper);

                    case CollectionResultExpression collectionResult:
                        return base.Visit(collectionResult);
                }

                if (expression is MethodCallExpression methodCallExpression)
                {
                    if (methodCallExpression is
                        {
                            Method.IsGenericMethod: true,
                            Method.Name: nameof(Enumerable.ToList),
                            Method: var method,
                            Arguments: [var argument]
                        }
                        && method.DeclaringType == typeof(Enumerable)
                        && argument.Type.TryGetElementType(typeof(IQueryable<>)) != null)
                    {
                        if (_queryableMethodTranslatingExpressionVisitor.TranslateSubquery(argument) is { } subquery)
                        {
                            _clientProjections!.Add(subquery);
                            // expression.Type here will be List<T>
                            return new CollectionResultExpression(
                                new ProjectionBindingExpression(_selectExpression, _clientProjections.Count - 1, expression.Type),
                                structuralProperty: null,
                                methodCallExpression.Method.GetGenericArguments()[0]);
                        }
                    }
                    else
                    {
                        var subquery = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);
                        if (subquery != null)
                        {
                            _clientProjections!.Add(subquery);
                            var type = expression.Type;
                            if (type.IsGenericType
                                && type.GetGenericTypeDefinition() == typeof(IQueryable<>))
                            {
                                type = typeof(List<>).MakeGenericType(type.GetSequenceType());
                            }

                            var projectionBindingExpression = new ProjectionBindingExpression(
                                _selectExpression, _clientProjections.Count - 1, type);
                            return subquery.ResultCardinality == ResultCardinality.Enumerable
                                ? new CollectionResultExpression(
                                    projectionBindingExpression, structuralProperty: null, subquery.ShaperExpression.Type)
                                : projectionBindingExpression;
                        }
                    }
                }

                return base.Visit(expression);
            }

            default:
            {
                switch (_sqlTranslator.TranslateProjection(expression))
                {
                    case SqlExpression sqlExpression:
                        _projectionMapping[_projectionMembers.Peek()] = sqlExpression;
                        return new ProjectionBindingExpression(
                            _selectExpression, _projectionMembers.Peek(), expression.Type.MakeNullable());

                    // This handles the case of a complex type being projected out of a Select.
                    // Note that an entity type being projected is (currently) handled differently
                    case RelationalStructuralTypeShaperExpression { StructuralType: IComplexType } shaper:
                        return base.Visit(shaper);

                    case CollectionResultExpression collectionResult:
                        return base.Visit(collectionResult);

                    case null or RelationalStructuralTypeShaperExpression { StructuralType: IEntityType }:
                        return QueryCompilationContext.NotTranslatedExpression;

                    default:
                        throw new UnreachableException();
                }
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        var left = MatchTypes(Visit(binaryExpression.Left), binaryExpression.Left.Type);
        var right = MatchTypes(Visit(binaryExpression.Right), binaryExpression.Right.Type);

        return binaryExpression.Update(left, VisitAndConvert(binaryExpression.Conversion, "VisitBinary"), right);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
    {
        var test = Visit(conditionalExpression.Test);
        var ifTrue = Visit(conditionalExpression.IfTrue);
        var ifFalse = Visit(conditionalExpression.IfFalse);

        if (test.Type == typeof(bool?))
        {
            test = Expression.Equal(test, Expression.Constant(true, typeof(bool?)));
        }

        ifTrue = MatchTypes(ifTrue, conditionalExpression.IfTrue.Type);
        ifFalse = MatchTypes(ifFalse, conditionalExpression.IfFalse.Type);

        return conditionalExpression.Update(test, ifTrue, ifFalse);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case StructuralTypeShaperExpression shaper:
            {
                // TODO: Make this easier to understand some day.
                StructuralTypeProjectionExpression projection;

                if (shaper.ValueBufferExpression is JsonQueryExpression jsonQueryExpression)
                {
                    if (_indexBasedBinding)
                    {
                        _clientProjections!.Add(jsonQueryExpression);
                        var jsonProjectionBinding = new ProjectionBindingExpression(
                            _selectExpression, _clientProjections.Count - 1, typeof(ValueBuffer));

                        return shaper.Update(jsonProjectionBinding);
                    }

                    _projectionMapping[_projectionMembers.Peek()] = jsonQueryExpression;

#pragma warning disable EF1001
                    return shaper.Update(
                        new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), typeof(ValueBuffer)))
                            // This is to handle have correct type for the shaper expression. It is later fixed in MatchTypes.
                            // This mirrors for structural types what we do for scalars.
                            .MakeClrTypeNullable();
#pragma warning restore EF1001
                }

                if (shaper.ValueBufferExpression is ProjectionBindingExpression projectionBindingExpression)
                {
                    if (projectionBindingExpression.ProjectionMember == null
                        && !_indexBasedBinding)
                    {
                        // If projectionBinding is not mapped via projection member then it is bound via index
                        // Hence we need to switch to index based binding too.
                        return QueryCompilationContext.NotTranslatedExpression;
                    }

                    var projection2 = ((SelectExpression)projectionBindingExpression.QueryExpression)
                        .GetProjection(projectionBindingExpression);
                    if (projection2 is JsonQueryExpression jsonQuery)
                    {
                        if (_indexBasedBinding)
                        {
                            var projectionBinding = AddClientProjection(jsonQuery, typeof(ValueBuffer));

#pragma warning disable EF1001
                            return shaper.Update(projectionBinding)
                                // This is to handle have correct type for the shaper expression. It is later fixed in MatchTypes.
                                // This mirrors for structural types what we do for scalars.
                                .MakeClrTypeNullable();
#pragma warning restore EF1001
                        }

                        _projectionMapping[_projectionMembers.Peek()] = jsonQuery;

#pragma warning disable EF1001
                        return shaper.Update(
                            new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), typeof(ValueBuffer)))
                            // This is to handle have correct type for the shaper expression. It is later fixed in MatchTypes.
                            // This mirrors for structural types what we do for scalars.
                            .MakeClrTypeNullable();
#pragma warning restore EF1001
                    }

                    projection = (StructuralTypeProjectionExpression)projection2;
                }
                else
                {
                    projection = (StructuralTypeProjectionExpression)shaper.ValueBufferExpression;
                }

                if (_indexBasedBinding)
                {
                    if (!_projectionBindingCache!.TryGetValue(projection, out var entityProjectionBinding))
                    {
                        entityProjectionBinding = AddClientProjection(projection, typeof(ValueBuffer));
                        _projectionBindingCache[projection] = entityProjectionBinding;
                    }

#pragma warning disable EF1001
                    return shaper.Update(entityProjectionBinding)
                        // This is to handle have correct type for the shaper expression. It is later fixed in MatchTypes.
                        // This mirrors for structural types what we do for scalars.
                        .MakeClrTypeNullable();
#pragma warning restore EF1001
                }

                _projectionMapping[_projectionMembers.Peek()] = projection;

#pragma warning disable EF1001
                return shaper
                    .Update(new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), typeof(ValueBuffer)))
                    // This is to handle have correct type for the shaper expression. It is later fixed in MatchTypes.
                    // This mirrors for structural types what we do for scalars.
                    .MakeClrTypeNullable();
#pragma warning restore EF1001
            }

            case IncludeExpression includeExpression:
            {
                if (_indexBasedBinding)
                {
                    // we prune nested json includes - we only need the first level of include so that we know the json column
                    // and the json entity that is the start of the include chain - the rest will be added in the shaper phase
                    return includeExpression.Navigation.DeclaringEntityType.IsMappedToJson()
                        ? Visit(includeExpression.EntityExpression)
                        : base.VisitExtension(extensionExpression);
                }

                return QueryCompilationContext.NotTranslatedExpression;
            }

            case CollectionResultExpression
            {
                QueryExpression: ProjectionBindingExpression projectionBindingExpression
            } collectionResultExpression:
            {
                // TODO this should not be needed at some point, we shouldn't be revisiting same projection.
                // This happens because we don't process result selector for Join/SelectMany directly.
                if (_indexBasedBinding)
                {
                    Check.DebugAssert(
                        ReferenceEquals(_selectExpression, projectionBindingExpression.QueryExpression),
                        "The projection should belong to same select expression.");
                    var mappedProjection = _selectExpression.GetProjection(projectionBindingExpression);
                    _clientProjections!.Add(mappedProjection);

                    return collectionResultExpression.Update(
                        new ProjectionBindingExpression(
                            _selectExpression, _clientProjections.Count - 1, collectionResultExpression.Type));
                }

                return QueryCompilationContext.NotTranslatedExpression;
            }

            case CollectionResultExpression { QueryExpression: JsonQueryExpression jsonQuery } collectionResult:
            {
                if (_indexBasedBinding)
                {
                    _clientProjections!.Add(jsonQuery);
                }
                else
                {
                    _projectionMapping[_projectionMembers.Peek()] = jsonQuery;
                }

                return collectionResult.Update(
                    new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), collectionResult.Type));
            }

            default:
                throw new InvalidOperationException(CoreStrings.TranslationFailed(extensionExpression.Print()));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ElementInit VisitElementInit(ElementInit elementInit)
        => elementInit.Update(elementInit.Arguments.Select(e => MatchTypes(Visit(e), e.Type)));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMember(MemberExpression memberExpression)
    {
        var expression = Visit(memberExpression.Expression);
        Expression updatedMemberExpression = memberExpression.Update(
            expression != null ? MatchTypes(expression, memberExpression.Expression!.Type) : expression);

        if (expression?.Type.IsNullableType() == true
            && !_includeFindingExpressionVisitor.ContainsInclude(expression))
        {
            var nullableReturnType = memberExpression.Type.MakeNullable();
            if (!memberExpression.Type.IsNullableType())
            {
                updatedMemberExpression = Expression.Convert(updatedMemberExpression, nullableReturnType);
            }

            updatedMemberExpression = Expression.Condition(
                Expression.Equal(expression, Expression.Default(expression.Type)),
                Expression.Constant(null, nullableReturnType),
                updatedMemberExpression);
        }

        return updatedMemberExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MemberAssignment VisitMemberAssignment(MemberAssignment memberAssignment)
    {
        var expression = memberAssignment.Expression;
        Expression? visitedExpression;
        if (_indexBasedBinding)
        {
            visitedExpression = Visit(memberAssignment.Expression);
        }
        else
        {
            var projectionMember = _projectionMembers.Peek().Append(memberAssignment.Member);
            _projectionMembers.Push(projectionMember);

            visitedExpression = Visit(memberAssignment.Expression);
            if (visitedExpression == QueryCompilationContext.NotTranslatedExpression)
            {
                return memberAssignment.Update(Expression.Convert(visitedExpression, expression.Type));
            }

            _projectionMembers.Pop();
        }

        visitedExpression = MatchTypes(visitedExpression, expression.Type);

        return memberAssignment.Update(visitedExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
    {
        // #30915: if the whole non-entity object is being projected from the nullable side of an outer join, gate its
        // materialization on the recorded marker so it becomes null (rather than a constructed all-NULL object) on no-match rows.
        var hasNullabilityMarker = _selectExpression.TryGetNonEntityNullabilityMarker(memberInitExpression, out var markerBinding);

        var newExpression = Visit(memberInitExpression.NewExpression);
        if (newExpression == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        var newBindings = new MemberBinding[memberInitExpression.Bindings.Count];
        for (var i = 0; i < newBindings.Length; i++)
        {
            if (memberInitExpression.Bindings[i].BindingType != MemberBindingType.Assignment)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            newBindings[i] = VisitMemberBinding(memberInitExpression.Bindings[i]);
            if (newBindings[i] is MemberAssignment { Expression: UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression }
                && unaryExpression.Operand == QueryCompilationContext.NotTranslatedExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }

        var visited = memberInitExpression.Update((NewExpression)newExpression, newBindings);

        if (hasNullabilityMarker && _rootIsTransparentIdentifier)
        {
            // See the matching comment in VisitNew: at the intermediate TransparentIdentifier-rooted projection we suppress
            // gating but must rebind the (now stale) marker and re-key the recorded entry onto the rebuilt node, so the final
            // whole-object projection over the same SelectExpression still finds a valid node and marker binding to gate on.
            var reboundMarker = BindNullabilityMarker(markerBinding!);
            _selectExpression.RemapNonEntityNullabilityMarker(memberInitExpression, visited, reboundMarker);
        }

        return hasNullabilityMarker
            ? GateNonEntityOnNullabilityMarker(visited, markerBinding!, memberInitExpression.Type)
            : visited;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        var @object = Visit(methodCallExpression.Object);
        var arguments = new Expression[methodCallExpression.Arguments.Count];
        for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
        {
            var argument = methodCallExpression.Arguments[i];
            arguments[i] = MatchTypes(Visit(argument), argument.Type);
        }

        Expression updatedMethodCallExpression = methodCallExpression.Update(
            @object != null ? MatchTypes(@object, methodCallExpression.Object!.Type) : @object!,
            arguments);

        if (@object?.Type.IsNullableType() == true
            && methodCallExpression.Object != null
            && !methodCallExpression.Object.Type.IsNullableType())
        {
            var nullableReturnType = methodCallExpression.Type.MakeNullable();
            if (!methodCallExpression.Type.IsNullableType())
            {
                updatedMethodCallExpression = Expression.Convert(updatedMethodCallExpression, nullableReturnType);
            }

            return Expression.Condition(
                Expression.Equal(@object, Expression.Default(@object.Type)),
                Expression.Constant(null, nullableReturnType),
                updatedMethodCallExpression);
        }

        return updatedMethodCallExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNew(NewExpression newExpression)
    {
        if (newExpression.Arguments.Count == 0)
        {
            return newExpression;
        }

        if (!_indexBasedBinding
            && newExpression.Members == null)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        // #30915: if the whole non-entity object is being projected from the nullable side of an outer join, gate its
        // materialization on the recorded marker so it becomes null (rather than a constructed all-NULL object) on no-match rows.
        var hasNullabilityMarker = _selectExpression.TryGetNonEntityNullabilityMarker(newExpression, out var markerBinding);

        var newArguments = new Expression[newExpression.Arguments.Count];
        for (var i = 0; i < newArguments.Length; i++)
        {
            var argument = newExpression.Arguments[i];
            Expression? visitedArgument;
            if (_indexBasedBinding)
            {
                visitedArgument = Visit(argument);
            }
            else
            {
                var projectionMember = _projectionMembers.Peek().Append(newExpression.Members![i]);
                _projectionMembers.Push(projectionMember);
                visitedArgument = Visit(argument);
                if (visitedArgument == QueryCompilationContext.NotTranslatedExpression)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                _projectionMembers.Pop();
            }

            newArguments[i] = MatchTypes(visitedArgument, argument.Type);
        }

        var visited = newExpression.Update(newArguments);

        if (hasNullabilityMarker && _rootIsTransparentIdentifier)
        {
            // Intermediate (TransparentIdentifier-rooted) projection: do not gate here; the recorded object is composed further
            // and must remain a bare New for downstream member-folding. But this phase rebinds the node's columns into the final
            // projection representation (index-/member-based) and may rebuild the node instance, so the originally-recorded marker
            // binding (a member binding valid only against the pre-rebind projection) goes stale. Rebind the marker through this
            // phase's projection mechanism and re-key it onto the rebuilt node, so the *final* user projection (a later Translate
            // pass over the same SelectExpression, where the whole object is projected) finds a still-valid marker binding.
            var reboundMarker = BindNullabilityMarker(markerBinding!);
            _selectExpression.RemapNonEntityNullabilityMarker(newExpression, visited, reboundMarker);
        }

        return hasNullabilityMarker
            ? GateNonEntityOnNullabilityMarker(visited, markerBinding!, newExpression.Type)
            : visited;
    }

    // #30915: returns true iff the root of the projection expression is a TransparentIdentifier New. A transparent identifier
    // (produced by TransparentIdentifierFactory.Create) is the intermediate outer/inner result of a join that EF Core composes
    // further (a subsequent Where/Join/Select member-accesses its Outer/Inner parts). When the projection root is such a node, a
    // recorded non-entity object lives nested inside it and must stay a bare New so that downstream member-folding keeps working;
    // the marker only needs to gate the final user projection, which is never itself a transparent identifier.
    private static bool IsTransparentIdentifierProjection(Expression expression)
        => expression is NewExpression newExpression && TransparentIdentifierFactory.IsTransparentIdentifierType(newExpression.Type);

    // #30915: builds a Condition that gates the materialization of a non-entity object projected from the nullable side of an
    // outer join: it returns the default (null) for the object's CLR type when the recorded marker column reads NULL (no-match
    // row), otherwise it constructs the object as usual. The marker is bound through the same projection mechanism the visitor
    // uses for every other projected sub-expression, so it survives to the reader as a readable nullable scalar projection.
    private Expression GateNonEntityOnNullabilityMarker(Expression visited, Expression markerBinding, Type objectType)
    {
        // objectType is always the shaper node's own CLR type: the reference type for a reference-type New/MemberInit, or the
        // (non-nullable) struct for a value-type MemberInit. It is never Nullable<T> here -- see the Nullable<T> note below.
        //
        // Reference type: Expression.Default(objectType) is null, gating the whole object to null on a no-match row (the #30915
        // case). Value type (mutable struct / record struct projected via MemberInit): Expression.Default(objectType) is a zeroed
        // struct rather than null -- which is exactly right, mirroring LINQ-to-Objects DefaultIfEmpty semantics for a value-type
        // sequence (a no-match row yields default(T), not an exception). Both are gated here.
        //
        // Two shapes reach SelectExpression.AddJoin's New/MemberInit recording condition (so a markerBinding is produced) but are
        // NOT meaningfully gated here:
        //  - Constructor-bound structs (positional record structs, read-only DTOs, ValueTuple) are NewExpressions, so a marker is
        //    injected, but they fail to TRANSLATE at an earlier, deferred point before the query ever reaches this gate (tracked
        //    separately); the injected marker is simply never consumed.
        //  - A user-visible Nullable<T> whole-object arrives as Convert(MemberInit, T?): the marker is recorded against the inner
        //    MemberInit, so this gate DOES fire -- but with objectType = the underlying struct, producing a zeroed struct that the
        //    outer Convert then lifts to a non-null T? (HasValue == true). That is correct: casting a real (if zeroed) struct to
        //    Nullable<T> can never itself yield null, matching DefaultIfEmpty over a value-type sequence.
        //
        // Intermediate (TransparentIdentifier-rooted) projections are suppressed: there the recorded object is composed further
        // and must stay a bare New for downstream member-folding.
        if (_rootIsTransparentIdentifier)
        {
            return visited;
        }

        var boundMarker = BindNullabilityMarker(markerBinding);

        if (visited.Type != objectType)
        {
            visited = Expression.Convert(visited, objectType);
        }

        return Expression.Condition(
            Expression.Equal(boundMarker, Expression.Constant(null, boundMarker.Type)),
            Expression.Default(objectType),
            visited);
    }

    // #30915: binds the recorded marker ProjectionBindingExpression (typed int?, bound against _selectExpression) into the
    // projection currently being built, returning a readable ProjectionBindingExpression. This reuses the visitor's two binding
    // representations (client projections vs. projection mapping). A unique synthetic projection member is used in mapping mode
    // so the marker never collides with a real projected member.
    private Expression BindNullabilityMarker(Expression markerBinding)
    {
        var mappedSqlExpression = _selectExpression.GetProjection((ProjectionBindingExpression)markerBinding);

        if (_indexBasedBinding)
        {
            return AddClientProjection(mappedSqlExpression, typeof(int?));
        }

        // Reuse SelectExpression's synthetic marker member so that when the outer SelectExpression applies this projection mapping,
        // SelectExpression.GetProjectionAlias forces the lowercase "marker" alias for the marker column on the outer SELECT too,
        // keeping the synthesized internal column consistently lowercase (cf. ApplyDefaultIfEmpty's "empty").
        var projectionMember = _projectionMembers.Peek().Append(SelectExpression.NullabilityMarkerMemberInfo);
        _projectionMapping[projectionMember] = mappedSqlExpression;
        return new ProjectionBindingExpression(_selectExpression, projectionMember, typeof(int?));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
        => newArrayExpression.Update(newArrayExpression.Expressions.Select(e => MatchTypes(Visit(e), e.Type)));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        var operand = Visit(unaryExpression.Operand);

        return unaryExpression.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked
            && unaryExpression.Type == operand.Type
                ? operand
                : unaryExpression.Update(MatchTypes(operand, unaryExpression.Operand.Type));
    }

    [DebuggerStepThrough]
    private static Expression MatchTypes(Expression expression, Type targetType)
    {
        if (targetType != expression.Type
            && targetType.TryGetElementType(typeof(IQueryable<>)) == null)
        {
            Check.DebugAssert(
                targetType.MakeNullable() == expression.Type,
                $"expression has type {expression.Type.Name}, but must be nullable over {targetType.Name}");

            return expression switch
            {
#pragma warning disable EF1001
                RelationalStructuralTypeShaperExpression structuralShaper => structuralShaper.MakeClrTypeNonNullable(),
#pragma warning restore EF1001

                _ =>  Expression.Convert(expression, targetType),
            };
        }

        return expression;
    }

    private ProjectionBindingExpression AddClientProjection(Expression expression, Type type)
    {
        var existingIndex = _clientProjections!.FindIndex(e => e.Equals(expression));
        if (existingIndex == -1)
        {
            _clientProjections.Add(expression);
            existingIndex = _clientProjections.Count - 1;
        }

        return new ProjectionBindingExpression(_selectExpression, existingIndex, type);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
#pragma warning restore IDE0052 // Remove unread private members
        => (T)queryContext.Parameters[parameterName]!;

    private sealed class IncludeFindingExpressionVisitor : ExpressionVisitor
    {
        private bool _containsInclude;

        public bool ContainsInclude(Expression expression)
        {
            _containsInclude = false;

            Visit(expression);

            return _containsInclude;
        }

        [return: NotNullIfNotNull(nameof(expression))]
        public override Expression? Visit(Expression? expression)
            => _containsInclude ? expression : base.Visit(expression);

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is IncludeExpression)
            {
                _containsInclude = true;

                return extensionExpression;
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}
