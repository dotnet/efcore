// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class InMemoryExpressionTranslatingExpressionVisitor : ExpressionVisitor
    {
        private InMemoryQueryExpression _inMemoryQueryExpression;

        public Expression Translate(InMemoryQueryExpression inMemoryQueryExpression, Expression expression)
        {
            _inMemoryQueryExpression = inMemoryQueryExpression;

            try
            {
                return Visit(expression);
            }
            finally
            {
                _inMemoryQueryExpression = null;
            }
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var innerExpression = Visit(memberExpression.Expression);
            if (innerExpression is EntityShaperExpression entityShaper)
            {
                var entityType = entityShaper.EntityType;
                var property = entityType.FindProperty(memberExpression.Member.GetSimpleMemberName());

                return _inMemoryQueryExpression.BindProperty(entityShaper.ValueBufferExpression, property);
            }

            return memberExpression.Update(innerExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
            {
                if (source is EntityShaperExpression entityShaper)
                {
                    var entityType = entityShaper.EntityType;
                    var property = entityType.FindProperty(propertyName);

                    return _inMemoryQueryExpression.BindProperty(entityShaper.ValueBufferExpression, property);
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is EntityShaperExpression)
            {
                return extensionExpression;
            }

            if (extensionExpression is ProjectionBindingExpression projectionBindingExpression)
            {
                return _inMemoryQueryExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember);
            }

            if (extensionExpression is NullConditionalExpression nullConditionalExpression)
            {
                var translation = Visit(nullConditionalExpression.AccessOperation);

                return translation.Type == nullConditionalExpression.Type
                    ? translation
                    : Expression.Convert(translation, nullConditionalExpression.Type);
            }

            return base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            if (parameterExpression.Name.StartsWith(CompiledQueryCache.CompiledQueryParameterPrefix))
            {
                return Expression.Call(
                    _getParameterValueMethodInfo.MakeGenericMethod(parameterExpression.Type),
                    QueryCompilationContext.QueryContextParameter,
                    Expression.Constant(parameterExpression.Name));
            }

            throw new InvalidOperationException();
        }

        private static readonly MethodInfo _getParameterValueMethodInfo
            = typeof(InMemoryExpressionTranslatingExpressionVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(GetParameterValue));

#pragma warning disable IDE0052 // Remove unread private members
        private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
#pragma warning restore IDE0052 // Remove unread private members
            => (T)queryContext.ParameterValues[parameterName];
    }

}
