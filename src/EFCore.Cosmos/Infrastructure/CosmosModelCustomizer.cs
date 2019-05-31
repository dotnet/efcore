// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Cosmos.Infrastructure
{
    /// <summary>
    ///     Builds the model for a given context. This implementation builds the model by calling
    ///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> on the context and
    ///     using the context type name as the default container name.
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
            ((IConventionModel)modelBuilder.Model).Builder.ForCosmosHasDefaultContainerName(context.GetType().Name);

            base.Customize(modelBuilder, context);
        }
    }
}
