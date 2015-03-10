// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Sql
{
    public class DefaultSqlQueryGenerator : ThrowingExpressionTreeVisitor, ISqlExpressionVisitor, ISqlQueryGenerator
    {
        private IndentedStringBuilder _sql;
        private List<string> _parameters;
        private IDictionary<string, object> _parameterValues;

        public virtual string GenerateSql(
            SelectExpression selectExpression, IDictionary<string, object> parameterValues)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));
            Check.NotNull(parameterValues, nameof(parameterValues));

            _sql = new IndentedStringBuilder();
            _parameters = new List<string>();
            _parameterValues = parameterValues;

            selectExpression.Accept(this);

            return _sql.ToString();
        }

        public virtual IEnumerable<string> Parameters => _parameters;

        protected virtual IndentedStringBuilder Sql => _sql;

        protected virtual string ConcatOperator => "+";

        protected virtual string ParameterPrefix => "@";

        public virtual Expression VisitSelectExpression(SelectExpression selectExpression)
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
            else if (selectExpression.ProjectionExpression != null)
            {
                VisitExpression(selectExpression.ProjectionExpression);
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
                    VisitExpression(selectExpression.Predicate);

                    if (selectExpression.Predicate is ColumnExpression
                        || selectExpression.Predicate is ParameterExpression)
                    {
                        _sql.Append(" = 1");
                    }
                }
            }

            if (selectExpression.OrderBy.Any())
            {
                _sql.AppendLine()
                    .Append("ORDER BY ");

                VisitJoin(selectExpression.OrderBy, t =>
                    {
                        var columnExpression = t.Expression as ColumnExpression;
                        if (columnExpression != null)
                        {
                            _sql.Append(DelimitIdentifier(columnExpression.TableAlias))
                                .Append(".")
                                .Append(DelimitIdentifier(columnExpression.Name));
                        }
                        else
                        {
                            VisitExpression(t.Expression);
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
                    .Append(") AS ")
                    .Append(DelimitIdentifier(selectExpression.Alias));
            }

            return selectExpression;
        }

        private void VisitJoin(
            IReadOnlyList<Expression> expressions, Action<IndentedStringBuilder> joinAction = null)
        {
            VisitJoin(expressions, e => VisitExpression(e), joinAction);
        }

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

        public virtual Expression VisitTableExpression(TableExpression tableExpression)
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

        public virtual Expression VisitCrossJoinExpression(CrossJoinExpression crossJoinExpression)
        {
            Check.NotNull(crossJoinExpression, nameof(crossJoinExpression));

            _sql.Append("CROSS JOIN ");

            VisitExpression(crossJoinExpression.TableExpression);

            return crossJoinExpression;
        }

        public virtual Expression VisitCountExpression(CountExpression countExpression)
        {
            Check.NotNull(countExpression, nameof(countExpression));

            _sql.Append("COUNT(*)");

            return countExpression;
        }

        public virtual Expression VisitSumExpression(SumExpression sumExpression)
        {
            Check.NotNull(sumExpression, nameof(sumExpression));

            _sql.Append("SUM(");

            VisitExpression(sumExpression.Expression);

            _sql.Append(")");

            return sumExpression;
        }

        public virtual Expression VisitMinExpression(MinExpression minExpression)
        {
            Check.NotNull(minExpression, nameof(minExpression));

            _sql.Append("MIN(");

            VisitExpression(minExpression.Expression);

            _sql.Append(")");

            return minExpression;
        }

        public virtual Expression VisitMaxExpression(MaxExpression maxExpression)
        {
            Check.NotNull(maxExpression, nameof(maxExpression));

            _sql.Append("MAX(");

            VisitExpression(maxExpression.Expression);

            _sql.Append(")");

            return maxExpression;
        }

        public virtual Expression VisitInExpression(InExpression inExpression)
        {
            VisitExpression(inExpression.Column);

            _sql.Append(" IN (");

            VisitInExpressionValues(inExpression.Values);
            
            _sql.Append(")");

            return inExpression;
        }

        protected virtual Expression VisitNotInExpression(InExpression inExpression)
        {
            VisitExpression(inExpression.Column);

            _sql.Append(" NOT IN (");

            VisitInExpressionValues(inExpression.Values);

            _sql.Append(")");

            return inExpression;
        }

        protected virtual void VisitInExpressionValues(IReadOnlyList<Expression> inExpressionValues)
        {
            bool first = true;
            foreach (var inValue in inExpressionValues)
            {
                if (!first)
                {
                    _sql.Append(", ");
                }

                var inConstant = inValue as ConstantExpression;
                if (inConstant != null)
                {
                    VisitConstantExpression(inConstant);
                }

                var inParameter = inValue as ParameterExpression;
                if (inParameter != null)
                {
                    var parameterValue = _parameterValues[inParameter.Name];
                    var valuesCollection = parameterValue as IEnumerable;

                    if (valuesCollection != null 
                        && parameterValue.GetType() != typeof(string) 
                        && parameterValue.GetType() != typeof(byte[]))
                    {
                        foreach (var value in valuesCollection)
                        {
                            if (!first)
                            {
                                _sql.Append(", ");
                            }

                            _sql.Append(GenerateLiteral((dynamic)value));

                            first = false;
                        }
                    }
                    else
                    {
                        VisitParameterExpression(inParameter);
                    }
                }

                first = false;
            }
        }

        public virtual Expression VisitInnerJoinExpression(InnerJoinExpression innerJoinExpression)
        {
            Check.NotNull(innerJoinExpression, nameof(innerJoinExpression));

            _sql.Append("INNER JOIN ");

            VisitExpression(innerJoinExpression.TableExpression);

            _sql.Append(" ON ");

            VisitExpression(innerJoinExpression.Predicate);

            return innerJoinExpression;
        }

        public virtual Expression VisitOuterJoinExpression(LeftOuterJoinExpression leftOuterJoinExpression)
        {
            Check.NotNull(leftOuterJoinExpression, nameof(leftOuterJoinExpression));

            _sql.Append("LEFT JOIN ");

            VisitExpression(leftOuterJoinExpression.TableExpression);

            _sql.Append(" ON ");

            VisitExpression(leftOuterJoinExpression.Predicate);

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
                if (!selectExpression.OrderBy.Any())
                {
                    throw new InvalidOperationException(Strings.SkipNeedsOrderBy);
                }

                _sql.Append(" OFFSET ")
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

        public virtual Expression VisitCaseExpression(CaseExpression caseExpression)
        {
            Check.NotNull(caseExpression, nameof(caseExpression));

            _sql.AppendLine("CASE WHEN (");

            using (_sql.Indent())
            {
                VisitExpression(caseExpression.When);
            }

            _sql.Append(") THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END");

            return caseExpression;
        }

        public virtual Expression VisitExistsExpression(ExistsExpression existsExpression)
        {
            Check.NotNull(existsExpression, nameof(existsExpression));

            _sql.AppendLine("EXISTS (");

            using (_sql.Indent())
            {
                VisitExpression(existsExpression.Expression);
            }

            _sql.AppendLine()
                .AppendLine(")");

            return existsExpression;
        }

        protected override Expression VisitBinaryExpression([NotNull] BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            var maybeNullComparisonExpression = TransformNullComparison(binaryExpression);

            if (maybeNullComparisonExpression != null)
            {
                VisitExpression(maybeNullComparisonExpression);
            }
            else
            {
                var needClosingParen = false;

                var leftBinaryExpression = binaryExpression.Left as BinaryExpression;

                if (leftBinaryExpression != null
                    && leftBinaryExpression.NodeType != binaryExpression.NodeType)
                {
                    _sql.Append("(");

                    needClosingParen = true;
                }

                VisitExpression(binaryExpression.Left);

                if (binaryExpression.IsLogicalOperation()
                    && (binaryExpression.Left is ColumnExpression
                        || binaryExpression.Left is ParameterExpression))
                {
                    _sql.Append(" = 1");
                }

                if (needClosingParen)
                {
                    _sql.Append(")");
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
                        op = " " + ConcatOperator + " ";
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
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _sql.Append(op);

                needClosingParen = false;

                var rightBinaryExpression = binaryExpression.Right as BinaryExpression;

                if (rightBinaryExpression != null
                    && rightBinaryExpression.NodeType != binaryExpression.NodeType)
                {
                    _sql.Append("(");

                    needClosingParen = true;
                }

                VisitExpression(binaryExpression.Right);

                if (binaryExpression.IsLogicalOperation()
                    && (binaryExpression.Right is ColumnExpression
                        || binaryExpression.Right is ParameterExpression))
                {
                    _sql.Append(" = 1");
                }

                if (needClosingParen)
                {
                    _sql.Append(")");
                }
            }

            return binaryExpression;
        }

        private Expression TransformNullComparison(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                var parameter
                    = binaryExpression.Right as ParameterExpression
                      ?? binaryExpression.Left as ParameterExpression;

                object parameterValue;
                if (parameter != null
                    && _parameterValues.TryGetValue(parameter.Name, out parameterValue)
                    && parameterValue == null)
                {
                    var columnExpression
                        = binaryExpression.Left as ColumnExpression
                          ?? binaryExpression.Right as ColumnExpression;

                    if (columnExpression != null)
                    {
                        return
                            binaryExpression.NodeType == ExpressionType.Equal
                                ? (Expression)new IsNullExpression(columnExpression)
                                : new IsNotNullExpression(columnExpression);
                    }
                }
            }

            return null;
        }

        public virtual Expression VisitColumnExpression(ColumnExpression columnExpression)
        {
            Check.NotNull(columnExpression, nameof(columnExpression));

            _sql.Append(DelimitIdentifier(columnExpression.TableAlias))
                .Append(".")
                .Append(DelimitIdentifier(columnExpression.Name));

            if (columnExpression.Alias != null)
            {
                _sql.Append(" AS ")
                    .Append(DelimitIdentifier(columnExpression.Alias));
            }

            return columnExpression;
        }

        public virtual Expression VisitIsNullExpression(IsNullExpression isNullExpression)
        {
            Check.NotNull(isNullExpression, nameof(isNullExpression));

            VisitExpression(isNullExpression.Operand);

            _sql.Append(" IS NULL");

            return isNullExpression;
        }

        public virtual Expression VisitIsNotNullExpression(IsNotNullExpression isNotNullExpression)
        {
            Check.NotNull(isNotNullExpression, nameof(isNotNullExpression));

            VisitExpression(isNotNullExpression.Operand);

            _sql.Append(" IS NOT NULL");

            return isNotNullExpression;
        }

        public virtual Expression VisitLikeExpression(LikeExpression likeExpression)
        {
            Check.NotNull(likeExpression, nameof(likeExpression));

            VisitExpression(likeExpression.Match);

            _sql.Append(" LIKE ");

            VisitExpression(likeExpression.Pattern);

            return likeExpression;
        }

        public virtual Expression VisitLiteralExpression(LiteralExpression literalExpression)
        {
            Check.NotNull(literalExpression, nameof(literalExpression));

            _sql.Append(GenerateLiteral(literalExpression.Literal));

            return literalExpression;
        }

        protected override Expression VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            Check.NotNull(unaryExpression, nameof(unaryExpression));

            if (unaryExpression.NodeType == ExpressionType.Not)
            {
                var inExpression = unaryExpression.Operand as InExpression;

                if (inExpression != null)
                {
                    return VisitNotInExpression(inExpression);
                }

                var isColumnOperand = unaryExpression.Operand is ColumnExpression;

                if (!isColumnOperand)
                {
                    _sql.Append("NOT ");
                }

                VisitExpression(unaryExpression.Operand);

                if (isColumnOperand)
                {
                    _sql.Append(" = 0");
                }

                return unaryExpression;
            }

            if (unaryExpression.NodeType == ExpressionType.Convert)
            {
                VisitExpression(unaryExpression.Operand);

                return unaryExpression;
            }

            return base.VisitUnaryExpression(unaryExpression);
        }

        protected override Expression VisitConstantExpression(ConstantExpression constantExpression)
        {
            Check.NotNull(constantExpression, nameof(constantExpression));

            _sql.Append(constantExpression.Value == null
                ? "NULL"
                : GenerateLiteral((dynamic)constantExpression.Value));

            return constantExpression;
        }

        protected override Expression VisitParameterExpression(ParameterExpression parameterExpression)
        {
            _sql.Append(ParameterPrefix + parameterExpression.Name);

            _parameters.Add(parameterExpression.Name);

            return parameterExpression;
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            return new NotImplementedException(visitMethod);
        }

        // TODO: Share the code below (#1559)

        private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK";
        private const string DateTimeOffsetFormat = "yyyy-MM-ddTHH:mm:ss.fffzzz";

        protected virtual string GenerateLiteral([NotNull] object value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }

        protected virtual string GenerateLiteral(bool value)
        {
            return value ? "1" : "0";
        }

        protected virtual string GenerateLiteral([NotNull] string value)
        {
            Check.NotNull(value, nameof(value));

            return "'" + EscapeLiteral(value) + "'";
        }

        protected virtual string GenerateLiteral(Guid value)
        {
            return "'" + value + "'";
        }

        protected virtual string GenerateLiteral(DateTime value)
        {
            return "'" + value.ToString(DateTimeFormat, CultureInfo.InvariantCulture) + "'";
        }

        protected virtual string GenerateLiteral(DateTimeOffset value)
        {
            return "'" + value.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture) + "'";
        }

        protected virtual string GenerateLiteral(TimeSpan value)
        {
            return "'" + value + "'";
        }

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
        {
            Check.NotNull(literal, nameof(literal));

            return literal.Replace("'", "''");
        }

        protected virtual string DelimitIdentifier([NotNull] string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));

            return "\"" + identifier + "\"";
        }
    }
}
