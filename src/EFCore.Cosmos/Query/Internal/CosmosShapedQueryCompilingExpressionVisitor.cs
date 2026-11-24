// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
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
    private delegate T Shaper<T>(QueryContext queryContext, ReadOnlyMemory<byte> data, int ordinal, out int bytesConsumed);

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

        var shaperBody = shapedQueryExpression.ShaperExpression;

        var (paging, maxItemCount, continuationToken, responseContinuationTokenLimitInKb) =
            (false, (SqlParameterExpression?)null, (SqlParameterExpression?)null, (SqlParameterExpression?)null);

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

        var shaperSelect = selectExpression;

        // Cosmos can order by the indexed path, but cannot order by the computed null discriminator.
        string? extremumKeyProjection = null;
        string? valueProjection = null;
        (string Alias, bool Ascending)[]? tieBreakers = null;
        if (cosmosQueryCompilationContext.IsMinMaxBy
            && shapedQueryExpression.ResultCardinality != ResultCardinality.Enumerable
            && selectExpression is
            {
                ReadItemInfo: null,
                Limit: SqlConstantExpression { Value: 1 },
                Offset: null or SqlConstantExpression { Value: 0 },
                Orderings.Count: >= 2
            })
        {
            var orderings = selectExpression.Orderings;
            var hasNullDiscriminator = orderings[0] is
                { IsAscending: true, Expression: SqlBinaryExpression
                    { OperatorType: ExpressionType.Equal, Left: ScalarAccessExpression discriminatorKey, Right: SqlConstantExpression { Value: null } } }
                && discriminatorKey.Equals(orderings[1].Expression);
            var keyIndex = hasNullDiscriminator ? 1 : 0;
            var keyOrdering = orderings[keyIndex];
            if (keyOrdering.Expression is ScalarAccessExpression key)
            {
                var sourceOrderings = orderings.Skip(keyIndex + 1).Where(o => !o.Expression.Equals(key)).ToArray();
                var compareTies = sourceOrderings.Length > 0 && sourceOrderings.All(o => IsScalarSortKey(o.Expression));
                var readCandidates = (hasNullDiscriminator && keyOrdering.IsAscending) || compareTies;
                selectExpression = selectExpression.Update(
                    [.. selectExpression.Sources], selectExpression.Predicate, [.. selectExpression.Projection],
                    compareTies ? [keyOrdering] : [keyOrdering, .. sourceOrderings], offset: null,
                    readCandidates ? null : selectExpression.Limit);
                if (readCandidates)
                {
                    var index = selectExpression.AddToProjection(key);
                    if (compareTies)
                    {
                        tieBreakers = sourceOrderings.Select(o =>
                        {
                            var projectionIndex = selectExpression.AddToProjection(o.Expression);
                            return (selectExpression.Projection[projectionIndex].Alias, o.IsAscending);
                        }).ToArray();
                    }

                    var projection = selectExpression.Projection[index];
                    extremumKeyProjection = projection.IsValueProjection ? string.Empty : projection.Alias;
                    if (!projection.IsValueProjection && shaperSelect.Projection is [{ IsValueProjection: true } original])
                    {
                        valueProjection = original.Alias;
                    }
                }
            }
        }

        VerifyNoClientConstant(shaperBody);

        // Because the shaper might process the data twice (duplicated shaper),
        // we pass the data as ROM, and the shaper will create a JsonReaderData where needed.
        var dataParameter = Parameter(typeof(ReadOnlyMemory<byte>), "data");
        // The shaper will always read the whole next json token/object, and will know how many bytes that was
        // we get the amount of bytes read from the shaper, so that we can advance the data in the QueryingEnumerable as needed, without having to scan the data twice (once for shaper, once for advancing the data).
        var bytesConsumedParameter = Parameter(typeof(int).MakeByRefType(), "bytesConsumed");
        var shaperLambda = new ShaperProcessingExpressionVisitor(this, shaperSelect, dataParameter, bytesConsumedParameter)
            .ProcessShaper(shaperBody);

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
                Constant(maxItemCount!.Name),
                Constant(continuationToken!.Name),
                Constant(responseContinuationTokenLimitInKb!.Name)),

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
                threadSafetyConstant,
                Constant(extremumKeyProjection, typeof(string)),
                Constant(valueProjection, typeof(string)),
                Constant(tieBreakers, typeof((string, bool)[])))
        };
    }

    private static bool IsScalarSortKey(SqlExpression expression)
    {
        var type = (expression.TypeMapping?.Converter?.ProviderClrType ?? expression.Type).UnwrapNullableType();
        return type.IsNumeric() || type.IsEnum || type == typeof(bool) || type == typeof(string)
            || type == typeof(Guid) || type == typeof(DateTime) || type == typeof(DateTimeOffset)
            || type == typeof(DateOnly) || type == typeof(TimeOnly) || type == typeof(TimeSpan);
    }

    private static PartitionKey GeneratePartitionKey(
        IEntityType rootEntityType,
        List<Expression> partitionKeyPropertyValues,
        IReadOnlyDictionary<string, object?> parameterValues)
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
}
