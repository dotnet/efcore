// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosProjectionBindingExpressionVisitor : ExpressionVisitor
{
    private readonly CosmosQueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
    private readonly CosmosSqlTranslatingExpressionVisitor _sqlTranslator;
    private readonly ITypeMappingSource _typeMappingSource;
    private readonly IModel _model;
    private SelectExpression _selectExpression;

    private readonly Dictionary<ProjectionMember, Expression> _projectionMapping = new();
    private readonly Stack<ProjectionMember> _projectionMembers = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosProjectionBindingExpressionVisitor(
        IModel model,
        CosmosQueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
        CosmosSqlTranslatingExpressionVisitor sqlTranslator,
        ITypeMappingSource typeMappingSource)
    {
        _model = model;
        _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
        _sqlTranslator = sqlTranslator;
        _typeMappingSource = typeMappingSource;
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

        _projectionMembers.Push(new ProjectionMember());

        var result = Visit(expression);

        _selectExpression.ReplaceProjectionMapping(_projectionMapping);
        _selectExpression = null!;
        _projectionMembers.Clear();
        _projectionMapping.Clear();

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
            case null:
                return null;
            case NewExpression or MemberInitExpression or StructuralTypeShaperExpression or IncludeExpression or MaterializeCollectionNavigationExpression:
                return base.Visit(expression);
            default:
                var translation = _sqlTranslator.TranslateProjection(expression);
                switch (translation)
                {
                    case SqlExpression sqlExpression:
                        _projectionMapping[_projectionMembers.Peek()] = sqlExpression;
                        return new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), expression.Type.MakeNullable());
                    case StructuralTypeShaperExpression shaper:
                        return base.Visit(shaper);
                    default:
                        return base.Visit(expression);
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
            case StructuralTypeShaperExpression structuralTypeShaper:
            {
                StructuralTypeProjectionExpression structuralTypeProjection;

                switch (structuralTypeShaper.ValueBufferExpression)
                {
                    case ProjectionBindingExpression innerProjectionBinding:

                        if (innerProjectionBinding.ProjectionMember is null)
                        {
                            return QueryCompilationContext.NotTranslatedExpression;
                        }

                        VerifySelectExpression(innerProjectionBinding);

                        structuralTypeProjection =
                            (StructuralTypeProjectionExpression)_selectExpression
                                .GetMappedProjection(innerProjectionBinding.ProjectionMember);

                        break;
                    default:
                        structuralTypeProjection = (StructuralTypeProjectionExpression)structuralTypeShaper.ValueBufferExpression;
                        break;
                }

                _projectionMapping[_projectionMembers.Peek()] = structuralTypeProjection;

                var projectionBinding = new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), typeof(ValueBuffer));

                if (structuralTypeShaper.StructuralType is IComplexType { ComplexProperty: { } complexProperty })
                {
                    if (complexProperty.IsCollection && structuralTypeShaper.ValueBufferExpression is StructuralTypeProjectionExpression)
                    {
                        structuralTypeShaper = structuralTypeShaper.Update(Expression.Convert(Expression.Convert(structuralTypeShaper.ValueBufferExpression, typeof(object)), typeof(ValueBuffer)));

                        return new CollectionShaperExpression(
                            projectionBinding,
                            structuralTypeShaper,
                            complexProperty,
                            complexProperty.ComplexType.ClrType);
                    }

                    if (complexProperty.IsNullable)
                    {
                        // This is to handle have correct type for the shaper expression. It is later fixed in MatchTypes.
                        // This mirrors for structural types what we do for scalars.
#pragma warning disable EF1001 // Internal EF Core API usage.
                        structuralTypeShaper = structuralTypeShaper.MakeClrTypeNullable();
#pragma warning restore EF1001 // Internal EF Core API usage.
                    }
                }

                structuralTypeShaper = structuralTypeShaper.Update(projectionBinding);

                return structuralTypeShaper;
            }

            case MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression:
                if (materializeCollectionNavigationExpression.Navigation is not INavigation includableCollectionNavigation
                    || !includableCollectionNavigation.IsEmbedded())
                {
                    throw new InvalidOperationException(
                        CosmosStrings.NonEmbeddedIncludeNotSupported(materializeCollectionNavigationExpression.Navigation));
                }

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

                var shaper = _sqlTranslator.TranslateProjection(subquery) as StructuralTypeShaperExpression;
                if (shaper == null)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                var valueBuffer = new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), typeof(ValueBuffer));
                _projectionMapping[_projectionMembers.Peek()] = shaper.ValueBufferExpression;

                shaper = shaper.Update(Expression.Convert(Expression.Convert(shaper.ValueBufferExpression, typeof(object)), typeof(ValueBuffer)));

                return new CollectionShaperExpression(
                    valueBuffer,
                    shaper,
                    includableCollectionNavigation,
                    includableCollectionNavigation.TargetEntityType.ClrType);

            case IncludeExpression includeExpression:
                if (includeExpression.Navigation is not INavigation includableNavigation || !includableNavigation.IsEmbedded())
                {
                    throw new InvalidOperationException(
                        CosmosStrings.NonEmbeddedIncludeNotSupported(includeExpression.Navigation));
                }

                // we prune includes, we only need the root of the include which is the the projected StructuralType
                // the rest will be added in the shaper phase
                return Visit(includeExpression.EntityExpression);

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
    protected override MemberAssignment VisitMemberAssignment(MemberAssignment memberAssignment)
    {
        var expression = memberAssignment.Expression;

        var projectionMember = _projectionMembers.Peek().Append(memberAssignment.Member);
        _projectionMembers.Push(projectionMember);

        var visitedExpression = Visit(memberAssignment.Expression);
        if (visitedExpression == QueryCompilationContext.NotTranslatedExpression)
        {
            return memberAssignment.Update(Expression.Convert(visitedExpression, expression.Type));
        }

        _projectionMembers.Pop();

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
        if (methodCallExpression is
            {
                Method.IsGenericMethod: true,
                Method: var method,
            }
            && (method.DeclaringType == typeof(Enumerable) || method.DeclaringType == typeof(Queryable)))
        {
            throw new InvalidOperationException(
                CoreStrings.TranslationFailedWithDetails(methodCallExpression.Print(),
                    "Could not project out subquery.")); // @TODO: Create issue?
        }

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

        if (newExpression.Members == null)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        var newArguments = new Expression[newExpression.Arguments.Count];
        for (var i = 0; i < newArguments.Length; i++)
        {
            var argument = newExpression.Arguments[i];
            var projectionMember = _projectionMembers.Peek().Append(newExpression.Members![i]);
            _projectionMembers.Push(projectionMember);
            var visitedArgument = Visit(argument);
            if (visitedArgument == QueryCompilationContext.NotTranslatedExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            _projectionMembers.Pop();

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

        return (unaryExpression.NodeType == ExpressionType.Convert
                || unaryExpression.NodeType == ExpressionType.ConvertChecked)
            && unaryExpression.Type == operand.Type
                ? operand
                : unaryExpression.Update(MatchTypes(operand, unaryExpression.Operand.Type));
    }

    // TODO: Debugging
    private void VerifySelectExpression(ProjectionBindingExpression projectionBindingExpression)
    {
        if (projectionBindingExpression.QueryExpression != _selectExpression)
        {
            throw new InvalidOperationException(CoreStrings.TranslationFailed(projectionBindingExpression.Print()));
        }
    }

    private static Expression MatchTypes(Expression expression, Type targetType)
    {
        if (targetType != expression.Type
            && targetType.TryGetSequenceType() == null)
        {
            Check.DebugAssert(targetType.MakeNullable() == expression.Type, "expression.Type must be nullable of targetType");

            expression = Expression.Convert(expression, targetType);
        }

        return expression;
    }
}
