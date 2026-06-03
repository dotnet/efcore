// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Model extension methods for Cosmos metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public static class CosmosModelExtensions
{
    /// <summary>
    ///     Returns the default container name.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The default container name.</returns>
    public static string? GetDefaultContainer(this IReadOnlyModel model)
        => (string?)model[CosmosAnnotationNames.ContainerName];

    /// <summary>
    ///     Sets the default container name.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="name">The name to set.</param>
    public static void SetDefaultContainer(this IMutableModel model, string? name)
        => model.SetOrRemoveAnnotation(
            CosmosAnnotationNames.ContainerName,
            Check.NullButNotEmpty(name, nameof(name)));

    /// <summary>
    ///     Sets the default container name.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="name">The name to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetDefaultContainer(
        this IConventionModel model,
        string? name,
        bool fromDataAnnotation = false)
        => (string?)model.SetOrRemoveAnnotation(
            CosmosAnnotationNames.ContainerName,
            Check.NullButNotEmpty(name, nameof(name)),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the configuration source for the default container name.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The configuration source for the default container name.</returns>
    public static ConfigurationSource? GetDefaultContainerConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(CosmosAnnotationNames.ContainerName)?.GetConfigurationSource();

    /// <summary>
    ///     Returns a value indicating whether the setting for always creating the "__id" property can be set
    ///     from the current configuration source
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The configured value.</returns>
    public static bool? GetHasShadowIds(this IReadOnlyModel model)
        => (bool?)model[CosmosAnnotationNames.HasShadowId];

    /// <summary>
    ///     Forces model building to always create a "__id" shadow property mapped to the JSON "id". This was the default
    ///     behavior before EF Core 9.0.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="alwaysCreate">
    ///     <see langword="true" /> to force __id creation, <see langword="false" /> to not force __id creation,
    ///     <see langword="null" /> to revert to the default setting.
    /// </param>
    public static void SetHasShadowIds(this IMutableModel model, bool? alwaysCreate)
        => model.SetOrRemoveAnnotation(CosmosAnnotationNames.HasShadowId, alwaysCreate);

    /// <summary>
    ///     Forces model building to always create a "__id" shadow property mapped to the JSON "id". This was the default
    ///     behavior before EF Core 9.0.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="alwaysCreate">
    ///     <see langword="true" /> to force __id creation, <see langword="false" /> to not force __id creation,
    ///     <see langword="null" /> to revert to the default setting.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static bool? SetHasShadowIds(
        this IConventionModel model,
        bool? alwaysCreate,
        bool fromDataAnnotation = false)
        => (bool?)model.SetOrRemoveAnnotation(CosmosAnnotationNames.HasShadowId, alwaysCreate, fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" />
    ///     for <see cref="GetHasShadowIds" />.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The <see cref="ConfigurationSource" />.</returns>
    public static ConfigurationSource? GetHasShadowIdsConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(CosmosAnnotationNames.HasShadowId)?.GetConfigurationSource();

    /// <summary>
    ///     Returns a value indicating whether the entity type discriminator should be included in the JSON "id" value.
    ///     Prior to EF Core 9, it was always included. Starting with EF Core 9, it is not included by default.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The <see cref="IdDiscriminatorMode" /> or <see langword="null" /> if not set.</returns>
    public static IdDiscriminatorMode? GetDiscriminatorInKey(this IReadOnlyModel model)
        => (IdDiscriminatorMode?)model[CosmosAnnotationNames.DiscriminatorInKey];

    /// <summary>
    ///     Includes the entity type discriminator in the JSON "id".
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="behavior">The behavior to use, or <see langword="null" /> to reset the behavior to the default.</param>
    public static void SetDiscriminatorInKey(this IMutableModel model, IdDiscriminatorMode? behavior)
        => model.SetOrRemoveAnnotation(CosmosAnnotationNames.DiscriminatorInKey, behavior);

    /// <summary>
    ///     Includes the entity type discriminator in the JSON "id".
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="behavior">The behavior to use, or <see langword="null" /> to reset the behavior to the default.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static IdDiscriminatorMode? SetDiscriminatorInKey(
        this IConventionModel model,
        IdDiscriminatorMode? behavior,
        bool fromDataAnnotation = false)
        => (IdDiscriminatorMode?)model.SetOrRemoveAnnotation(
            CosmosAnnotationNames.DiscriminatorInKey, behavior, fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" />
    ///     for <see cref="GetDiscriminatorInKey" />.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The <see cref="ConfigurationSource" />.</returns>
    public static ConfigurationSource? GetDiscriminatorInKeyConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(CosmosAnnotationNames.DiscriminatorInKey)?.GetConfigurationSource();

    /// <summary>
    ///     Returns the provisioned throughput at database scope.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The throughput.</returns>
    public static ThroughputProperties? GetThroughput(this IReadOnlyModel model)
        => (ThroughputProperties?)model[CosmosAnnotationNames.Throughput];

    /// <summary>
    ///     Sets the provisioned throughput at database scope.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="throughput">The throughput to set.</param>
    /// <param name="autoscale">Whether autoscale is enabled.</param>
    public static void SetThroughput(this IMutableModel model, int? throughput, bool? autoscale)
        => model.SetOrRemoveAnnotation(
            CosmosAnnotationNames.Throughput,
            throughput == null || autoscale == null
                ? null
                : autoscale.Value
                    ? ThroughputProperties.CreateAutoscaleThroughput(throughput.Value)
                    : ThroughputProperties.CreateManualThroughput(throughput.Value));

    /// <summary>
    ///     Sets the provisioned throughput at database scope.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="throughput">The throughput to set.</param>
    /// <param name="autoscale">Whether autoscale is enabled.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static int? SetThroughput(
        this IConventionModel model,
        int? throughput,
        bool? autoscale,
        bool fromDataAnnotation = false)
    {
        var valueSet = (ThroughputProperties?)model.SetOrRemoveAnnotation(
            CosmosAnnotationNames.Throughput,
            throughput == null || autoscale == null
                ? null
                : autoscale.Value
                    ? ThroughputProperties.CreateAutoscaleThroughput(throughput.Value)
                    : ThroughputProperties.CreateManualThroughput(throughput.Value),
            fromDataAnnotation)?.Value;
        return valueSet?.AutoscaleMaxThroughput ?? valueSet?.Throughput;
    }

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the provisioned throughput at database scope.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the throughput.</returns>
    public static ConfigurationSource? GetThroughputConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(CosmosAnnotationNames.Throughput)
            ?.GetConfigurationSource();
}
