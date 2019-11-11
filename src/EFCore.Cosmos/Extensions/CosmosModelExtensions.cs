// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IModel" /> for Cosmos metadata.
    /// </summary>
    public static class CosmosModelExtensions
    {
        /// <summary>
        ///     Returns the default container name.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The default container name. </returns>
        public static string GetDefaultContainer([NotNull] this IModel model)
            => (string)model[CosmosAnnotationNames.ContainerName];

        /// <summary>
        ///     Sets the default container name.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="name"> The name to set. </param>
        public static void SetDefaultContainer([NotNull] this IMutableModel model, [CanBeNull] string name)
            => model.SetOrRemoveAnnotation(
                CosmosAnnotationNames.ContainerName,
                Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        ///     Sets the default container name.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetDefaultContainer(
            [NotNull] this IConventionModel model, [CanBeNull] string name, bool fromDataAnnotation = false)
            => model.SetOrRemoveAnnotation(
                CosmosAnnotationNames.ContainerName,
                Check.NullButNotEmpty(name, nameof(name)),
                fromDataAnnotation);

        /// <summary>
        ///     Returns the configuration source for the default container name.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The configuration source for the default container name.</returns>
        public static ConfigurationSource? GetDefaultContainerConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(CosmosAnnotationNames.ContainerName)?.GetConfigurationSource();
    }
}
