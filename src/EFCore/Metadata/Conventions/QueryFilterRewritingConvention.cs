// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Convention that converts accesses of <see cref="DbSet{TEntity}" /> inside query filters into <see cref="QueryRootExpression" />.
    ///     This makes them consistent with how DbSet accesses in the actual queries are represented, which allows for easier processing in the
    ///     query pipeline.
    /// </summary>
    public class QueryFilterRewritingConvention : IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="QueryFilterRewritingConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public QueryFilterRewritingConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
            DbSetAccessRewriter = new DbSetAccessRewritingExpressionVisitor(dependencies.ContextType);
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Visitor used to rewrite <see cref="DbSet{TEntity}" /> accesses encountered in query filters to <see cref="QueryRootExpression" />.
        /// </summary>
        protected virtual DbSetAccessRewritingExpressionVisitor DbSetAccessRewriter { get; [param: NotNull] set; }

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var queryFilter = entityType.GetQueryFilter();
                if (queryFilter != null)
                {
                    entityType.SetQueryFilter((LambdaExpression)DbSetAccessRewriter.Rewrite(modelBuilder.Metadata, queryFilter));
                }
            }
        }

        /// <summary>
        ///     A visitor that rewrites DbSet accesses encountered in an expression to <see cref="QueryRootExpression" />.
        /// </summary>
        protected class DbSetAccessRewritingExpressionVisitor : ExpressionVisitor
        {
            private readonly Type _contextType;
            private IModel _model;

            /// <summary>
            ///     Creates a new instance of <see cref="DbSetAccessRewritingExpressionVisitor" />.
            /// </summary>
            /// <param name="contextType"> The clr type of derived DbContext. </param>
            public DbSetAccessRewritingExpressionVisitor(Type contextType)
            {
                _contextType = contextType;
            }

            /// <summary>
            ///     Rewrites DbSet accesses encountered in the expression to <see cref="QueryRootExpression" />.
            /// </summary>
            /// <param name="model"> The model to look for entity types. </param>
            /// <param name="expression"> The query expression to rewrite. </param>
            public Expression Rewrite(IModel model, Expression expression)
            {
                _model = model;

                return Visit(expression);
            }

            /// <inheritdoc />
            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                Check.NotNull(memberExpression, nameof(memberExpression));

                if (memberExpression.Expression != null
                    && (memberExpression.Expression.Type.IsAssignableFrom(_contextType)
                        || _contextType.IsAssignableFrom(memberExpression.Expression.Type))
                    && memberExpression.Type.IsGenericType
                    && memberExpression.Type.GetGenericTypeDefinition() == typeof(DbSet<>)
                    && _model != null)
                {
                    return new QueryRootExpression(FindEntityType(memberExpression.Type));
                }

                return base.VisitMember(memberExpression);
            }

            /// <inheritdoc />
            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                Check.NotNull(methodCallExpression, nameof(methodCallExpression));

                if (methodCallExpression.Method.Name == nameof(DbContext.Set)
                    && methodCallExpression.Object != null
                    && typeof(DbContext).IsAssignableFrom(methodCallExpression.Object.Type)
                    && methodCallExpression.Type.IsGenericType
                    && methodCallExpression.Type.GetGenericTypeDefinition() == typeof(DbSet<>)
                    && _model != null)
                {
                    return new QueryRootExpression(FindEntityType(methodCallExpression.Type));
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            private IEntityType FindEntityType(Type dbSetType)
                => _model.FindRuntimeEntityType(dbSetType.GetGenericArguments()[0]);
        }
    }
}
