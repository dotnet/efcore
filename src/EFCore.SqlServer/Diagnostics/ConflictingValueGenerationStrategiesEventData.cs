// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that have
///     a property.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public class ConflictingValueGenerationStrategiesEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="sqlServerValueGenerationStrategy">The SQL Server value generation strategy.</param>
    /// <param name="otherValueGenerationStrategy">The other value generation strategy.</param>
    /// <param name="property">The property.</param>
    public ConflictingValueGenerationStrategiesEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        SqlServerValueGenerationStrategy sqlServerValueGenerationStrategy,
        string otherValueGenerationStrategy,
        IReadOnlyProperty property)
        : base(eventDefinition, messageGenerator)
    {
        SqlServerValueGenerationStrategy = sqlServerValueGenerationStrategy;
        OtherValueGenerationStrategy = otherValueGenerationStrategy;
        Property = property;
    }

    /// <summary>
    ///     The SQL Server value generation strategy.
    /// </summary>
    public virtual SqlServerValueGenerationStrategy SqlServerValueGenerationStrategy { get; }

    /// <summary>
    ///     The other value generation strategy.
    /// </summary>
    public virtual string OtherValueGenerationStrategy { get; }

    /// <summary>
    ///     The property.
    /// </summary>
    public virtual IReadOnlyProperty Property { get; }
}
