using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public class ProjectionExpressionTreeVisitor : DefaultQueryExpressionTreeVisitor
    {
        public ProjectionExpressionTreeVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor)
            : base(Check.NotNull(entityQueryModelVisitor, "entityQueryModelVisitor"))
        {
        }

        protected override Expression VisitSubQueryExpression(SubQueryExpression subQueryExpression)
        {
            var queryModelVisitor = CreateQueryModelVisitor();

            queryModelVisitor.VisitQueryModel(subQueryExpression.QueryModel);

            var subExpression = queryModelVisitor.Expression;

            var streamedSequenceInfo
                = subQueryExpression.QueryModel.GetOutputDataInfo() as StreamedSequenceInfo;

            if (streamedSequenceInfo == null)
            {
                return subExpression;
            }

            var typeInfo = subQueryExpression.Type.GetTypeInfo();

            if (typeof(IQueryable).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                subExpression
                    = Expression.Call(
                        QueryModelVisitor.LinqOperatorProvider.AsQueryable
                            .MakeGenericMethod(streamedSequenceInfo.ResultItemType),
                        subExpression);
            }
            else if (typeInfo.IsGenericType
                     && typeInfo.GetGenericTypeDefinition() == typeof(IOrderedEnumerable<>))
            {
                var elementType
                    = subExpression.Type.TryGetElementType(typeof(IOrderedAsyncEnumerable<>));

                if (elementType != null)
                {
                    subExpression
                        = Expression.Call(
                            QueryModelVisitor.LinqOperatorProvider.AsQueryable
                                .MakeGenericMethod(streamedSequenceInfo.ResultItemType),
                            subExpression);
                }
            }

            return Expression.Convert(subExpression, subQueryExpression.Type);
        }
    }
}