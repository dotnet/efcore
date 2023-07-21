// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerQueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
{
    private readonly OpenJsonPostprocessor _openJsonPostprocessor;
    private readonly SkipWithoutOrderByInSplitQueryVerifier _skipWithoutOrderByInSplitQueryVerifier = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerQueryTranslationPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext,
        IRelationalTypeMappingSource typeMappingSource)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _openJsonPostprocessor = new(typeMappingSource, relationalDependencies.SqlExpressionFactory);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression Process(Expression query)
    {
        query = base.Process(query);

        query = _openJsonPostprocessor.Process(query);
        _skipWithoutOrderByInSplitQueryVerifier.Visit(query);

        return query;
    }

    private sealed class SkipWithoutOrderByInSplitQueryVerifier : ExpressionVisitor
    {
        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case ShapedQueryExpression shapedQueryExpression:
                    Visit(shapedQueryExpression.ShaperExpression);
                    return shapedQueryExpression;

                case RelationalSplitCollectionShaperExpression relationalSplitCollectionShaperExpression:
                    foreach (var table in relationalSplitCollectionShaperExpression.SelectExpression.Tables)
                    {
                        Visit(table);
                    }

                    Visit(relationalSplitCollectionShaperExpression.InnerShaper);

                    return relationalSplitCollectionShaperExpression;

                case SelectExpression { Offset: not null, Orderings.Count: 0 }:
                    throw new InvalidOperationException(SqlServerStrings.SplitQueryOffsetWithoutOrderBy);

                case NonQueryExpression nonQueryExpression:
                    return nonQueryExpression;

                default:
                    return base.Visit(expression);
            }
        }
    }

    /// <summary>
    ///     Converts <see cref="SqlServerOpenJsonExpression" /> expressions with WITH (the default) to OPENJSON without WITH when an
    ///     ordering still exists on the [key] column, i.e. when the ordering of the original JSON array needs to be preserved
    ///     (e.g. limit/offset).
    /// </summary>
    private sealed class OpenJsonPostprocessor : ExpressionVisitor
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly Dictionary<(SqlServerOpenJsonExpression, string), RelationalTypeMapping> _castsToApply = new();

        public OpenJsonPostprocessor(IRelationalTypeMappingSource typeMappingSource, ISqlExpressionFactory sqlExpressionFactory)
            => (_typeMappingSource, _sqlExpressionFactory) = (typeMappingSource, sqlExpressionFactory);

        public Expression Process(Expression expression)
        {
            _castsToApply.Clear();
            return Visit(expression);
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case ShapedQueryExpression shapedQueryExpression:
                    return shapedQueryExpression.UpdateQueryExpression(Visit(shapedQueryExpression.QueryExpression));

                case SelectExpression selectExpression:
                {
                    var newTables = default(TableExpressionBase[]);
                    var appliedCasts = new List<(SqlServerOpenJsonExpression, string)>();

                    for (var i = 0; i < selectExpression.Tables.Count; i++)
                    {
                        var table = selectExpression.Tables[i];
                        if ((table is SqlServerOpenJsonExpression { ColumnInfos: not null }
                            or JoinExpressionBase { Table: SqlServerOpenJsonExpression { ColumnInfos: not null } })
                            && selectExpression.Orderings.Select(o => o.Expression)
                                .Concat(selectExpression.Projection.Select(p => p.Expression))
                                .Any(x => IsKeyColumn(x, table)))
                        {
                            // Remove the WITH clause from the OPENJSON expression
                            var openJsonExpression = (SqlServerOpenJsonExpression)((table as JoinExpressionBase)?.Table ?? table);
                            var newOpenJsonExpression = openJsonExpression.Update(
                                openJsonExpression.JsonExpression,
                                openJsonExpression.Path,
                                columnInfos: null);

                            TableExpressionBase newTable = table switch
                            {
                                InnerJoinExpression ij => ij.Update(newOpenJsonExpression, ij.JoinPredicate),
                                LeftJoinExpression lj => lj.Update(newOpenJsonExpression, lj.JoinPredicate),
                                CrossJoinExpression cj => cj.Update(newOpenJsonExpression),
                                CrossApplyExpression ca => ca.Update(newOpenJsonExpression),
                                OuterApplyExpression oa => oa.Update(newOpenJsonExpression),
                                _ => newOpenJsonExpression,
                            };

                            if (newTables is not null)
                            {
                                newTables[i] = newTable;
                            }
                            else if (!table.Equals(newTable))
                            {
                                newTables = new TableExpressionBase[selectExpression.Tables.Count];
                                for (var j = 0; j < i; j++)
                                {
                                    newTables[j] = selectExpression.Tables[j];
                                }

                                newTables[i] = newTable;
                            }

                            foreach (var column in openJsonExpression.ColumnInfos!)
                            {
                                var typeMapping = _typeMappingSource.FindMapping(column.StoreType);
                                Check.DebugAssert(
                                    typeMapping is not null,
                                    $"Could not find mapping for store type {column.StoreType} when converting OPENJSON/WITH");

                                // Binary data (varbinary) is stored in JSON as base64, which OPENJSON knows how to decode as long the type is
                                // specified in the WITH clause. We're now removing the WITH and applying a relational CAST, but that doesn't work
                                // for base64 data.
                                if (typeMapping is SqlServerByteArrayTypeMapping)
                                {
                                    throw new InvalidOperationException(SqlServerStrings.QueryingOrderedBinaryJsonCollectionsNotSupported);
                                }

                                _castsToApply.Add((newOpenJsonExpression, column.Name), typeMapping);
                                appliedCasts.Add((newOpenJsonExpression, column.Name));
                            }

                            continue;
                        }

                        if (newTables is not null)
                        {
                            newTables[i] = table;
                        }
                    }

                    // SelectExpression.Update always creates a new instance - we should avoid it when tables haven't changed
                    // see #31276
                    var newSelectExpression = newTables is not null
                        ? selectExpression.Update(
                            selectExpression.Projection,
                            newTables,
                            selectExpression.Predicate,
                            selectExpression.GroupBy,
                            selectExpression.Having,
                            selectExpression.Orderings,
                            selectExpression.Limit,
                            selectExpression.Offset)
                        : selectExpression;

                    // Record the OPENJSON expression and its projected column(s), along with the store type we just removed from the WITH
                    // clause. Then visit the select expression, adding a cast around the matching ColumnExpressions.
                    var result = base.Visit(newSelectExpression);

                    foreach (var appliedCast in appliedCasts)
                    {
                        _castsToApply.Remove(appliedCast);
                    }

                    return result;
                }

                case ColumnExpression columnExpression:
                {
                    return columnExpression.Table switch
                    {
                        SqlServerOpenJsonExpression openJsonTable
                            when _castsToApply.TryGetValue((openJsonTable, columnExpression.Name), out var typeMapping)
                            => _sqlExpressionFactory.Convert(columnExpression, columnExpression.Type, typeMapping),
                        JoinExpressionBase { Table: SqlServerOpenJsonExpression innerOpenJsonTable }
                            when _castsToApply.TryGetValue((innerOpenJsonTable, columnExpression.Name), out var innerTypeMapping)
                            => _sqlExpressionFactory.Convert(columnExpression, columnExpression.Type, innerTypeMapping),
                        _ => base.Visit(expression)
                    };
                }

                default:
                    return base.Visit(expression);
            }

            static bool IsKeyColumn(SqlExpression sqlExpression, TableExpressionBase table)
                => (sqlExpression is ColumnExpression { Name: "key", Table: var keyColumnTable }
                    && keyColumnTable == table)
                || (sqlExpression is SqlUnaryExpression { OperatorType: ExpressionType.Convert,
                    Operand: SqlExpression operand } && IsKeyColumn(operand, table));
        }
    }
}
