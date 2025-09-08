// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGQuerySqlGenerator : QuerySqlGenerator
    {
        // The order in which the types are specified matters, because types get matched by using StartsWith.
        private static readonly Dictionary<string, string[]> _castMappings = new Dictionary<string, string[]>
        {
            { "signed", new []{ "tinyint", "smallint", "mediumint", "int", "bigint", "bit" }},
            { "decimal(65,30)", new []{ "decimal" } },
            { "double", new []{ "double" } },
            { "float", new []{ "float" } },
            { "binary", new []{ "binary", "varbinary", "tinyblob", "blob", "mediumblob", "longblob" } },
            { "datetime(6)", new []{ "datetime(6)" } },
            { "datetime(5)", new []{ "datetime(5)" } },
            { "datetime(4)", new []{ "datetime(4)" } },
            { "datetime(3)", new []{ "datetime(3)" } },
            { "datetime(2)", new []{ "datetime(2)" } },
            { "datetime(1)", new []{ "datetime(1)" } },
            { "datetime", new []{ "datetime" } },
            { "date", new []{ "date" } },
            { "timestamp(6)", new []{ "timestamp(6)" } },
            { "timestamp(5)", new []{ "timestamp(5)" } },
            { "timestamp(4)", new []{ "timestamp(4)" } },
            { "timestamp(3)", new []{ "timestamp(3)" } },
            { "timestamp(2)", new []{ "timestamp(2)" } },
            { "timestamp(1)", new []{ "timestamp(1)" } },
            { "timestamp", new []{ "timestamp" } },
            { "time(6)", new []{ "time(6)" } },
            { "time(5)", new []{ "time(5)" } },
            { "time(4)", new []{ "time(4)" } },
            { "time(3)", new []{ "time(3)" } },
            { "time(2)", new []{ "time(2)" } },
            { "time(1)", new []{ "time(1)" } },
            { "time", new []{ "time" } },
            { "json", new []{ "json" } },
            { "char", new []{ "char", "varchar", "text", "tinytext", "mediumtext", "longtext" } },
            { "nchar", new []{ "nchar", "nvarchar" } },
        };

        private const ulong LimitUpperBound = 18446744073709551610;

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly IXGOptions _options;
        private string _removeTableAliasOld;
        private string _removeTableAliasNew;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XGQuerySqlGenerator([NotNull] QuerySqlGeneratorDependencies dependencies,
            IRelationalTypeMappingSource typeMappingSource,
            [CanBeNull] IXGOptions options)
            : base(dependencies)
        {
            _typeMappingSource = typeMappingSource;
            _options = options;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression switch
            {
                XGJsonTraversalExpression jsonTraversalExpression => VisitJsonPathTraversal(jsonTraversalExpression),
                XGColumnAliasReferenceExpression columnAliasReferenceExpression => VisitColumnAliasReference(columnAliasReferenceExpression),
                XGJsonTableExpression jsonTableExpression => VisitJsonTableExpression(jsonTableExpression),
                XGInlinedParameterExpression inlinedParameterExpression => VisitInlinedParameterExpression(inlinedParameterExpression),
                _ => base.VisitExtension(extensionExpression)
            };

        private Expression VisitColumnAliasReference(XGColumnAliasReferenceExpression columnAliasReferenceExpression)
        {
            Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(columnAliasReferenceExpression.Alias));

            return columnAliasReferenceExpression;
        }

        private Expression VisitInlinedParameterExpression(XGInlinedParameterExpression inlinedParameterExpression)
        {
            Visit(inlinedParameterExpression.ValueExpression);

            return inlinedParameterExpression;
        }

        protected virtual Expression VisitJsonPathTraversal(XGJsonTraversalExpression expression)
        {
            // If the path contains parameters, then the -> and ->> aliases are not supported by MySQL, because
            // we need to concatenate the path and the parameters.
            // We will use JSON_EXTRACT (and JSON_UNQUOTE if needed) only in this case, because the aliases
            // are much more readable.
            var isSimplePath = expression.Path.All(
                l => l is SqlConstantExpression ||
                     l is XGJsonArrayIndexExpression e && e.Expression is SqlConstantExpression);

            if (expression.ReturnsText)
            {
                Sql.Append("JSON_UNQUOTE(");
            }

            if (expression.Path.Count > 0)
            {
                Sql.Append("JSON_EXTRACT(");
            }

            Visit(expression.Expression);

            if (expression.Path.Count > 0)
            {
                Sql.Append(", ");

                if (!isSimplePath)
                {
                    Sql.Append("CONCAT(");
                }

                Sql.Append("'$");

                foreach (var location in expression.Path)
                {
                    if (location is XGJsonArrayIndexExpression arrayIndexExpression)
                    {
                        var isConstantExpression = arrayIndexExpression.Expression is SqlConstantExpression;

                        Sql.Append("[");

                        if (!isConstantExpression)
                        {
                            Sql.Append("', ");
                        }

                        Visit(arrayIndexExpression.Expression);

                        if (!isConstantExpression)
                        {
                            Sql.Append(", '");
                        }

                        Sql.Append("]");
                    }
                    else
                    {
                        Sql.Append(".");
                        Visit(location);
                    }
                }

                Sql.Append("'");

                if (!isSimplePath)
                {
                    Sql.Append(")");
                }

                Sql.Append(")");
            }

            if (expression.ReturnsText)
            {
                Sql.Append(")");
            }

            return expression;
        }

        protected override Expression VisitColumn(ColumnExpression columnExpression)
        {
            if (_removeTableAliasOld is not null &&
                columnExpression.TableAlias == _removeTableAliasOld)
            {
                if (_removeTableAliasNew is not null)
                {
                    Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(_removeTableAliasNew))
                        .Append(".");
                }

                Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(columnExpression.Name));

                return columnExpression;
            }

            return base.VisitColumn(columnExpression);
        }

        protected override Expression VisitTable(TableExpression tableExpression)
        {
            if (_removeTableAliasOld is not null &&
                tableExpression.Alias == _removeTableAliasOld)
            {
                if (_removeTableAliasNew is not null)
                {
                    Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(_removeTableAliasNew))
                        .Append(AliasSeparator);
                }

                Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(tableExpression.Name));

                return tableExpression;
            }

            return base.VisitTable(tableExpression);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Limit != null)
            {
                Sql.AppendLine().Append("LIMIT ");
                Visit(selectExpression.Limit);
            }

            if (selectExpression.Offset != null)
            {
                if (selectExpression.Limit == null)
                {
                    // if we want to use Skip() without Take() we have to define the upper limit of LIMIT
                    Sql.AppendLine().Append($"LIMIT {LimitUpperBound}");
                }

                Sql.Append(" OFFSET ");
                Visit(selectExpression.Offset);
            }
        }

        protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            if (sqlFunctionExpression.Name.StartsWith("@@", StringComparison.Ordinal))
            {
                Sql.Append(sqlFunctionExpression.Name);

                return sqlFunctionExpression;
            }

            return base.VisitSqlFunction(sqlFunctionExpression);
        }

        protected override Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
        {
            Sql.Append("JOIN ");

            if (crossApplyExpression.Table is not TableExpression)
            {
                Sql.Append("LATERAL ");
            }

            Visit(crossApplyExpression.Table);

            Sql.Append(" ON TRUE");

            return crossApplyExpression;
        }

        protected override Expression VisitOuterApply(OuterApplyExpression outerApplyExpression)
        {
            Sql.Append("LEFT JOIN ");

            if (outerApplyExpression.Table is not TableExpression &&
                outerApplyExpression.Table is not XGJsonTableExpression)
            {
                Sql.Append("LATERAL ");
            }

            Visit(outerApplyExpression.Table);

            Sql.Append(" ON TRUE");

            return outerApplyExpression;
        }

        protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
        {
            Check.NotNull(sqlBinaryExpression, nameof(sqlBinaryExpression));

            if (sqlBinaryExpression.OperatorType == ExpressionType.Add &&
                sqlBinaryExpression.Type == typeof(string) &&
                sqlBinaryExpression.Left.TypeMapping?.ClrType == typeof(string) &&
                sqlBinaryExpression.Right.TypeMapping?.ClrType == typeof(string))
            {
                Sql.Append("CONCAT(");
                Visit(sqlBinaryExpression.Left);
                Sql.Append(", ");
                Visit(sqlBinaryExpression.Right);
                Sql.Append(")");

                return sqlBinaryExpression;
            }

            var requiresBrackets = RequiresBrackets(sqlBinaryExpression.Left);

            if (requiresBrackets)
            {
                Sql.Append("(");
            }

            Visit(sqlBinaryExpression.Left);

            if (requiresBrackets)
            {
                Sql.Append(")");
            }

            Sql.Append(GetOperator(sqlBinaryExpression));

            // EF uses unary Equal and NotEqual to represent is-null checking.
            // These need to be surrounded with parenthesis in various cases (e.g. where TRUE = x IS NOT NULL).
            // See https://github.com/PomeloFoundation/Microsoft.EntityFrameworkCore.XuGu/issues/1309
            requiresBrackets = RequiresBrackets(sqlBinaryExpression.Right) ||
                               !requiresBrackets &&
                               sqlBinaryExpression.Right is SqlUnaryExpression sqlUnaryExpression &&
                               (sqlUnaryExpression.OperatorType == ExpressionType.Equal || sqlUnaryExpression.OperatorType == ExpressionType.NotEqual);

            if (requiresBrackets)
            {
                Sql.Append("(");
            }

            Visit(sqlBinaryExpression.Right);

            if (requiresBrackets)
            {
                Sql.Append(")");
            }

            return sqlBinaryExpression;
        }

        protected override Expression VisitDelete(DeleteExpression deleteExpression)
        {
            var selectExpression = deleteExpression.SelectExpression;

            if (selectExpression.Offset == null
                && selectExpression.Having == null
                && selectExpression.GroupBy.Count == 0
                && selectExpression.Projection.Count == 0
                && (selectExpression.Tables.Count == 1 || selectExpression.Orderings.Count == 0 && selectExpression.Limit is null))
            {
                var removeSingleTableAlias = selectExpression.Tables.Count == 1 &&
                                             selectExpression.Orderings.Count > 0 || selectExpression.Limit is not null;

                Sql.Append($"DELETE");

                if (!removeSingleTableAlias)
                {
                    Sql.Append($" {Dependencies.SqlGenerationHelper.DelimitIdentifier(deleteExpression.Table.Alias)}");
                }

                Sql.AppendLine().Append("FROM ");

                if (removeSingleTableAlias)
                {
                    _removeTableAliasOld = selectExpression.Tables[0].Alias;
                    _removeTableAliasNew = null;
                }

                GenerateList(selectExpression.Tables, e => Visit(e), sql => sql.AppendLine());

                if (selectExpression.Predicate != null)
                {
                    Sql.AppendLine().Append("WHERE ");

                    Visit(selectExpression.Predicate);
                }

                GenerateOrderings(selectExpression);
                GenerateLimitOffset(selectExpression);

                if (removeSingleTableAlias)
                {
                    _removeTableAliasOld = null;
                }

                return deleteExpression;
            }

            throw new InvalidOperationException(
                RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(nameof(EntityFrameworkQueryableExtensions.ExecuteDelete)));
        }

        protected override Expression VisitUpdate(UpdateExpression updateExpression)
        {
            var selectExpression = updateExpression.SelectExpression;

            if (selectExpression.Offset == null
                && selectExpression.Having == null
                && selectExpression.Orderings.Count == 0
                && selectExpression.GroupBy.Count == 0
                && selectExpression.Projection.Count == 0)
            {
                Sql.Append("UPDATE ");

                if (selectExpression.Tables.Count > 1)
                {
                    var tables = selectExpression.Tables;

                    if (selectExpression.Tables.All(t => !updateExpression.Table.Equals(t is JoinExpressionBase join ? join.Table : t)))
                    {
                        Visit(updateExpression.Table);
                        Sql.AppendLine(",");

                        if (tables[0] is not JoinExpressionBase)
                        {
                            tables = tables
                                .Skip(1)
                                .Prepend(new CrossJoinExpression(tables[0]))
                                .ToArray();
                        }
                    }

                    GenerateList(tables, e => Visit(e), sql => sql.AppendLine());
                }
                else
                {
                    Visit(updateExpression.Table);
                }

                Sql.AppendLine().Append("SET ");
                Visit(updateExpression.ColumnValueSetters[0].Column);
                Sql.Append(" = ");
                Visit(updateExpression.ColumnValueSetters[0].Value);

                using (Sql.Indent())
                {
                    foreach (var columnValueSetter in updateExpression.ColumnValueSetters.Skip(1))
                    {
                        Sql.AppendLine(",");
                        Visit(columnValueSetter.Column);
                        Sql.Append(" = ");
                        Visit(columnValueSetter.Value);
                    }
                }

                if (selectExpression.Predicate != null)
                {
                    Sql.AppendLine().Append("WHERE ");
                    Visit(selectExpression.Predicate);
                }

                GenerateLimitOffset(selectExpression);

                return updateExpression;
            }

            throw new InvalidOperationException(
                RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(nameof(EntityFrameworkQueryableExtensions.ExecuteUpdate)));
        }

        protected override Expression VisitJsonScalar(JsonScalarExpression jsonScalarExpression)
        {
            // TODO: Stop producing empty JsonScalarExpressions, #30768
            var path = jsonScalarExpression.Path;
            if (path.Count == 0)
            {
                return jsonScalarExpression;
            }

            var jsonPathNeedsConcat = JsonPathNeedsConcat(jsonScalarExpression.Path);
            var useJsonValue = !jsonPathNeedsConcat && _options.ServerVersion.Supports.JsonValue;
            var jsonFunctionName = useJsonValue ? "JSON_VALUE" : "JSON_UNQUOTE(JSON_EXTRACT";
            string castStoreType = null;

            // if (/*jsonScalarExpression.TypeMapping is SqlServerJsonTypeMapping
            //     ||*/ jsonScalarExpression.TypeMapping?.ElementTypeMapping is not null)
            // {
            //     jsonFunctionName = "JSON_UNQUOTE(JSON_EXTRACT";
            // }
            // else
            // {
            //     // JSON_VALUE returns varchar(512) by default (https://dev.mysql.com/doc/refman/8.0/en/json-search-functions.html#function_json-value),
            //     // so we let it cast the result to the expected type using the RETURNING clause.
            //     // CHECK: - except if it's a string (since the cast interferes with indexes over the JSON property).
            //     // if (jsonScalarExpression.TypeMapping is not StringTypeMapping)
            //     // {
            //         castStoreType = GetCastStoreType(jsonScalarExpression.TypeMapping);
            //     // }
            //
            //     jsonFunctionName = "JSON_VALUE";
            // }

            // if (jsonScalarExpression.TypeMapping?.ElementTypeMapping is null &&
            //     jsonScalarExpression.TypeMapping is not StringTypeMapping &&
            //     jsonPathNeedsConcat)
            // {
                castStoreType = GetCastStoreType(jsonScalarExpression.TypeMapping);
            // }

            if (castStoreType is not null)
            {
                Sql.Append("CAST(");
            }

            Sql.Append(jsonFunctionName);
            Sql.Append("(");

            Visit(jsonScalarExpression.Json);

            Sql.Append(", ");
            GenerateJsonPath(jsonScalarExpression.Path);
            Sql.Append(useJsonValue ? ")" : "))");

            if (castStoreType is not null)
            {
                Sql.Append(" AS ");
                Sql.Append(castStoreType);
                Sql.Append(")");
            }

            return jsonScalarExpression;
        }

        protected override void GenerateValues(ValuesExpression valuesExpression)
        {
            if (valuesExpression.RowValues is null)
            {
                throw new UnreachableException();
            }

            if (_options.ServerVersion.Supports.Values ||
                _options.ServerVersion.Supports.ValuesWithRows)
            {
                base.GenerateValues(valuesExpression);
                return;
            }

            var rowValues = valuesExpression.RowValues;

            //
            // Use backwards compatible SELECT statements:
            //

            Sql.Append("SELECT ");

            Check.DebugAssert(rowValues.Count > 0, "rowValues.Count > 0");
            var firstRowValues = rowValues[0].Values;
            for (var i = 0; i < firstRowValues.Count; i++)
            {
                if (i > 0)
                {
                    Sql.Append(", ");
                }

                Visit(firstRowValues[i]);

                Sql
                    .Append(AliasSeparator)
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(valuesExpression.ColumnNames[i]));
            }

            if (rowValues.Count > 1)
            {
                for (var r = 1; r < rowValues.Count; r++)
                {
                    Sql.Append(" UNION ALL SELECT ");
                    Visit(rowValues[r]);
                }
            }
        }

        protected override Expression VisitRowValue(RowValueExpression rowValueExpression)
        {
            if (_options.ServerVersion.Supports.Values)
            {
                return base.VisitRowValue(rowValueExpression);
            }

            if (_options.ServerVersion.Supports.ValuesWithRows)
            {
                Sql.Append("ROW");
                return base.VisitRowValue(rowValueExpression);
            }

            //
            // Columns for backwards compatible SELECT statement:
            //

            var values = rowValueExpression.Values;
            var count = values.Count;
            for (var i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    Sql.Append(", ");
                }

                Visit(values[i]);
            }

            return rowValueExpression;
        }

        protected virtual void GenerateList<T>(
            IReadOnlyList<T> items,
            Action<T> generationAction,
            Action<IRelationalCommandBuilder> joinAction = null)
        {
            joinAction ??= (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(Sql);
                }

                generationAction(items[i]);
            }
        }

        private static bool RequiresBrackets(SqlExpression expression)
            => expression is SqlBinaryExpression
               || expression is LikeExpression
               || (expression is SqlUnaryExpression unary
                   && unary.Operand.Type == typeof(bool)
                   && (unary.OperatorType == ExpressionType.Equal
                       || unary.OperatorType == ExpressionType.NotEqual));

        public virtual Expression VisitXGRegexp(XGRegexpExpression xgRegexpExpression)
        {
            Check.NotNull(xgRegexpExpression, nameof(xgRegexpExpression));

            Visit(xgRegexpExpression.Match);
            Sql.Append(" REGEXP ");
            Visit(xgRegexpExpression.Pattern);

            return xgRegexpExpression;
        }

        public virtual Expression VisitXGMatch(XGMatchExpression xgMatchExpression)
        {
            Check.NotNull(xgMatchExpression, nameof(xgMatchExpression));

            Sql.Append("MATCH ");
            Sql.Append("(");
            Visit(xgMatchExpression.Match);
            Sql.Append(")");
            Sql.Append(" AGAINST ");
            Sql.Append("(");
            Visit(xgMatchExpression.Against);

            switch (xgMatchExpression.SearchMode)
            {
                case XGMatchSearchMode.NaturalLanguage:
                    break;
                case XGMatchSearchMode.NaturalLanguageWithQueryExpansion:
                    Sql.Append(" WITH QUERY EXPANSION");
                    break;
                case XGMatchSearchMode.Boolean:
                    Sql.Append(" IN BOOLEAN MODE");
                    break;
            }

            Sql.Append(")");

            return xgMatchExpression;
        }

        protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
            => sqlUnaryExpression.OperatorType == ExpressionType.Convert
                ? VisitConvert(sqlUnaryExpression)
                : base.VisitSqlUnary(sqlUnaryExpression);

        private SqlUnaryExpression VisitConvert(SqlUnaryExpression sqlUnaryExpression)
        {
            var castMapping = GetCastStoreType(sqlUnaryExpression.TypeMapping);

            if (castMapping == "binary")
            {
                Sql.Append("UNHEX(HEX(");
                Visit(sqlUnaryExpression.Operand);
                Sql.Append("))");
                return sqlUnaryExpression;
            }

            // There needs to be no CAST() applied between the exact same store type. This could happen, e.g. if
            // `System.DateTime` and `System.DateTimeOffset` are used in conjunction, because both use different type
            // mappings, but map to the same store type (e.g. `datetime(6)`).
            //
            // There also is no need for a double CAST() to the same type. Due to only rudimentary CAST() support in
            // MySQL, the final store type of a CAST() operation might be different than the store type of the type
            // mapping of the expression (e.g. "float" will be cast to "double"). So we optimize these cases too.
            //
            // An exception is the JSON data type, when used in conjunction with a parameter (like `JsonDocument`).
            // JSON parameters like that will be serialized to string and supplied as a string parameter to MySQL
            // (at least this seems to be the case currently with XuguClient). To make assignments and comparisons
            // between JSON columns and JSON parameters (supplied as string) work, the string needs to be explicitly
            // converted to JSON.

            var sameInnerCastStoreType = sqlUnaryExpression.Operand is SqlUnaryExpression operandUnary &&
                                         operandUnary.OperatorType == ExpressionType.Convert &&
                                         castMapping.Equals(GetCastStoreType(operandUnary.TypeMapping), StringComparison.OrdinalIgnoreCase);

            if (castMapping == "json" && !_options.ServerVersion.Supports.JsonDataTypeEmulation ||
                !castMapping.Equals(sqlUnaryExpression.Operand.TypeMapping.StoreType, StringComparison.OrdinalIgnoreCase) &&
                !sameInnerCastStoreType)
            {
                var useDecimalToDoubleWorkaround = false;

                if (castMapping.StartsWith("double", StringComparison.OrdinalIgnoreCase) &&
                    !_options.ServerVersion.Supports.DoubleCast)
                {
                    useDecimalToDoubleWorkaround = true;
                    castMapping = "decimal(65,30)";
                }

                if (useDecimalToDoubleWorkaround)
                {
                    Sql.Append("(");
                }

                Sql.Append("CAST(");
                Visit(sqlUnaryExpression.Operand);
                Sql.Append(" AS ");
                Sql.Append(castMapping);
                Sql.Append(")");

                // FLOAT and DOUBLE are supported by CAST() as of MySQL 8.0.17.
                // For server versions before that, a workaround is applied, that casts to a DECIMAL,
                // that is then added to 0e0, which results in a DOUBLE.
                // REF: https://dev.mysql.com/doc/refman/8.0/en/number-literals.html
                if (useDecimalToDoubleWorkaround)
                {
                    Sql.Append(" + 0e0)");
                }
            }
            else
            {
                Visit(sqlUnaryExpression.Operand);
            }

            return sqlUnaryExpression;
        }

        private string GetCastStoreType(RelationalTypeMapping typeMapping)
        {
            var storeTypeLower = typeMapping.StoreType.ToLower();
            string castMapping = null;
            foreach (var kvp in _castMappings)
            {
                foreach (var storeType in kvp.Value)
                {
                    if (storeTypeLower.StartsWith(storeType, StringComparison.OrdinalIgnoreCase))
                    {
                        castMapping = kvp.Key;
                        break;
                    }
                }

                if (castMapping != null)
                {
                    break;
                }
            }

            if (castMapping == null)
            {
                throw new InvalidOperationException($"Cannot cast from type '{typeMapping.StoreType}'");
            }

            if (castMapping == "signed" && storeTypeLower.Contains("unsigned"))
            {
                castMapping = "unsigned";
            }

            // As of MySQL 8.0.18, a FLOAT cast might unnecessarily drop decimal places and round,
            // so we just keep casting to double instead. XuguClient ensures, that a System.Single
            // will be returned if expected, even if we return a DOUBLE.
            if (castMapping.StartsWith("float", StringComparison.OrdinalIgnoreCase) &&
                !_options.ServerVersion.Supports.FloatCast)
            {
                castMapping = "double";
            }

            return castMapping;
        }

        public virtual Expression VisitXGComplexFunctionArgumentExpression(XGComplexFunctionArgumentExpression xgComplexFunctionArgumentExpression)
        {
            Check.NotNull(xgComplexFunctionArgumentExpression, nameof(xgComplexFunctionArgumentExpression));

            var first = true;
            foreach (var argument in xgComplexFunctionArgumentExpression.ArgumentParts)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Sql.Append(xgComplexFunctionArgumentExpression.Delimiter);
                }

                Visit(argument);
            }

            return xgComplexFunctionArgumentExpression;
        }

        public virtual Expression VisitXGCollateExpression(XGCollateExpression xgCollateExpression)
        {
            Check.NotNull(xgCollateExpression, nameof(xgCollateExpression));

            Sql.Append("CONVERT(");

            Visit(xgCollateExpression.ValueExpression);

            Sql.Append($" USING {xgCollateExpression.Charset}) COLLATE {xgCollateExpression.Collation}");

            return xgCollateExpression;
        }

        public virtual Expression VisitXGBinaryExpression(XGBinaryExpression xgBinaryExpression)
        {
            if (xgBinaryExpression.OperatorType == XGBinaryExpressionOperatorType.NonOptimizedEqual)
            {
                var equalExpression = new SqlBinaryExpression(
                    ExpressionType.Equal,
                    xgBinaryExpression.Left,
                    xgBinaryExpression.Right,
                    xgBinaryExpression.Type,
                    xgBinaryExpression.TypeMapping);

                Visit(equalExpression);
            }
            else
            {
                Sql.Append("(");
                Visit(xgBinaryExpression.Left);
                Sql.Append(")");

                switch (xgBinaryExpression.OperatorType)
                {
                    case XGBinaryExpressionOperatorType.IntegerDivision:
                        Sql.Append(" DIV ");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Sql.Append("(");
                Visit(xgBinaryExpression.Right);
                Sql.Append(")");
            }

            return xgBinaryExpression;
        }

        protected virtual Expression VisitJsonTableExpression(XGJsonTableExpression jsonTableExpression)
        {
            // if (jsonTableExpression.ColumnInfos is not { Count: > 0 })
            // {
            //     var hasStringElement = jsonTableExpression.JsonExpression.TypeMapping?.ElementTypeMapping?.ClrType == typeof(string);
            //
            //     if (hasStringElement)
            //     {
            //         Sql.Append("JSON_UNQUOTE(");
            //     }
            //
            //     Sql.Append("JSON_EXTRACT(");
            //     Visit(jsonTableExpression.JsonExpression);
            //     Sql.Append(", ");
            //     GenerateJsonPath(jsonTableExpression.Path);
            //     Sql.Append(")");
            //
            //     if (hasStringElement)
            //     {
            //         Sql.Append(")");
            //     }
            //
            //     return jsonTableExpression;
            // }

            Sql.Append("JSON_TABLE(");

            Visit(jsonTableExpression.JsonExpression);

            Sql.Append(", ");
            GenerateJsonPath(jsonTableExpression.Path);

            if (jsonTableExpression.ColumnInfos is not { Count: > 0 })
            {
                throw new InvalidOperationException("JSON_TABLE expression does not contain any columns.");
            }

            Sql.AppendLine(" COLUMNS (");

            using (var _ = Sql.Indent())
            {
                Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier("key"));
                Sql.AppendLine(" FOR ORDINALITY,");

                for (var i = 0; i < jsonTableExpression.ColumnInfos.Count; i++)
                {
                    var columnInfo = jsonTableExpression.ColumnInfos[i];

                    if (i > 0)
                    {
                        Sql.AppendLine(",");
                    }

                    GenerateColumnInfo(columnInfo);
                }

                Sql.AppendLine();
            }

            Sql.Append(")");

            void GenerateColumnInfo(XGJsonTableExpression.ColumnInfo columnInfo)
            {
                Sql
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(columnInfo.Name))
                    .Append(" ")
                    .Append(columnInfo.TypeMapping.StoreType);

                if (columnInfo.Path is not null)
                {
                    Sql.Append(" PATH ");
                    GenerateJsonPath(columnInfo.Path);
                }

                // if (columnInfo.AsJson)
                // {
                //     Sql.Append(" AS ").Append("JSON");
                // }
            }

            Sql.Append(")");
            Sql.Append(AliasSeparator).Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(jsonTableExpression.Alias));

            return jsonTableExpression;
        }

        protected virtual bool JsonPathNeedsConcat(IReadOnlyList<PathSegment> path)
            => path.Any(s => s.ArrayIndex is not null && s.ArrayIndex is not SqlConstantExpression);

        protected virtual void GenerateJsonPath(IReadOnlyList<PathSegment> path, bool? needsConcat = null)
        {
            path ??= Array.Empty<PathSegment>();
            needsConcat ??= JsonPathNeedsConcat(path);

            if (needsConcat.Value)
            {
                Sql.Append("CONCAT(");
            }

            Sql.Append("'$");

            foreach (var pathSegment in path)
            {
                switch (pathSegment)
                {
                    case { PropertyName: string propertyName }:
                        Sql.Append(".").Append(propertyName);
                        break;

                    case { ArrayIndex: SqlExpression arrayIndex }:
                        Sql.Append("[");

                        if (arrayIndex is SqlConstantExpression)
                        {
                            Visit(arrayIndex);
                        }
                        else
                        {
                            Sql.Append("', ");

                            Visit(
                                new SqlUnaryExpression(
                                    ExpressionType.Convert,
                                    arrayIndex,
                                    typeof(string),
                                    _typeMappingSource.GetMapping(typeof(string))));

                            Sql.Append(", '");
                        }

                        Sql.Append("]");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Sql.Append("'");

            if (needsConcat.Value)
            {
                Sql.Append(")");
            }
        }

        /// <inheritdoc />
        protected override void CheckComposableSql(string sql)
        {
            // MySQL supports CTE (WITH) expressions within subqueries, as well as others,
            // so we allow any raw SQL to be composed over.
        }
    }
}
