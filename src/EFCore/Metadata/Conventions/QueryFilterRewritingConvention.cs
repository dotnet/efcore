// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Convention that converts accesses of <see cref="DbSet{TEntity}" /> inside query filters into <see cref="EntityQueryRootExpression" />.
///     This makes them consistent with how DbSet accesses in the actual queries are represented, which allows for easier processing in the
///     query pipeline.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class QueryFilterRewritingConvention : IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="QueryFilterRewritingConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public QueryFilterRewritingConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
        DbSetAccessRewriter = new DbSetAccessRewritingExpressionVisitor(dependencies.ContextType);
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Visitor used to rewrite <see cref="DbSet{TEntity}" /> accesses encountered in query filters
    ///     to <see cref="EntityQueryRootExpression" />.
    /// </summary>
    protected virtual DbSetAccessRewritingExpressionVisitor DbSetAccessRewriter { get; set; }

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
    ///     A visitor that rewrites DbSet accesses encountered in an expression to <see cref="EntityQueryRootExpression" />.
    /// </summary>
    protected class DbSetAccessRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly Type _contextType;
        private IReadOnlyModel? _model;

        /// <summary>
        ///     Creates a new instance of <see cref="DbSetAccessRewritingExpressionVisitor" />.
        /// </summary>
        /// <param name="contextType">The clr type of derived DbContext.</param>
        public DbSetAccessRewritingExpressionVisitor(Type contextType)
        {
            _contextType = contextType;
        }

        /// <summary>
        ///     Rewrites DbSet accesses encountered in the expression to <see cref="EntityQueryRootExpression" />.
        /// </summary>
        /// <param name="model">The model to look for entity types.</param>
        /// <param name="expression">The query expression to rewrite.</param>
        public Expression Rewrite(IReadOnlyModel model, Expression expression)
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
                var entityClrType = memberExpression.Type.GetGenericArguments()[0];
                return new EntityQueryRootExpression(FindEntityType(entityClrType)!);
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
                IEntityType? entityType;
                var entityClrType = methodCallExpression.Type.GetGenericArguments()[0];
                if (methodCallExpression.Arguments.Count == 1)
                {
                    // STET Set method
                    var entityTypeName = methodCallExpression.Arguments[0].GetConstantValue<string>();
                    entityType = (IEntityType?)_model.FindEntityType(entityTypeName);
                }
                else
                {
                    entityType = FindEntityType(entityClrType);
                }

                if (entityType == null)
                {
                    if (_model.IsShared(entityClrType))
                    {
                        throw new InvalidOperationException(CoreStrings.InvalidSetSharedType(entityClrType.ShortDisplayName()));
                    }

                    var findSameTypeName = ((IModel)_model).FindSameTypeNameWithDifferentNamespace(entityClrType);
                    //if the same name exists in your entity types we will show you the full namespace of the type
                    if (!string.IsNullOrEmpty(findSameTypeName))
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InvalidSetSameTypeWithDifferentNamespace(entityClrType.DisplayName(), findSameTypeName));
                    }

                    throw new InvalidOperationException(CoreStrings.InvalidSetType(entityClrType.ShortDisplayName()));
                }

                if (entityType.IsOwned())
                {
                    var message = CoreStrings.InvalidSetTypeOwned(
                        entityType.DisplayName(), entityType.FindOwnership()!.PrincipalEntityType.DisplayName());

                    throw new InvalidOperationException(message);
                }

                if (entityType.ClrType != entityClrType)
                {
                    var message = CoreStrings.DbSetIncorrectGenericType(
                        entityType.ShortName(), entityType.ClrType.ShortDisplayName(), entityClrType.ShortDisplayName());

                    throw new InvalidOperationException(message);
                }

                return new EntityQueryRootExpression(entityType);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private IEntityType? FindEntityType(Type entityClrType)
            => ((IModel)_model!).FindRuntimeEntityType(entityClrType);
    }
}
