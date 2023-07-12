// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The <see cref="DiagnosticSource" /> event payload for
///     <see cref="RelationalEventId" /> migration scripting events.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class MigrationScriptingEventData : MigrationEventData
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
    /// <param name="fromMigration">
    ///     The migration that scripting is starting from.
    /// </param>
    /// <param name="toMigration">
    ///     The migration that scripting is going to.
    /// </param>
    /// <param name="idempotent">
    ///     Indicates whether or not the script is idempotent.
    /// </param>
    public MigrationScriptingEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IMigrator migrator,
        Migration migration,
        string? fromMigration,
        string? toMigration,
        bool idempotent)
        : base(eventDefinition, messageGenerator, migrator, migration)
    {
        FromMigration = fromMigration;
        ToMigration = toMigration;
        IsIdempotent = idempotent;
    }

    /// <summary>
    ///     The migration that scripting is starting from.
    /// </summary>
    public virtual string? FromMigration { get; }

    /// <summary>
    ///     The migration that scripting is going to.
    /// </summary>
    public virtual string? ToMigration { get; }

    /// <summary>
    ///     Indicates whether or not the script is idempotent.
    /// </summary>
    public virtual bool IsIdempotent { get; }
}
