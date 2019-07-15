// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public abstract class SqlExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case SelectExpression selectExpression:
                    return VisitSelect(selectExpression);

                case ProjectionExpression projectionExpression:
                    return VisitProjection(projectionExpression);

                case EntityProjectionExpression entityProjectionExpression:
                    return VisitEntityProjection(entityProjectionExpression);

                case ObjectArrayProjectionExpression arrayProjectionExpression:
                    return VisitObjectArrayProjection(arrayProjectionExpression);

                case RootReferenceExpression rootReferenceExpression:
                    return VisitRootReference(rootReferenceExpression);

                case KeyAccessExpression keyAccessExpression:
                    return VisitKeyAccess(keyAccessExpression);

                case ObjectAccessExpression objectAccessExpression:
                    return VisitObjectAccess(objectAccessExpression);

                case SqlBinaryExpression sqlBinaryExpression:
                    return VisitSqlBinary(sqlBinaryExpression);

                case SqlConstantExpression sqlConstantExpression:
                    return VisitSqlConstant(sqlConstantExpression);

                case SqlUnaryExpression sqlUnaryExpression:
                    return VisitSqlUnary(sqlUnaryExpression);

                case SqlConditionalExpression sqlConditionalExpression:
                    return VisitSqlConditional(sqlConditionalExpression);

                case SqlParameterExpression sqlParameterExpression:
                    return VisitSqlParameter(sqlParameterExpression);

                case InExpression inExpression:
                    return VisitIn(inExpression);

                case SqlFunctionExpression sqlFunctionExpression:
                    return VisitSqlFunction(sqlFunctionExpression);

                case OrderingExpression orderingExpression:
                    return VisitOrdering(orderingExpression);
            }

            return base.VisitExtension(extensionExpression);
        }

        protected abstract Expression VisitOrdering(OrderingExpression orderingExpression);
        protected abstract Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression);
        protected abstract Expression VisitIn(InExpression inExpression);
        protected abstract Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression);
        protected abstract Expression VisitSqlConditional(SqlConditionalExpression caseExpression);
        protected abstract Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression);
        protected abstract Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression);
        protected abstract Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression);
        protected abstract Expression VisitKeyAccess(KeyAccessExpression keyAccessExpression);
        protected abstract Expression VisitObjectAccess(ObjectAccessExpression objectAccessExpression);
        protected abstract Expression VisitRootReference(RootReferenceExpression rootReferenceExpression);
        protected abstract Expression VisitEntityProjection(EntityProjectionExpression entityProjectionExpression);
        protected abstract Expression VisitObjectArrayProjection(ObjectArrayProjectionExpression objectArrayProjectionExpression);
        protected abstract Expression VisitProjection(ProjectionExpression projectionExpression);
        protected abstract Expression VisitSelect(SelectExpression selectExpression);
    }
}
