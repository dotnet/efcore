// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Infrastructure.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public partial class InMemoryShapedQueryCompilingExpressionVisitor
    {
        private sealed class InMemoryProjectionBindingRemovingExpressionVisitor : ExpressionVisitor
        {
            private readonly IDictionary<ParameterExpression, (IDictionary<IProperty, int> IndexMap, ParameterExpression valueBuffer)>
                _materializationContextBindings
                    = new Dictionary<ParameterExpression, (IDictionary<IProperty, int> IndexMap, ParameterExpression valueBuffer)>();

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                Check.NotNull(binaryExpression, nameof(binaryExpression));

                if (binaryExpression.NodeType == ExpressionType.Assign
                    && binaryExpression.Left is ParameterExpression parameterExpression
                    && parameterExpression.Type == typeof(MaterializationContext))
                {
                    var newExpression = (NewExpression)binaryExpression.Right;

                    var projectionBindingExpression = (ProjectionBindingExpression)newExpression.Arguments[0];
                    var queryExpression = (InMemoryQueryExpression)projectionBindingExpression.QueryExpression;

                    _materializationContextBindings[parameterExpression]
                        = ((IDictionary<IProperty, int>)GetProjectionIndex(queryExpression, projectionBindingExpression),
                            ((InMemoryQueryExpression)projectionBindingExpression.QueryExpression).CurrentParameter);

                    var updatedExpression = Expression.New(
                        newExpression.Constructor,
                        Expression.Constant(ValueBuffer.Empty),
                        newExpression.Arguments[1]);

                    return Expression.MakeBinary(ExpressionType.Assign, binaryExpression.Left, updatedExpression);
                }

                if (binaryExpression.NodeType == ExpressionType.Assign
                    && binaryExpression.Left is MemberExpression memberExpression
                    && memberExpression.Member is FieldInfo fieldInfo
                    && fieldInfo.IsInitOnly)
                {
                    return memberExpression.Assign(Visit(binaryExpression.Right));
                }

                return base.VisitBinary(binaryExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                Check.NotNull(methodCallExpression, nameof(methodCallExpression));

                if (methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == ExpressionExtensions.ValueBufferTryReadValueMethod)
                {
                    var property = (IProperty)((ConstantExpression)methodCallExpression.Arguments[2]).Value;
                    var (indexMap, valueBuffer) =
                        _materializationContextBindings[
                            (ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object];

                    Check.DebugAssert(
                        property != null || methodCallExpression.Type.IsNullableType(), "Must read nullable value without property");

                    return Expression.Call(
                        methodCallExpression.Method,
                        valueBuffer,
                        Expression.Constant(indexMap[property]),
                        methodCallExpression.Arguments[2]);
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                Check.NotNull(extensionExpression, nameof(extensionExpression));

                if (extensionExpression is ProjectionBindingExpression projectionBindingExpression)
                {
                    var queryExpression = (InMemoryQueryExpression)projectionBindingExpression.QueryExpression;
                    var projectionIndex = (int)GetProjectionIndex(queryExpression, projectionBindingExpression);
                    var valueBuffer = queryExpression.CurrentParameter;
                    var property = InferPropertyFromInner(queryExpression.Projection[projectionIndex]);

                    Check.DebugAssert(
                        property != null
                        || projectionBindingExpression.Type.IsNullableType()
                        || projectionBindingExpression.Type == typeof(ValueBuffer), "Must read nullable value without property");

                    return valueBuffer.CreateValueBufferReadValueExpression(projectionBindingExpression.Type, projectionIndex, property);
                }

                return base.VisitExtension(extensionExpression);
            }

            private IPropertyBase InferPropertyFromInner(Expression expression)
            {
                if (expression is MethodCallExpression methodCallExpression
                    && methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == ExpressionExtensions.ValueBufferTryReadValueMethod)
                {
                    return (IPropertyBase)((ConstantExpression)methodCallExpression.Arguments[2]).Value;
                }

                return null;
            }

            private object GetProjectionIndex(
                InMemoryQueryExpression queryExpression,
                ProjectionBindingExpression projectionBindingExpression)
            {
                return projectionBindingExpression.ProjectionMember != null
                    ? ((ConstantExpression)queryExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)).Value
                    : (projectionBindingExpression.Index != null
                        ? (object)projectionBindingExpression.Index
                        : projectionBindingExpression.IndexMap);
            }
        }
    }
}
