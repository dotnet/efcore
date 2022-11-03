// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for the
///     <see cref="RelationalEventId.IndexPropertiesMappedToNonOverlappingTables" /> event.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class IndexWithPropertiesEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload for the <see cref="RelationalEventId.IndexPropertiesMappedToNonOverlappingTables" /> event.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="entityType">The entity type on which the index is defined.</param>
    /// <param name="indexName">The name of the index.</param>
    /// <param name="indexPropertyNames">The names of the properties which define the index.</param>
    /// <param name="property1Name">The name of the first property name which causes this event.</param>
    /// <param name="tablesMappedToProperty1">The tables mapped to the first property.</param>
    /// <param name="property2Name">The name of the second property name which causes this event.</param>
    /// <param name="tablesMappedToProperty2">The tables mapped to the second property.</param>
    public IndexWithPropertiesEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IEntityType entityType,
        string? indexName,
        List<string> indexPropertyNames,
        string property1Name,
        List<(string Table, string? Schema)> tablesMappedToProperty1,
        string property2Name,
        List<(string Table, string? Schema)> tablesMappedToProperty2)
        : base(eventDefinition, messageGenerator)
    {
        EntityType = entityType;
        Name = indexName;
        PropertyNames = indexPropertyNames;
        Property1Name = property1Name;
        TablesMappedToProperty1 = tablesMappedToProperty1;
        Property2Name = property2Name;
        TablesMappedToProperty2 = tablesMappedToProperty2;
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
    ///     The name of the first property.
    /// </summary>
    public virtual string Property1Name { get; }

    /// <summary>
    ///     The tables mapped to the first property.
    /// </summary>
    public virtual List<(string Table, string? Schema)> TablesMappedToProperty1 { get; }

    /// <summary>
    ///     The name of the second property.
    /// </summary>
    public virtual string Property2Name { get; }

    /// <summary>
    ///     The tables mapped to the second property.
    /// </summary>
    public virtual List<(string Table, string? Schema)> TablesMappedToProperty2 { get; }
}
