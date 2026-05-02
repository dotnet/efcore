// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Internal;
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
    private static readonly MethodInfo GetParameterValueMethodInfo
        = typeof(CosmosProjectionBindingExpressionVisitor)
            .GetTypeInfo().GetDeclaredMethod(nameof(GetParameterValue))!;

    private string? _translationErrorDetails;

    private readonly CosmosQueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
    private readonly CosmosSqlTranslatingExpressionVisitor _sqlTranslator;
    private readonly ITypeMappingSource _typeMappingSource;
    private readonly IModel _model;
    private SelectExpression _selectExpression;
    private bool _clientEval;

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
        _clientEval = false;

        _projectionMembers.Push(new ProjectionMember());

        var result = Visit(expression);

        if (result == QueryCompilationContext.NotTranslatedExpression)
        {
            _clientEval = true;

            result = Visit(expression);

            _projectionMapping.Clear();
        }

        if (result == QueryCompilationContext.NotTranslatedExpression) // @TODO: Instead throw where needed? Then we can remove all the if NotTranslatedExpression in other methods?
        {
            var message = _translationErrorDetails == null
                ? CoreStrings.TranslationFailed(expression.Print())
                : CoreStrings.TranslationFailedWithDetails(expression.Print(), _translationErrorDetails);
            throw new InvalidOperationException(message);
        }

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
            case ConstantExpression when _clientEval:
                return expression;
            case QueryParameterExpression queryParameter when _clientEval:
                return Expression.Call(
                    GetParameterValueMethodInfo.MakeGenericMethod(queryParameter.Type),
                    QueryCompilationContext.QueryContextParameter,
                    Expression.Constant(queryParameter.Name));
            default:
                var translation = _sqlTranslator.TranslateProjection(expression);
                switch (translation)
                {
                    case SqlExpression when _clientEval:
                        return new ProjectionBindingExpression(
                            _selectExpression, _selectExpression.AddToProjection(translation), expression.Type.MakeNullable());
                    case SqlExpression when !_clientEval:
                        _projectionMapping[_projectionMembers.Peek()] = translation;
                        return new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), expression.Type.MakeNullable());
                    case StructuralTypeShaperExpression:
                        return base.Visit(translation);
                    default:
                        if (_clientEval)
                        {
                            return base.Visit(expression);
                        }
                        return QueryCompilationContext.NotTranslatedExpression;
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
        var left = Visit(binaryExpression.Left);
        if (left == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }
        var right = Visit(binaryExpression.Right);
        if (right == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }
        left = MatchTypes(left, binaryExpression.Left.Type);
        right = MatchTypes(right, binaryExpression.Right.Type);

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
        if (test == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }
        var ifTrue = Visit(conditionalExpression.IfTrue);
        if (ifTrue == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }
        var ifFalse = Visit(conditionalExpression.IfFalse);
        if (ifFalse == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

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

                ProjectionBindingExpression projectionBinding;
                if (_clientEval)
                {
                    projectionBinding = new ProjectionBindingExpression(_selectExpression, _selectExpression.AddToProjection(structuralTypeProjection), typeof(ValueBuffer));
                }
                else
                {
                    _projectionMapping[_projectionMembers.Peek()] = structuralTypeProjection;
                    projectionBinding = new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), typeof(ValueBuffer));
                }

                if (structuralTypeShaper.StructuralType is IComplexType { ComplexProperty: { } complexProperty })
                {
                    if (complexProperty.IsCollection && structuralTypeShaper.ValueBufferExpression is StructuralTypeProjectionExpression)
                    {
                        // @TODO: Shouldn't this actually be a ProjectionBindingExpression related to a select expression that holds a projection?
                        structuralTypeShaper = structuralTypeShaper.Update(Expression.Convert(Expression.Convert(structuralTypeShaper.ValueBufferExpression, typeof(object)), typeof(ValueBuffer)));

                        return new CollectionShaperExpression(
                            projectionBinding,
                            structuralTypeShaper,
                            complexProperty.GetCollectionAccessor()!,
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

                ProjectionBindingExpression valueBuffer;
                if (_clientEval)
                {
                    valueBuffer = new ProjectionBindingExpression(_selectExpression, _selectExpression.AddToProjection(shaper.ValueBufferExpression), typeof(ValueBuffer));
                }
                else
                {
                    valueBuffer = new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), typeof(ValueBuffer));
                    _projectionMapping[_projectionMembers.Peek()] = shaper.ValueBufferExpression;
                }

                // @TODO: Shouldn't this actually be a ProjectionBindingExpression related to a select expression that holds a projection?
                shaper = shaper.Update(Expression.Convert(Expression.Convert(shaper.ValueBufferExpression, typeof(object)), typeof(ValueBuffer)));

                return new CollectionShaperExpression(
                    valueBuffer,
                    shaper,
                    includableCollectionNavigation.GetCollectionAccessor()!,
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
    protected override Expression VisitMember(MemberExpression memberExpression)
    {
        var expression = Visit(memberExpression.Expression);
        if (expression == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }
        Expression updatedMemberExpression = memberExpression.Update(
            expression != null ? MatchTypes(expression, memberExpression.Expression!.Type) : expression);

        if (expression?.Type.IsNullableType() == true)
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

        if (_clientEval)
        {
            visitedExpression = Visit(memberAssignment.Expression);
        }
        else
        {
            var projectionMember = _projectionMembers.Peek().Append(memberAssignment.Member);
            _projectionMembers.Push(projectionMember);

            visitedExpression = Visit(memberAssignment.Expression);

            _projectionMembers.Pop();
        }

        if (visitedExpression == QueryCompilationContext.NotTranslatedExpression)
        {
            return memberAssignment.Update(Expression.Convert(visitedExpression, expression.Type));
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
        // Translate subqueries in projections
        // @TODO: Shouldn't this happen in CosmosQueryableMethodTranslator?
        if (methodCallExpression is
            {
                Method.IsGenericMethod: true,
                Method: var method,
            }
            && (method.DeclaringType == typeof(Enumerable) || method.DeclaringType == typeof(Queryable))
            && methodCallExpression.Arguments is [var collectionArgument, ..]
            && collectionArgument.Type.TryGetElementType(typeof(IQueryable<>)) is { } elementType)
        {
            if (method is not { Name: nameof(Enumerable.ToList) or nameof(Enumerable.ToArray) })
            {
                _translationErrorDetails = "Use ToList or ToArray over the subquery projection to indicate the array allocation on the SQL side. Cosmos requires projections to be a single value and must explicitly convert projected subqueries to be converted to an array."; // @TODO: resource string
                return QueryCompilationContext.NotTranslatedExpression; // @TODO: Should we create an issue?
            }

            if (_queryableMethodTranslatingExpressionVisitor.TranslateSubquery(collectionArgument) is not { } subquery
                        || !subquery.TryConvertToArray(_typeMappingSource, out var array))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            ((SelectExpression)subquery.QueryExpression).ApplyProjection();

            // If ToList() was composed over a subquery with operators, the result here is an ArrayExpression (ARRAY(SELECT ...)), whose
            // CLR Type is IEnumerable<T>. This can be directly used in the resulting ProjectingBindingExpression - the shaper will
            // simply read the JSON results out successfully.
            // But if ToList() is composed directly over an array property, that property could have type e.g. T[], which will be read
            // in the shaper, and then the cast from T[] to List<T> will fail. As a result, wrap the array in an additional
            // "reprojection" subquery, effectively to change the CLR type.
            if (array is SqlExpression scalarArray && !(array.Type.IsGenericType && array.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                Check.DebugAssert(
                array is not ScalarArrayExpression and not ObjectArrayExpression, "ArrayExpression should be IEnumerable");

                if (scalarArray is not { TypeMapping.ElementTypeMapping: CosmosTypeMapping elementTypeMapping })
                {
                    throw new UnreachableException("Scalar array with no element type mapping");
                }

                // @TODO: Doesn't this cause an additional ARRAY(SELECT ...) to be generated in the SQL? This is bad for RU's as it will allocate an additional array
                // TODO: Proper alias management (#33894).
                var arrayReprojectionSubquery = SelectExpression.CreateForCollection(
                    array, "i", new ScalarReferenceExpression("i", elementTypeMapping.ClrType, elementTypeMapping));
                arrayReprojectionSubquery.ApplyProjection();

                array = new ScalarArrayExpression(
                    arrayReprojectionSubquery,
                    methodCallExpression.Type, // List<>
                    _typeMappingSource.FindMapping(methodCallExpression.Type, _model, elementTypeMapping));
            }

            // @TODO: Ask if there is a better way for this..
            // We should update projection bindings in the subquery shaper to relate to a query expression that can actually provide the correct binding information.
            // Then are able to use the projection binding's query expression directly in ShaperProcessingVisitor, instead of storing the select expression separately there
            // This appears to be needed because ShapedQueryExpression doesn't properly replace projection bindings their query expression when updating the shaper expression.
            // But fixing that causes a lot of errors (in other providers?).
            var innerShaper = new ProjectionBindigQueryProjectionApplyingExpressionVisitor().Visit(subquery.ShaperExpression);

            var listType = typeof(List<>).MakeGenericType(elementType);
            var collectionCreator = (Func<object>)Expression.Lambda(Expression.New(listType.GetConstructor(Type.EmptyTypes)!)).Compile();

            var binding = CreateBinding();
            Expression shaper = new CollectionShaperExpression(binding, subquery.ShaperExpression, listType, collectionCreator, elementType);

            if (method.Name == nameof(Enumerable.ToArray))
            {
                shaper = Expression.Call(shaper, listType.GetMethod(nameof(List<>.ToArray))!);
            }

            return shaper;

            ProjectionBindingExpression CreateBinding()
            {
                if (_clientEval)
                {
                    return new ProjectionBindingExpression(
                        _selectExpression,
                        _selectExpression.AddToProjection(array),
                        listType);
                }
                else
                {
                    _projectionMapping[_projectionMembers.Peek()] = array;
                    return new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), listType);
                }
            }
        }

        var @object = Visit(methodCallExpression.Object);
        if (@object == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        var arguments = new Expression[methodCallExpression.Arguments.Count];
        for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
        {
            var argument = Visit(methodCallExpression.Arguments[i]);
            if (argument == QueryCompilationContext.NotTranslatedExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
            arguments[i] = MatchTypes(argument, methodCallExpression.Arguments[i].Type);
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

    private class ProjectionBindigQueryProjectionApplyingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            if (node is ProjectionBindingExpression projectionBindingExpression)
            {
                var selectExpression = (SelectExpression)projectionBindingExpression.QueryExpression;
                selectExpression.ApplyProjection();
            }

            return base.VisitExtension(node);
        }
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

        if (!_clientEval && newExpression.Members == null)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        var newArguments = new Expression[newExpression.Arguments.Count];
        for (var i = 0; i < newArguments.Length; i++)
        {
            var argument = newExpression.Arguments[i];
            Expression visitedArgument;
            if (_clientEval)
            {
                visitedArgument = Visit(argument);
                if (visitedArgument == QueryCompilationContext.NotTranslatedExpression)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }
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
    {
        var expressions = new Expression[newArrayExpression.Expressions.Count];
        for (var i = 0; i < expressions.Length; i++)
        {
            var expression = Visit(newArrayExpression.Expressions[i]);
            if (expression == QueryCompilationContext.NotTranslatedExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
            expressions[i] = MatchTypes(expression, newArrayExpression.Expressions[i].Type);
        }

        return newArrayExpression.Update(expressions);
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
        if (operand == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

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

    [UsedImplicitly]
    private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
        => (T)queryContext.Parameters[parameterName]!;
}
