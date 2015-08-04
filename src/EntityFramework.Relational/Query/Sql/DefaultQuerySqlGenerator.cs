// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.Sql
{
    public class DefaultQuerySqlGenerator : ThrowingExpressionVisitor, ISqlExpressionVisitor, ISqlQueryGenerator
    {
        private readonly SelectExpression _selectExpression;

        private IndentedStringBuilder _sql;
        private List<CommandParameter> _commandParameters;
        private IDictionary<string, object> _parameterValues;
        private int _rawSqlParameterIndex;

        public DefaultQuerySqlGenerator(
            [NotNull] SelectExpression selectExpression,
            [CanBeNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            _selectExpression = selectExpression;
            TypeMapper = typeMapper;
        }

        public virtual IRelationalTypeMapper TypeMapper { get; }

        public virtual SelectExpression SelectExpression => _selectExpression;

        public virtual string GenerateSql(IDictionary<string, object> parameterValues)
        {
            Check.NotNull(parameterValues, nameof(parameterValues));

            _sql = new IndentedStringBuilder();
            _commandParameters = new List<CommandParameter>();
            _parameterValues = parameterValues;
            _rawSqlParameterIndex = 0;

            Visit(_selectExpression);

            return _sql.ToString();
        }

        public virtual IRelationalValueBufferFactory CreateValueBufferFactory(
            IRelationalValueBufferFactoryFactory relationalValueBufferFactoryFactory, DbDataReader _)
        {
            Check.NotNull(relationalValueBufferFactoryFactory, nameof(relationalValueBufferFactoryFactory));

            return relationalValueBufferFactoryFactory
                .Create(_selectExpression.GetProjectionTypes().ToArray(), indexMap: null);
        }

        public virtual IReadOnlyList<CommandParameter> Parameters => _commandParameters;

        protected virtual IndentedStringBuilder Sql => _sql;

        protected virtual string ConcatOperator => "+";
        protected virtual string ParameterPrefix => "@";
        protected virtual string TrueLiteral => "1";
        protected virtual string FalseLiteral => "0";
        protected virtual string TypedTrueLiteral => "CAST(1 AS BIT)";
        protected virtual string TypedFalseLiteral => "CAST(0 AS BIT)";

        public virtual Expression VisitSelect(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            IDisposable subQueryIndent = null;

            if (selectExpression.Alias != null)
            {
                _sql.AppendLine("(");

                subQueryIndent = _sql.Indent();
            }

            _sql.Append("SELECT ");

            if (selectExpression.IsDistinct)
            {
                _sql.Append("DISTINCT ");
            }

            GenerateTop(selectExpression);

            if (selectExpression.Projection.Any())
            {
                VisitJoin(selectExpression.Projection);
            }
            else if (selectExpression.IsProjectStar)
            {
                _sql.Append(DelimitIdentifier(selectExpression.Tables.Single().Alias))
                    .Append(".*");
            }
            else
            {
                _sql.Append("1");
            }

            if (selectExpression.Tables.Any())
            {
                _sql.AppendLine()
                    .Append("FROM ");

                VisitJoin(selectExpression.Tables, sql => sql.AppendLine());
            }

            if (selectExpression.Predicate != null)
            {
                _sql.AppendLine()
                    .Append("WHERE ");

                var constantExpression = selectExpression.Predicate as ConstantExpression;

                if (constantExpression != null)
                {
                    _sql.Append((bool)constantExpression.Value ? "1 = 1" : "1 = 0");
                }
                else
                {
                    var predicate
                        = new NullComparisonTransformingVisitor(_parameterValues)
                            .Visit(selectExpression.Predicate);

                    // we have to optimize out comparisons to null-valued parameters before we can expand null semantics 
                    if (_parameterValues.Count > 0)
                    {
                        var optimizedNullExpansionVisitor = new NullSemanticsOptimizedExpandingVisitor();
                        var nullSemanticsExpandedOptimized = optimizedNullExpansionVisitor.Visit(predicate);
                        if (optimizedNullExpansionVisitor.OptimizedExpansionPossible)
                        {
                            predicate = nullSemanticsExpandedOptimized;
                        }
                        else
                        {
                            predicate = new NullSemanticsExpandingVisitor()
                                .Visit(predicate);
                        }
                    }

                    predicate = new ReducingExpressionVisitor().Visit(predicate);

                    Visit(predicate);

                    if (selectExpression.Predicate is ParameterExpression
                        || selectExpression.Predicate.IsAliasWithColumnExpression()
                        || selectExpression.Predicate is SelectExpression)
                    {
                        _sql.Append(" = ");
                        _sql.Append(TrueLiteral);
                    }
                }
            }

            if (selectExpression.OrderBy.Any())
            {
                _sql.AppendLine()
                    .Append("ORDER BY ");

                VisitJoin(selectExpression.OrderBy, t =>
                    {
                        var aliasExpression = t.Expression as AliasExpression;

                        if (aliasExpression != null)
                        {
                            if (aliasExpression.Alias != null)
                            {
                                var columnExpression = aliasExpression.TryGetColumnExpression();

                                if (columnExpression != null)
                                {
                                    _sql.Append(DelimitIdentifier(columnExpression.TableAlias))
                                        .Append(".");
                                }

                                _sql.Append(DelimitIdentifier(aliasExpression.Alias));
                            }
                            else
                            {
                                Visit(aliasExpression.Expression);
                            }
                        }
                        else
                        {
                            Visit(t.Expression);
                        }

                        if (t.OrderingDirection == OrderingDirection.Desc)
                        {
                            _sql.Append(" DESC");
                        }
                    });
            }

            GenerateLimitOffset(selectExpression);

            if (subQueryIndent != null)
            {
                subQueryIndent.Dispose();

                _sql.AppendLine()
                    .Append(")");

                if (selectExpression.Alias.Length > 0)
                {
                    _sql.Append(" AS ")
                        .Append(DelimitIdentifier(selectExpression.Alias));
                }
            }

            return selectExpression;
        }

        private void VisitJoin(
            IReadOnlyList<Expression> expressions, Action<IndentedStringBuilder> joinAction = null)
            => VisitJoin(expressions, e => Visit(e), joinAction);

        private void VisitJoin<T>(
            IReadOnlyList<T> items, Action<T> itemAction, Action<IndentedStringBuilder> joinAction = null)
        {
            joinAction = joinAction ?? (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(_sql);
                }

                itemAction(items[i]);
            }
        }

        public virtual Expression VisitRawSqlDerivedTable(RawSqlDerivedTableExpression rawSqlDerivedTableExpression)
        {
            Check.NotNull(rawSqlDerivedTableExpression, nameof(rawSqlDerivedTableExpression));

            _sql.AppendLine("(");

            using (_sql.Indent())
            {
                var substitutions = new object[rawSqlDerivedTableExpression.Parameters.Count()];

                for (var index = 0; index < rawSqlDerivedTableExpression.Parameters.Count(); index++)
                {
                    var parameterName = ParameterPrefix + "p" + _rawSqlParameterIndex++;
                    var value = rawSqlDerivedTableExpression.Parameters[index];

                    _commandParameters.Add(
                        new CommandParameter(parameterName, value, TypeMapper.GetDefaultMapping(value)));

                    substitutions[index] = parameterName;
                }

                _sql.AppendLines(string.Format(
                    rawSqlDerivedTableExpression.Sql,
                    substitutions));
            }

            _sql.Append(") AS ")
                .Append(DelimitIdentifier(rawSqlDerivedTableExpression.Alias));

            return rawSqlDerivedTableExpression;
        }

        public virtual Expression VisitTable(TableExpression tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            if (tableExpression.Schema != null)
            {
                _sql.Append(DelimitIdentifier(tableExpression.Schema))
                    .Append(".");
            }

            _sql.Append(DelimitIdentifier(tableExpression.Table))
                .Append(" AS ")
                .Append(DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }

        public virtual Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
        {
            Check.NotNull(crossJoinExpression, nameof(crossJoinExpression));

            _sql.Append("CROSS JOIN ");

            Visit(crossJoinExpression.TableExpression);

            return crossJoinExpression;
        }

        public virtual Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
        {
            Check.NotNull(crossApplyExpression, nameof(crossApplyExpression));

            _sql.Append("CROSS APPLY ");

            Visit(crossApplyExpression.TableExpression);

            return crossApplyExpression;
        }

        public virtual Expression VisitCount(CountExpression countExpression)
        {
            Check.NotNull(countExpression, nameof(countExpression));

            _sql.Append("COUNT(*)");

            return countExpression;
        }

        public virtual Expression VisitSum(SumExpression sumExpression)
        {
            Check.NotNull(sumExpression, nameof(sumExpression));

            _sql.Append("SUM(");

            Visit(sumExpression.Expression);

            _sql.Append(")");

            return sumExpression;
        }

        public virtual Expression VisitMin(MinExpression minExpression)
        {
            Check.NotNull(minExpression, nameof(minExpression));

            _sql.Append("MIN(");

            Visit(minExpression.Expression);

            _sql.Append(")");

            return minExpression;
        }

        public virtual Expression VisitMax(MaxExpression maxExpression)
        {
            Check.NotNull(maxExpression, nameof(maxExpression));

            _sql.Append("MAX(");

            Visit(maxExpression.Expression);

            _sql.Append(")");

            return maxExpression;
        }

        public virtual Expression VisitIn(InExpression inExpression)
        {
            if (inExpression.Values != null)
            {
                var inValues = ProcessInExpressionValues(inExpression.Values);
                var inValuesNotNull = ExtractNonNullExpressionValues(inValues);

                if (inValues.Count != inValuesNotNull.Count)
                {
                    var nullSemanticsInExpression = Expression.OrElse(
                        new InExpression(inExpression.Operand, inValuesNotNull),
                        new IsNullExpression(inExpression.Operand));

                    return Visit(nullSemanticsInExpression);
                }

                if (inValuesNotNull.Count > 0)
                {
                    Visit(inExpression.Operand);

                    _sql.Append(" IN (");

                    VisitJoin(inValuesNotNull);

                    _sql.Append(")");
                }
                else
                {
                    _sql.Append("1 = 0");
                }
            }
            else
            {
                Visit(inExpression.Operand);

                _sql.Append(" IN ");

                Visit(inExpression.SubQuery);
            }

            return inExpression;
        }

        protected virtual Expression VisitNotIn(InExpression inExpression)
        {
            if (inExpression.Values != null)
            {
                var inValues = ProcessInExpressionValues(inExpression.Values);
                var inValuesNotNull = ExtractNonNullExpressionValues(inValues);

                if (inValues.Count != inValuesNotNull.Count)
                {
                    var nullSemanticsNotInExpression = Expression.AndAlso(
                        Expression.Not(new InExpression(inExpression.Operand, inValuesNotNull)),
                        Expression.Not(new IsNullExpression(inExpression.Operand)));

                    return Visit(nullSemanticsNotInExpression);
                }

                if (inValues.Count > 0)
                {
                    Visit(inExpression.Operand);

                    _sql.Append(" NOT IN (");

                    VisitJoin(inValues);

                    _sql.Append(")");
                }
                else
                {
                    _sql.Append("1 = 1");
                }
            }
            else
            {
                Visit(inExpression.Operand);

                _sql.Append(" NOT IN ");

                Visit(inExpression.SubQuery);
            }
            
            return inExpression;
        }

        protected virtual IReadOnlyList<Expression> ProcessInExpressionValues(
            [NotNull] IReadOnlyList<Expression> inExpressionValues)
        {
            Check.NotNull(inExpressionValues, nameof(inExpressionValues));

            var inConstants = new List<Expression>();

            foreach (var inValue in inExpressionValues)
            {
                var inConstant = inValue as ConstantExpression;
                if (inConstant != null)
                {
                    inConstants.Add(inConstant);
                    continue;
                }

                var inParameter = inValue as ParameterExpression;
                if (inParameter != null)
                {
                    object parameterValue;
                    if (_parameterValues.TryGetValue(inParameter.Name, out parameterValue))
                    {
                        var valuesCollection = parameterValue as IEnumerable;

                        if (valuesCollection != null
                            && parameterValue.GetType() != typeof(string)
                            && parameterValue.GetType() != typeof(byte[]))
                        {
                            inConstants.AddRange(valuesCollection.Cast<object>().Select(Expression.Constant));
                        }
                        else
                        {
                            inConstants.Add(inParameter);
                        }
                    }
                }
            }

            return inConstants;
        }

        protected virtual IReadOnlyList<Expression> ExtractNonNullExpressionValues(
            IReadOnlyList<Expression> inExpressionValues)
        {
            var inValuesNotNull = new List<Expression>();
            foreach (var inValue in inExpressionValues)
            {
                var inConstant = inValue as ConstantExpression;
                if (inConstant?.Value != null)
                {
                    inValuesNotNull.Add(inValue);
                    continue;
                }

                var inParameter = inValue as ParameterExpression;
                if (inParameter != null)
                {
                    object parameterValue;
                    if (_parameterValues.TryGetValue(inParameter.Name, out parameterValue))
                    {
                        if (parameterValue != null)
                        {
                            inValuesNotNull.Add(inValue);
                        }
                    }
                }
            }

            return inValuesNotNull;
        }

        public virtual Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression)
        {
            Check.NotNull(innerJoinExpression, nameof(innerJoinExpression));

            _sql.Append("INNER JOIN ");

            Visit(innerJoinExpression.TableExpression);

            _sql.Append(" ON ");

            Visit(innerJoinExpression.Predicate);

            return innerJoinExpression;
        }

        public virtual Expression VisitOuterJoin(LeftOuterJoinExpression leftOuterJoinExpression)
        {
            Check.NotNull(leftOuterJoinExpression, nameof(leftOuterJoinExpression));

            _sql.Append("LEFT JOIN ");

            Visit(leftOuterJoinExpression.TableExpression);

            _sql.Append(" ON ");

            Visit(leftOuterJoinExpression.Predicate);

            return leftOuterJoinExpression;
        }

        protected virtual void GenerateTop([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Limit != null
                && selectExpression.Offset == null)
            {
                _sql.Append("TOP(")
                    .Append(selectExpression.Limit)
                    .Append(") ");
            }
        }

        protected virtual void GenerateLimitOffset([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Offset != null)
            {
                _sql.AppendLine()
                    .Append("OFFSET ")
                    .Append(selectExpression.Offset)
                    .Append(" ROWS");

                if (selectExpression.Limit != null)
                {
                    _sql.Append(" FETCH NEXT ")
                        .Append(selectExpression.Limit)
                        .Append(" ROWS ONLY");
                }
            }
        }

        protected override Expression VisitConditional(ConditionalExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            _sql.AppendLine("CASE");

            using (_sql.Indent())
            {
                _sql.AppendLine("WHEN");

                using (_sql.Indent())
                {
                    _sql.Append("(");

                    Visit(expression.Test);

                    _sql.AppendLine(")");
                }

                _sql.Append("THEN ");

                var constantIfTrue = expression.IfTrue as ConstantExpression;

                if (constantIfTrue != null
                    && constantIfTrue.Type == typeof(bool))
                {
                    _sql.Append((bool)constantIfTrue.Value ? TypedTrueLiteral : TypedFalseLiteral);
                }
                else
                {
                    Visit(expression.IfTrue);
                }

                _sql.Append(" ELSE ");

                var constantIfFalse = expression.IfFalse as ConstantExpression;

                if (constantIfFalse != null
                    && constantIfFalse.Type == typeof(bool))
                {
                    _sql.Append((bool)constantIfFalse.Value ? TypedTrueLiteral : TypedFalseLiteral);
                }
                else
                {
                    Visit(expression.IfFalse);
                }

                _sql.AppendLine();
            }

            _sql.Append("END");

            return expression;
        }

        public virtual Expression VisitExists(ExistsExpression existsExpression)
        {
            Check.NotNull(existsExpression, nameof(existsExpression));

            _sql.AppendLine("EXISTS (");

            using (_sql.Indent())
            {
                Visit(existsExpression.Expression);
            }

            _sql.AppendLine(")");

            return existsExpression;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            if (binaryExpression.NodeType == ExpressionType.Coalesce)
            {
                _sql.Append("COALESCE(");
                Visit(binaryExpression.Left);
                _sql.Append(", ");
                Visit(binaryExpression.Right);
                _sql.Append(")");
            }
            else
            {
                var needParentheses
                    = !binaryExpression.Left.IsSimpleExpression()
                      || !binaryExpression.Right.IsSimpleExpression()
                      || binaryExpression.IsLogicalOperation();

                if (needParentheses)
                {
                    _sql.Append("(");
                }

                Visit(binaryExpression.Left);

                if (binaryExpression.IsLogicalOperation()
                    && binaryExpression.Left.IsSimpleExpression())
                {
                    _sql.Append(" = ");
                    _sql.Append(TrueLiteral);
                }

                string op;

                switch (binaryExpression.NodeType)
                {
                    case ExpressionType.Equal:
                        op = " = ";
                        break;
                    case ExpressionType.NotEqual:
                        op = " <> ";
                        break;
                    case ExpressionType.GreaterThan:
                        op = " > ";
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        op = " >= ";
                        break;
                    case ExpressionType.LessThan:
                        op = " < ";
                        break;
                    case ExpressionType.LessThanOrEqual:
                        op = " <= ";
                        break;
                    case ExpressionType.AndAlso:
                        op = " AND ";
                        break;
                    case ExpressionType.OrElse:
                        op = " OR ";
                        break;
                    case ExpressionType.Add:
                        op = (binaryExpression.Left.Type == typeof(string)
                              && binaryExpression.Right.Type == typeof(string))
                            ? " " + ConcatOperator + " "
                            : " + ";
                        break;
                    case ExpressionType.Subtract:
                        op = " - ";
                        break;
                    case ExpressionType.Multiply:
                        op = " * ";
                        break;
                    case ExpressionType.Divide:
                        op = " / ";
                        break;
                    case ExpressionType.Modulo:
                        op = " % ";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _sql.Append(op);

                Visit(binaryExpression.Right);

                if (binaryExpression.IsLogicalOperation()
                    && binaryExpression.Right.IsSimpleExpression())
                {
                    _sql.Append(" = ");
                    _sql.Append(TrueLiteral);
                }

                if (needParentheses)
                {
                    _sql.Append(")");
                }
            }

            return binaryExpression;
        }

        public virtual Expression VisitColumn(ColumnExpression columnExpression)
        {
            Check.NotNull(columnExpression, nameof(columnExpression));

            _sql.Append(DelimitIdentifier(columnExpression.TableAlias))
                .Append(".")
                .Append(DelimitIdentifier(columnExpression.Name));

            return columnExpression;
        }

        public virtual Expression VisitAlias(AliasExpression aliasExpression)
        {
            Check.NotNull(aliasExpression, nameof(aliasExpression));

            if (!aliasExpression.Projected)
            {
                Visit(aliasExpression.Expression);

                if (aliasExpression.Alias != null)
                {
                    _sql.Append(" AS ");
                }
            }

            if (aliasExpression.Alias != null)
            {
                _sql.Append(DelimitIdentifier(aliasExpression.Alias));
            }

            return aliasExpression;
        }

        public virtual Expression VisitIsNull(IsNullExpression isNullExpression)
        {
            Check.NotNull(isNullExpression, nameof(isNullExpression));

            Visit(isNullExpression.Operand);

            _sql.Append(" IS NULL");

            return isNullExpression;
        }

        public virtual Expression VisitIsNotNull([NotNull] IsNullExpression isNotNullExpression)
        {
            Check.NotNull(isNotNullExpression, nameof(isNotNullExpression));

            Visit(isNotNullExpression.Operand);

            _sql.Append(" IS NOT NULL");

            return isNotNullExpression;
        }

        public virtual Expression VisitLike(LikeExpression likeExpression)
        {
            Check.NotNull(likeExpression, nameof(likeExpression));

            Visit(likeExpression.Match);

            _sql.Append(" LIKE ");

            Visit(likeExpression.Pattern);

            return likeExpression;
        }

        public virtual Expression VisitLiteral(LiteralExpression literalExpression)
        {
            Check.NotNull(literalExpression, nameof(literalExpression));

            _sql.Append(GenerateLiteral(literalExpression.Literal));

            return literalExpression;
        }

        public virtual Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            _sql.Append(sqlFunctionExpression.FunctionName);
            _sql.Append("(");

            VisitJoin(sqlFunctionExpression.Arguments.ToList());

            _sql.Append(")");

            return sqlFunctionExpression;
        }

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            Check.NotNull(unaryExpression, nameof(unaryExpression));

            if (unaryExpression.NodeType == ExpressionType.Not)
            {
                var inExpression = unaryExpression.Operand as InExpression;
                if (inExpression != null)
                {
                    return VisitNotIn(inExpression);
                }

                var isNullExpression = unaryExpression.Operand as IsNullExpression;
                if (isNullExpression != null)
                {
                    return VisitIsNotNull(isNullExpression);
                }

                var isColumnOrParameterOperand =
                    unaryExpression.Operand is ColumnExpression
                    || unaryExpression.Operand is ParameterExpression
                    || unaryExpression.Operand.IsAliasWithColumnExpression();

                if (!isColumnOrParameterOperand)
                {
                    _sql.Append("NOT (");
                    Visit(unaryExpression.Operand);
                    _sql.Append(")");
                }
                else
                {
                    Visit(unaryExpression.Operand);
                    _sql.Append(" = ");
                    _sql.Append(FalseLiteral);
                }

                return unaryExpression;
            }

            if (unaryExpression.NodeType == ExpressionType.Convert)
            {
                Visit(unaryExpression.Operand);

                return unaryExpression;
            }

            return base.VisitUnary(unaryExpression);
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            Check.NotNull(constantExpression, nameof(constantExpression));

            _sql.Append(constantExpression.Value == null
                ? "NULL"
                : GenerateLiteral((dynamic)constantExpression.Value));

            return constantExpression;
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            Check.NotNull(parameterExpression, nameof(parameterExpression));

            var parameterName = ParameterPrefix + GenerateParameterName(parameterExpression.Name);

            _sql.Append(parameterName);

            if (_commandParameters.All(commandParameter => commandParameter.Name != parameterName))
            {
                object value;
                if (!_parameterValues.TryGetValue(parameterExpression.Name, out value))
                {
                    value = string.Empty;
                }


                _commandParameters.Add(new CommandParameter(parameterName, value, TypeMapper.GetDefaultMapping(value)));
            }

            return parameterExpression;
        }

        protected virtual string GenerateParameterName([NotNull] string parameterName)
        {
            Check.NotEmpty(parameterName, nameof(parameterName));

            return parameterName;
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
            => new NotImplementedException(visitMethod);

        // TODO: Share the code below (#1559)

        private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK";
        private const string DateTimeOffsetFormat = "yyyy-MM-ddTHH:mm:ss.fffzzz";
        private const string FloatingPointFormat = "{0}E0";

        protected virtual string GenerateLiteral([NotNull] object value)
            => string.Format(CultureInfo.InvariantCulture, "{0}", value);

        protected virtual string GenerateLiteral([NotNull] Enum value)
            => string.Format(CultureInfo.InvariantCulture, "{0:d}", value);


        private readonly Dictionary<DbType, string> _dbTypeNameMapping = new Dictionary<DbType, string>
        {
            { DbType.Byte, "tinyint" },
            { DbType.Decimal, "decimal" },
            { DbType.Double, "float" },
            { DbType.Int16, "smallint" },
            { DbType.Int32, "int" },
            { DbType.Int64, "bigint" },
            { DbType.String, "nvarchar" },
        };

        protected virtual string GenerateLiteral(DbType value)
            => _dbTypeNameMapping[value];

        protected virtual string GenerateLiteral(int value)
            => value.ToString();

        protected virtual string GenerateLiteral(short value)
            => value.ToString();

        protected virtual string GenerateLiteral(long value)
            => value.ToString();

        protected virtual string GenerateLiteral(byte value)
            => value.ToString();

        protected virtual string GenerateLiteral(decimal value)
            => string.Format(value.ToString(CultureInfo.InvariantCulture));

        protected virtual string GenerateLiteral(double value)
            => string.Format(CultureInfo.InvariantCulture, FloatingPointFormat, value);

        protected virtual string GenerateLiteral(float value)
            => string.Format(CultureInfo.InvariantCulture, FloatingPointFormat, value);

        protected virtual string GenerateLiteral(bool value)
            => value ? TrueLiteral : FalseLiteral;

        protected virtual string GenerateLiteral([NotNull] string value)
            => "'" + EscapeLiteral(Check.NotNull(value, nameof(value))) + "'";

        protected virtual string GenerateLiteral(Guid value)
            => "'" + value + "'";

        protected virtual string GenerateLiteral(DateTime value)
            => "'" + value.ToString(DateTimeFormat, CultureInfo.InvariantCulture) + "'";

        protected virtual string GenerateLiteral(DateTimeOffset value)
            => "'" + value.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture) + "'";

        protected virtual string GenerateLiteral(TimeSpan value)
            => "'" + value + "'";

        protected virtual string GenerateLiteral([NotNull] byte[] value)
        {
            var stringBuilder = new StringBuilder("0x");

            foreach (var @byte in value)
            {
                stringBuilder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
            }

            return stringBuilder.ToString();
        }

        protected virtual string EscapeLiteral([NotNull] string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");

        protected virtual string DelimitIdentifier([NotNull] string identifier)
            => "\"" + Check.NotEmpty(identifier, nameof(identifier)) + "\"";

        private class NullComparisonTransformingVisitor : RelinqExpressionVisitor
        {
            private readonly IDictionary<string, object> _parameterValues;

            public NullComparisonTransformingVisitor(IDictionary<string, object> parameterValues)
            {
                _parameterValues = parameterValues;
            }

            protected override Expression VisitBinary(BinaryExpression expression)
            {
                if (expression.NodeType == ExpressionType.Equal
                    || expression.NodeType == ExpressionType.NotEqual)
                {
                    var parameter
                        = expression.Right as ParameterExpression
                          ?? expression.Left as ParameterExpression;

                    object parameterValue;
                    if (parameter != null
                        && _parameterValues.TryGetValue(parameter.Name, out parameterValue)
                        && parameterValue == null)
                    {
                        var columnExpression
                            = expression.Left.RemoveConvert().TryGetColumnExpression()
                              ?? expression.Right.RemoveConvert().TryGetColumnExpression();

                        if (columnExpression != null)
                        {
                            return
                                expression.NodeType == ExpressionType.Equal
                                    ? (Expression)new IsNullExpression(columnExpression)
                                    : Expression.Not(new IsNullExpression(columnExpression));
                        }
                    }
                }

                return base.VisitBinary(expression);
            }
        }
    }
}
