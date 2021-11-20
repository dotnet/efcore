// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
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

            // Command events
            ExecutingSqlQuery = CoreEventId.ProviderBaseId + 100,
            ExecutingReadItem,
            ExecutedReadNext,
            ExecutedReadItem,
            ExecutedCreateItem,
            ExecutedReplaceItem,
            ExecutedDeleteItem
        }

        private static readonly string _commandPrefix = DbLoggerCategory.Database.Command.Name + ".";

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
            = new((int)Id.ExecutingSqlQuery, _commandPrefix + Id.ExecutingSqlQuery);

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
            = new((int)Id.ExecutingReadItem, _commandPrefix + Id.ExecutingReadItem);

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
            = new((int)Id.ExecutedReadNext, _commandPrefix + Id.ExecutedReadNext);

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
            = new((int)Id.ExecutedReadItem, _commandPrefix + Id.ExecutedReadItem);

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
            = new((int)Id.ExecutedCreateItem, _commandPrefix + Id.ExecutedCreateItem);

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
            = new((int)Id.ExecutedReplaceItem, _commandPrefix + Id.ExecutedReplaceItem);

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
            = new((int)Id.ExecutedDeleteItem, _commandPrefix + Id.ExecutedDeleteItem);
    }
}
