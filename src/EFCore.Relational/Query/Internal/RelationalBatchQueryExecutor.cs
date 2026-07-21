// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
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

            var template = element.ResolveCommandTemplate();
            var renameMap = new Dictionary<string, string>();

            foreach (var parameter in template.Parameters)
            {
                parameters.Add(CloneParameter(parameter, i, queryContext.Parameters, parameterValues, renameMap));
            }

            statements[i] = RewriteParameterPlaceholders(template.CommandText, renameMap);
            logStatements[i] = RewriteParameterPlaceholders(template.LogCommandText, renameMap);

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
        // Rows-affected result sets of non-query elements are buffered with a synthetic single-int column.
        IReadOnlyList<ReaderColumn?>? readerColumns = null;
        if (bufferingCount > 0 && bufferingCount == queryCount)
        {
            var affectedRowsColumns = new[] { ReaderColumn.Create(typeof(int), nullable: false, null, null, AffectedRowsReadFunc) };
            readerColumns = new BatchReaderColumns(
                [.. elements.Select(e => e.ReaderColumns ?? affectedRowsColumns)]);
        }
        else if (bufferingCount > 0)
        {
            throw new InvalidOperationException(RelationalStrings.BatchQueryBufferingNotSupported);
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

    private static IRelationalParameter CloneParameter(
        IRelationalParameter parameter,
        int queryIndex,
        IReadOnlyDictionary<string, object?> sourceValues,
        Dictionary<string, object?> targetValues,
        Dictionary<string, string> renameMap)
    {
        switch (parameter)
        {
            case TypeMappedRelationalParameter typeMapped:
            {
                var newInvariantName = RenameInvariantName(typeMapped.InvariantName, queryIndex);
                targetValues[newInvariantName] = sourceValues[typeMapped.InvariantName];

                return new TypeMappedRelationalParameter(
                    newInvariantName,
                    RenameParameterName(typeMapped.Name, queryIndex, renameMap),
                    typeMapped.RelationalTypeMapping,
                    typeMapped.IsNullable,
                    typeMapped.Direction);
            }

            case CompositeRelationalParameter composite:
            {
                var subParameters = new List<IRelationalParameter>(composite.RelationalParameters.Count);
                foreach (var subParameter in composite.RelationalParameters)
                {
                    if (subParameter is not TypeMappedRelationalParameter subTypeMapped)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.BatchQueryUnsupportedParameter(subParameter.GetType().ShortDisplayName()));
                    }

                    subParameters.Add(
                        new TypeMappedRelationalParameter(
                            RenameInvariantName(subTypeMapped.InvariantName, queryIndex),
                            RenameParameterName(subTypeMapped.Name, queryIndex, renameMap),
                            subTypeMapped.RelationalTypeMapping,
                            subTypeMapped.IsNullable,
                            subTypeMapped.Direction));
                }

                var newInvariantName = RenameInvariantName(composite.InvariantName, queryIndex);
                targetValues[newInvariantName] = sourceValues[composite.InvariantName];

                return new CompositeRelationalParameter(newInvariantName, subParameters);
            }

            default:
                throw new InvalidOperationException(
                    RelationalStrings.BatchQueryUnsupportedParameter(parameter.GetType().ShortDisplayName()));
        }
    }

    private static string RenameInvariantName(string invariantName, int queryIndex)
        => $"b{queryIndex}_{invariantName}";

    private static string RenameParameterName(string name, int queryIndex, Dictionary<string, string> renameMap)
    {
        var hasPrefix = name.StartsWith('@');
        var bareName = hasPrefix ? name[1..] : name;
        var newBareName = $"b{queryIndex}_{bareName}";

        renameMap["@" + bareName] = "@" + newBareName;

        return hasPrefix ? "@" + newBareName : newBareName;
    }

    private static string RewriteParameterPlaceholders(string commandText, Dictionary<string, string> renameMap)
    {
        if (renameMap.Count == 0)
        {
            return commandText;
        }

        // A trailing lookahead ensures a placeholder never matches a prefix of a longer placeholder (e.g. "@p1" in "@p10");
        // the regex engine backtracks into the alternation until the full placeholder matches. A parameter placeholder
        // appearing inside a SQL string literal would be rewritten too - not something LINQ-generated SQL produces.
        var pattern = "(?:" + string.Join("|", renameMap.Keys.Select(Regex.Escape)) + ")(?![A-Za-z0-9_$@])";

        return Regex.Replace(commandText, pattern, match => renameMap[match.Value]);
    }
}
