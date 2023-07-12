// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that
///     specify the entities being saved and the rows affected.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class SaveChangesEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="entries">Entries for the entities being saved.</param>
    /// <param name="rowsAffected">The rows affected.</param>
    public SaveChangesEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IEnumerable<IUpdateEntry> entries,
        int rowsAffected)
        : base(eventDefinition, messageGenerator)
    {
        Entries = entries;
        RowsAffected = rowsAffected;
    }

    /// <summary>
    ///     Entries for the entities being saved.
    /// </summary>
    public virtual IEnumerable<IUpdateEntry> Entries { get; }

    /// <summary>
    ///     The rows affected.
    /// </summary>
    public virtual int RowsAffected { get; }
}
