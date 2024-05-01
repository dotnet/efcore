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

                return New(
                    typeof(QueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                    Convert(
                        QueryCompilationContext.QueryContextParameter,
                        typeof(CosmosQueryContext)),
                    Constant(sqlExpressionFactory),
                    Constant(querySqlGeneratorFactory),
                    Constant(selectExpression),
                    Constant(shaperLambda.Compile()),
                    Constant(_contextType),
                    Constant(_partitionKeyValueFromExtension, typeof(PartitionKey)),
                    Constant(
                        QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                    Constant(_threadSafetyChecksEnabled));

            case ReadItemExpression readItemExpression:
                shaperBody = new CosmosProjectionBindingRemovingReadItemExpressionVisitor(
                        readItemExpression, jObjectParameter,
                        QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll)
                    .Visit(shaperBody);

                var shaperReadItemLambda = Lambda(
                    shaperBody,
                    QueryCompilationContext.QueryContextParameter,
                    jObjectParameter);

                return New(
                    typeof(ReadItemQueryingEnumerable<>).MakeGenericType(shaperReadItemLambda.ReturnType).GetConstructors()[0],
                    Convert(
                        QueryCompilationContext.QueryContextParameter,
                        typeof(CosmosQueryContext)),
                    Constant(readItemExpression),
                    Constant(shaperReadItemLambda.Compile()),
                    Constant(_contextType),
                    Constant(
                        QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                    Constant(_threadSafetyChecksEnabled));

            default:
                throw new NotSupportedException(CoreStrings.UnhandledExpressionNode(shapedQueryExpression.QueryExpression));
        }
    }
}
