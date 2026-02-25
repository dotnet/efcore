// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for Cosmos transactional batch executed events.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class CosmosTransactionalBatchExecutedEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="elapsed">The time elapsed since the batch was sent to the database.</param>
    /// <param name="requestCharge">The request charge in RU.</param>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="containerId">The ID of the Cosmos container being queried.</param>
    /// <param name="partitionKeyValue">The key of the Cosmos partition that the batch is using.</param>
    /// <param name="documentIds">The ids of the documents that were written to in the transactional batch.</param>
    /// <param name="logSensitiveData">Indicates whether the application allows logging of sensitive data.</param>
    public CosmosTransactionalBatchExecutedEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        TimeSpan elapsed,
        double requestCharge,
        string activityId,
        string containerId,
        PartitionKey partitionKeyValue,
        string documentIds,
        bool logSensitiveData)
        : base(eventDefinition, messageGenerator)
    {
        Elapsed = elapsed;
        RequestCharge = requestCharge;
        ActivityId = activityId;
        ContainerId = containerId;
        PartitionKeyValue = partitionKeyValue;
        DocumentIds = documentIds;
        LogSensitiveData = logSensitiveData;
    }

    /// <summary>
    ///     The time elapsed since the command was sent to the database.
    /// </summary>
    public virtual TimeSpan Elapsed { get; }

    /// <summary>
    ///     The request charge in RU.
    /// </summary>
    public virtual double RequestCharge { get; }

    /// <summary>
    ///     The activity ID.
    /// </summary>
    public virtual string ActivityId { get; }

    /// <summary>
    ///     The ID of the Cosmos container being queried.
    /// </summary>
    public virtual string ContainerId { get; }

    /// <summary>
    ///     The key of the Cosmos partition that the query is using.
    /// </summary>
    public virtual PartitionKey PartitionKeyValue { get; }

    /// <summary>
    ///     The ids of the documents that were written to in the transactional batch.
    /// </summary>
    public virtual string DocumentIds { get; }

    /// <summary>
    ///     Indicates whether the application allows logging of sensitive data.
    /// </summary>
    public virtual bool LogSensitiveData { get; }
}
