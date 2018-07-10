// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.ExpressionVisitors.Internal
{
    public class CosmosSqlGenerator : ExpressionVisitor
    {
        private StringBuilder _sqlBuilder = new StringBuilder();

        private IDictionary<ExpressionType, string> _operatorMap = new Dictionary<ExpressionType, string>
        {
            { ExpressionType.Equal, " = " },
            { ExpressionType.AndAlso, " AND " },
        };

        public CosmosSqlGenerator()
        {
        }

        public string GenerateSql(SelectExpression selectExpression)
        {
            _sqlBuilder.Clear();
            Visit(selectExpression);
            return _sqlBuilder.ToString();
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (_operatorMap.ContainsKey(binaryExpression.NodeType))
            {
                Visit(binaryExpression.Left);
                _sqlBuilder.Append(_operatorMap[binaryExpression.NodeType]);
                Visit(binaryExpression.Right);

                return binaryExpression;
            }

            return base.VisitBinary(binaryExpression);
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            _sqlBuilder.Append($"\"{constantExpression.Value}\"");

            return constantExpression;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
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

                    return extensionExpression;

                case RootReferenceExpression rootReferenceExpression:
                    _sqlBuilder.Append(rootReferenceExpression.ToString());
                    return extensionExpression;

                case KeyAccessExpression keyAccessExpression:
                    _sqlBuilder.Append(keyAccessExpression.ToString());
                    return extensionExpression;

                case EntityProjectionExpression entityProjectionExpression:
                    _sqlBuilder.Append(entityProjectionExpression.ToString());
                    return extensionExpression;
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}
