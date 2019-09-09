// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
        public static ModelBuilder HasDefaultContainer(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            modelBuilder.Model.SetDefaultContainer(name);

            return modelBuilder;
        }

        /// <summary>
        ///     Configures the default container name that will be used if no name
        ///     is explicitly configured for an entity type.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The default container name. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionModelBuilder HasDefaultContainer(
            [NotNull] this IConventionModelBuilder modelBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
        {
            if (!modelBuilder.CanSetDefaultContainer(name, fromDataAnnotation))
            {
                return null;
            }

            modelBuilder.Metadata.SetDefaultContainer(name, fromDataAnnotation);

            return modelBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the given container name can be set as default.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The default container name. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given container name can be set as default. </returns>
        public static bool CanSetDefaultContainer(
            [NotNull] this IConventionModelBuilder modelBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            return modelBuilder.CanSetAnnotation(CosmosAnnotationNames.ContainerName, name, fromDataAnnotation);
        }
    }
}
