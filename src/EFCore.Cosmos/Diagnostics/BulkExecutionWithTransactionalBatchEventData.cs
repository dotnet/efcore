// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for Cosmos bulk execution enabled without AutoTransactionBehaviour.Never events.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class BulkExecutionWithTransactionalBatchEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="autoTransactionBehavior">The AutoTransactionBehaviour that was used.</param>
    public BulkExecutionWithTransactionalBatchEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        AutoTransactionBehavior autoTransactionBehavior) : base(eventDefinition, messageGenerator)
    {
        AutoTransactionBehavior = autoTransactionBehavior;
    }

    /// <summary>
    ///     The AutoTransactionBehaviour that was used.
    /// </summary>
    public virtual AutoTransactionBehavior AutoTransactionBehavior { get; }
}
