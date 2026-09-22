// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The <see cref="DiagnosticSource" /> event payload for
///     <see cref="RelationalEventId" /> events of a specific migration.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class MigrationCommandEventData : MigratorEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="migrator">
    ///     The <see cref="IMigrator" /> in use.
    /// </param>
    /// <param name="migration">
    ///     The <see cref="Migration" /> being processed.
    /// </param>
    /// <param name="command">
    ///     The <see cref="MigrationCommand" /> being processed.
    /// </param>
    public MigrationCommandEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IMigrator migrator,
        Migration migration,
        MigrationCommand command)
        : base(eventDefinition, messageGenerator, migrator)
    {
        Migration = migration;
        MigrationCommand = command;
    }

    /// <summary>
    ///     The <see cref="Migration" /> being processed.
    /// </summary>
    public virtual Migration Migration { get; }

    /// <summary>
    ///     The <see cref="MigrationCommand" /> being processed.
    /// </summary>
    public virtual MigrationCommand MigrationCommand { get; }
}
