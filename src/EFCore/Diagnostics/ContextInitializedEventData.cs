// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for context initialization events.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class ContextInitializedEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="context">The <see cref="DbContext" /> that is initialized.</param>
    /// <param name="contextOptions">The <see cref="DbContextOptions" /> being used.</param>
    public ContextInitializedEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        DbContext context,
        DbContextOptions contextOptions)
        : base(eventDefinition, messageGenerator)
    {
        Context = context;
        ContextOptions = contextOptions;
    }

    /// <summary>
    ///     The <see cref="DbContext" /> that is initialized.
    /// </summary>
    public virtual DbContext Context { get; }

    /// <summary>
    ///     The <see cref="DbContextOptions" /> being used.
    /// </summary>
    public virtual DbContextOptions ContextOptions { get; }
}
