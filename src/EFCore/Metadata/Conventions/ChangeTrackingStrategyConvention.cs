// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that sets a flag on the model to always skip detecting changes if no entity type is using the
    ///     <see cref="ChangeTrackingStrategy.Snapshot" /> strategy.
    /// </summary>
    public class ChangeTrackingStrategyConvention : IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ChangeTrackingStrategyConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public ChangeTrackingStrategyConvention(ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                if (entityType.GetChangeTrackingStrategy() == ChangeTrackingStrategy.Snapshot)
                {
                    return;
                }
            }

            if (modelBuilder.Metadata is Model model)
            {
                model.SetSkipDetectChanges(true);
            }
        }
    }
}
