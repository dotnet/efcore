// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Cosmos-specific extension methods for <see cref="ModelBuilder" />.
    /// </summary>
    public static class CosmosModelBuilderExtensions
    {
        /// <summary>
        ///     Configures the default container name that will be used if no name
        ///     is explicitly configured for an entity type.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The default container name. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder HasDefaultContainerName(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            modelBuilder.GetInfrastructure().Cosmos(ConfigurationSource.Explicit).HasDefaultContainerName(name);

            return modelBuilder;
        }
    }
}
