// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that have columns.
/// </summary>
public class ColumnsEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="storeObject">The table.</param>
    /// <param name="columns">The columns.</param>
    public ColumnsEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        StoreObjectIdentifier storeObject,
        IReadOnlyList<string> columns)
        : base(eventDefinition, messageGenerator)
    {
        StoreObject = storeObject;
        Columns = columns;
    }

    /// <summary>
    ///     Gets the table.
    /// </summary>
    /// <value> The table. </value>
    public virtual StoreObjectIdentifier StoreObject { get; }

    /// <summary>
    ///     Gets the columns.
    /// </summary>
    /// <value> The columns. </value>
    public virtual IReadOnlyList<string> Columns { get; }
}
