// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Cosmos DB specific extension methods for <see cref="TriggerBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
/// </remarks>
public static class CosmosTriggerBuilderExtensions
{
    /// <summary>
    ///     Configures the Cosmos DB trigger type for this trigger.
    /// </summary>
    /// <param name="triggerBuilder">The builder for the trigger being configured.</param>
    /// <param name="triggerType">The Cosmos DB trigger type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionTriggerBuilder? HasTriggerType(
        this IConventionTriggerBuilder triggerBuilder,
        TriggerType? triggerType,
        bool fromDataAnnotation = false)
    {
        if (!triggerBuilder.CanSetTriggerType(triggerType, fromDataAnnotation))
        {
            return null;
        }

        triggerBuilder.Metadata.SetTriggerType(triggerType, fromDataAnnotation);
        return triggerBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given Cosmos DB trigger type can be set for this trigger.
    /// </summary>
    /// <param name="triggerBuilder">The builder for the trigger being configured.</param>
    /// <param name="triggerType">The Cosmos DB trigger type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the Cosmos DB trigger type can be set for this trigger.</returns>
    public static bool CanSetTriggerType(
        this IConventionTriggerBuilder triggerBuilder,
        TriggerType? triggerType,
        bool fromDataAnnotation = false)
        => triggerBuilder.CanSetAnnotation(CosmosAnnotationNames.TriggerType, triggerType, fromDataAnnotation);

    /// <summary>
    ///     Configures the Cosmos DB trigger operation for this trigger.
    /// </summary>
    /// <param name="triggerBuilder">The builder for the trigger being configured.</param>
    /// <param name="triggerOperation">The Cosmos DB trigger operation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionTriggerBuilder? HasTriggerOperation(
        this IConventionTriggerBuilder triggerBuilder,
        TriggerOperation? triggerOperation,
        bool fromDataAnnotation = false)
    {
        if (!triggerBuilder.CanSetTriggerOperation(triggerOperation, fromDataAnnotation))
        {
            return null;
        }

        triggerBuilder.Metadata.SetTriggerOperation(triggerOperation, fromDataAnnotation);
        return triggerBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given Cosmos DB trigger operation can be set for this trigger.
    /// </summary>
    /// <param name="triggerBuilder">The builder for the trigger being configured.</param>
    /// <param name="triggerOperation">The Cosmos DB trigger operation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the Cosmos DB trigger operation can be set for this trigger.</returns>
    public static bool CanSetTriggerOperation(
        this IConventionTriggerBuilder triggerBuilder,
        TriggerOperation? triggerOperation,
        bool fromDataAnnotation = false)
        => triggerBuilder.CanSetAnnotation(CosmosAnnotationNames.TriggerOperation, triggerOperation, fromDataAnnotation);
}
