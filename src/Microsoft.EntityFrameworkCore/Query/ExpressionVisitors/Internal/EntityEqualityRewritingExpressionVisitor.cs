// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityEqualityRewritingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly IModel _model;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityEqualityRewritingExpressionVisitor([NotNull] IModel model)
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

                if (isLeftNullConstant && isRightNullConstant)
                {
                    return newBinaryExpression;
                }

                var isNullComparison = isLeftNullConstant || isRightNullConstant;
                var nonNullExpression = isLeftNullConstant ? newBinaryExpression.Right : newBinaryExpression.Left;
                // If a navigation being compared to null then don't rewrite
                if (isNullComparison
                    && !(nonNullExpression is QuerySourceReferenceExpression))
                {
                    return newBinaryExpression;
                }

                var entityType = _model.FindEntityType(nonNullExpression.Type);

                if (entityType != null)
                {
                    var primaryKeyProperties = entityType.FindPrimaryKey().Properties;

                    var newLeftExpression = isLeftNullConstant
                        ? Expression.Constant(null, typeof(object))
                        : CreateKeyAccessExpression(newBinaryExpression.Left, primaryKeyProperties, isNullComparison);

                    var newRightExpression = isRightNullConstant
                        ? Expression.Constant(null, typeof(object))
                        : CreateKeyAccessExpression(newBinaryExpression.Right, primaryKeyProperties, isNullComparison);

                    return Expression.MakeBinary(newBinaryExpression.NodeType, newLeftExpression, newRightExpression);
                }
            }

            return newBinaryExpression;
        }

        private static Expression CreateKeyAccessExpression(Expression target, IReadOnlyList<IProperty> properties, bool nullComparison)
        {
            // If comparing with null then we need only first PK property
            return (properties.Count == 1) || nullComparison
                ? EntityQueryModelVisitor.CreatePropertyExpression(target, properties[0])
                : Expression.New(
                    CompositeKey.CompositeKeyCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        properties
                            .Select(p => Expression.Convert(EntityQueryModelVisitor.CreatePropertyExpression(target, p), typeof(object)))
                            .Cast<Expression>()
                            .ToArray()));
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
    }
}
