// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ByteArrayEqualityRewritingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly IModel _model;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ByteArrayEqualityRewritingExpressionVisitor([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            _model = model;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Rewrite([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            queryModel.TransformExpressions(Visit);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            var newBinaryExpression = (BinaryExpression)base.VisitBinary(binaryExpression);

            if (binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                var constantExpression = newBinaryExpression.Left.RemoveConvert() as ConstantExpression;
                var isLeftNullConstant = constantExpression != null && constantExpression.Value == null;

                constantExpression = newBinaryExpression.Right.RemoveConvert() as ConstantExpression;
                var isRightNullConstant = constantExpression != null && constantExpression.Value == null;

                if (isLeftNullConstant || isRightNullConstant)
                {
                    return newBinaryExpression;
                }

                var isLeftByteArray = newBinaryExpression.Left.Type == typeof(byte[]);
                var isRightByteArray = newBinaryExpression.Right.Type == typeof(byte[]);

                if (isLeftByteArray && isRightByteArray)
                {
                    return Expression.Call(_sequenceEqualsMethodInfo, newBinaryExpression.Left, newBinaryExpression.Right);
                }
            }

            return newBinaryExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
        {
            Check.NotNull(subQueryExpression, nameof(subQueryExpression));

            subQueryExpression.QueryModel.TransformExpressions(Visit);

            return subQueryExpression;
        }

        private static readonly MethodInfo _sequenceEqualsMethodInfo
            = typeof(Enumerable).GetTypeInfo()
                .GetDeclaredMethods(nameof(Enumerable.SequenceEqual))
                .Single(m => m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(byte));
    }
}
