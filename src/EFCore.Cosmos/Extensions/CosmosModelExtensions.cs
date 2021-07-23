// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Model extension methods for Cosmos metadata.
    /// </summary>
    public static class CosmosModelExtensions
    {
        /// <summary>
        ///     Returns the default container name.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The default container name. </returns>
        public static string? GetDefaultContainer(this IReadOnlyModel model)
            => (string?)model[CosmosAnnotationNames.ContainerName];

        /// <summary>
        ///     Sets the default container name.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="name"> The name to set. </param>
        public static void SetDefaultContainer(this IMutableModel model, string? name)
            => model.SetOrRemoveAnnotation(
                CosmosAnnotationNames.ContainerName,
                Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        ///     Sets the default container name.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetDefaultContainer(
            this IConventionModel model,
            string? name,
            bool fromDataAnnotation = false)
        {
            model.SetOrRemoveAnnotation(
                CosmosAnnotationNames.ContainerName,
                Check.NullButNotEmpty(name, nameof(name)),
                fromDataAnnotation);

            return name;
        }

        /// <summary>
        ///     Returns the configuration source for the default container name.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The configuration source for the default container name.</returns>
        public static ConfigurationSource? GetDefaultContainerConfigurationSource(this IConventionModel model)
            => model.FindAnnotation(CosmosAnnotationNames.ContainerName)?.GetConfigurationSource();
    }
}
