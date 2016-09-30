// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class QueryModelPrinter
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Print([NotNull] QueryModel queryModel)
        {
            var clonedQueryModel = queryModel.Clone();

            var transformingVisitor = new QueryModelPrintingExpressionVisitor();
            var queryModelPrintingVisitor = new QueryModelPrintingVisitor(transformingVisitor);

            queryModelPrintingVisitor.VisitQueryModel(clonedQueryModel);

            return PrintVisitedQueryModel(clonedQueryModel, 0);
        }

        private static string PrintVisitedQueryModel(QueryModel queryModel, int indent)
        {
            var stringBuilder = new IndentedStringBuilder();

            if (indent > 0)
            {
                stringBuilder.AppendLine();
            }

            for (int i = 0; i < indent; i++)
            {
                stringBuilder.IncrementIndent();
            }

            stringBuilder.AppendLine(queryModel.MainFromClause);
            foreach (var bodyClause in queryModel.BodyClauses)
            {
                stringBuilder.AppendLine(bodyClause);
            }

            foreach (var resultOperator in queryModel.ResultOperators)
            {
                stringBuilder.AppendLine(resultOperator);
            }

            stringBuilder.Append(queryModel.SelectClause);

            for (int i = 0; i < indent; i++)
            {
                stringBuilder.DecrementIndent();
            }

            return stringBuilder.ToString();
        }

        private class QueryModelPrintingVisitor : ExpressionTransformingQueryModelVisitor<QueryModelPrintingExpressionVisitor>
        {
            private QueryModelPrintingExpressionVisitor _transformingVisitor;

            public QueryModelPrintingVisitor([NotNull] QueryModelPrintingExpressionVisitor transformingVisitor) 
                : base(transformingVisitor)
            {
                _transformingVisitor = transformingVisitor;
                _transformingVisitor.QueryModelVisitor = this;
            }
        }

        private class QueryModelPrintingExpressionVisitor : ExpressionVisitorBase
        {
            public QueryModelPrintingVisitor QueryModelVisitor { get; set; }

            private int _indent = 0;

            protected override Expression VisitConstant(ConstantExpression node)
                => node.Type.GetTypeInfo().IsGenericType && node.Type.GetTypeInfo().GetGenericTypeDefinition() == typeof(EntityQueryable<>)
                    ? new PrintedQueryModelFragmentExpression($"[EntityQueryable<{node.Type.GetGenericArguments()[0].Name}>]", node.Type)
                    : base.VisitConstant(node);

            protected override Expression VisitSubQuery(SubQueryExpression expression)
            {
                _indent++;
                QueryModelVisitor.VisitQueryModel(expression.QueryModel);

                var queryModelString = PrintVisitedQueryModel(expression.QueryModel, _indent);

                _indent--;

                return new PrintedQueryModelFragmentExpression(queryModelString, expression.Type);
            }

            private class PrintedQueryModelFragmentExpression : Expression
            {
                private string _queryModelString;
                private Type _type;

                public PrintedQueryModelFragmentExpression(string queryModelString, Type type)
                {
                    _queryModelString = queryModelString;
                    _type = type;
                }

                public override Type Type => _type;

                public override ExpressionType NodeType => ExpressionType.Extension;

                public override string ToString()
                {
                    return _queryModelString;
                }
            }
        }
    }
}
