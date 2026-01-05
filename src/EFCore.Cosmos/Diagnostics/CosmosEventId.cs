// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Event IDs for Cosmos events that correspond to messages logged to an <see cref="ILogger" />
///     and events sent to a <see cref="DiagnosticSource" />.
/// </summary>
/// <remarks>
///     <para>
///         These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
///         behavior of warnings.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see>, and
///         <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
///     </para>
/// </remarks>
public static class CosmosEventId
{
    // Warning: These values must not change between releases.
    // Only add new values to the end of sections, never in the middle.
    // Try to use <Noun><Verb> naming and be consistent with existing names.
    private enum Id
    {
        // Database events
        [Obsolete("Synchronous I/O has been fully removed and now always throws.")]
        SyncNotSupported = CoreEventId.ProviderBaseId,

        // Command events
        ExecutingSqlQuery = CoreEventId.ProviderBaseId + 100,
        ExecutingReadItem,
        ExecutedReadNext,
        ExecutedReadItem,
        ExecutedCreateItem,
        ExecutedReplaceItem,
        ExecutedDeleteItem,
        ExecutedTransactionalBatch,

        // Update events
        PrimaryKeyValueNotSet = CoreEventId.ProviderBaseId + 200,
        BulkExecutionWithTransactionalBatch,

        // Model validation events
        NoPartitionKeyDefined = CoreEventId.ProviderBaseId + 600,
    }

    private static readonly string CommandPrefix = DbLoggerCategory.Database.Command.Name + ".";

    /// <summary>
    ///     A SQL query is going to be executed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CosmosQueryEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ExecutingSqlQuery
        = new((int)Id.ExecutingSqlQuery, CommandPrefix + Id.ExecutingSqlQuery);

    /// <summary>
    ///     ReadItem is going to be executed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CosmosReadItemEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ExecutingReadItem
        = new((int)Id.ExecutingReadItem, CommandPrefix + Id.ExecutingReadItem);

    /// <summary>
    ///     ReadNext was executed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CosmosQueryExecutedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ExecutedReadNext
        = new((int)Id.ExecutedReadNext, CommandPrefix + Id.ExecutedReadNext);

    /// <summary>
    ///     ReadItem was executed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CosmosItemCommandExecutedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ExecutedReadItem
        = new((int)Id.ExecutedReadItem, CommandPrefix + Id.ExecutedReadItem);

    /// <summary>
    ///     TransactionalBatch was executed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CosmosTransactionalBatchExecutedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ExecutedTransactionalBatch
        = new((int)Id.ExecutedTransactionalBatch, CommandPrefix + Id.ExecutedTransactionalBatch);

    /// <summary>
    ///     CreateItem was executed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CosmosItemCommandExecutedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ExecutedCreateItem
        = new((int)Id.ExecutedCreateItem, CommandPrefix + Id.ExecutedCreateItem);

    /// <summary>
    ///     ReplaceItem was executed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CosmosItemCommandExecutedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ExecutedReplaceItem
        = new((int)Id.ExecutedReplaceItem, CommandPrefix + Id.ExecutedReplaceItem);

    /// <summary>
    ///     DeleteItem was executed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CosmosItemCommandExecutedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ExecutedDeleteItem
        = new((int)Id.ExecutedDeleteItem, CommandPrefix + Id.ExecutedDeleteItem);

    private static EventId MakeValidationId(Id id)
        => new((int)id, DbLoggerCategory.Model.Validation.Name + "." + id);

    /// <summary>
    ///     No partition key has been configured for an entity type. It is highly recommended that an appropriate partition key be defined.
    ///     See https://aka.ms/efdocs-cosmos-partition-keys for more information.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="EntityTypeEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId NoPartitionKeyDefined = MakeValidationId(Id.NoPartitionKeyDefined);

    private static EventId MakeUpdateId(Id id)
        => new((int)id, DbLoggerCategory.Update.Name + "." + id);

    /// <summary>
    ///     A property is not configured to generate values and has the CLR default or sentinel value while saving a new entity
    ///     to the database. The Azure Cosmos DB database provider for EF Core does not generate key values by default. This means key
    ///     values must be explicitly set before saving new entities. See https://aka.ms/ef-cosmos-keys for more information.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId PrimaryKeyValueNotSet = MakeUpdateId(Id.PrimaryKeyValueNotSet);

    /// <summary>
    ///     SaveChanges was invoked with both bulk execution and batching being enabled. Transactional batches can not be run in bulk thus they
    ///     will skip bulk execution. Use AutoTransactionBehavior.Never to leverage bulk execution. If batching was intended, suppress this warning
    ///     using <c>DbContextOptionsBuilder.ConfigureWarnings(w => w.Ignore(CosmosEventId.BulkExecutionWithTransactionalBatch))</c>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.AutoTransactionBehaviorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId BulkExecutionWithTransactionalBatch = MakeUpdateId(Id.BulkExecutionWithTransactionalBatch);
}
