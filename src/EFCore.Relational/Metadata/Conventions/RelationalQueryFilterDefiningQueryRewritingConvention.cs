// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class RelationalQueryFilterDefiningQueryRewritingConvention : QueryFilterDefiningQueryRewritingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalQueryFilterDefiningQueryRewritingConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public RelationalQueryFilterDefiningQueryRewritingConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies)
        {
            DbSetAccessRewriter = new RelationalDbSetAccessRewritingExpressionVisitor(Dependencies.ContextType);
        }

        protected class RelationalDbSetAccessRewritingExpressionVisitor : DbSetAccessRewritingExpressionVisitor
        {
            public RelationalDbSetAccessRewritingExpressionVisitor(Type contextType)
                : base(contextType)
            {
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.DeclaringType == typeof(RelationalQueryableExtensions)
                    && (methodCallExpression.Method.Name == nameof(RelationalQueryableExtensions.FromSql)
                        || methodCallExpression.Method.Name == nameof(RelationalQueryableExtensions.FromSqlRaw)
                        || methodCallExpression.Method.Name == nameof(RelationalQueryableExtensions.FromSqlInterpolated)))
                {
                    var newSource = Visit(methodCallExpression.Arguments[0]);
                    var fromSqlOnQueryableMethod =
                        RelationalQueryableExtensions.FromSqlOnQueryableMethodInfo.MakeGenericMethod(
                            newSource.Type.GetGenericArguments()[0]);

                    switch (methodCallExpression.Method.Name)
                    {
                        case nameof(RelationalQueryableExtensions.FromSqlRaw):
                            return Expression.Call(
                                null,
                                fromSqlOnQueryableMethod,
                                newSource,
                                methodCallExpression.Arguments[1],
                                methodCallExpression.Arguments[2]);

                        case nameof(RelationalQueryableExtensions.FromSqlInterpolated):
                        case nameof(RelationalQueryableExtensions.FromSql) when methodCallExpression.Arguments.Count == 2:
                            var formattableString = Expression.Lambda<Func<FormattableString>>(
                                Expression.Convert(methodCallExpression.Arguments[1], typeof(FormattableString))).Compile().Invoke();

                            return Expression.Call(
                                null,
                                fromSqlOnQueryableMethod,
                                newSource,
                                Expression.Constant(formattableString.Format),
                                Expression.Constant(formattableString.GetArguments()));

                        case nameof(RelationalQueryableExtensions.FromSql) when methodCallExpression.Arguments.Count == 3:
#pragma warning disable CS0618 // Type or member is obsolete
                            var rawSqlStringString = Expression
                                .Lambda<Func<RawSqlString>>(Expression.Convert(methodCallExpression.Arguments[1], typeof(RawSqlString)))
                                .Compile().Invoke();
#pragma warning restore CS0618 // Type or member is obsolete

                            return Expression.Call(
                                null,
                                fromSqlOnQueryableMethod,
                                newSource,
                                Expression.Constant(rawSqlStringString.Format),
                                methodCallExpression.Arguments[2]);
                    }
                }

                return base.VisitMethodCall(methodCallExpression);
            }
        }
    }
}
