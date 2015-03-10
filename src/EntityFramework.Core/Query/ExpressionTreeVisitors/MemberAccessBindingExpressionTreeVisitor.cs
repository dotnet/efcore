// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public class MemberAccessBindingExpressionTreeVisitor : ReferenceReplacingExpressionTreeVisitor
    {
        private readonly EntityQueryModelVisitor _queryModelVisitor;

        public MemberAccessBindingExpressionTreeVisitor(
            [NotNull] QuerySourceMapping querySourceMapping,
            [NotNull] EntityQueryModelVisitor queryModelVisitor)
            : base(
                Check.NotNull(querySourceMapping, nameof(querySourceMapping)),
                throwOnUnmappedReferences: false)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _queryModelVisitor = queryModelVisitor;
        }

        protected override Expression VisitMemberExpression(MemberExpression memberExpression)
        {
            var newExpression = VisitExpression(memberExpression.Expression);

            if (newExpression != memberExpression.Expression)
            {
                if (newExpression.Type == typeof(IValueReader))
                {
                    return _queryModelVisitor.BindMemberToValueReader(memberExpression, newExpression);
                }

                var member = memberExpression.Member;
                var typeInfo = newExpression.Type.GetTypeInfo();

                if (typeInfo.IsGenericType
                    && typeInfo.GetGenericTypeDefinition() == typeof(IAsyncGrouping<,>))
                {
                    member = typeInfo.GetDeclaredProperty("Key");
                }

                return Expression.MakeMemberAccess(newExpression, member);
            }

            return memberExpression;
        }

        protected override Expression VisitMethodCallExpression(MethodCallExpression methodCallExpression)
        {
            var newExpression
                = (MethodCallExpression)base.VisitMethodCallExpression(methodCallExpression);

            if (newExpression != methodCallExpression
                && newExpression.Arguments.Any()
                && newExpression.Arguments[0].Type == typeof(IValueReader))
            {
                return
                    _queryModelVisitor
                        .BindMethodCallToValueReader(methodCallExpression, newExpression.Arguments[0])
                    ?? newExpression;
            }

            return _queryModelVisitor
                .BindMethodCallExpression(
                    methodCallExpression,
                    (property, _) => Expression.Call(
                        _getValueMethodInfo.MakeGenericMethod(newExpression.Method.GetGenericArguments()[0]),
                        EntityQueryModelVisitor.QueryContextParameter,
                        newExpression.Arguments[0],
                        Expression.Constant(property)))
                   ?? newExpression;
        }

        private static readonly MethodInfo _getValueMethodInfo
            = typeof(MemberAccessBindingExpressionTreeVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(GetValue));

        [UsedImplicitly]
        private static T GetValue<T>(QueryContext queryContext, object entity, IProperty property)
        {
            return (T)queryContext.QueryBuffer.GetPropertyValue(entity, property);
        }
    }
}
