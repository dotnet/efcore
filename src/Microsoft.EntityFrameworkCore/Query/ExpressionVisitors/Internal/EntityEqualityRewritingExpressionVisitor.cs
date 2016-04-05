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
    public class EntityEqualityRewritingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly IModel _model;

        public EntityEqualityRewritingExpressionVisitor([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            _model = model;
        }

        public virtual void Rewrite([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            queryModel.TransformExpressions(Visit);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            if (binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                var newBinaryExpression = (BinaryExpression)base.VisitBinary(binaryExpression);

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

            return binaryExpression;
        }

        private static Expression CreateKeyAccessExpression(Expression target, IReadOnlyList<IProperty> properties)
        {
            return properties.Count == 1
                ? CreatePropertyExpression(target, properties[0])
                : Expression.New(
                    CompositeKey.CompositeKeyCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        properties
                            .Select(p => Expression.Convert(CreatePropertyExpression(target, p), typeof(object)))
                            .Cast<Expression>()
                            .ToArray()));
        }

        private static readonly MethodInfo _efPropertyMethod
            = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(EF.Property));

        private static Expression CreatePropertyExpression(Expression target, IProperty property)
            => Expression.Call(
                null,
                _efPropertyMethod.MakeGenericMethod(property.ClrType.MakeNullable()),
                target,
                Expression.Constant(property.Name));

        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
        {
            Check.NotNull(subQueryExpression, nameof(subQueryExpression));

            subQueryExpression.QueryModel.TransformExpressions(Visit);

            return subQueryExpression;
        }
    }
}
