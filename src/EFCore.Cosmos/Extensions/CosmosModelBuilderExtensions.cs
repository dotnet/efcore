// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Cosmos-specific extension methods for <see cref="ModelBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public static class CosmosModelBuilderExtensions
{
    /// <summary>
    ///     Configures the default container name that will be used if no name
    ///     is explicitly configured for an entity type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The default container name.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasDefaultContainer(
        this ModelBuilder modelBuilder,
        string? name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        modelBuilder.Model.SetDefaultContainer(name);

        return modelBuilder;
    }

    /// <summary>
    ///     Configures the default container name that will be used if no name
    ///     is explicitly configured for an entity type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The default container name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionModelBuilder? HasDefaultContainer(
        this IConventionModelBuilder modelBuilder,
        string? name,
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The default container name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given container name can be set as default.</returns>
    public static bool CanSetDefaultContainer(
        this IConventionModelBuilder modelBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(name, nameof(name));

        return modelBuilder.CanSetAnnotation(CosmosAnnotationNames.ContainerName, name, fromDataAnnotation);
    }

    /// <summary>
    ///     Configures the manual provisioned throughput offering.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="throughput">The throughput to set.</param>
    public static ModelBuilder HasManualThroughput(this ModelBuilder modelBuilder, int? throughput)
    {
        modelBuilder.Model.SetThroughput(throughput, autoscale: false);

        return modelBuilder;
    }

    /// <summary>
    ///     Configures the autoscale provisioned throughput offering.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="throughput">The throughput to set.</param>
    public static ModelBuilder HasAutoscaleThroughput(this ModelBuilder modelBuilder, int? throughput)
    {
        modelBuilder.Model.SetThroughput(throughput, autoscale: true);

        return modelBuilder;
    }

    /// <summary>
    ///     Configures the provisioned throughput.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="throughput">The throughput to set.</param>
    /// <param name="autoscale">Whether autoscale is enabled.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static IConventionModelBuilder? HasThroughput(
        this IConventionModelBuilder modelBuilder,
        int? throughput,
        bool autoscale,
        bool fromDataAnnotation = false)
    {
        if (!modelBuilder.CanSetThroughput(throughput, autoscale, fromDataAnnotation))
        {
            return null;
        }

        modelBuilder.Metadata.SetThroughput(throughput, autoscale, fromDataAnnotation);

        return modelBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given throughput can be set.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="throughput">The throughput to set.</param>
    /// <param name="autoscale">Whether autoscale is enabled.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given container name can be set as default.</returns>
    public static bool CanSetThroughput(
        this IConventionModelBuilder modelBuilder,
        int? throughput,
        bool autoscale,
        bool fromDataAnnotation = false)
    {
        var existingAnnotation = modelBuilder.Metadata.FindAnnotation(CosmosAnnotationNames.Throughput);
        if (existingAnnotation == null)
        {
            return true;
        }

        var configurationSource = fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention;
        if (configurationSource.Overrides(existingAnnotation.GetConfigurationSource()))
        {
            return true;
        }

        var existingThroughput = (ThroughputProperties?)existingAnnotation.Value;
        return autoscale
            ? existingThroughput?.Throughput == throughput
            : existingThroughput?.AutoscaleMaxThroughput == throughput;
    }
}
