// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.Sql.Internal
{
    public class SqlServerQuerySqlGenerator : DefaultQuerySqlGenerator, ISqlServerExpressionVisitor
    {
        public SqlServerQuerySqlGenerator(
            [NotNull] IRelationalCommandBuilderFactory relationalCommandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IRelationalTypeMapper relationalTypeMapper,
            [NotNull] SelectExpression selectExpression)
            : base(
                  relationalCommandBuilderFactory,
                  sqlGenerationHelper,
                  parameterNameGeneratorFactory,
                  relationalTypeMapper,
                  selectExpression)
        {
        }

        public override Expression VisitLateralJoin(LateralJoinExpression lateralJoinExpression)
        {
            Check.NotNull(lateralJoinExpression, nameof(lateralJoinExpression));

            Sql.Append("CROSS APPLY ");

            Visit(lateralJoinExpression.TableExpression);

            return lateralJoinExpression;
        }

        public override Expression VisitCount(CountExpression countExpression)
        {
            Check.NotNull(countExpression, nameof(countExpression));

            if (countExpression.Type == typeof(long))
            {
                Sql.Append("COUNT_BIG(*)");

                return countExpression;
            }

            return base.VisitCount(countExpression);
        }

        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            if (selectExpression.Projection.OfType<RowNumberExpression>().Any())
            {
                return;
            }

            if (selectExpression.Offset != null
                && !selectExpression.OrderBy.Any())
            {
                Sql.AppendLine().Append("ORDER BY @@ROWCOUNT");
            }

            base.GenerateLimitOffset(selectExpression);
        }

        protected override void VisitProjection(IReadOnlyList<Expression> projections)
        {
            var comparisonTransformer = new ProjectionComparisonTransformingVisitor();
            var transformedProjections = projections.Select(comparisonTransformer.Visit).ToList();

            base.VisitProjection(transformedProjections);
        }

        public virtual Expression VisitRowNumber(RowNumberExpression rowNumberExpression)
        {
            Check.NotNull(rowNumberExpression, nameof(rowNumberExpression));

            Sql.Append("ROW_NUMBER() OVER(");
            GenerateOrderBy(rowNumberExpression.Orderings);
            Sql.Append(") AS ").Append(SqlGenerator.DelimitIdentifier(rowNumberExpression.ColumnExpression.Name));

            return rowNumberExpression;
        }

        public override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            if (sqlFunctionExpression.FunctionName.StartsWith("@@"))
            {
                Sql.Append(sqlFunctionExpression.FunctionName);
                return sqlFunctionExpression;
            }
            return base.VisitSqlFunction(sqlFunctionExpression);
        }

        private class ProjectionComparisonTransformingVisitor : RelinqExpressionVisitor
        {
            private bool _insideConditionalTest = false;

            protected override Expression VisitUnary(UnaryExpression node)
            {
                if (!_insideConditionalTest
                    && node.NodeType == ExpressionType.Not
                    && node.Operand is AliasExpression)
                {
                    return Expression.Condition(
                        node,
                        Expression.Constant(true, typeof(bool)),
                        Expression.Constant(false, typeof(bool)));
                }

                return base.VisitUnary(node);
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (!_insideConditionalTest
                    && (node.IsComparisonOperation()
                        || node.IsLogicalOperation()))
                    {
                    return Expression.Condition(
                        node,
                        Expression.Constant(true, typeof(bool)),
                        Expression.Constant(false, typeof(bool)));
                }

                return base.VisitBinary(node);
            }


            protected override Expression VisitConditional(ConditionalExpression node)
            {
                _insideConditionalTest = true;
                var test = Visit(node.Test);
                _insideConditionalTest = false;
                if (test is AliasExpression)
                {
                    return Expression.Condition(
                        Expression.Equal(test, Expression.Constant(true, typeof(bool))),
                        Visit(node.IfTrue),
                        Visit(node.IfFalse));
                }

                var condition = test as ConditionalExpression;
                if (condition != null)
                {
                    return Expression.Condition(
                        condition.Test,
                        Visit(node.IfTrue),
                        Visit(node.IfFalse));
                }
                return Expression.Condition(test,
                    Visit(node.IfTrue),
                    Visit(node.IfFalse));
            }
        }

    }
}
