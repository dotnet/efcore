// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Convention that converts accesses of <see cref="DbSet{TEntity}" /> inside query filters and defining queries into
    ///     <see cref="QueryRootExpression" />.
    ///     This makes them consistent with how DbSet accesses in the actual queries are represented, which allows for easier processing in the
    ///     query pipeline.
    /// </summary>
    public class DefiningQueryRewritingConvention : QueryFilterRewritingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="QueryFilterRewritingConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public DefiningQueryRewritingConvention(ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc />
        public override void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var definingQuery = entityType.GetInMemoryQuery();
                if (definingQuery != null)
                {
                    entityType.SetInMemoryQuery((LambdaExpression)DbSetAccessRewriter.Rewrite(modelBuilder.Metadata, definingQuery));
                }
            }
        }
    }
}
