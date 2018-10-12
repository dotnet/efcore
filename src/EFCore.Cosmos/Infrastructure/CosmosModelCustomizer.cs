// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Builds the model for a given context. This default implementation builds the model by calling
    ///         <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> on the context.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class CosmosModelCustomizer : ModelCustomizer
    {
        public CosmosModelCustomizer(ModelCustomizerDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     <para>
        ///         Performs additional configuration of the model in addition to what is discovered by convention. This implementation
        ///         builds the model for a given context by calling <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" />
        ///         on the context.
        ///     </para>
        /// </summary>
        /// <param name="modelBuilder">
        ///     The builder being used to construct the model.
        /// </param>
        /// <param name="context">
        ///     The context instance that the model is being created for.
        /// </param>
        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.GetInfrastructure().Cosmos(ConfigurationSource.Convention).HasDefaultContainerName(context.GetType().Name);

            base.Customize(modelBuilder, context);
        }
    }
}
