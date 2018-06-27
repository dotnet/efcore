// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.ExpressionVisitors.Internal
{
    public class CosmosSqlGenerator : ExpressionVisitor
    {
        private StringBuilder _sqlBuilder = new StringBuilder();

        public CosmosSqlGenerator()
        {
        }

        public string GenerateSql(SelectExpression selectExpression)
        {
            _sqlBuilder.Clear();
            Visit(selectExpression);
            return _sqlBuilder.ToString();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.Equal)
            {
                Visit(node.Left);
                _sqlBuilder.Append(" = ");
                Visit(node.Right);

                return node;
            }

            return base.VisitBinary(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _sqlBuilder.Append($"\"{node.Value}\"");

            return node;
        }

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case SelectExpression selectExpression:

                    _sqlBuilder.Append("SELECT ");
                    Visit(selectExpression.Projection);
                    _sqlBuilder.AppendLine(" AS query");

                    _sqlBuilder.Append("FROM root ");
                    Visit(selectExpression.FromExpression);
                    _sqlBuilder.AppendLine();

                    _sqlBuilder.Append("WHERE ");
                    Visit(selectExpression.FilterExpression);
                    _sqlBuilder.AppendLine();

                    return node;

                case RootReferenceExpression rootReferenceExpression:
                    _sqlBuilder.Append(rootReferenceExpression.ToString());
                    return node;

                case KeyAccessExpression keyAccessExpression:
                    _sqlBuilder.Append(keyAccessExpression.ToString());
                    return node;

                case EntityProjectionExpression entityProjectionExpression:
                    _sqlBuilder.Append(entityProjectionExpression.ToString());
                    return node;
            }

            return base.VisitExtension(node);
        }
    }
}
