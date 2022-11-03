// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The <see cref="DiagnosticSource" /> event payload for
///     <see cref="CoreEventId" /> execution strategy events.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class ExecutionStrategyEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="exceptionsEncountered">
    ///     The exceptions that have been caught during the execution of an operation.
    /// </param>
    /// <param name="delay">The delay before retrying the operation.</param>
    /// <param name="async">
    ///     Indicates whether or not the command was executed asynchronously.
    /// </param>
    public ExecutionStrategyEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IReadOnlyList<Exception> exceptionsEncountered,
        TimeSpan delay,
        bool async)
        : base(eventDefinition, messageGenerator)
    {
        ExceptionsEncountered = exceptionsEncountered;
        Delay = delay;
        IsAsync = async;
    }

    /// <summary>
    ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
    /// </summary>
    public virtual IReadOnlyList<Exception> ExceptionsEncountered { get; }

    /// <summary>
    ///     The delay before retrying the operation.
    /// </summary>
    public virtual TimeSpan Delay { get; }

    /// <summary>
    ///     Indicates whether or not the operation is being executed asynchronously.
    /// </summary>
    public virtual bool IsAsync { get; }
}
