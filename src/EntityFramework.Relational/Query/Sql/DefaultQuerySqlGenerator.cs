// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Query.ExpressionVisitors.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.Sql
{
    public class DefaultQuerySqlGenerator : ThrowingExpressionVisitor, ISqlExpressionVisitor, ISqlQueryGenerator
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;

        private RelationalCommandBuilder _sql;
        private ParameterNameGenerator _parameterNameGenerator;
        private IDictionary<string, object> _parameterValues;

        private static readonly Dictionary<ExpressionType, string> _binaryOperatorMap = new Dictionary<ExpressionType, string>
        {
            { ExpressionType.Equal, " = " },
            { ExpressionType.NotEqual, " <> " },
            { ExpressionType.GreaterThan, " > " },
            { ExpressionType.GreaterThanOrEqual, " >= " },
            { ExpressionType.LessThan, " < " },
            { ExpressionType.LessThanOrEqual, " <= " },
            { ExpressionType.AndAlso, " AND " },
            { ExpressionType.OrElse, " OR " },
            { ExpressionType.Subtract, " - " },
            { ExpressionType.Multiply, " * " },
            { ExpressionType.Divide, " / " },
            { ExpressionType.Modulo, " % " },
        };

        public DefaultQuerySqlGenerator(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerator sqlGenerator,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));
            Check.NotNull(selectExpression, nameof(selectExpression));

            _commandBuilderFactory = commandBuilderFactory;
            _sqlGenerator = sqlGenerator;
            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
            SelectExpression = selectExpression;
        }

        protected virtual SelectExpression SelectExpression { get; }

        protected virtual ISqlGenerator SqlGenerator => _sqlGenerator;

        protected virtual ParameterNameGenerator ParameterNameGenerator => _parameterNameGenerator;

        public virtual RelationalCommand GenerateSql(IDictionary<string, object> parameterValues)
        {
            Check.NotNull(parameterValues, nameof(parameterValues));

            _sql =  _commandBuilderFactory.Create();
            _parameterNameGenerator = _parameterNameGeneratorFactory.Create();
            _parameterValues = parameterValues;

            Visit(SelectExpression);

            return _sql.BuildRelationalCommand();
        }

        public virtual IRelationalValueBufferFactory CreateValueBufferFactory(
            IRelationalValueBufferFactoryFactory relationalValueBufferFactoryFactory, DbDataReader _)
        {
            Check.NotNull(relationalValueBufferFactoryFactory, nameof(relationalValueBufferFactoryFactory));

            return relationalValueBufferFactoryFactory
                .Create(SelectExpression.GetProjectionTypes().ToArray(), indexMap: null);
        }

        protected virtual RelationalCommandBuilder Sql => _sql;

        protected virtual string ConcatOperator => "+";
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

            var projectionAdded = false;
            if (selectExpression.IsProjectStar)
            {
                _sql.Append(_sqlGenerator.DelimitIdentifier(selectExpression.Tables.Single().Alias))
                    .Append(".*");
                projectionAdded = true;
            }

            if (selectExpression.Projection.Any())
            {
                if (selectExpression.IsProjectStar)
                {
                    _sql.Append(", ");
                }
                VisitJoin(selectExpression.Projection);
                projectionAdded = true;
            }

            if (!projectionAdded)
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
                        var optimizedNullExpansionVisitor = new RelationalNullsOptimizedExpandingVisitor();
                        var relationalNullsExpandedOptimized = optimizedNullExpansionVisitor.Visit(predicate);
                        if (optimizedNullExpansionVisitor.OptimizedExpansionPossible)
                        {
                            predicate = relationalNullsExpandedOptimized;
                        }
                        else
                        {
                            predicate = new RelationalNullsExpandingVisitor()
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
                _sql.AppendLine();
                GenerateOrderBy(selectExpression.OrderBy);
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
                        .Append(_sqlGenerator.DelimitIdentifier(selectExpression.Alias));
                }
            }

            return selectExpression;
        }

        protected virtual void GenerateOrderBy([NotNull] IReadOnlyList<Ordering> orderings)
        {
            _sql.Append("ORDER BY ");

            VisitJoin(orderings, t =>
                {
                    var aliasExpression = t.Expression as AliasExpression;

                    if (aliasExpression != null)
                    {
                        if (aliasExpression.Alias != null)
                        {
                            var columnExpression = aliasExpression.TryGetColumnExpression();

                            if (columnExpression != null)
                            {
                                _sql.Append(_sqlGenerator.DelimitIdentifier(columnExpression.TableAlias))
                                    .Append(".");
                            }

                            _sql.Append(_sqlGenerator.DelimitIdentifier(aliasExpression.Alias));
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

        private void VisitJoin(
            IReadOnlyList<Expression> expressions, Action<RelationalCommandBuilder> joinAction = null)
            => VisitJoin(expressions, e => Visit(e), joinAction);

        private void VisitJoin<T>(
            IReadOnlyList<T> items, Action<T> itemAction, Action<RelationalCommandBuilder> joinAction = null)
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
                var substitutions = new string[rawSqlDerivedTableExpression.Parameters.Length];

                for (var index = 0; index < substitutions.Length; index++)
                {
                    substitutions[index] =
                        _sqlGenerator.GenerateParameterName(
                            ParameterNameGenerator.GenerateNext());

                    _sql.AddParameter(
                        substitutions[index],
                        rawSqlDerivedTableExpression.Parameters[index]);
                }

                _sql.AppendLines(
                    // ReSharper disable once CoVariantArrayConversion
                    string.Format(rawSqlDerivedTableExpression.Sql, substitutions));
            }

            _sql.Append(") AS ")
                .Append(_sqlGenerator.DelimitIdentifier(rawSqlDerivedTableExpression.Alias));

            return rawSqlDerivedTableExpression;
        }

        public virtual Expression VisitTable(TableExpression tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            if (tableExpression.Schema != null)
            {
                _sql.Append(_sqlGenerator.DelimitIdentifier(tableExpression.Schema))
                    .Append(".");
            }

            _sql.Append(_sqlGenerator.DelimitIdentifier(tableExpression.Table))
                .Append(" AS ")
                .Append(_sqlGenerator.DelimitIdentifier(tableExpression.Alias));

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

        public virtual Expression VisitStringCompare(StringCompareExpression stringCompareExpression)
        {
            Visit(stringCompareExpression.Left);
            _sql.Append(GenerateBinaryOperator(stringCompareExpression.Operator));
            Visit(stringCompareExpression.Right);

            return stringCompareExpression;
        }

        public virtual Expression VisitIn(InExpression inExpression)
        {
            if (inExpression.Values != null)
            {
                var inValues = ProcessInExpressionValues(inExpression.Values);
                var inValuesNotNull = ExtractNonNullExpressionValues(inValues);

                if (inValues.Count != inValuesNotNull.Count)
                {
                    var relatioalNullsInExpression = Expression.OrElse(
                        new InExpression(inExpression.Operand, inValuesNotNull),
                        new IsNullExpression(inExpression.Operand));

                    return Visit(relatioalNullsInExpression);
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

        protected virtual Expression VisitNotIn([NotNull] InExpression inExpression)
        {
            if (inExpression.Values != null)
            {
                var inValues = ProcessInExpressionValues(inExpression.Values);
                var inValuesNotNull = ExtractNonNullExpressionValues(inValues);

                if (inValues.Count != inValuesNotNull.Count)
                {
                    var relationalNullsNotInExpression = Expression.AndAlso(
                        Expression.Not(new InExpression(inExpression.Operand, inValuesNotNull)),
                        Expression.Not(new IsNullExpression(inExpression.Operand)));

                    return Visit(relationalNullsNotInExpression);
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
            [NotNull] IReadOnlyList<Expression> inExpressionValues)
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
                _sql.Append("WHEN ");

                Visit(expression.Test);

                _sql.AppendLine();
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

            _sql.Append(")");

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
                var needParens = binaryExpression.Left is BinaryExpression;

                if (needParens)
                {
                    _sql.Append("(");
                }

                Visit(binaryExpression.Left);

                if (needParens)
                {
                    _sql.Append(")");
                }

                if (binaryExpression.IsLogicalOperation()
                    && binaryExpression.Left.IsSimpleExpression())
                {
                    _sql.Append(" = ");
                    _sql.Append(TrueLiteral);
                }

                string op;
                if (!TryGenerateBinaryOperator(binaryExpression.NodeType, out op))
                {
                    switch (binaryExpression.NodeType)
                    {
                        case ExpressionType.Add:
                            op = binaryExpression.Type == typeof(string)
                                ? " " + ConcatOperator + " "
                                : " + ";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                _sql.Append(op);

                needParens = binaryExpression.Right is BinaryExpression;

                if (needParens)
                {
                    _sql.Append("(");
                }

                Visit(binaryExpression.Right);

                if (needParens)
                {
                    _sql.Append(")");
                }

                if (binaryExpression.IsLogicalOperation()
                    && binaryExpression.Right.IsSimpleExpression())
                {
                    _sql.Append(" = ");
                    _sql.Append(TrueLiteral);
                }
            }

            return binaryExpression;
        }

        public virtual Expression VisitColumn(ColumnExpression columnExpression)
        {
            Check.NotNull(columnExpression, nameof(columnExpression));

            _sql.Append(_sqlGenerator.DelimitIdentifier(columnExpression.TableAlias))
                .Append(".")
                .Append(_sqlGenerator.DelimitIdentifier(columnExpression.Name));

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
                _sql.Append(_sqlGenerator.DelimitIdentifier(aliasExpression.Alias));
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

            _sql.Append(_sqlGenerator.GenerateLiteral(literalExpression.Literal));

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
                : _sqlGenerator.GenerateLiteral(constantExpression.Value));

            return constantExpression;
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            Check.NotNull(parameterExpression, nameof(parameterExpression));

            object value;
            if (!_parameterValues.TryGetValue(parameterExpression.Name, out value))
            {
                value = string.Empty;
            }

            var name = _sqlGenerator.GenerateParameterName(parameterExpression.Name);

            _sql.Append(name);

            _sql.AddParameter(name, value, parameterExpression.Type);

            return parameterExpression;
        }

        protected virtual bool TryGenerateBinaryOperator(ExpressionType op, [NotNull] out string result) 
            => _binaryOperatorMap.TryGetValue(op, out result);

        protected virtual string GenerateBinaryOperator(ExpressionType op) => _binaryOperatorMap[op];

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
            => new NotImplementedException(visitMethod);

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
