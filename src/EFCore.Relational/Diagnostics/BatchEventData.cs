// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The <see cref="DiagnosticSource" /> event payload for
///     <see cref="RelationalEventId" /> batch events.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class BatchEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="entries">The entries being updated.</param>
    /// <param name="commandCount">The command count.</param>
    public BatchEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IEnumerable<IUpdateEntry> entries,
        int commandCount)
        : base(eventDefinition, messageGenerator)
    {
        Entries = entries;
        CommandCount = commandCount;
    }

    /// <summary>
    ///     The entries being updated.
    /// </summary>
    public virtual IEnumerable<IUpdateEntry> Entries { get; }

    /// <summary>
    ///     The command count.
    /// </summary>
    public virtual int CommandCount { get; }
}
