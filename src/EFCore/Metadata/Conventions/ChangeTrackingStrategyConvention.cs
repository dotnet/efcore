// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that sets a flag on the model to always skip detecting changes if no entity type is using the
    ///     <see cref="ChangeTrackingStrategy.Snapshot"/> strategy.
    /// </summary>
    public class ChangeTrackingStrategyConvention : IModelFinalizedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ChangeTrackingStrategyConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public ChangeTrackingStrategyConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after a model is finalized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelFinalized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                if (entityType.GetChangeTrackingStrategy() == ChangeTrackingStrategy.Snapshot)
                {
                    return;
                }
            }

            modelBuilder.HasAnnotation(ChangeDetector.SkipDetectChangesAnnotation, "true");
        }
    }
}
