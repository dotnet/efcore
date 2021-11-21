// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The <see cref="DiagnosticSource" /> event payload base class for events that
///     reference an entity type and a schema
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class EntityTypeSchemaEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="schema">The schema.</param>
    public EntityTypeSchemaEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IEntityType entityType,
        string schema)
        : base(eventDefinition, messageGenerator)
    {
        EntityType = entityType;
        Schema = schema;
    }

    /// <summary>
    ///     The entity type.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <summary>
    ///     The schema.
    /// </summary>
    public virtual string Schema { get; }
}
