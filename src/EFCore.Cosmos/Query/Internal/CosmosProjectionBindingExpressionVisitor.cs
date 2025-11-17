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

    private readonly CosmosQueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
    private readonly CosmosSqlTranslatingExpressionVisitor _sqlTranslator;
    private readonly ITypeMappingSource _typeMappingSource;
    private readonly IModel _model;
    private SelectExpression _selectExpression;
    private bool _clientEval;

    private readonly Dictionary<ProjectionMember, Expression> _projectionMapping = new();
    private readonly Stack<ProjectionMember> _projectionMembers = new();
    private readonly Dictionary<ParameterExpression, CollectionShaperExpression> _collectionShaperMapping = new();
    private readonly Stack<INavigation> _includedNavigations = new();

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
        if (expression == null)
        {
            return null;
        }

        if (expression is NewExpression or MemberInitExpression or StructuralTypeShaperExpression)
        {
            return base.Visit(expression);
        }

        if (_clientEval)
        {
            switch (expression)
            {
                case ConstantExpression:
                    return expression;

                case ParameterExpression parameterExpression:
                    if (_collectionShaperMapping.ContainsKey(parameterExpression))
                    {
                        return parameterExpression;
                    }

                    if (parameterExpression.Name?.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal)
                        == true)
                    {
                        return Expression.Call(
                            GetParameterValueMethodInfo.MakeGenericMethod(parameterExpression.Type),
                            QueryCompilationContext.QueryContextParameter,
                            Expression.Constant(parameterExpression.Name));
                    }

                    throw new InvalidOperationException(CoreStrings.TranslationFailed(parameterExpression.Print()));

                case MaterializeCollectionNavigationExpression:
                    return base.Visit(expression);
            }

            var translation = _sqlTranslator.Translate(expression);
            if (translation == null)
            {
                return base.Visit(expression);
            }

            return new ProjectionBindingExpression(
                _selectExpression, _selectExpression.AddToProjection(translation), expression.Type.MakeNullable());
        }
        else
        {
            var translation = _sqlTranslator.Translate(expression);
            if (translation == null)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            _projectionMapping[_projectionMembers.Peek()] = translation;

            return new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), expression.Type.MakeNullable());
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
            case StructuralTypeShaperExpression entityShaperExpression:
            {
                var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;
                VerifySelectExpression(projectionBindingExpression);

                if (projectionBindingExpression.ProjectionMember is null)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                var projection = _selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember);

                if (_clientEval)
                {
                    var entityProjection = (EntityProjectionExpression)projection;

                    return entityShaperExpression.Update(
                        new ProjectionBindingExpression(
                            _selectExpression, _selectExpression.AddToProjection(entityProjection), typeof(ValueBuffer)));
                }

                _projectionMapping[_projectionMembers.Peek()] = projection;

                return entityShaperExpression.Update(
                    new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), typeof(ValueBuffer)));
            }

            case MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression:
                return materializeCollectionNavigationExpression.Navigation is INavigation embeddableNavigation
                    && embeddableNavigation.IsEmbedded()
                        ? base.Visit(materializeCollectionNavigationExpression.Subquery)
                        : base.VisitExtension(materializeCollectionNavigationExpression);

            case IncludeExpression includeExpression:
                if (!_clientEval)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                if (includeExpression.Navigation is not INavigation includableNavigation || !includableNavigation.IsEmbedded())
                {
                    throw new InvalidOperationException(
                        CosmosStrings.NonEmbeddedIncludeNotSupported(includeExpression.Navigation));
                }

                _includedNavigations.Push(includableNavigation);

                var newIncludeExpression = base.VisitExtension(includeExpression);

                _includedNavigations.Pop();

                return newIncludeExpression;

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
        if (!_clientEval)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        var innerExpression = Visit(memberExpression.Expression);

        StructuralTypeShaperExpression? shaperExpression;
        switch (innerExpression)
        {
            case StructuralTypeShaperExpression shaper:
                shaperExpression = shaper;
                break;

            case UnaryExpression unaryExpression:
                shaperExpression = unaryExpression.Operand as StructuralTypeShaperExpression;
                if (shaperExpression == null
                    || unaryExpression.NodeType != ExpressionType.Convert)
                {
                    return NullSafeUpdate(innerExpression);
                }

                break;

            default:
                return NullSafeUpdate(innerExpression);
        }

        var innerEntityProjection = shaperExpression.ValueBufferExpression switch
        {
            ProjectionBindingExpression innerProjectionBindingExpression
                => (EntityProjectionExpression)_selectExpression.Projection[innerProjectionBindingExpression.Index!.Value].Expression,

            // Unwrap EntityProjectionExpression when the root entity is not projected
            UnaryExpression unaryExpression
                => (EntityProjectionExpression)((UnaryExpression)unaryExpression.Operand).Operand,

            _ => throw new InvalidOperationException(CoreStrings.TranslationFailed(memberExpression.Print()))
        };

        var navigationProjection = innerEntityProjection.BindMember(
            memberExpression.Member, innerExpression.Type, clientEval: true, out var propertyBase);

        if (propertyBase is not INavigation navigation
            || !navigation.IsEmbedded())
        {
            return NullSafeUpdate(innerExpression);
        }

        switch (navigationProjection)
        {
            case EntityProjectionExpression entityProjection:
                return new StructuralTypeShaperExpression(
                    navigation.TargetEntityType,
                    Expression.Convert(Expression.Convert(entityProjection, typeof(object)), typeof(ValueBuffer)),
                    nullable: true);

            case ObjectArrayAccessExpression objectArrayProjectionExpression:
            {
                var innerShaperExpression = new StructuralTypeShaperExpression(
                    navigation.TargetEntityType,
                    Expression.Convert(
                        Expression.Convert(objectArrayProjectionExpression.InnerProjection, typeof(object)), typeof(ValueBuffer)),
                    nullable: true);

                return new CollectionShaperExpression(
                    objectArrayProjectionExpression,
                    innerShaperExpression,
                    navigation,
                    innerShaperExpression.StructuralType.ClrType);
            }

            default:
                throw new InvalidOperationException(CoreStrings.TranslationFailed(memberExpression.Print()));
        }

        Expression NullSafeUpdate(Expression? expression)
        {
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
        Expression visitedExpression;
        if (_clientEval)
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
        if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var memberName)
            || methodCallExpression.TryGetIndexerArguments(_model, out source, out memberName))
        {
            if (!_clientEval)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            var visitedSource = Visit(source);

            StructuralTypeShaperExpression? shaperExpression;
            switch (visitedSource)
            {
                case StructuralTypeShaperExpression s:
                    shaperExpression = s;
                    break;

                case UnaryExpression { NodeType: ExpressionType.Convert, Operand: StructuralTypeShaperExpression s }:
                    shaperExpression = s;
                    break;

                case ParameterExpression parameterExpression
                    when _collectionShaperMapping.TryGetValue(parameterExpression, out var collectionShaper):
                    shaperExpression = (StructuralTypeShaperExpression)collectionShaper.InnerShaper;
                    break;

                default:
                    return QueryCompilationContext.NotTranslatedExpression;
            }

            var innerEntityProjection = shaperExpression.ValueBufferExpression switch
            {
                EntityProjectionExpression entityProjection
                    => entityProjection,

                ProjectionBindingExpression innerProjectionBindingExpression
                    => (EntityProjectionExpression)_selectExpression.Projection[innerProjectionBindingExpression.Index!.Value].Expression,

                UnaryExpression unaryExpression
                    => (EntityProjectionExpression)((UnaryExpression)unaryExpression.Operand).Operand,

                _ => throw new InvalidOperationException(CoreStrings.TranslationFailed(methodCallExpression.Print()))
            };

            Expression? navigationProjection;
            var navigation = _includedNavigations.FirstOrDefault(n => n.Name == memberName);
            if (navigation == null)
            {
                navigationProjection = innerEntityProjection.BindMember(
                    memberName, visitedSource.Type, clientEval: true, out var propertyBase);

                if (propertyBase is not INavigation projectedNavigation || !projectedNavigation.IsEmbedded())
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                navigation = projectedNavigation;
            }
            else
            {
                navigationProjection = innerEntityProjection.BindNavigation(navigation, clientEval: true);
            }

            switch (navigationProjection)
            {
                case StructuralTypeShaperExpression shaper when navigation.IsCollection:
                    var objectArrayAccessExpression = shaper.ValueBufferExpression as ObjectArrayAccessExpression;
                    Check.DebugAssert(objectArrayAccessExpression is not null, "Expected ObjectArrayAccessExpression");

                    var innerShaperExpression = new StructuralTypeShaperExpression(
                        navigation.TargetEntityType,
                        Expression.Convert(
                            Expression.Convert(objectArrayAccessExpression.InnerProjection, typeof(object)), typeof(ValueBuffer)),
                        nullable: true);

                    return new CollectionShaperExpression(
                        objectArrayAccessExpression,
                        innerShaperExpression,
                        navigation,
                        innerShaperExpression.StructuralType.ClrType);

                case StructuralTypeShaperExpression shaper:
                    return new StructuralTypeShaperExpression(
                        shaper.StructuralType,
                        Expression.Convert(Expression.Convert(shaper.ValueBufferExpression, typeof(object)), typeof(ValueBuffer)),
                        shaper.IsNullable);

                default:
                    throw new InvalidOperationException(CoreStrings.TranslationFailed(methodCallExpression.Print()));
            }
        }

        if (_clientEval)
        {
            var method = methodCallExpression.Method;
            if (method.DeclaringType == typeof(Queryable))
            {
                var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;
                var visitedSource = Visit(methodCallExpression.Arguments[0]);

                switch (method.Name)
                {
                    case nameof(Queryable.AsQueryable)
                        when genericMethod == QueryableMethods.AsQueryable:
                        // Unwrap AsQueryable
                        return visitedSource;

                    case nameof(Queryable.Select)
                        when genericMethod == QueryableMethods.Select:
                        if (visitedSource is not CollectionShaperExpression shaper)
                        {
                            return QueryCompilationContext.NotTranslatedExpression;
                        }

                        var lambda = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();

                        _collectionShaperMapping.Add(lambda.Parameters.Single(), shaper);

                        lambda = Expression.Lambda(Visit(lambda.Body), lambda.Parameters);
                        return Expression.Call(
                            EnumerableMethods.Select.MakeGenericMethod(method.GetGenericArguments()),
                            shaper,
                            lambda);
                }
            }
            else if (method is { Name: nameof(Enumerable.ToList), IsGenericMethod: true }
                     && method.DeclaringType == typeof(Enumerable)
                     && methodCallExpression.Arguments is [var argument]
                     && argument.Type.TryGetElementType(typeof(IQueryable<>)) != null)
            {
                if (_queryableMethodTranslatingExpressionVisitor.TranslateSubquery(argument) is not ShapedQueryExpression subquery
                    || !subquery.TryConvertToArray(_typeMappingSource, out var array))
                {
                    throw new InvalidOperationException(CoreStrings.TranslationFailed(methodCallExpression.Print()));
                }

                // If ToList() was composed over a subquery with operators, the result here is an ArrayExpression (ARRAY(SELECT ...)), whose
                // CLR Type is IEnumerable<T>. This can be directly used in the resulting ProjectingBindingExpression - the shaper will
                // simply read the JSON results out successfully.
                // But if ToList() is composed directly over an array property, that property could have type e.g. T[], which will be read
                // in the shaper, and then the cast from T[] to List<T> will fail. As a result, wrap the array in an additional
                // "reprojection" subquery, effectively to change the CLR type.
                if (array is SqlExpression scalarArray
                    && !(array.Type.IsGenericType && array.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    Check.DebugAssert(
                        array is not ScalarArrayExpression and not ObjectArrayExpression, "ArrayExpression should be IEnumerable");

                    if (scalarArray is not { TypeMapping.ElementTypeMapping: CosmosTypeMapping elementTypeMapping })
                    {
                        throw new UnreachableException("Scalar array with no element type mapping");
                    }

                    // TODO: Proper alias management (#33894).
                    var arrayReprojectionSubquery = SelectExpression.CreateForCollection(
                        array, "i", new ScalarReferenceExpression("i", elementTypeMapping.ClrType, elementTypeMapping));
                    arrayReprojectionSubquery.ApplyProjection();

                    array = new ScalarArrayExpression(
                        arrayReprojectionSubquery,
                        methodCallExpression.Type, // List<>
                        _typeMappingSource.FindMapping(methodCallExpression.Type, _model, elementTypeMapping));
                }

                return new ProjectionBindingExpression(
                    _selectExpression,
                    _selectExpression.AddToProjection(array),
                    methodCallExpression.Type);
            }
        }

        var @object = Visit(methodCallExpression.Object);
        var arguments = new Expression[methodCallExpression.Arguments.Count];
        for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
        {
            var argument = methodCallExpression.Arguments[i];
            arguments[i] = MatchTypes(Visit(argument), argument.Type);
        }

        Expression updatedMethodCallExpression = methodCallExpression.Update(
            @object != null ? MatchTypes(@object, methodCallExpression.Object!.Type) : @object,
            arguments);

        if (@object?.Type.IsNullableType() == true
            && !methodCallExpression.Object!.Type.IsNullableType())
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

        if (!_clientEval
            && newExpression.Members == null)
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
        => (T)queryContext.ParameterValues[parameterName]!;
}
