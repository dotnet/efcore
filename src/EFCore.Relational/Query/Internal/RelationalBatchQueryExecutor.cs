// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class RelationalBatchQueryExecutor
{
    private static readonly Expression<Func<DbDataReader, int[], int>> AffectedRowsReadFunc
        = (reader, _) => reader.GetInt32(0);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static BatchResultsReader Execute(
        DbContext context,
        IReadOnlyList<RelationalBatchElement> elements)
    {
        var (command, parameterObject, concurrencyDetector) = PrepareCommand(context, elements);

        using var _ = concurrencyDetector?.EnterCriticalSection();

        // When the queries were compiled for buffering (e.g. a retrying execution strategy is configured), all result
        // sets are buffered eagerly inside ExecuteReader, making the execution safe to retry.
        var dataReader = elements[0].QueryContext.ExecutionStrategy.Execute(
            (command, parameterObject),
            static (_, state) => state.command.ExecuteReader(state.parameterObject),
            null);

        return new BatchResultsReader(elements, dataReader);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static async Task<BatchResultsReader> ExecuteAsync(
        DbContext context,
        IReadOnlyList<RelationalBatchElement> elements,
        CancellationToken cancellationToken)
    {
        var (command, parameterObject, concurrencyDetector) = PrepareCommand(context, elements);

        foreach (var element in elements)
        {
            element.QueryContext.CancellationToken = cancellationToken;
        }

        using var _ = concurrencyDetector?.EnterCriticalSection();

        var dataReader = await elements[0].QueryContext.ExecutionStrategy.ExecuteAsync(
                (command, parameterObject),
                static (_, state, cancellationToken) => state.command.ExecuteReaderAsync(state.parameterObject, cancellationToken),
                null,
                cancellationToken)
            .ConfigureAwait(false);

        return new BatchResultsReader(elements, dataReader, cancellationToken);
    }

    private static (IRelationalCommand Command, RelationalCommandParameterObject ParameterObject, IConcurrencyDetector? ConcurrencyDetector)
        PrepareCommand(DbContext context, IReadOnlyList<RelationalBatchElement> elements)
    {
        if (elements.Count == 0)
        {
            throw new InvalidOperationException(RelationalStrings.BatchQueryEmpty);
        }

        var sqlGenerationHelper = context.GetService<ISqlGenerationHelper>();

        var firstQueryContext = elements[0].QueryContext;
        var statements = new string[elements.Count];
        var logStatements = new string[elements.Count];
        var parameters = new List<IRelationalParameter>();
        var parameterValues = new Dictionary<string, object?>();
        var queryCount = 0;
        var bufferingCount = 0;

        for (var i = 0; i < elements.Count; i++)
        {
            var element = elements[i];
            var queryContext = element.QueryContext;

            if (!element.IsNonQuery)
            {
                queryCount++;
                if (element.ReaderColumns != null)
                {
                    bufferingCount++;
                }
            }
            else if (sqlGenerationHelper.AffectedRowsCountStatement == null)
            {
                throw new InvalidOperationException(RelationalStrings.BatchQueryNonQueryNotSupported);
            }

            if (!ReferenceEquals(queryContext.Connection, firstQueryContext.Connection)
                || !ReferenceEquals(queryContext.Context, context))
            {
                throw new InvalidOperationException(RelationalStrings.BatchQueryDifferentContext);
            }

            // Parameter names are prefixed per element while the SQL is generated, so the parameters of different
            // elements never collide in the combined command and the generated SQL text is never rewritten.
            var parameterNamePrefix = GetParameterNamePrefix(i);
            var template = element.ResolveCommandTemplate(parameterNamePrefix);

            foreach (var parameter in template.Parameters)
            {
                parameters.Add(CloneParameter(parameter, parameterNamePrefix, queryContext.Parameters, parameterValues));
            }

            statements[i] = template.CommandText;
            logStatements[i] = template.LogCommandText;

            if (element.IsNonQuery)
            {
                // Surface the rows-affected count of the DML statement as a result set of its own, keeping result
                // sets aligned one-to-one with the batch's elements.
                var affectedRowsSuffix = sqlGenerationHelper.StatementTerminator
                    + Environment.NewLine
                    + sqlGenerationHelper.AffectedRowsCountStatement;
                statements[i] += affectedRowsSuffix;
                logStatements[i] += affectedRowsSuffix;
            }
        }

        // Either all queries were compiled for buffering (e.g. a retrying execution strategy is configured on the
        // context) or none were; a mix would require buffering some result sets while streaming others on one reader.
        if (bufferingCount > 0 && bufferingCount != queryCount)
        {
            throw new InvalidOperationException(RelationalStrings.BatchQueryBufferingNotSupported);
        }

        // A batch consisting only of non-queries has no compiled reader columns to go by, so it buffers whenever the
        // execution strategy retries. Rows-affected result sets are buffered with a synthetic single-int column.
        var buffering = queryCount == 0
            ? firstQueryContext.ExecutionStrategy.RetriesOnFailure
            : bufferingCount > 0;

        IReadOnlyList<ReaderColumn?>? readerColumns = null;
        if (buffering)
        {
            var affectedRowsColumns = new[] { ReaderColumn.Create(typeof(int), nullable: false, null, null, AffectedRowsReadFunc) };
            readerColumns = new BatchReaderColumns(
                [.. elements.Select(e => e.ReaderColumns ?? affectedRowsColumns)]);
        }

        var separator = sqlGenerationHelper.StatementTerminator + Environment.NewLine + Environment.NewLine;

        var command = new RelationalCommand(
            context.GetService<RelationalCommandBuilderDependencies>(),
            string.Join(separator, statements),
            string.Join(separator, logStatements),
            parameters);

        var parameterObject = new RelationalCommandParameterObject(
            firstQueryContext.Connection,
            parameterValues,
            readerColumns,
            context,
            firstQueryContext.CommandLogger,
            elements.FirstOrDefault(e => !e.IsNonQuery)?.DetailedErrorsEnabled ?? false,
            CommandSource.LinqQuery);

        var concurrencyDetector = elements[0].ThreadSafetyChecksEnabled
            ? firstQueryContext.ConcurrencyDetector
            : null;

        return (command, parameterObject, concurrencyDetector);
    }

    private static string GetParameterNamePrefix(int elementIndex)
        => $"b{elementIndex}_";

    // The parameter names in the template were already prefixed during SQL generation; only the invariant names,
    // which key the parameter values, still need to be made unique across the batch.
    private static IRelationalParameter CloneParameter(
        IRelationalParameter parameter,
        string parameterNamePrefix,
        IReadOnlyDictionary<string, object?> sourceValues,
        Dictionary<string, object?> targetValues)
    {
        switch (parameter)
        {
            case TypeMappedRelationalParameter typeMapped:
            {
                var newInvariantName = parameterNamePrefix + typeMapped.InvariantName;
                targetValues[newInvariantName] = sourceValues[typeMapped.InvariantName];

                return new TypeMappedRelationalParameter(
                    newInvariantName,
                    typeMapped.Name,
                    typeMapped.RelationalTypeMapping,
                    typeMapped.IsNullable,
                    typeMapped.Direction);
            }

            case CompositeRelationalParameter composite:
            {
                foreach (var subParameter in composite.RelationalParameters)
                {
                    if (subParameter is not TypeMappedRelationalParameter)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.BatchQueryUnsupportedParameter(subParameter.GetType().ShortDisplayName()));
                    }
                }

                var newInvariantName = parameterNamePrefix + composite.InvariantName;
                targetValues[newInvariantName] = sourceValues[composite.InvariantName];

                return new CompositeRelationalParameter(newInvariantName, composite.RelationalParameters);
            }

            default:
                throw new InvalidOperationException(
                    RelationalStrings.BatchQueryUnsupportedParameter(parameter.GetType().ShortDisplayName()));
        }
    }
}
