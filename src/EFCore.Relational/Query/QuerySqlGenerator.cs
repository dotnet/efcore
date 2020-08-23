// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A query SQL generator to get <see cref="IRelationalCommand" /> for given <see cref="SelectExpression" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class QuerySqlGenerator : SqlExpressionVisitor
    {
        private readonly IRelationalCommandBuilderFactory _relationalCommandBuilderFactory;
        private readonly ISqlGenerationHelper _sqlGenerationHelper;
        private IRelationalCommandBuilder _relationalCommandBuilder;

        private static readonly Dictionary<ExpressionType, string> _operatorMap = new Dictionary<ExpressionType, string>
        {
            { ExpressionType.Equal, " = " },
            { ExpressionType.NotEqual, " <> " },
            { ExpressionType.GreaterThan, " > " },
            { ExpressionType.GreaterThanOrEqual, " >= " },
            { ExpressionType.LessThan, " < " },
            { ExpressionType.LessThanOrEqual, " <= " },
            { ExpressionType.AndAlso, " AND " },
            { ExpressionType.OrElse, " OR " },
            { ExpressionType.Add, " + " },
            { ExpressionType.Subtract, " - " },
            { ExpressionType.Multiply, " * " },
            { ExpressionType.Divide, " / " },
            { ExpressionType.Modulo, " % " },
            { ExpressionType.And, " & " },
            { ExpressionType.Or, " | " }
        };

        /// <summary>
        ///     Creates a new instance of the <see cref="QuerySqlGenerator" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        public QuerySqlGenerator([NotNull] QuerySqlGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;

            _relationalCommandBuilderFactory = dependencies.RelationalCommandBuilderFactory;
            _sqlGenerationHelper = dependencies.SqlGenerationHelper;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual QuerySqlGeneratorDependencies Dependencies { get; }

        /// <summary>
        ///     Gets a relational command for a <see cref="SelectExpression" />.
        /// </summary>
        /// <param name="selectExpression"> A select expression to print in command text. </param>
        /// <returns> A relational command with a SQL represented by the select expression. </returns>
        public virtual IRelationalCommand GetCommand([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            _relationalCommandBuilder = _relationalCommandBuilderFactory.Create();

            GenerateTagsHeaderComment(selectExpression);

            if (selectExpression.IsNonComposedFromSql())
            {
                GenerateFromSql((FromSqlExpression)selectExpression.Tables[0]);
            }
            else
            {
                VisitSelect(selectExpression);
            }

            return _relationalCommandBuilder.Build();
        }

        /// <summary>
        ///     The default alias separator.
        /// </summary>
        protected virtual string AliasSeparator { get; } = " AS ";

        /// <summary>
        ///     The current SQL command builder.
        /// </summary>
        protected virtual IRelationalCommandBuilder Sql
            => _relationalCommandBuilder;

        /// <summary>
        ///     Generates the head comment for tags.
        /// </summary>
        /// <param name="selectExpression"> A select expression to generate tags for. </param>
        protected virtual void GenerateTagsHeaderComment([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Tags.Count > 0)
            {
                foreach (var tag in selectExpression.Tags)
                {
                    _relationalCommandBuilder
                        .AppendLines(_sqlGenerationHelper.GenerateComment(tag))
                        .AppendLine();
                }
            }
        }

        /// <inheritdoc />
        protected override Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression)
        {
            Check.NotNull(sqlFragmentExpression, nameof(sqlFragmentExpression));

            _relationalCommandBuilder.Append(sqlFragmentExpression.Sql);

            return sqlFragmentExpression;
        }

        private bool IsNonComposedSetOperation(SelectExpression selectExpression)
            => selectExpression.Offset == null
                && selectExpression.Limit == null
                && !selectExpression.IsDistinct
                && selectExpression.Predicate == null
                && selectExpression.Having == null
                && selectExpression.Orderings.Count == 0
                && selectExpression.GroupBy.Count == 0
                && selectExpression.Tables.Count == 1
                && selectExpression.Tables[0] is SetOperationBase setOperation
                && selectExpression.Projection.Count == setOperation.Source1.Projection.Count
                && selectExpression.Projection.Select(
                        (pe, index) => pe.Expression is ColumnExpression column
                            && string.Equals(column.Table.Alias, setOperation.Alias, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(
                                column.Name, setOperation.Source1.Projection[index].Alias, StringComparison.OrdinalIgnoreCase))
                    .All(e => e);

        /// <inheritdoc />
        protected override Expression VisitSelect(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (IsNonComposedSetOperation(selectExpression))
            {
                // Naked set operation
                GenerateSetOperation((SetOperationBase)selectExpression.Tables[0]);

                return selectExpression;
            }

            IDisposable subQueryIndent = null;

            if (selectExpression.Alias != null)
            {
                _relationalCommandBuilder.AppendLine("(");
                subQueryIndent = _relationalCommandBuilder.Indent();
            }

            _relationalCommandBuilder.Append("SELECT ");

            if (selectExpression.IsDistinct)
            {
                _relationalCommandBuilder.Append("DISTINCT ");
            }

            GenerateTop(selectExpression);

            if (selectExpression.Projection.Any())
            {
                GenerateList(selectExpression.Projection, e => Visit(e));
            }
            else
            {
                _relationalCommandBuilder.Append("1");
            }

            if (selectExpression.Tables.Any())
            {
                _relationalCommandBuilder.AppendLine().Append("FROM ");

                GenerateList(selectExpression.Tables, e => Visit(e), sql => sql.AppendLine());
            }
            else
            {
                GeneratePseudoFromClause();
            }

            if (selectExpression.Predicate != null)
            {
                _relationalCommandBuilder.AppendLine().Append("WHERE ");

                Visit(selectExpression.Predicate);
            }

            if (selectExpression.GroupBy.Count > 0)
            {
                _relationalCommandBuilder.AppendLine().Append("GROUP BY ");

                GenerateList(selectExpression.GroupBy, e => Visit(e));
            }

            if (selectExpression.Having != null)
            {
                _relationalCommandBuilder.AppendLine().Append("HAVING ");

                Visit(selectExpression.Having);
            }

            GenerateOrderings(selectExpression);
            GenerateLimitOffset(selectExpression);

            if (selectExpression.Alias != null)
            {
                subQueryIndent.Dispose();

                _relationalCommandBuilder.AppendLine()
                    .Append(")" + AliasSeparator + _sqlGenerationHelper.DelimitIdentifier(selectExpression.Alias));
            }

            return selectExpression;
        }

        /// <summary>
        ///     Generates a pseudo FROM clause. Required by some providers when a query has no actual FROM clause.
        /// </summary>
        protected virtual void GeneratePseudoFromClause()
        {
        }

        /// <inheritdoc />
        protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            Check.NotNull(projectionExpression, nameof(projectionExpression));

            Visit(projectionExpression.Expression);

            if (!string.Equals(string.Empty, projectionExpression.Alias)
                && !(projectionExpression.Expression is ColumnExpression column
                    && string.Equals(column.Name, projectionExpression.Alias)))
            {
                _relationalCommandBuilder.Append(AliasSeparator + _sqlGenerationHelper.DelimitIdentifier(projectionExpression.Alias));
            }

            return projectionExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            Check.NotNull(sqlFunctionExpression, nameof(sqlFunctionExpression));

            if (sqlFunctionExpression.IsBuiltIn)
            {
                if (sqlFunctionExpression.Instance != null)
                {
                    Visit(sqlFunctionExpression.Instance);
                    _relationalCommandBuilder.Append(".");
                }

                _relationalCommandBuilder.Append(sqlFunctionExpression.Name);
            }
            else
            {
                if (!string.IsNullOrEmpty(sqlFunctionExpression.Schema))
                {
                    _relationalCommandBuilder
                        .Append(_sqlGenerationHelper.DelimitIdentifier(sqlFunctionExpression.Schema))
                        .Append(".");
                }

                _relationalCommandBuilder
                    .Append(_sqlGenerationHelper.DelimitIdentifier(sqlFunctionExpression.Name));
            }

            if (!sqlFunctionExpression.IsNiladic)
            {
                _relationalCommandBuilder.Append("(");
                GenerateList(sqlFunctionExpression.Arguments, e => Visit(e));
                _relationalCommandBuilder.Append(")");
            }

            return sqlFunctionExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitTableValuedFunction(TableValuedFunctionExpression tableValuedFunctionExpression)
        {
            Check.NotNull(tableValuedFunctionExpression, nameof(tableValuedFunctionExpression));

            if (!string.IsNullOrEmpty(tableValuedFunctionExpression.StoreFunction.Schema))
            {
                _relationalCommandBuilder
                    .Append(_sqlGenerationHelper.DelimitIdentifier(tableValuedFunctionExpression.StoreFunction.Schema))
                    .Append(".");
            }

            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableValuedFunctionExpression.StoreFunction.Name))
                .Append("(");

            GenerateList(tableValuedFunctionExpression.Arguments, e => Visit(e));

            _relationalCommandBuilder
                .Append(")")
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableValuedFunctionExpression.Alias));

            return tableValuedFunctionExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitColumn(ColumnExpression columnExpression)
        {
            Check.NotNull(columnExpression, nameof(columnExpression));

            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.Table.Alias))
                .Append(".")
                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.Name));

            return columnExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitTable(TableExpression tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Name, tableExpression.Schema))
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }

        private void GenerateFromSql(FromSqlExpression fromSqlExpression)
        {
            var sql = fromSqlExpression.Sql;
            string[] substitutions = null;

            switch (fromSqlExpression.Arguments)
            {
                case ConstantExpression constantExpression
                    when constantExpression.Value is CompositeRelationalParameter compositeRelationalParameter:
                {
                    var subParameters = compositeRelationalParameter.RelationalParameters;
                    substitutions = new string[subParameters.Count];
                    for (var i = 0; i < subParameters.Count; i++)
                    {
                        substitutions[i] = _sqlGenerationHelper.GenerateParameterNamePlaceholder(subParameters[i].InvariantName);
                    }

                    _relationalCommandBuilder.AddParameter(compositeRelationalParameter);

                    break;
                }

                case ConstantExpression constantExpression
                    when constantExpression.Value is object[] constantValues:
                {
                    substitutions = new string[constantValues.Length];
                    for (var i = 0; i < constantValues.Length; i++)
                    {
                        var value = constantValues[i];
                        if (value is RawRelationalParameter rawRelationalParameter)
                        {
                            substitutions[i] = _sqlGenerationHelper.GenerateParameterNamePlaceholder(rawRelationalParameter.InvariantName);
                            _relationalCommandBuilder.AddParameter(rawRelationalParameter);
                        }
                        else if (value is SqlConstantExpression sqlConstantExpression)
                        {
                            substitutions[i] = sqlConstantExpression.TypeMapping.GenerateSqlLiteral(sqlConstantExpression.Value);
                        }
                    }

                    break;
                }
            }

            if (substitutions != null)
            {
                // ReSharper disable once CoVariantArrayConversion
                // InvariantCulture not needed since substitutions are all strings
                sql = string.Format(sql, substitutions);
            }

            _relationalCommandBuilder.AppendLines(sql);
        }

        /// <inheritdoc />
        protected override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
        {
            Check.NotNull(fromSqlExpression, nameof(fromSqlExpression));

            _relationalCommandBuilder.AppendLine("(");

            CheckComposableSql(fromSqlExpression.Sql);

            using (_relationalCommandBuilder.Indent())
            {
                GenerateFromSql(fromSqlExpression);
            }

            _relationalCommandBuilder.Append(")")
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(fromSqlExpression.Alias));

            return fromSqlExpression;
        }

        /// <summary>
        ///     Checks whether a given SQL string is composable, i.e. can be embedded as a subquery within a
        ///     larger SQL query.
        /// </summary>
        /// <param name="sql"> An SQL string to be checked for composability. </param>
        /// <exception cref="InvalidOperationException"> The given SQL isn't composable. </exception>
        protected virtual void CheckComposableSql([NotNull] string sql)
        {
            Check.NotNull(sql, nameof(sql));

            var pos = -1;
            char c;

            while (true)
            {
                c = NextChar();

                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                // SQL -- comment
                if (c == '-')
                {
                    if (NextChar() != '-')
                    {
                        throw new InvalidOperationException(RelationalStrings.FromSqlNonComposable);
                    }

                    while (NextChar() != '\n') { }

                    continue;
                }

                // SQL /* */ comment
                if (c == '/')
                {
                    if (NextChar() != '*')
                    {
                        throw new InvalidOperationException(RelationalStrings.FromSqlNonComposable);
                    }

                    while (true)
                    {
                        while (NextChar() != '*') { }

                        if (NextChar() == '/')
                        {
                            break;
                        }
                    }

                    continue;
                }

                if (char.ToLowerInvariant(c) == 's'
                    && char.ToLowerInvariant(NextChar()) == 'e'
                    && char.ToLowerInvariant(NextChar()) == 'l'
                    && char.ToLowerInvariant(NextChar()) == 'e'
                    && char.ToLowerInvariant(NextChar()) == 'c'
                    && char.ToLowerInvariant(NextChar()) == 't')
                {
                    var (c1, c2) = (NextChar(), NextChar());
                    if (char.IsWhiteSpace(c1)
                        || c1 == '-' && c2 == '-'
                        || c1 == '/' && c2 == '*')
                    {
                        return;
                    }
                }

                throw new InvalidOperationException(RelationalStrings.FromSqlNonComposable);
            }

            char NextChar()
                => ++pos < sql.Length
                    ? sql[pos]
                    : throw new InvalidOperationException(RelationalStrings.FromSqlNonComposable);
        }

        /// <inheritdoc />
        protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
        {
            Check.NotNull(sqlBinaryExpression, nameof(sqlBinaryExpression));

            var requiresBrackets = RequiresBrackets(sqlBinaryExpression.Left);

            if (requiresBrackets)
            {
                _relationalCommandBuilder.Append("(");
            }

            Visit(sqlBinaryExpression.Left);

            if (requiresBrackets)
            {
                _relationalCommandBuilder.Append(")");
            }

            _relationalCommandBuilder.Append(GetOperator(sqlBinaryExpression));

            requiresBrackets = RequiresBrackets(sqlBinaryExpression.Right);

            if (requiresBrackets)
            {
                _relationalCommandBuilder.Append("(");
            }

            Visit(sqlBinaryExpression.Right);

            if (requiresBrackets)
            {
                _relationalCommandBuilder.Append(")");
            }

            return sqlBinaryExpression;
        }

        private static bool RequiresBrackets(SqlExpression expression)
            => expression is SqlBinaryExpression || expression is LikeExpression;

        /// <inheritdoc />
        protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
        {
            Check.NotNull(sqlConstantExpression, nameof(sqlConstantExpression));

            _relationalCommandBuilder
                .Append(sqlConstantExpression.TypeMapping.GenerateSqlLiteral(sqlConstantExpression.Value));

            return sqlConstantExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
        {
            Check.NotNull(sqlParameterExpression, nameof(sqlParameterExpression));

            var parameterNameInCommand = _sqlGenerationHelper.GenerateParameterName(sqlParameterExpression.Name);

            if (_relationalCommandBuilder.Parameters
                .All(p => p.InvariantName != sqlParameterExpression.Name))
            {
                _relationalCommandBuilder.AddParameter(
                    sqlParameterExpression.Name,
                    parameterNameInCommand,
                    sqlParameterExpression.TypeMapping,
                    sqlParameterExpression.IsNullable);
            }

            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.GenerateParameterNamePlaceholder(sqlParameterExpression.Name));

            return sqlParameterExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitOrdering(OrderingExpression orderingExpression)
        {
            Check.NotNull(orderingExpression, nameof(orderingExpression));

            if (orderingExpression.Expression is SqlConstantExpression
                || orderingExpression.Expression is SqlParameterExpression)
            {
                _relationalCommandBuilder.Append("(SELECT 1)");
            }
            else
            {
                Visit(orderingExpression.Expression);
            }

            if (!orderingExpression.IsAscending)
            {
                _relationalCommandBuilder.Append(" DESC");
            }

            return orderingExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitLike(LikeExpression likeExpression)
        {
            Check.NotNull(likeExpression, nameof(likeExpression));

            Visit(likeExpression.Match);
            _relationalCommandBuilder.Append(" LIKE ");
            Visit(likeExpression.Pattern);

            if (likeExpression.EscapeChar != null)
            {
                _relationalCommandBuilder.Append(" ESCAPE ");
                Visit(likeExpression.EscapeChar);
            }

            return likeExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitCollate(CollateExpression collateExpresion)
        {
            Check.NotNull(collateExpresion, nameof(collateExpresion));

            Visit(collateExpresion.Operand);

            _relationalCommandBuilder
                .Append(" COLLATE ")
                .Append(collateExpresion.Collation);

            return collateExpresion;
        }

        /// <inheritdoc />
        protected override Expression VisitDistinct(DistinctExpression distinctExpression)
        {
            Check.NotNull(distinctExpression, nameof(distinctExpression));

            _relationalCommandBuilder.Append("DISTINCT (");
            Visit(distinctExpression.Operand);
            _relationalCommandBuilder.Append(")");

            return distinctExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitCase(CaseExpression caseExpression)
        {
            Check.NotNull(caseExpression, nameof(caseExpression));

            _relationalCommandBuilder.Append("CASE");

            if (caseExpression.Operand != null)
            {
                _relationalCommandBuilder.Append(" ");
                Visit(caseExpression.Operand);
            }

            using (_relationalCommandBuilder.Indent())
            {
                foreach (var whenClause in caseExpression.WhenClauses)
                {
                    _relationalCommandBuilder
                        .AppendLine()
                        .Append("WHEN ");
                    Visit(whenClause.Test);
                    _relationalCommandBuilder.Append(" THEN ");
                    Visit(whenClause.Result);
                }

                if (caseExpression.ElseResult != null)
                {
                    _relationalCommandBuilder
                        .AppendLine()
                        .Append("ELSE ");
                    Visit(caseExpression.ElseResult);
                }
            }

            _relationalCommandBuilder
                .AppendLine()
                .Append("END");

            return caseExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
        {
            Check.NotNull(sqlUnaryExpression, nameof(sqlUnaryExpression));

            switch (sqlUnaryExpression.OperatorType)
            {
                case ExpressionType.Convert:
                {
                    _relationalCommandBuilder.Append("CAST(");
                    var requiresBrackets = RequiresBrackets(sqlUnaryExpression.Operand);
                    if (requiresBrackets)
                    {
                        _relationalCommandBuilder.Append("(");
                    }

                    Visit(sqlUnaryExpression.Operand);
                    if (requiresBrackets)
                    {
                        _relationalCommandBuilder.Append(")");
                    }

                    _relationalCommandBuilder.Append(" AS ");
                    _relationalCommandBuilder.Append(sqlUnaryExpression.TypeMapping.StoreType);
                    _relationalCommandBuilder.Append(")");
                    break;
                }

                case ExpressionType.Not
                    when sqlUnaryExpression.Type == typeof(bool):
                {
                    _relationalCommandBuilder.Append("NOT (");
                    Visit(sqlUnaryExpression.Operand);
                    _relationalCommandBuilder.Append(")");
                    break;
                }

                case ExpressionType.Not:
                {
                    _relationalCommandBuilder.Append("~");
                    Visit(sqlUnaryExpression.Operand);
                    break;
                }

                case ExpressionType.Equal:
                {
                    Visit(sqlUnaryExpression.Operand);
                    _relationalCommandBuilder.Append(" IS NULL");
                    break;
                }

                case ExpressionType.NotEqual:
                {
                    Visit(sqlUnaryExpression.Operand);
                    _relationalCommandBuilder.Append(" IS NOT NULL");
                    break;
                }

                case ExpressionType.Negate:
                {
                    _relationalCommandBuilder.Append("-");
                    var requiresBrackets = RequiresBrackets(sqlUnaryExpression.Operand);
                    if (requiresBrackets)
                    {
                        _relationalCommandBuilder.Append("(");
                    }

                    Visit(sqlUnaryExpression.Operand);
                    if (requiresBrackets)
                    {
                        _relationalCommandBuilder.Append(")");
                    }

                    break;
                }
            }

            return sqlUnaryExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitExists(ExistsExpression existsExpression)
        {
            Check.NotNull(existsExpression, nameof(existsExpression));

            if (existsExpression.IsNegated)
            {
                _relationalCommandBuilder.Append("NOT ");
            }

            _relationalCommandBuilder.AppendLine("EXISTS (");

            using (_relationalCommandBuilder.Indent())
            {
                Visit(existsExpression.Subquery);
            }

            _relationalCommandBuilder.Append(")");

            return existsExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitIn(InExpression inExpression)
        {
            Check.NotNull(inExpression, nameof(inExpression));

            if (inExpression.Values != null)
            {
                Visit(inExpression.Item);
                _relationalCommandBuilder.Append(inExpression.IsNegated ? " NOT IN " : " IN ");
                _relationalCommandBuilder.Append("(");
                var valuesConstant = (SqlConstantExpression)inExpression.Values;
                var valuesList = ((IEnumerable<object>)valuesConstant.Value)
                    .Select(v => new SqlConstantExpression(Expression.Constant(v), valuesConstant.TypeMapping)).ToList();
                GenerateList(valuesList, e => Visit(e));
                _relationalCommandBuilder.Append(")");
            }
            else
            {
                Visit(inExpression.Item);
                _relationalCommandBuilder.Append(inExpression.IsNegated ? " NOT IN " : " IN ");
                _relationalCommandBuilder.AppendLine("(");

                using (_relationalCommandBuilder.Indent())
                {
                    Visit(inExpression.Subquery);
                }

                _relationalCommandBuilder.AppendLine().Append(")");
            }

            return inExpression;
        }

        /// <summary>
        ///     Generates a SQL operator for a SQL binary operation.
        /// </summary>
        /// <param name="binaryExpression"> A SQL binary operation. </param>
        /// <returns> A string representation of the binary operator. </returns>
        [Obsolete("Use GetOperator instead.")]
        protected virtual string GenerateOperator([NotNull] SqlBinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            return _operatorMap[binaryExpression.OperatorType];
        }

        /// <summary>
        ///     Gets a SQL operator for a SQL binary operation.
        /// </summary>
        /// <param name="binaryExpression"> A SQL binary operation. </param>
        /// <returns> A string representation of the binary operator. </returns>
        protected virtual string GetOperator([NotNull] SqlBinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            return _operatorMap[binaryExpression.OperatorType];
        }

        /// <summary>
        ///     Generates a TOP construct in the relational command
        /// </summary>
        /// <param name="selectExpression"> A select expression to use. </param>
        protected virtual void GenerateTop([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));
        }

        /// <summary>
        ///     Generates an ORDER BY clause in the relational command
        /// </summary>
        /// <param name="selectExpression"> A select expression to use. </param>
        protected virtual void GenerateOrderings([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Orderings.Any())
            {
                var orderings = selectExpression.Orderings.ToList();

                if (selectExpression.Limit == null
                    && selectExpression.Offset == null)
                {
                    orderings.RemoveAll(oe => oe.Expression is SqlConstantExpression || oe.Expression is SqlParameterExpression);
                }

                if (orderings.Count > 0)
                {
                    _relationalCommandBuilder.AppendLine()
                        .Append("ORDER BY ");

                    GenerateList(orderings, e => Visit(e));
                }
            }
        }

        /// <summary>
        ///     Generates a LIMIT...OFFSET... construct in the relational command
        /// </summary>
        /// <param name="selectExpression"> A select expression to use. </param>
        protected virtual void GenerateLimitOffset([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Offset != null)
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("OFFSET ");

                Visit(selectExpression.Offset);

                _relationalCommandBuilder.Append(" ROWS");

                if (selectExpression.Limit != null)
                {
                    _relationalCommandBuilder.Append(" FETCH NEXT ");

                    Visit(selectExpression.Limit);

                    _relationalCommandBuilder.Append(" ROWS ONLY");
                }
            }
            else if (selectExpression.Limit != null)
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("FETCH FIRST ");

                Visit(selectExpression.Limit);

                _relationalCommandBuilder.Append(" ROWS ONLY");
            }
        }

        private void GenerateList<T>(
            IReadOnlyList<T> items,
            Action<T> generationAction,
            Action<IRelationalCommandBuilder> joinAction = null)
        {
            joinAction ??= (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(_relationalCommandBuilder);
                }

                generationAction(items[i]);
            }
        }

        /// <inheritdoc />
        protected override Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
        {
            Check.NotNull(crossJoinExpression, nameof(crossJoinExpression));

            _relationalCommandBuilder.Append("CROSS JOIN ");
            Visit(crossJoinExpression.Table);

            return crossJoinExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
        {
            Check.NotNull(crossApplyExpression, nameof(crossApplyExpression));

            _relationalCommandBuilder.Append("CROSS APPLY ");
            Visit(crossApplyExpression.Table);

            return crossApplyExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitOuterApply(OuterApplyExpression outerApplyExpression)
        {
            Check.NotNull(outerApplyExpression, nameof(outerApplyExpression));

            _relationalCommandBuilder.Append("OUTER APPLY ");
            Visit(outerApplyExpression.Table);

            return outerApplyExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression)
        {
            Check.NotNull(innerJoinExpression, nameof(innerJoinExpression));

            _relationalCommandBuilder.Append("INNER JOIN ");
            Visit(innerJoinExpression.Table);
            _relationalCommandBuilder.Append(" ON ");
            Visit(innerJoinExpression.JoinPredicate);

            return innerJoinExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitLeftJoin(LeftJoinExpression leftJoinExpression)
        {
            Check.NotNull(leftJoinExpression, nameof(leftJoinExpression));

            _relationalCommandBuilder.Append("LEFT JOIN ");
            Visit(leftJoinExpression.Table);
            _relationalCommandBuilder.Append(" ON ");
            Visit(leftJoinExpression.JoinPredicate);

            return leftJoinExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitScalarSubquery(ScalarSubqueryExpression scalarSubqueryExpression)
        {
            Check.NotNull(scalarSubqueryExpression, nameof(scalarSubqueryExpression));

            _relationalCommandBuilder.AppendLine("(");
            using (_relationalCommandBuilder.Indent())
            {
                Visit(scalarSubqueryExpression.Subquery);
            }

            _relationalCommandBuilder.Append(")");

            return scalarSubqueryExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitRowNumber(RowNumberExpression rowNumberExpression)
        {
            Check.NotNull(rowNumberExpression, nameof(rowNumberExpression));

            _relationalCommandBuilder.Append("ROW_NUMBER() OVER(");
            if (rowNumberExpression.Partitions.Any())
            {
                _relationalCommandBuilder.Append("PARTITION BY ");
                GenerateList(rowNumberExpression.Partitions, e => Visit(e));
                _relationalCommandBuilder.Append(" ");
            }

            _relationalCommandBuilder.Append("ORDER BY ");
            GenerateList(rowNumberExpression.Orderings, e => Visit(e));
            _relationalCommandBuilder.Append(")");

            return rowNumberExpression;
        }

        /// <summary>
        ///     Generates a set operation in the relational command.
        /// </summary>
        /// <param name="setOperation"> A set operation to print. </param>
        protected virtual void GenerateSetOperation([NotNull] SetOperationBase setOperation)
        {
            Check.NotNull(setOperation, nameof(setOperation));

            GenerateSetOperationOperand(setOperation, setOperation.Source1);
            _relationalCommandBuilder.AppendLine();
            _relationalCommandBuilder.AppendLine($"{GetSetOperation(setOperation)}{(setOperation.IsDistinct ? "" : " ALL")}");
            GenerateSetOperationOperand(setOperation, setOperation.Source2);

            static string GetSetOperation(SetOperationBase operation)
                => operation switch
                {
                    ExceptExpression _ => "EXCEPT",
                    IntersectExpression _ => "INTERSECT",
                    UnionExpression _ => "UNION",
                    _ => throw new InvalidOperationException(CoreStrings.UnknownEntity("SetOperationType")),
                };
        }

        /// <summary>
        ///     Generates an operand for a given set operation in the relational command.
        /// </summary>
        /// <param name="setOperation"> A set operation to use. </param>
        /// <param name="operand"> A set operation operand to print. </param>
        protected virtual void GenerateSetOperationOperand([NotNull] SetOperationBase setOperation, [NotNull] SelectExpression operand)
        {
            Check.NotNull(setOperation, nameof(setOperation));
            Check.NotNull(operand, nameof(operand));

            // INTERSECT has higher precedence over UNION and EXCEPT, but otherwise evaluation is left-to-right.
            // To preserve meaning, add parentheses whenever a set operation is nested within a different set operation.
            if (IsNonComposedSetOperation(operand)
                && operand.Tables[0].GetType() != setOperation.GetType())
            {
                _relationalCommandBuilder.AppendLine("(");
                using (_relationalCommandBuilder.Indent())
                {
                    Visit(operand);
                }

                _relationalCommandBuilder.AppendLine().Append(")");
            }
            else
            {
                Visit(operand);
            }
        }

        private void GenerateSetOperationHelper(SetOperationBase setOperation)
        {
            _relationalCommandBuilder.AppendLine("(");
            using (_relationalCommandBuilder.Indent())
            {
                GenerateSetOperation(setOperation);
            }

            _relationalCommandBuilder.AppendLine()
                .Append(")")
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(setOperation.Alias));
        }

        /// <inheritdoc />
        protected override Expression VisitExcept(ExceptExpression exceptExpression)
        {
            Check.NotNull(exceptExpression, nameof(exceptExpression));

            GenerateSetOperationHelper(exceptExpression);

            return exceptExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitIntersect(IntersectExpression intersectExpression)
        {
            Check.NotNull(intersectExpression, nameof(intersectExpression));

            GenerateSetOperationHelper(intersectExpression);

            return intersectExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitUnion(UnionExpression unionExpression)
        {
            Check.NotNull(unionExpression, nameof(unionExpression));

            GenerateSetOperationHelper(unionExpression);

            return unionExpression;
        }
    }
}
