// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the default container name as the context type name.
    /// </summary>
    public class ContextContainerConvention : IModelInitializedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ContextContainerConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public ContextContainerConvention(ProviderConventionSetBuilderDependencies dependencies)
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
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(context, nameof(context));

            modelBuilder.HasDefaultContainer(Dependencies.ContextType.Name);
        }
    }
}
