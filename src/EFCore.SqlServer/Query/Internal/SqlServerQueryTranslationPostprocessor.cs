// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

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

                case SelectExpression
                {
                    Tables: [SqlServerOpenJsonExpression { ColumnInfos: not null } openJsonExpression, ..],
                    Orderings:
                    [
                        {
                            Expression: SqlUnaryExpression
                            {
                                OperatorType: ExpressionType.Convert,
                                Operand: ColumnExpression { Name: "key", Table: var keyColumnTable }
                            }
                        }
                    ]
                } selectExpression
                    when keyColumnTable == openJsonExpression:
                {
                    // Remove the WITH clause from the OPENJSON expression
                    var newOpenJsonExpression = openJsonExpression.Update(
                        openJsonExpression.JsonExpression,
                        openJsonExpression.Path,
                        columnInfos: null);

                    var newTables = selectExpression.Tables.ToArray();
                    newTables[0] = newOpenJsonExpression;

                    var newSelectExpression = selectExpression.Update(
                        selectExpression.Projection,
                        newTables,
                        selectExpression.Predicate,
                        selectExpression.GroupBy,
                        selectExpression.Having,
                        selectExpression.Orderings,
                        selectExpression.Limit,
                        selectExpression.Offset);

                    // Record the OPENJSON expression and its projected column(s), along with the store type we just removed from the WITH
                    // clause. Then visit the select expression, adding a cast around the matching ColumnExpressions.
                    // TODO: Need to pass through the type mapping API for converting the JSON value (nvarchar) to the relational store type
                    // (e.g. datetime2), see #30677
                    foreach (var column in openJsonExpression.ColumnInfos)
                    {
                        var typeMapping = _typeMappingSource.FindMapping(column.StoreType);
                        Check.DebugAssert(
                            typeMapping is not null,
                            $"Could not find mapping for store type {column.StoreType} when converting OPENJSON/WITH");

                        _castsToApply.Add((newOpenJsonExpression, column.Name), typeMapping);
                    }

                    var result = base.Visit(newSelectExpression);

                    foreach (var column in openJsonExpression.ColumnInfos)
                    {
                        _castsToApply.Remove((newOpenJsonExpression, column.Name));
                    }

                    return result;
                }

                case ColumnExpression { Table: SqlServerOpenJsonExpression openJsonTable, Name: var name } columnExpression
                    when _castsToApply.TryGetValue((openJsonTable, name), out var typeMapping):
                {
                    return _sqlExpressionFactory.Convert(columnExpression, columnExpression.Type, typeMapping);
                }

                default:
                    return base.Visit(expression);
            }
        }
    }
}
