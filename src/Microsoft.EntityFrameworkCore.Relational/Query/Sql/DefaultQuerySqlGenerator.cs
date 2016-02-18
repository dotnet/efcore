// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.Sql
{
    public class DefaultQuerySqlGenerator : ThrowingExpressionVisitor, ISqlExpressionVisitor, IQuerySqlGenerator
    {
        private readonly IRelationalCommandBuilderFactory _relationalCommandBuilderFactory;
        private readonly ISqlGenerationHelper _sqlGenerationHelper;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
        private readonly IRelationalTypeMapper _relationalTypeMapper;

        private IRelationalCommandBuilder _relationalCommandBuilder;
        private IReadOnlyDictionary<string, object> _parametersValues;
        private ParameterNameGenerator _parameterNameGenerator;

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
            { ExpressionType.Modulo, " % " }
        };

        public DefaultQuerySqlGenerator(
            [NotNull] IRelationalCommandBuilderFactory relationalCommandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IRelationalTypeMapper relationalTypeMapper,
            [NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(relationalCommandBuilderFactory, nameof(relationalCommandBuilderFactory));
            Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));
            Check.NotNull(relationalTypeMapper, nameof(relationalTypeMapper));
            Check.NotNull(selectExpression, nameof(selectExpression));

            _relationalCommandBuilderFactory = relationalCommandBuilderFactory;
            _sqlGenerationHelper = sqlGenerationHelper;
            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
            _relationalTypeMapper = relationalTypeMapper;

            SelectExpression = selectExpression;
        }

        protected virtual SelectExpression SelectExpression { get; }

        protected virtual ISqlGenerationHelper SqlGenerator => _sqlGenerationHelper;

        protected virtual IReadOnlyDictionary<string, object> ParameterValues => _parametersValues;

        public virtual IRelationalCommand GenerateSql(IReadOnlyDictionary<string, object> parameterValues)
        {
            Check.NotNull(parameterValues, nameof(parameterValues));

            _relationalCommandBuilder = _relationalCommandBuilderFactory.Create();
            _parameterNameGenerator = _parameterNameGeneratorFactory.Create();

            _parametersValues = parameterValues;

            Visit(SelectExpression);

            return _relationalCommandBuilder.Build();
        }

        public virtual IRelationalValueBufferFactory CreateValueBufferFactory(
            IRelationalValueBufferFactoryFactory relationalValueBufferFactoryFactory, DbDataReader dataReader)
        {
            Check.NotNull(relationalValueBufferFactoryFactory, nameof(relationalValueBufferFactoryFactory));

            return relationalValueBufferFactoryFactory
                .Create(SelectExpression.GetProjectionTypes().ToArray(), indexMap: null);
        }

        protected virtual IRelationalCommandBuilder Sql => _relationalCommandBuilder;

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
                _relationalCommandBuilder.AppendLine("(");

                subQueryIndent = _relationalCommandBuilder.Indent();
            }

            _relationalCommandBuilder.Append("SELECT ");

            if (selectExpression.IsDistinct)
            {
                _relationalCommandBuilder.Append("DISTINCT ");
            }

            GenerateTop(selectExpression);

            var projectionAdded = false;

            if (selectExpression.IsProjectStar)
            {
                _relationalCommandBuilder
                    .Append(_sqlGenerationHelper.DelimitIdentifier(selectExpression.Tables.Last().Alias))
                    .Append(".*");

                projectionAdded = true;
            }

            if (selectExpression.Projection.Any())
            {
                if (selectExpression.IsProjectStar)
                {
                    _relationalCommandBuilder.Append(", ");
                }

                VisitProjection(selectExpression.Projection);

                projectionAdded = true;
            }

            if (!projectionAdded)
            {
                _relationalCommandBuilder.Append("1");
            }

            if (selectExpression.Tables.Any())
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("FROM ");

                VisitJoin(selectExpression.Tables, sql => sql.AppendLine());
            }

            if (selectExpression.Predicate != null)
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("WHERE ");

                var constantExpression = selectExpression.Predicate as ConstantExpression;

                if (constantExpression != null)
                {
                    _relationalCommandBuilder.Append((bool)constantExpression.Value ? "1 = 1" : "1 = 0");
                }
                else
                {
                    Visit(ApplyNullSemantics(selectExpression.Predicate));

                    if (selectExpression.Predicate is ParameterExpression
                        || selectExpression.Predicate.IsAliasWithColumnExpression()
                        || selectExpression.Predicate is SelectExpression)
                    {
                        _relationalCommandBuilder.Append(" = ");
                        _relationalCommandBuilder.Append(TrueLiteral);
                    }
                }
            }

            if (selectExpression.OrderBy.Any())
            {
                _relationalCommandBuilder.AppendLine();

                GenerateOrderBy(selectExpression.OrderBy);
            }

            GenerateLimitOffset(selectExpression);

            if (subQueryIndent != null)
            {
                subQueryIndent.Dispose();

                _relationalCommandBuilder.AppendLine()
                    .Append(")");

                if (selectExpression.Alias.Length > 0)
                {
                    _relationalCommandBuilder.Append(" AS ")
                        .Append(_sqlGenerationHelper.DelimitIdentifier(selectExpression.Alias));
                }
            }

            return selectExpression;
        }

        private Expression ApplyNullSemantics(Expression expression)
        {
            var newExpression
                        = new NullComparisonTransformingVisitor(_parametersValues)
                            .Visit(expression);

            var relationalNullsOptimizedExpandingVisitor = new RelationalNullsOptimizedExpandingVisitor();
            var optimizedExpression = relationalNullsOptimizedExpandingVisitor.Visit(newExpression);

            newExpression
                = relationalNullsOptimizedExpandingVisitor.IsOptimalExpansion
                    ? optimizedExpression
                    : new RelationalNullsExpandingVisitor().Visit(newExpression);

            newExpression = new PredicateNegationExpressionOptimizer().Visit(newExpression);
            newExpression = new ReducingExpressionVisitor().Visit(newExpression);

            return newExpression;
        }

        protected virtual void VisitProjection([NotNull] IReadOnlyList<Expression> projections) => VisitJoin(
            projections
                .Select(ApplyNullSemantics)
                .ToList());

        protected virtual void GenerateOrderBy([NotNull] IReadOnlyList<Ordering> orderings)
        {
            _relationalCommandBuilder.Append("ORDER BY ");

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
                                _relationalCommandBuilder
                                    .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.TableAlias))
                                    .Append(".");
                            }

                            _relationalCommandBuilder.Append(_sqlGenerationHelper.DelimitIdentifier(aliasExpression.Alias));
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
                        _relationalCommandBuilder.Append(" DESC");
                    }
                });
        }

        private void VisitJoin(
            IReadOnlyList<Expression> expressions, Action<IRelationalCommandBuilder> joinAction = null)
            => VisitJoin(expressions, e => Visit(e), joinAction);

        private void VisitJoin<T>(
            IReadOnlyList<T> items, Action<T> itemAction, Action<IRelationalCommandBuilder> joinAction = null)
        {
            joinAction = joinAction ?? (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(_relationalCommandBuilder);
                }

                itemAction(items[i]);
            }
        }

        public virtual Expression VisitFromSql(FromSqlExpression fromSqlExpression)
        {
            Check.NotNull(fromSqlExpression, nameof(fromSqlExpression));

            _relationalCommandBuilder.AppendLine("(");

            using (_relationalCommandBuilder.Indent())
            {
                GenerateFromSql(fromSqlExpression.Sql, fromSqlExpression.Arguments, _parametersValues);
            }

            _relationalCommandBuilder.Append(") AS ")
                .Append(_sqlGenerationHelper.DelimitIdentifier(fromSqlExpression.Alias));

            return fromSqlExpression;
        }

        protected virtual void GenerateFromSql(
            [NotNull] string sql,
            [NotNull] Expression arguments,
            [NotNull] IReadOnlyDictionary<string, object> parameters)
        {
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(parameters, nameof(parameters));

            string[] substitutions = null;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (arguments.NodeType)
            {
                case ExpressionType.Parameter:
                {
                    var parameterExpression = (ParameterExpression)arguments;

                    object parameterValue;
                    if (parameters.TryGetValue(parameterExpression.Name, out parameterValue))
                    {
                        var argumentValues = (object[])parameterValue;
                        var relationalParameters = new IRelationalParameter[argumentValues.Length];

                        substitutions = new string[argumentValues.Length];

                        for (var i = 0; i < argumentValues.Length; i++)
                        {
                            var parameterName = _parameterNameGenerator.GenerateNext();

                            substitutions[i] = SqlGenerator.GenerateParameterName(parameterName);

                            var value = argumentValues[i];

                            relationalParameters[i]
                                = _relationalCommandBuilder
                                    .CreateParameter(
                                        substitutions[i],
                                        value,
                                        t => t.GetMappingForValue(value),
                                        value?.GetType().IsNullableType(),
                                        parameterName);
                        }

                        _relationalCommandBuilder.AddParameter(
                            new CompositeRelationalParameter(
                                parameterExpression.Name,
                                relationalParameters));
                    }

                    break;
                }
                case ExpressionType.Constant:
                {
                    var constantExpression = (ConstantExpression)arguments;
                    var argumentValues = (object[])constantExpression.Value;

                    substitutions = new string[argumentValues.Length];

                    for (var i = 0; i < argumentValues.Length; i++)
                    {
                        substitutions[i] = SqlGenerator.GenerateLiteral(argumentValues[i]);
                    }

                    break;
                }
                case ExpressionType.NewArrayInit:
                {
                    var newArrayExpression = (NewArrayExpression)arguments;

                    substitutions = new string[newArrayExpression.Expressions.Count];

                    for (var i = 0; i < newArrayExpression.Expressions.Count; i++)
                    {
                        var expression = newArrayExpression.Expressions[i].RemoveConvert();

                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (expression.NodeType)
                        {
                            case ExpressionType.Constant:
                            {
                                substitutions[i]
                                    = SqlGenerator
                                        .GenerateLiteral(((ConstantExpression)expression).Value);

                                break;
                            }
                            case ExpressionType.Parameter:
                            {
                                var parameter = (ParameterExpression)expression;

                                object value;
                                if (_parametersValues.TryGetValue(parameter.Name, out value))
                                {
                                    var parameterName = _sqlGenerationHelper.GenerateParameterName(parameter.Name);

                                    substitutions[i] = parameterName;

                                    _relationalCommandBuilder.AddParameter(parameterName, value, parameter.Name);
                                }

                                break;
                            }
                        }
                    }

                    break;
                }
            }

            if (substitutions != null)
            {
                // ReSharper disable once CoVariantArrayConversion
                sql = string.Format(sql, substitutions);
            }

            _relationalCommandBuilder.AppendLines(sql);
        }

        public virtual Expression VisitTable(TableExpression tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            if (tableExpression.Schema != null)
            {
                _relationalCommandBuilder.Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Schema))
                    .Append(".");
            }

            _relationalCommandBuilder.Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Table))
                .Append(" AS ")
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }

        public virtual Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
        {
            Check.NotNull(crossJoinExpression, nameof(crossJoinExpression));

            _relationalCommandBuilder.Append("CROSS JOIN ");

            Visit(crossJoinExpression.TableExpression);

            return crossJoinExpression;
        }

        public virtual Expression VisitLateralJoin(LateralJoinExpression lateralJoinExpression)
        {
            Check.NotNull(lateralJoinExpression, nameof(lateralJoinExpression));

            _relationalCommandBuilder.Append("CROSS JOIN LATERAL ");

            Visit(lateralJoinExpression.TableExpression);

            return lateralJoinExpression;
        }

        public virtual Expression VisitCount(CountExpression countExpression)
        {
            Check.NotNull(countExpression, nameof(countExpression));

            _relationalCommandBuilder.Append("COUNT(*)");

            return countExpression;
        }

        public virtual Expression VisitSum(SumExpression sumExpression)
        {
            Check.NotNull(sumExpression, nameof(sumExpression));

            _relationalCommandBuilder.Append("SUM(");

            Visit(sumExpression.Expression);

            _relationalCommandBuilder.Append(")");

            return sumExpression;
        }

        public virtual Expression VisitMin(MinExpression minExpression)
        {
            Check.NotNull(minExpression, nameof(minExpression));

            _relationalCommandBuilder.Append("MIN(");

            Visit(minExpression.Expression);

            _relationalCommandBuilder.Append(")");

            return minExpression;
        }

        public virtual Expression VisitMax(MaxExpression maxExpression)
        {
            Check.NotNull(maxExpression, nameof(maxExpression));

            _relationalCommandBuilder.Append("MAX(");

            Visit(maxExpression.Expression);

            _relationalCommandBuilder.Append(")");

            return maxExpression;
        }

        public virtual Expression VisitStringCompare(StringCompareExpression stringCompareExpression)
        {
            Visit(stringCompareExpression.Left);

            _relationalCommandBuilder.Append(GenerateBinaryOperator(stringCompareExpression.Operator));

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
                    var relationalNullsInExpression
                        = Expression.OrElse(
                            new InExpression(inExpression.Operand, inValuesNotNull),
                            new IsNullExpression(inExpression.Operand));

                    return Visit(relationalNullsInExpression);
                }

                if (inValuesNotNull.Count > 0)
                {
                    Visit(inExpression.Operand);

                    _relationalCommandBuilder.Append(" IN (");

                    VisitJoin(inValuesNotNull);

                    _relationalCommandBuilder.Append(")");
                }
                else
                {
                    _relationalCommandBuilder.Append("1 = 0");
                }
            }
            else
            {
                Visit(inExpression.Operand);

                _relationalCommandBuilder.Append(" IN ");

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
                    var relationalNullsNotInExpression
                        = Expression.AndAlso(
                            Expression.Not(new InExpression(inExpression.Operand, inValuesNotNull)),
                            Expression.Not(new IsNullExpression(inExpression.Operand)));

                    return Visit(relationalNullsNotInExpression);
                }

                if (inValues.Count > 0)
                {
                    Visit(inExpression.Operand);

                    _relationalCommandBuilder.Append(" NOT IN (");

                    VisitJoin(inValues);

                    _relationalCommandBuilder.Append(")");
                }
                else
                {
                    _relationalCommandBuilder.Append("1 = 1");
                }
            }
            else
            {
                Visit(inExpression.Operand);

                _relationalCommandBuilder.Append(" NOT IN ");

                Visit(inExpression.SubQuery);
            }

            return inExpression;
        }

        protected virtual IReadOnlyList<Expression> ProcessInExpressionValues(
            [NotNull] IEnumerable<Expression> inExpressionValues)
        {
            Check.NotNull(inExpressionValues, nameof(inExpressionValues));

            var inConstants = new List<Expression>();

            foreach (var inValue in inExpressionValues)
            {
                var inConstant = inValue as ConstantExpression;

                if (inConstant != null)
                {
                    AddInExpressionValues(inConstant.Value, inConstants, inConstant);
                }
                else
                {
                    var inParameter = inValue as ParameterExpression;

                    if (inParameter != null)
                    {
                        object parameterValue;
                        if (_parametersValues.TryGetValue(inParameter.Name, out parameterValue))
                        {
                            AddInExpressionValues(parameterValue, inConstants, inParameter);
                        }
                    }
                    else
                    {
                        var inListInit = inValue as ListInitExpression;

                        if (inListInit != null)
                        {
                            inConstants.AddRange(ProcessInExpressionValues(
                                inListInit.Initializers.SelectMany(i => i.Arguments)));
                        }
                        else
                        {
                            var newArray = inValue as NewArrayExpression;

                            if (newArray != null)
                            {
                                inConstants.AddRange(ProcessInExpressionValues(newArray.Expressions));
                            }
                        }
                    }
                }
            }

            return inConstants;
        }

        private static void AddInExpressionValues(
            object value, List<Expression> inConstants, Expression expression)
        {
            var valuesEnumerable = value as IEnumerable;

            if (valuesEnumerable != null
                && value.GetType() != typeof(string)
                && value.GetType() != typeof(byte[]))
            {
                inConstants.AddRange(valuesEnumerable.Cast<object>().Select(Expression.Constant));
            }
            else
            {
                inConstants.Add(expression);
            }
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

                    if (_parametersValues.TryGetValue(inParameter.Name, out parameterValue))
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

            _relationalCommandBuilder.Append("INNER JOIN ");

            Visit(innerJoinExpression.TableExpression);

            _relationalCommandBuilder.Append(" ON ");

            Visit(innerJoinExpression.Predicate);

            return innerJoinExpression;
        }

        public virtual Expression VisitLeftOuterJoin(LeftOuterJoinExpression leftOuterJoinExpression)
        {
            Check.NotNull(leftOuterJoinExpression, nameof(leftOuterJoinExpression));

            _relationalCommandBuilder.Append("LEFT JOIN ");

            Visit(leftOuterJoinExpression.TableExpression);

            _relationalCommandBuilder.Append(" ON ");

            Visit(leftOuterJoinExpression.Predicate);

            return leftOuterJoinExpression;
        }

        protected virtual void GenerateTop([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Limit != null
                && selectExpression.Offset == null)
            {
                _relationalCommandBuilder.Append("TOP(");

                Visit(selectExpression.Limit);

                _relationalCommandBuilder.Append(") ");
            }
        }

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
        }

        protected override Expression VisitConditional(ConditionalExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            _relationalCommandBuilder.AppendLine("CASE");

            using (_relationalCommandBuilder.Indent())
            {
                _relationalCommandBuilder.Append("WHEN ");

                Visit(expression.Test);

                _relationalCommandBuilder.AppendLine();
                _relationalCommandBuilder.Append("THEN ");

                var constantIfTrue = expression.IfTrue as ConstantExpression;

                if (constantIfTrue != null
                    && constantIfTrue.Type == typeof(bool))
                {
                    _relationalCommandBuilder.Append((bool)constantIfTrue.Value ? TypedTrueLiteral : TypedFalseLiteral);
                }
                else
                {
                    Visit(expression.IfTrue);
                }

                _relationalCommandBuilder.Append(" ELSE ");

                var constantIfFalse = expression.IfFalse as ConstantExpression;

                if (constantIfFalse != null
                    && constantIfFalse.Type == typeof(bool))
                {
                    _relationalCommandBuilder.Append((bool)constantIfFalse.Value ? TypedTrueLiteral : TypedFalseLiteral);
                }
                else
                {
                    Visit(expression.IfFalse);
                }

                _relationalCommandBuilder.AppendLine();
            }

            _relationalCommandBuilder.Append("END");

            return expression;
        }

        public virtual Expression VisitExists(ExistsExpression existsExpression)
        {
            Check.NotNull(existsExpression, nameof(existsExpression));

            _relationalCommandBuilder.AppendLine("EXISTS (");

            using (_relationalCommandBuilder.Indent())
            {
                Visit(existsExpression.Expression);
            }

            _relationalCommandBuilder.Append(")");

            return existsExpression;
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (expression.NodeType == ExpressionType.Coalesce)
            {
                _relationalCommandBuilder.Append("COALESCE(");
                Visit(expression.Left);
                _relationalCommandBuilder.Append(", ");
                Visit(expression.Right);
                _relationalCommandBuilder.Append(")");
            }
            else
            {
                var needParens = expression.Left is BinaryExpression;

                if (needParens)
                {
                    _relationalCommandBuilder.Append("(");
                }

                Visit(expression.Left);

                if (needParens)
                {
                    _relationalCommandBuilder.Append(")");
                }

                if (expression.IsLogicalOperation()
                    && (expression.Left.IsSimpleExpression()
                        || expression.Left is SelectExpression))
                {
                    _relationalCommandBuilder.Append(" = ");
                    _relationalCommandBuilder.Append(TrueLiteral);
                }

                string op;
                if (!TryGenerateBinaryOperator(expression.NodeType, out op))
                {
                    switch (expression.NodeType)
                    {
                        case ExpressionType.Add:
                            op = expression.Type == typeof(string)
                                ? " " + ConcatOperator + " "
                                : " + ";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                _relationalCommandBuilder.Append(op);

                needParens = expression.Right is BinaryExpression;

                if (needParens)
                {
                    _relationalCommandBuilder.Append("(");
                }

                Visit(expression.Right);

                if (needParens)
                {
                    _relationalCommandBuilder.Append(")");
                }

                if (expression.IsLogicalOperation()
                    && (expression.Right.IsSimpleExpression()
                        || expression.Right is SelectExpression))
                {
                    _relationalCommandBuilder.Append(" = ");
                    _relationalCommandBuilder.Append(TrueLiteral);
                }
            }

            return expression;
        }

        public virtual Expression VisitColumn(ColumnExpression columnExpression)
        {
            Check.NotNull(columnExpression, nameof(columnExpression));

            _relationalCommandBuilder.Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.TableAlias))
                .Append(".")
                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.Name));

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
                    _relationalCommandBuilder.Append(" AS ");
                }
            }

            if (aliasExpression.Alias != null)
            {
                _relationalCommandBuilder.Append(_sqlGenerationHelper.DelimitIdentifier(aliasExpression.Alias));
            }

            return aliasExpression;
        }

        public virtual Expression VisitIsNull(IsNullExpression isNullExpression)
        {
            Check.NotNull(isNullExpression, nameof(isNullExpression));

            Visit(isNullExpression.Operand);

            _relationalCommandBuilder.Append(" IS NULL");

            return isNullExpression;
        }

        public virtual Expression VisitIsNotNull([NotNull] IsNullExpression isNotNullExpression)
        {
            Check.NotNull(isNotNullExpression, nameof(isNotNullExpression));

            Visit(isNotNullExpression.Operand);

            _relationalCommandBuilder.Append(" IS NOT NULL");

            return isNotNullExpression;
        }

        public virtual Expression VisitLike(LikeExpression likeExpression)
        {
            Check.NotNull(likeExpression, nameof(likeExpression));

            Visit(likeExpression.Match);

            _relationalCommandBuilder.Append(" LIKE ");

            Visit(likeExpression.Pattern);

            return likeExpression;
        }

        public virtual Expression VisitLiteral(LiteralExpression literalExpression)
        {
            Check.NotNull(literalExpression, nameof(literalExpression));

            _relationalCommandBuilder.Append(_sqlGenerationHelper.GenerateLiteral(literalExpression.Literal));

            return literalExpression;
        }

        public virtual Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            _relationalCommandBuilder.Append(sqlFunctionExpression.FunctionName);
            _relationalCommandBuilder.Append("(");

            VisitJoin(sqlFunctionExpression.Arguments.ToList());

            _relationalCommandBuilder.Append(")");

            return sqlFunctionExpression;
        }

        public virtual Expression VisitExplicitCast(ExplicitCastExpression explicitCastExpression)
        {
            _relationalCommandBuilder.Append("CAST(");

            Visit(explicitCastExpression.Operand);

            _relationalCommandBuilder.Append(" AS ");

            var typeMapping = _relationalTypeMapper.FindMapping(explicitCastExpression.Type);

            if (typeMapping == null)
            {
                throw new NotSupportedException(RelationalStrings.UnsupportedType(explicitCastExpression.Type.Name));
            }

            _relationalCommandBuilder.Append(typeMapping.DefaultTypeName);

            _relationalCommandBuilder.Append(")");

            return explicitCastExpression;
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                {
                    var inExpression = expression.Operand as InExpression;

                    if (inExpression != null)
                    {
                        return VisitNotIn(inExpression);
                    }

                    var isNullExpression = expression.Operand as IsNullExpression;

                    if (isNullExpression != null)
                    {
                        return VisitIsNotNull(isNullExpression);
                    }

                    if (expression.Operand is ExistsExpression)
                    {
                        _relationalCommandBuilder.Append("NOT ");

                        Visit(expression.Operand);

                        return expression;
                    }

                    if (!(expression.Operand is ColumnExpression
                          || expression.Operand is ParameterExpression
                          || expression.Operand.IsAliasWithColumnExpression()
                          || expression.Operand is SelectExpression))
                    {
                        _relationalCommandBuilder.Append("NOT (");

                        Visit(expression.Operand);

                        _relationalCommandBuilder.Append(")");
                    }
                    else
                    {
                        Visit(expression.Operand);

                        _relationalCommandBuilder.Append(" = ");
                        _relationalCommandBuilder.Append(FalseLiteral);
                    }

                    return expression;
                }
                case ExpressionType.Convert:
                {
                    Visit(expression.Operand);

                    return expression;
                }
            }

            return base.VisitUnary(expression);
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            _relationalCommandBuilder.Append(expression.Value == null
                ? "NULL"
                : _sqlGenerationHelper.GenerateLiteral(expression.Value));

            return expression;
        }

        protected override Expression VisitParameter(ParameterExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            object value;
            if (_parametersValues.TryGetValue(expression.Name, out value))
            {
                var name = _sqlGenerationHelper.GenerateParameterName(expression.Name);

                _relationalCommandBuilder.AppendParameter(name, value, expression.Type, expression.Name);
            }

            return expression;
        }

        protected virtual bool TryGenerateBinaryOperator(ExpressionType op, [NotNull] out string result)
            => _binaryOperatorMap.TryGetValue(op, out result);

        protected virtual string GenerateBinaryOperator(ExpressionType op) => _binaryOperatorMap[op];

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
            => new NotImplementedException(visitMethod);

        private class NullComparisonTransformingVisitor : RelinqExpressionVisitor
        {
            private readonly IReadOnlyDictionary<string, object> _parameterValues;

            public NullComparisonTransformingVisitor(IReadOnlyDictionary<string, object> parameterValues)
            {
                _parameterValues = parameterValues;
            }

            protected override Expression VisitBinary(BinaryExpression expression)
            {
                if (expression.NodeType == ExpressionType.Equal
                    || expression.NodeType == ExpressionType.NotEqual)
                {
                    var leftExpression = expression.Left.RemoveConvert();
                    var rightExpression = expression.Right.RemoveConvert();

                    var parameter
                        = rightExpression as ParameterExpression
                          ?? leftExpression as ParameterExpression;

                    object parameterValue;
                    if (parameter != null
                        && _parameterValues.TryGetValue(parameter.Name, out parameterValue)
                        && parameterValue == null)
                    {
                        var columnExpression
                            = leftExpression.TryGetColumnExpression()
                              ?? rightExpression.TryGetColumnExpression();

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