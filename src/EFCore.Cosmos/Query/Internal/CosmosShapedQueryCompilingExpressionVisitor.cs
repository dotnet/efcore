// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

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
    private readonly Type _contextType = cosmosQueryCompilationContext.ContextType;
    private readonly bool _threadSafetyChecksEnabled = dependencies.CoreSingletonOptions.AreThreadSafetyChecksEnabled;

    private readonly PartitionKey _partitionKeyValueFromExtension = cosmosQueryCompilationContext.PartitionKeyValueFromExtension
        ?? PartitionKey.None;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
    {
        if (cosmosQueryCompilationContext.CosmosContainer is null)
        {
            throw new UnreachableException("No Cosmos container was set during query processing.");
        }

        var jObjectParameter = Parameter(typeof(JObject), "jObject");

        var shaperBody = shapedQueryExpression.ShaperExpression;
        shaperBody = new JObjectInjectingExpressionVisitor().Visit(shaperBody);
        shaperBody = InjectEntityMaterializers(shaperBody);

        switch (shapedQueryExpression.QueryExpression)
        {
            case SelectExpression selectExpression:
                shaperBody = new CosmosProjectionBindingRemovingExpressionVisitor(
                        selectExpression, jObjectParameter,
                        QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll)
                    .Visit(shaperBody);

                var shaperLambda = Lambda(
                    shaperBody,
                    QueryCompilationContext.QueryContextParameter,
                    jObjectParameter);

                var cosmosQueryContextConstant = Convert(QueryCompilationContext.QueryContextParameter, typeof(CosmosQueryContext));
                var shaperConstant = Constant(shaperLambda.Compile());
                var contextTypeConstant = Constant(_contextType);
                var containerConstant = Constant(cosmosQueryCompilationContext.CosmosContainer);
                var threadSafetyConstant = Constant(_threadSafetyChecksEnabled);
                var standAloneStateManagerConstant = Constant(
                    QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution);

                return selectExpression.ReadItemInfo != null
                    ? New(
                        typeof(ReadItemQueryingEnumerable<>).MakeGenericType(selectExpression.ReadItemInfo.Type).GetConstructors()[0],
                        cosmosQueryContextConstant,
                        containerConstant,
                        Constant(selectExpression.ReadItemInfo),
                        shaperConstant,
                        contextTypeConstant,
                        standAloneStateManagerConstant,
                        threadSafetyConstant)
                    : New(
                        typeof(QueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                        cosmosQueryContextConstant,
                        Constant(sqlExpressionFactory),
                        Constant(querySqlGeneratorFactory),
                        Constant(selectExpression),
                        shaperConstant,
                        contextTypeConstant,
                        containerConstant,
                        Constant(_partitionKeyValueFromExtension, typeof(PartitionKey)),
                        standAloneStateManagerConstant,
                        threadSafetyConstant);

            default:
                throw new NotSupportedException(CoreStrings.UnhandledExpressionNode(shapedQueryExpression.QueryExpression));
        }
    }
}
