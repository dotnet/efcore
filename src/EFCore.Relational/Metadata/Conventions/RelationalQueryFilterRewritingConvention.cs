// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <inheritdoc />
public class RelationalQueryFilterRewritingConvention : QueryFilterRewritingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationalQueryFilterRewritingConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public RelationalQueryFilterRewritingConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies)
    {
        RelationalDependencies = relationalDependencies;
        DbSetAccessRewriter = new RelationalDbSetAccessRewritingExpressionVisitor(Dependencies.ContextType);
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public override void ProcessModelFinalizing(
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

#pragma warning disable CS0618 // Type or member is obsolete
            var definingQuery = ((IEntityType)entityType).GetDefiningQuery();
            if (definingQuery != null)
            {
                entityType.SetDefiningQuery((LambdaExpression)DbSetAccessRewriter.Rewrite(modelBuilder.Metadata, definingQuery));
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }

    /// <inheritdoc />
    protected class RelationalDbSetAccessRewritingExpressionVisitor : DbSetAccessRewritingExpressionVisitor
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalDbSetAccessRewritingExpressionVisitor" />.
        /// </summary>
        /// <param name="contextType">The clr type of derived DbContext.</param>
        public RelationalDbSetAccessRewritingExpressionVisitor(Type contextType)
            : base(contextType)
        {
        }

        /// <inheritdoc />
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var methodName = methodCallExpression.Method.Name;
            if (methodCallExpression.Method.DeclaringType == typeof(RelationalQueryableExtensions)
                && methodName is nameof(RelationalQueryableExtensions.FromSqlRaw)
                    or nameof(RelationalQueryableExtensions.FromSqlInterpolated)
                    or nameof(RelationalQueryableExtensions.FromSql))
            {
                var newSource = (EntityQueryRootExpression)Visit(methodCallExpression.Arguments[0]);

                string sql;
                Expression argument;

                if (methodName == nameof(RelationalQueryableExtensions.FromSqlRaw))
                {
                    sql = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value!;
                    argument = methodCallExpression.Arguments[2];
                }
                else
                {
                    var formattableString = Expression.Lambda<Func<FormattableString>>(
                            Expression.Convert(methodCallExpression.Arguments[1], typeof(FormattableString)))
                        .Compile(preferInterpretation: true)
                        .Invoke();

                    sql = formattableString.Format;
                    argument = Expression.Constant(formattableString.GetArguments());
                }

                return new FromSqlQueryRootExpression(newSource.EntityType, sql, argument);
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }
}
