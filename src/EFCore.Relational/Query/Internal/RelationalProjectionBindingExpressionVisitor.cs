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
    [return: NotNullIfNotNull("expression")]
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

                    case ParameterExpression parameterExpression:
                        return parameterExpression.Name?.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal)
                            == true
                                ? Expression.Call(
                                    GetParameterValueMethodInfo.MakeGenericMethod(parameterExpression.Type),
                                    QueryCompilationContext.QueryContextParameter,
                                    Expression.Constant(parameterExpression.Name))
                                : throw new InvalidOperationException(CoreStrings.TranslationFailed(parameterExpression.Print()));

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

                if (_sqlTranslator.TranslateProjection(expression) is SqlExpression sqlExpression)
                {
                    return AddClientProjection(sqlExpression, expression.Type.MakeNullable());
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
                        if (_queryableMethodTranslatingExpressionVisitor.TranslateSubquery(argument) is ShapedQueryExpression subquery)
                        {
                            _clientProjections!.Add(subquery);
                            // expression.Type here will be List<T>
                            return new CollectionResultExpression(
                                new ProjectionBindingExpression(_selectExpression, _clientProjections.Count - 1, expression.Type),
                                navigation: null,
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
                                    projectionBindingExpression, navigation: null, subquery.ShaperExpression.Type)
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

                    return shaper.Update(
                        new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), typeof(ValueBuffer)));
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

                            return shaper.Update(projectionBinding);
                        }

                        _projectionMapping[_projectionMembers.Peek()] = jsonQuery;

                        return shaper.Update(
                            new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), typeof(ValueBuffer)));
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

                    return shaper.Update(entityProjectionBinding);
                }

                _projectionMapping[_projectionMembers.Peek()] = projection;

                return shaper.Update(
                    new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), typeof(ValueBuffer)));
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

            case CollectionResultExpression collectionResultExpression:
            {
                // TODO this should not be needed at some point, we shouldn't be revisit same projection.
                // This happens because we don't process result selector for Join/SelectMany directly.
                if (_indexBasedBinding)
                {
                    Check.DebugAssert(
                        ReferenceEquals(_selectExpression, collectionResultExpression.ProjectionBindingExpression.QueryExpression),
                        "The projection should belong to same select expression.");
                    var mappedProjection = _selectExpression.GetProjection(collectionResultExpression.ProjectionBindingExpression);
                    _clientProjections!.Add(mappedProjection);

                    return collectionResultExpression.Update(
                        new ProjectionBindingExpression(
                            _selectExpression, _clientProjections.Count - 1, collectionResultExpression.Type));
                }

                return QueryCompilationContext.NotTranslatedExpression;
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

        return memberInitExpression.Update((NewExpression)newExpression, newBindings);
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

        return newExpression.Update(newArguments);
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
            Check.DebugAssert(targetType.MakeNullable() == expression.Type, "expression.Type must be nullable of targetType");

            expression = Expression.Convert(expression, targetType);
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

    private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
#pragma warning restore IDE0052 // Remove unread private members
        => (T)queryContext.ParameterValues[parameterName]!;

    private sealed class IncludeFindingExpressionVisitor : ExpressionVisitor
    {
        private bool _containsInclude;

        public bool ContainsInclude(Expression expression)
        {
            _containsInclude = false;

            Visit(expression);

            return _containsInclude;
        }

        [return: NotNullIfNotNull("expression")]
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
