// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that indicate
///     <see cref="DbContext.SaveChanges()" /> has completed.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class SaveChangesCompletedEventData : DbContextEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="context">The current <see cref="DbContext" />.</param>
    /// <param name="entitiesSavedCount">The number of entities saved to the database.</param>
    public SaveChangesCompletedEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        DbContext context,
        int entitiesSavedCount)
        : base(eventDefinition, messageGenerator, context)
    {
        EntitiesSavedCount = entitiesSavedCount;
    }

    /// <summary>
    ///     The number of entities saved to the database.
    /// </summary>
    public virtual int EntitiesSavedCount { get; }
}
