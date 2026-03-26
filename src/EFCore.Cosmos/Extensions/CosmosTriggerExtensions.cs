// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Cosmos DB specific extension methods for <see cref="ITrigger" /> and related types.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
/// </remarks>
public static class CosmosTriggerExtensions
{
    /// <summary>
    ///     Gets the Cosmos DB trigger type for this trigger.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <returns>The Cosmos DB trigger type.</returns>
    public static TriggerType? GetTriggerType(this IReadOnlyTrigger trigger)
        => (TriggerType?)trigger[CosmosAnnotationNames.TriggerType];

    /// <summary>
    ///     Sets the Cosmos DB trigger type for this trigger.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <param name="triggerType">The Cosmos DB trigger type.</param>
    public static void SetTriggerType(this IMutableTrigger trigger, TriggerType? triggerType)
        => trigger.SetOrRemoveAnnotation(CosmosAnnotationNames.TriggerType, triggerType);

    /// <summary>
    ///     Sets the Cosmos DB trigger type for this trigger.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <param name="triggerType">The Cosmos DB trigger type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static TriggerType? SetTriggerType(
        this IConventionTrigger trigger,
        TriggerType? triggerType,
        bool fromDataAnnotation = false)
        => (TriggerType?)trigger.SetOrRemoveAnnotation(CosmosAnnotationNames.TriggerType, triggerType, fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the Cosmos DB trigger type.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the Cosmos DB trigger type.</returns>
    public static ConfigurationSource? GetTriggerTypeConfigurationSource(this IConventionTrigger trigger)
        => trigger.FindAnnotation(CosmosAnnotationNames.TriggerType)?.GetConfigurationSource();

    /// <summary>
    ///     Gets the Cosmos DB trigger operation for this trigger.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <returns>The Cosmos DB trigger operation.</returns>
    public static TriggerOperation? GetTriggerOperation(this IReadOnlyTrigger trigger)
        => (TriggerOperation?)trigger[CosmosAnnotationNames.TriggerOperation];

    /// <summary>
    ///     Sets the Cosmos DB trigger operation for this trigger.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <param name="triggerOperation">The Cosmos DB trigger operation.</param>
    public static void SetTriggerOperation(this IMutableTrigger trigger, TriggerOperation? triggerOperation)
        => trigger.SetOrRemoveAnnotation(CosmosAnnotationNames.TriggerOperation, triggerOperation);

    /// <summary>
    ///     Sets the Cosmos DB trigger operation for this trigger.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <param name="triggerOperation">The Cosmos DB trigger operation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static TriggerOperation? SetTriggerOperation(
        this IConventionTrigger trigger,
        TriggerOperation? triggerOperation,
        bool fromDataAnnotation = false)
        => (TriggerOperation?)trigger.SetOrRemoveAnnotation(CosmosAnnotationNames.TriggerOperation, triggerOperation, fromDataAnnotation)
            ?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the Cosmos DB trigger operation.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the Cosmos DB trigger operation.</returns>
    public static ConfigurationSource? GetTriggerOperationConfigurationSource(this IConventionTrigger trigger)
        => trigger.FindAnnotation(CosmosAnnotationNames.TriggerOperation)?.GetConfigurationSource();
}
