// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload used when a <see cref="DbUpdateConcurrencyException" /> is being thrown.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class ConcurrencyExceptionEventData : DbContextErrorEventData
{
    private readonly IReadOnlyList<IUpdateEntry> _internalEntries;
    private IReadOnlyList<EntityEntry>? _entries;

    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="context">The current <see cref="DbContext" />.</param>
    /// <param name="entries">The entries that were involved in the concurrency violation.</param>
    /// <param name="exception">The exception that will be thrown, unless throwing is suppressed.</param>
    public ConcurrencyExceptionEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        DbContext context,
        IReadOnlyList<IUpdateEntry> entries,
        DbUpdateConcurrencyException exception)
        : base(eventDefinition, messageGenerator, context, exception)
    {
        _internalEntries = entries;
    }

    /// <summary>
    ///     The exception that will be thrown, unless throwing is suppressed.
    /// </summary>
    public new virtual DbUpdateConcurrencyException Exception
        => (DbUpdateConcurrencyException)base.Exception;

    /// <summary>
    ///     The entries that were involved in the concurrency violation.
    /// </summary>
    public virtual IReadOnlyList<EntityEntry> Entries
        => _entries ??= _internalEntries.Select(e => new EntityEntry((InternalEntityEntry)e)).ToList();
}
