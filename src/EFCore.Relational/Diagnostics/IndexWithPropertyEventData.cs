// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for
///     the events involving an invalid property name on an index.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class IndexWithPropertyEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload for indexes with a invalid property.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="entityType">The entity type on which the index is defined.</param>
    /// <param name="indexName">The name of the index.</param>
    /// <param name="indexPropertyNames">The names of the properties which define the index.</param>
    /// <param name="invalidPropertyName">The property name which is invalid.</param>
    public IndexWithPropertyEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IEntityType entityType,
        string? indexName,
        List<string> indexPropertyNames,
        string invalidPropertyName)
        : base(eventDefinition, messageGenerator)
    {
        EntityType = entityType;
        Name = indexName;
        PropertyNames = indexPropertyNames;
        PropertyName = invalidPropertyName;
    }

    /// <summary>
    ///     The entity type on which the index is defined.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <summary>
    ///     The name of the index.
    /// </summary>
    public virtual string? Name { get; }

    /// <summary>
    ///     The list of properties which define the index.
    /// </summary>
    public virtual List<string> PropertyNames { get; }

    /// <summary>
    ///     The name of the specific property.
    /// </summary>
    public virtual string PropertyName { get; }
}
