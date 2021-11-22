// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that have
///     a <see cref="ValueConverter" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class ValueConverterEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="mappingClrType">The CLR type.</param>
    /// <param name="valueConverter">The <see cref="ValueConverter" />.</param>
    public ValueConverterEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        Type mappingClrType,
        ValueConverter valueConverter)
        : base(eventDefinition, messageGenerator)
    {
        MappingClrType = mappingClrType;
        ValueConverter = valueConverter;
    }

    /// <summary>
    ///     The CLR type.
    /// </summary>
    public virtual Type MappingClrType { get; }

    /// <summary>
    ///     The <see cref="ValueConverter" />.
    /// </summary>
    public virtual ValueConverter ValueConverter { get; }
}
