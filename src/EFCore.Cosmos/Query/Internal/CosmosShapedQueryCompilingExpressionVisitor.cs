// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
    private int _currentStructuralIndex;
    private ParameterExpression _parentJObject;
    private CosmosProjectionBindingRemovingExpressionVisitor _projectionBindingRemovingExpressionVisitor;
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

        if (shapedQueryExpression.QueryExpression is not SelectExpression selectExpression)
        {
            throw new NotSupportedException(CoreStrings.UnhandledExpressionNode(shapedQueryExpression.QueryExpression));
        }

        _projectionBindingRemovingExpressionVisitor = new CosmosProjectionBindingRemovingExpressionVisitor(
                selectExpression, jTokenParameter,
                QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll);

        shaperBody = new JObjectInjectingExpressionVisitor().Visit(shaperBody);
        shaperBody = InjectStructuralTypeMaterializers(shaperBody);
        shaperBody = _projectionBindingRemovingExpressionVisitor.Visit(shaperBody);

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

        if (shaper.StructuralType is IEntityType entityType)
        {
            foreach (var navigation in entityType.GetNavigations())
            {
                var jObjectVariable = Parameter(typeof(JObject), "ownedJObject" + ++_currentStructuralIndex);
                var tempValueBuffer = new StructuralPropertyBindingExpression(jObjectVariable);

                if (navigation.IsCollection)
                {
                    var ordinalParameter = Parameter(typeof(int), "ordinal");
                    var materializeExpression = CreateStructuralCollectionPopulateExpression(jObjectVariable, navigation, navigation.TargetEntityType, navigation.TargetEntityType.GetContainingPropertyName(), ordinalParameter, tempValueBuffer);
                    _projectionBindingRemovingExpressionVisitor.AddInclude(jObjectVariable, tempValueBuffer, expressions, navigation, materializeExpression, instanceVariable, ordinalParameter);
                }
                else
                {
                    variables.Add(jObjectVariable);

                    var assignJObjectVariable = Assign(jObjectVariable,
                        Call(
                            CosmosProjectionBindingRemovingExpressionVisitorBase.ToObjectWithSerializerMethodInfo.MakeGenericMethod(typeof(JObject)),
                            Call(_parentJObject, CosmosProjectionBindingRemovingExpressionVisitorBase.GetItemMethodInfo,
                                Constant(navigation.TargetEntityType.GetContainingPropertyName()))));

                    expressions.Add(assignJObjectVariable);

                    var materializeExpression = CreateStructuralTypeMaterializeExpression(navigation.TargetEntityType, jObjectVariable, tempValueBuffer);

                    _projectionBindingRemovingExpressionVisitor.AddInclude(jObjectVariable, tempValueBuffer, expressions, navigation, materializeExpression, instanceVariable);
                }
            }
        }
    }

    private BlockExpression CreateComplexPropertyAssignmentBlock(MemberExpression memberExpression, IComplexProperty complexProperty)
    {
        var jObjectVariable = Parameter(typeof(JObject), "complexJObject" + ++_currentStructuralIndex);
        var assignJObjectVariable = Assign(jObjectVariable,
            Call(
                CosmosProjectionBindingRemovingExpressionVisitorBase.ToObjectWithSerializerMethodInfo.MakeGenericMethod(typeof(JObject)),
                Call(_parentJObject, CosmosProjectionBindingRemovingExpressionVisitorBase.GetItemMethodInfo,
                    Constant(complexProperty.GetJsonPropertyName()))));

        var materializeExpression = CreateStructuralTypeMaterializeExpression(complexProperty.ComplexType, jObjectVariable);
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

    private Expression CreateComplexCollectionAssignmentBlock(MemberExpression memberExpression, IComplexProperty complexProperty)
    {
        var jObjectParameter = Parameter(typeof(JObject), "strucutralJObject" + _currentStructuralIndex);
        var populateExpression = CreateStructuralCollectionPopulateExpression(jObjectParameter, complexProperty, complexProperty.ComplexType, complexProperty.GetJsonPropertyName());

        return memberExpression.Assign(populateExpression);
    }

    private BlockExpression CreateStructuralCollectionPopulateExpression(ParameterExpression jObjectParameter, IPropertyBase structuralProperty, ITypeBase structuralType, string jsonPropertyName, ParameterExpression ordinalParameter = null, StructuralPropertyBindingExpression tempValueBuffer = null)
    {
        var structuralJArrayVariable = Variable(
            typeof(JArray),
            "structuralJArray" + ++_currentStructuralIndex);

        var assignJArrayVariable = Assign(structuralJArrayVariable,
            Call(
                CosmosProjectionBindingRemovingExpressionVisitorBase.ToObjectWithSerializerMethodInfo.MakeGenericMethod(typeof(JArray)),
                Call(_parentJObject, CosmosProjectionBindingRemovingExpressionVisitorBase.GetItemMethodInfo,
                    Constant(jsonPropertyName))));

        var materializeExpression = CreateStructuralTypeMaterializeExpression(structuralType, jObjectParameter, tempValueBuffer);

        var select = Call(
                    EnumerableMethods.SelectWithOrdinal.MakeGenericMethod(typeof(JObject), structuralType.ClrType),
                    Call(
                        EnumerableMethods.Cast.MakeGenericMethod(typeof(JObject)),
                        structuralJArrayVariable),
                    Lambda(materializeExpression, jObjectParameter, ordinalParameter ?? Parameter(typeof(int))));

        var populateExpression =
            Call(
                CosmosProjectionBindingRemovingExpressionVisitorBase.PopulateCollectionMethodInfo.MakeGenericMethod(structuralType.ClrType, structuralProperty.ClrType),
                Constant(structuralProperty.GetCollectionAccessor()),
                select);

        //if (complexProperty.IsNullable)
        //{
        //    populateExpression = Condition(Equal(complexJArrayVariable, Constant(null)),
        //        Default(complexProperty.ClrType.MakeNullable()),
        //        ConvertChecked(populateExpression, complexProperty.ClrType.MakeNullable()));
        //}

        return Block([structuralJArrayVariable],
              [assignJArrayVariable, populateExpression]);
    }

    private Expression CreateStructuralTypeMaterializeExpression(ITypeBase structuralType, ParameterExpression jObjectParameter, StructuralPropertyBindingExpression tempValueBuffer = null)
    {
        tempValueBuffer ??= new StructuralPropertyBindingExpression(jObjectParameter);
        var structuralTypeShaperExpression = new StructuralTypeShaperExpression(
            structuralType,
            tempValueBuffer,
            false);

        var oldParentJObject = _parentJObject;
        _parentJObject = jObjectParameter;
        var materializeExpression = InjectStructuralTypeMaterializers(structuralTypeShaperExpression);
        _parentJObject = oldParentJObject;

        if (structuralType.ClrType.IsNullableType())
        {
            materializeExpression = Condition(Equal(jObjectParameter, Constant(null)),
                Default(structuralType.ClrType),
                materializeExpression);
        }

        return materializeExpression;
    }

    private sealed class StructuralPropertyBindingExpression(ParameterExpression jObjectParameter) : Expression
    {
        public override Type Type => typeof(ValueBuffer);

        public override ExpressionType NodeType => ExpressionType.Extension;

        public ParameterExpression JObjectParameter { get; } = jObjectParameter;
    }
}
