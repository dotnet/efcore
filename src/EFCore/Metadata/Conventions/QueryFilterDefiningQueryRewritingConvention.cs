// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Convention that converts accesses of DbSets inside query filters and defining queries into EntityQueryables.
    ///     This makes them consistent with how DbSet accesses in the actual queries are represented, which allows for easier processing in the query pipeline.
    /// </summary>
    public class QueryFilterDefiningQueryRewritingConvention : IModelFinalizedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="QueryFilterDefiningQueryRewritingConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public QueryFilterDefiningQueryRewritingConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
            DbSetAccessRewriter = new DbSetAccessRewritingExpressionVisitor(dependencies.ContextType);
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Visitor used to rewrite DbSets accesses encountered in query filters and defining queries to EntityQueryables.
        /// </summary>
        protected virtual DbSetAccessRewritingExpressionVisitor DbSetAccessRewriter { get; [param: NotNull] set; }

        /// <summary>
        ///     Called after a model is finalized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelFinalized(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var queryFilter = entityType.GetQueryFilter();
                if (queryFilter != null)
                {
                    entityType.SetQueryFilter((LambdaExpression)DbSetAccessRewriter.Visit(queryFilter));
                }

                var definingQuery = entityType.GetDefiningQuery();
                if (definingQuery != null)
                {
                    entityType.SetDefiningQuery((LambdaExpression)DbSetAccessRewriter.Visit(definingQuery));
                }
            }
        }

        protected class DbSetAccessRewritingExpressionVisitor : ExpressionVisitor
        {
            private readonly Type _contextType;

            public DbSetAccessRewritingExpressionVisitor(Type contextType)
            {
                _contextType = contextType;
            }

            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                if (memberExpression.Expression != null
                    && (memberExpression.Expression.Type.IsAssignableFrom(_contextType)
                        || _contextType.IsAssignableFrom(memberExpression.Expression.Type))
                    && memberExpression.Type.IsGenericType
                    && (memberExpression.Type.GetGenericTypeDefinition() == typeof(DbSet<>)
#pragma warning disable CS0618 // Type or member is obsolete
                        || memberExpression.Type.GetGenericTypeDefinition() == typeof(DbQuery<>)))
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    return NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(memberExpression.Type.GetGenericArguments()[0]);
                }

                return base.VisitMember(memberExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.Name == nameof(DbContext.Set)
                    && methodCallExpression.Object != null
                    && typeof(DbContext).IsAssignableFrom(methodCallExpression.Object.Type)
                    && methodCallExpression.Type.IsGenericType
                    && methodCallExpression.Type.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    return NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(methodCallExpression.Type.GetGenericArguments()[0]);
                }

                return base.VisitMethodCall(methodCallExpression);
            }
        }
    }
}
