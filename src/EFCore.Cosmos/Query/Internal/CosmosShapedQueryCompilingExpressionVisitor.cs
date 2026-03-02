// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json.Linq;
using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public partial class CosmosShapedQueryCompilingExpressionVisitor(
    ShapedQueryCompilingExpressionVisitorDependencies dependencies,
    CosmosQueryCompilationContext cosmosQueryCompilationContext,
    ISqlExpressionFactory sqlExpressionFactory,
    IQuerySqlGeneratorFactory querySqlGeneratorFactory)
    : ShapedQueryCompilingExpressionVisitor(dependencies, cosmosQueryCompilationContext)
{
    private int _currentComplexIndex;
    private ParameterExpression _parentJObject;
    private readonly Type _contextType = cosmosQueryCompilationContext.ContextType;
    private readonly bool _threadSafetyChecksEnabled = dependencies.CoreSingletonOptions.AreThreadSafetyChecksEnabled;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
    {
        if (cosmosQueryCompilationContext.RootEntityType is not { } rootEntityType)
        {
            throw new UnreachableException("No root entity type was set during query processing.");
        }

        var jTokenParameter = Parameter(typeof(JToken), "jToken");
        _parentJObject = jTokenParameter;

        var shaperBody = shapedQueryExpression.ShaperExpression;

        var (paging, maxItemCount, continuationToken, responseContinuationTokenLimitInKb) =
            (false, (SqlParameterExpression)null, (SqlParameterExpression)null, (SqlParameterExpression)null);

        // If the query is terminated ToPageAsync(), CosmosQueryableMethodTranslatingExpressionVisitor composed a PagingExpression on top
        // of the shaper. We remove that to get the shaper for each actual document being read (as opposed to the page of those documents),
        // and extract the pagination arguments.
        if (shaperBody is PagingExpression pagingExpression)
        {
            paging = true;
            maxItemCount = pagingExpression.MaxItemCount;
            continuationToken = pagingExpression.ContinuationToken;
            responseContinuationTokenLimitInKb = pagingExpression.ResponseContinuationTokenLimitInKb;

            shaperBody = pagingExpression.Expression;
        }

        shaperBody = new JObjectInjectingExpressionVisitor().Visit(shaperBody);
        shaperBody = InjectStructuralTypeMaterializers(shaperBody);

        if (shapedQueryExpression.QueryExpression is not SelectExpression selectExpression)
        {
            throw new NotSupportedException(CoreStrings.UnhandledExpressionNode(shapedQueryExpression.QueryExpression));
        }

        shaperBody = new CosmosProjectionBindingRemovingExpressionVisitor(
                selectExpression, jTokenParameter,
                QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            .Visit(shaperBody);

        var shaperLambda = Lambda(
            shaperBody,
            QueryCompilationContext.QueryContextParameter,
            jTokenParameter);

        var cosmosQueryContextConstant = Convert(QueryCompilationContext.QueryContextParameter, typeof(CosmosQueryContext));
        var shaperConstant = Constant(shaperLambda.Compile());
        var contextTypeConstant = Constant(_contextType);
        var rootEntityTypeConstant = Constant(rootEntityType);
        var threadSafetyConstant = Constant(_threadSafetyChecksEnabled);
        var standAloneStateManagerConstant = Constant(
            QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution);

        Check.DebugAssert(!paging || selectExpression.ReadItemInfo is null, "ReadItem is being with paging, impossible.");

        return selectExpression switch
        {
            { ReadItemInfo: { } readItemInfo } => New(
                typeof(ReadItemQueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                cosmosQueryContextConstant,
                rootEntityTypeConstant,
                Constant(cosmosQueryCompilationContext.PartitionKeyPropertyValues),
                Constant(readItemInfo),
                shaperConstant,
                contextTypeConstant,
                standAloneStateManagerConstant,
                threadSafetyConstant),

            _ when paging => New(
                typeof(PagingQueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                cosmosQueryContextConstant,
                Constant(sqlExpressionFactory),
                Constant(querySqlGeneratorFactory),
                Constant(selectExpression),
                shaperConstant,
                contextTypeConstant,
                rootEntityTypeConstant,
                Constant(cosmosQueryCompilationContext.PartitionKeyPropertyValues),
                standAloneStateManagerConstant,
                threadSafetyConstant,
                Constant(maxItemCount.Name),
                Constant(continuationToken.Name),
                Constant(responseContinuationTokenLimitInKb.Name)),

            _ => New(
                typeof(QueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0], cosmosQueryContextConstant,
                Constant(sqlExpressionFactory),
                Constant(querySqlGeneratorFactory),
                Constant(selectExpression),
                shaperConstant,
                contextTypeConstant,
                rootEntityTypeConstant,
                Constant(cosmosQueryCompilationContext.PartitionKeyPropertyValues),
                standAloneStateManagerConstant,
                threadSafetyConstant)
        };
    }

    private static PartitionKey GeneratePartitionKey(
        IEntityType rootEntityType,
        List<Expression> partitionKeyPropertyValues,
        IReadOnlyDictionary<string, object> parameterValues)
    {
        if (partitionKeyPropertyValues.Count == 0)
        {
            return PartitionKey.None;
        }

        var builder = new PartitionKeyBuilder();

        var partitionKeyProperties = rootEntityType.GetPartitionKeyProperties();

        for (var i = 0; i < partitionKeyPropertyValues.Count && i < partitionKeyProperties.Count; i++)
        {
            var property = partitionKeyProperties[i];

            switch (partitionKeyPropertyValues[i])
            {
                case SqlConstantExpression constant:
                    builder.Add(constant.Value, property);
                    continue;

                case SqlParameterExpression parameter:
                {
                    builder.Add(
                        parameterValues.TryGetValue(parameter.Name, out var value)
                            ? value
                            : throw new UnreachableException("Couldn't find partition key parameter value"),
                        property);
                    continue;
                }

                default:
                    throw new UnreachableException();
            }
        }

        return builder.Build();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void AddStructuralTypeInitialization(StructuralTypeShaperExpression shaper, ParameterExpression instanceVariable, List<ParameterExpression> variables, List<Expression> expressions)
    {
        foreach (var complexProperty in shaper.StructuralType.GetComplexProperties())
        {
            var member = MakeMemberAccess(instanceVariable, complexProperty.GetMemberInfo(true, true));
            expressions.Add(complexProperty.IsCollection
                ? CreateComplexCollectionAssignmentBlock(member, complexProperty)
                : CreateComplexPropertyAssignmentBlock(member, complexProperty));
        }
    }

    private BlockExpression CreateComplexPropertyAssignmentBlock(MemberExpression memberExpression, IComplexProperty complexProperty)
    {
        var jObjectVariable = Parameter(typeof(JObject), "complexJObject" + ++_currentComplexIndex);
        var assignJObjectVariable = Assign(jObjectVariable,
            Call(
                CosmosProjectionBindingRemovingExpressionVisitorBase.ToObjectWithSerializerMethodInfo.MakeGenericMethod(typeof(JObject)),
                Call(_parentJObject, CosmosProjectionBindingRemovingExpressionVisitorBase.GetItemMethodInfo,
                    Constant(complexProperty.Name))));

        var materializeExpression = CreateComplexTypeMaterializeExpression(complexProperty, jObjectVariable);
        if (complexProperty.IsNullable)
        {
            materializeExpression = Condition(Equal(jObjectVariable, Constant(null)),
                Default(complexProperty.ClrType.MakeNullable()),
                ConvertChecked(materializeExpression, complexProperty.ClrType.MakeNullable()));
        }

        return Block(
            [jObjectVariable],
            [
                assignJObjectVariable,
                memberExpression.Assign(materializeExpression)
            ]
        );
    }

    private BlockExpression CreateComplexCollectionAssignmentBlock(MemberExpression memberExpression, IComplexProperty complexProperty)
    {
        var complexJArrayVariable = Variable(
            typeof(JArray),
            "complexJArray" + ++_currentComplexIndex);

        var assignJArrayVariable = Assign(complexJArrayVariable,
            Call(
                CosmosProjectionBindingRemovingExpressionVisitorBase.ToObjectWithSerializerMethodInfo.MakeGenericMethod(typeof(JArray)),
                Call(_parentJObject, CosmosProjectionBindingRemovingExpressionVisitorBase.GetItemMethodInfo,
                    Constant(complexProperty.Name))));

        var jObjectParameter = Parameter(typeof(JObject), "complexJObject" + _currentComplexIndex);
        var materializeExpression = CreateComplexTypeMaterializeExpression(complexProperty, jObjectParameter);

        var select = Call(
                    EnumerableMethods.Select.MakeGenericMethod(typeof(JObject), complexProperty.ComplexType.ClrType),
                    Call(
                        EnumerableMethods.Cast.MakeGenericMethod(typeof(JObject)),
                        complexJArrayVariable),
                    Lambda(materializeExpression, jObjectParameter));

        Expression populateExpression =
            Call(
                CosmosProjectionBindingRemovingExpressionVisitorBase.PopulateCollectionMethodInfo.MakeGenericMethod(complexProperty.ComplexType.ClrType, complexProperty.ClrType),
                Constant(complexProperty.GetCollectionAccessor()),
                select);

        if (complexProperty.IsNullable)
        {
            populateExpression = Condition(Equal(complexJArrayVariable, Constant(null)),
                Default(complexProperty.ClrType.MakeNullable()),
                ConvertChecked(populateExpression, complexProperty.ClrType.MakeNullable()));
        }

        return Block(
            [complexJArrayVariable],
            [
                assignJArrayVariable,
                memberExpression.Assign(populateExpression)
            ]
        );
    }

    private Expression CreateComplexTypeMaterializeExpression(IComplexProperty complexProperty, ParameterExpression jObjectParameter)
    {
        var tempValueBuffer = new ComplexPropertyBindingExpression(complexProperty, jObjectParameter);
        var structuralTypeShaperExpression = new StructuralTypeShaperExpression(
            complexProperty.ComplexType,
            tempValueBuffer,
            false);

        var oldParentJObject = _parentJObject;
        _parentJObject = jObjectParameter;
        var materializeExpression = InjectStructuralTypeMaterializers(structuralTypeShaperExpression);
        _parentJObject = oldParentJObject;

        if (complexProperty.ComplexType.ClrType.IsNullableType())
        {
            materializeExpression = Condition(Equal(jObjectParameter, Constant(null)),
                Default(complexProperty.ComplexType.ClrType),
                materializeExpression);
        }

        return materializeExpression;
    }

    private sealed class ComplexPropertyBindingExpression(IComplexProperty complexProperty, ParameterExpression jObjectParameter) : Expression
    {
        public override Type Type => typeof(ValueBuffer);

        public override ExpressionType NodeType => ExpressionType.Extension;

        public IComplexProperty ComplexProperty { get; } = complexProperty;
        public ParameterExpression JObjectParameter { get; } = jObjectParameter;
    }
}
