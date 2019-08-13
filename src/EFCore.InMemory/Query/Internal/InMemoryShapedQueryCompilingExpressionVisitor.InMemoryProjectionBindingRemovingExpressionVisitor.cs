// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public partial class InMemoryShapedQueryCompilingExpressionVisitor
    {
        private class InMemoryProjectionBindingRemovingExpressionVisitor : ExpressionVisitor
        {
            private readonly InMemoryQueryExpression _queryExpression;

            private readonly IDictionary<ParameterExpression, IDictionary<IProperty, int>> _materializationContextBindings
                = new Dictionary<ParameterExpression, IDictionary<IProperty, int>>();

            public InMemoryProjectionBindingRemovingExpressionVisitor(InMemoryQueryExpression queryExpression)
            {
                _queryExpression = queryExpression;
            }

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                if (binaryExpression.NodeType == ExpressionType.Assign
                    && binaryExpression.Left is ParameterExpression parameterExpression
                    && parameterExpression.Type == typeof(MaterializationContext))
                {
                    var newExpression = (NewExpression)binaryExpression.Right;

                    var projectionBindingExpression = (ProjectionBindingExpression)newExpression.Arguments[0];

                    _materializationContextBindings[parameterExpression]
                        = (IDictionary<IProperty, int>)GetProjectionIndex(projectionBindingExpression);

                    var updatedExpression = Expression.New(
                        newExpression.Constructor,
                        Expression.Constant(ValueBuffer.Empty),
                        newExpression.Arguments[1]);

                    return Expression.MakeBinary(ExpressionType.Assign, binaryExpression.Left, updatedExpression);
                }

                return base.VisitBinary(binaryExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == EntityMaterializerSource.TryReadValueMethod)
                {
                    var property = (IProperty)((ConstantExpression)methodCallExpression.Arguments[2]).Value;
                    var indexMap =
                        _materializationContextBindings[
                            (ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object];

                    return Expression.Call(
                        methodCallExpression.Method,
                        _queryExpression.ValueBufferParameter,
                        Expression.Constant(indexMap[property]),
                        methodCallExpression.Arguments[2]);
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is ProjectionBindingExpression projectionBindingExpression)
                {
                    var projectionIndex = (int)GetProjectionIndex(projectionBindingExpression);

                    return Expression.Call(
                        EntityMaterializerSource.TryReadValueMethod.MakeGenericMethod(projectionBindingExpression.Type),
                        _queryExpression.ValueBufferParameter,
                        Expression.Constant(projectionIndex),
                        Expression.Constant(InferPropertyFromInner(_queryExpression.Projection[projectionIndex]), typeof(IPropertyBase)));
                }

                return base.VisitExtension(extensionExpression);
            }

            private IPropertyBase InferPropertyFromInner(Expression expression)
            {
                if (expression is MethodCallExpression methodCallExpression
                    && methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == EntityMaterializerSource.TryReadValueMethod)
                {
                    return (IPropertyBase)((ConstantExpression)methodCallExpression.Arguments[2]).Value;
                }

                return null;
            }

            private object GetProjectionIndex(ProjectionBindingExpression projectionBindingExpression)
            {
                return projectionBindingExpression.ProjectionMember != null
                    ? ((ConstantExpression)_queryExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)).Value
                    : (projectionBindingExpression.Index != null
                        ? (object)projectionBindingExpression.Index
                        : projectionBindingExpression.IndexMap);
            }
        }
    }
}
