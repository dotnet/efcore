// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

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
                Check.NotNull(methodCallExpression, nameof(methodCallExpression));

                var methodName = methodCallExpression.Method.Name;
                if (methodCallExpression.Method.DeclaringType == typeof(RelationalQueryableExtensions)
                    && (methodName == nameof(RelationalQueryableExtensions.FromSqlRaw)
                        || methodName == nameof(RelationalQueryableExtensions.FromSqlInterpolated)))
                {
                    var newSource = (QueryRootExpression)Visit(methodCallExpression.Arguments[0]);

                    string sql;
                    Expression argument;

                    if (methodName == nameof(RelationalQueryableExtensions.FromSqlRaw))
                    {
                        sql = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                        argument = methodCallExpression.Arguments[2];
                    }
                    else
                    {
                        var formattableString = Expression.Lambda<Func<FormattableString>>(
                           Expression.Convert(methodCallExpression.Arguments[1], typeof(FormattableString))).Compile().Invoke();

                        sql = formattableString.Format;
                        argument = Expression.Constant(formattableString.GetArguments());
                    }

                    return new FromSqlQueryRootExpression(newSource.EntityType, sql, argument);
                }

                return base.VisitMethodCall(methodCallExpression);
            }
        }
    }
}
