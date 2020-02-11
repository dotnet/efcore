// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    /// In SQL Server, string equality ignores trailing whitespace. This replaces equality with constant strings to
    /// LIKE, which does an exact comparison and still utilizes indexes.
    /// </summary>
    public class StringEqualityConvertingExpressionVisitor : ExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public StringEqualityConvertingExpressionVisitor(ISqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            if (extensionExpression is ShapedQueryExpression shapedQueryExpression)
            {
                return shapedQueryExpression.Update(
                    Visit(shapedQueryExpression.QueryExpression),
                    shapedQueryExpression.ShaperExpression);
            }

            if (extensionExpression is SqlBinaryExpression binaryExpression
                && (binaryExpression.OperatorType == ExpressionType.Equal
                    || binaryExpression.OperatorType == ExpressionType.NotEqual)
                && binaryExpression.Left.TypeMapping is StringTypeMapping
                && binaryExpression.Right.TypeMapping is StringTypeMapping
                // Specifically avoid rewriting if both sides are constant (e.g. N'' = N'') - this gets handled
                // elsewhere and rewriting here interferes
                && (!(binaryExpression.Left is SqlConstantExpression)
                    || !(binaryExpression.Right is SqlConstantExpression)))
            {
                var likeExpression =
                    TransformToLikeIfPossible(binaryExpression.Left, binaryExpression.Right) ??
                    TransformToLikeIfPossible(binaryExpression.Right, binaryExpression.Left);

                if (likeExpression != null)
                {
                    return binaryExpression.OperatorType == ExpressionType.Equal
                        ? likeExpression
                        : (SqlExpression)_sqlExpressionFactory.Not(likeExpression);
                }
            }

            return base.VisitExtension(extensionExpression);
        }

        private LikeExpression TransformToLikeIfPossible(SqlExpression left, SqlExpression right)
            => right is SqlConstantExpression constantExpression
                && constantExpression.Value is string value
                && (
                    value.Length == 0
                    || value.All(c => !IsLikeWildChar(c))
                    && char.IsWhiteSpace(value[^1]))
                    ? _sqlExpressionFactory.Like(left, right)
                    : null;

        // See https://docs.microsoft.com/en-us/sql/t-sql/language-elements/like-transact-sql
        private bool IsLikeWildChar(char c) => c == '%' || c == '_' || c == '[';
    }
}
