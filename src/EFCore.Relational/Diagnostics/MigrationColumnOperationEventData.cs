// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The <see cref="DiagnosticSource" /> event payload for events that reference a Migrations column operation.
/// </summary>
public class MigrationColumnOperationEventData : EventData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MigrationColumnOperationEventData" /> class.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="columnOperation">The column operation.</param>
    public MigrationColumnOperationEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        ColumnOperation columnOperation)
        : base(eventDefinition, messageGenerator)
    {
        ColumnOperation = columnOperation;
    }

    /// <summary>
    ///     Gets the column operation.
    /// </summary>
    /// <value> The column operation. </value>
    public virtual ColumnOperation ColumnOperation { get; }
}
