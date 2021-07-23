// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that adds entity types based on the <see cref="DbSet{TEntity}" /> properties defined on the
    ///     derived <see cref="DbContext" /> class.
    /// </summary>
    public class DbSetFindingConvention : IModelInitializedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="DbSetFindingConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public DbSetFindingConvention(ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after a model is initialized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelInitialized(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var setInfo in Dependencies.SetFinder.FindSets(Dependencies.ContextType))
            {
                modelBuilder.Entity(setInfo.Type, fromDataAnnotation: true);
            }
        }
    }
}
