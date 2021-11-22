// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The <see cref="DiagnosticSource" /> event payload for
///     <see cref="RelationalEventId" /> migrations assembly events.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class MigrationAssemblyEventData : MigratorEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="migrator">The <see cref="IMigrator" /> in use.</param>
    /// <param name="migrationsAssembly">The <see cref="IMigrationsAssembly" /> in use.</param>
    public MigrationAssemblyEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IMigrator migrator,
        IMigrationsAssembly migrationsAssembly)
        : base(eventDefinition, messageGenerator, migrator)
    {
        MigrationsAssembly = migrationsAssembly;
    }

    /// <summary>
    ///     The <see cref="IMigrationsAssembly" /> in use.
    /// </summary>
    public virtual IMigrationsAssembly MigrationsAssembly { get; }
}
