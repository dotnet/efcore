// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

                if (constantExpression != null
                    && constantExpression.Value == null)
                {
                    return newBinaryExpression;
                }

                constantExpression = newBinaryExpression.Right.RemoveConvert() as ConstantExpression;

                if (constantExpression != null
                    && constantExpression.Value == null)
                {
                    return newBinaryExpression;
                }

                var entityType = _model.FindEntityType(newBinaryExpression.Left.Type);

                if (entityType != null)
                {
                    var primaryKeyProperties = entityType.FindPrimaryKey().Properties;

                    var newLeftExpression
                        = CreateKeyAccessExpression(newBinaryExpression.Left, primaryKeyProperties);

                    var newRightExpression
                        = CreateKeyAccessExpression(newBinaryExpression.Right, primaryKeyProperties);

                    return Expression.MakeBinary(newBinaryExpression.NodeType, newLeftExpression, newRightExpression);
                }
            }

            return newBinaryExpression;
        }

        private static Expression CreateKeyAccessExpression(Expression target, IReadOnlyList<IProperty> properties)
        {
            return properties.Count == 1
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
