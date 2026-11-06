// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The <see cref="DiagnosticSource" /> event payload for events that reference an entity-splitting mapping fragment.
/// </summary>
public class EntityTypeMappingFragmentEventData : EventData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityTypeMappingFragmentEventData" /> class.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="entityType">The entity type that owns the mapping fragment.</param>
    /// <param name="storeObject">The store object for the mapping fragment.</param>
    /// <param name="optional">A value indicating whether the fragment is now optional.</param>
    public EntityTypeMappingFragmentEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IEntityType entityType,
        StoreObjectIdentifier storeObject,
        bool optional)
        : base(eventDefinition, messageGenerator)
    {
        EntityType = entityType;
        StoreObject = storeObject;
        IsOptional = optional;
    }

    /// <summary>
    ///     Gets the entity type that owns the mapping fragment.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <summary>
    ///     Gets the store object for the mapping fragment.
    /// </summary>
    public virtual StoreObjectIdentifier StoreObject { get; }

    /// <summary>
    ///     Gets a value indicating whether the fragment is now optional.
    /// </summary>
    public virtual bool IsOptional { get; }
}
