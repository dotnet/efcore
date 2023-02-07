// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that have involving mapping of a property to a stored procedure.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class StoredProcedurePropertyEventData : PropertyEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="entityType">The entity type that the stored procedure is mapped to.</param>
    /// <param name="property">The property.</param>
    /// <param name="storedProcedureName">The stored procedure name.</param>
    public StoredProcedurePropertyEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IEntityType entityType,
        IProperty property,
        string storedProcedureName)
        : base(eventDefinition, messageGenerator, property)
    {
        EntityType = entityType;
        StoredProcedureName = storedProcedureName;
    }

    /// <summary>
    ///     The entity type that the stored procedure is mapped to.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <summary>
    ///     The stored procedure name.
    /// </summary>
    public virtual string StoredProcedureName { get; }
}
