// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The <see cref="DiagnosticSource" /> event payload for
///     <see cref="RelationalEventId" /> min batch size events.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class MinBatchSizeEventData : BatchEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="entries">The entries being updated.</param>
    /// <param name="commandCount">The command count.</param>
    /// <param name="minBatchSize">The minimum batch size.</param>
    public MinBatchSizeEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IEnumerable<IUpdateEntry> entries,
        int commandCount,
        int minBatchSize)
        : base(eventDefinition, messageGenerator, entries, commandCount)
    {
        MinBatchSize = minBatchSize;
    }

    /// <summary>
    ///     The minimum batch size.
    /// </summary>
    public virtual int MinBatchSize { get; }
}
