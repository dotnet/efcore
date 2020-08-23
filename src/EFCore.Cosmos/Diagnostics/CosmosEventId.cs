// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Event IDs for Cosmos events that correspond to messages logged to an <see cref="ILogger" />
    ///         and events sent to a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
    ///         behavior of warnings.
    ///     </para>
    /// </summary>
    public static class CosmosEventId
    {
        // Warning: These values must not change between releases.
        // Only add new values to the end of sections, never in the middle.
        // Try to use <Noun><Verb> naming and be consistent with existing names.
        private enum Id
        {
            // Update events

            // Query events
            ExecutingSqlQuery = CoreEventId.ProviderBaseId + 100,
            ExecutingReadItem
        }

        private static readonly string _queryPrefix = DbLoggerCategory.Query.Name + ".";

        private static EventId MakeQueryId(Id id)
            => new EventId((int)id, _queryPrefix + id);

        /// <summary>
        ///     <para>
        ///         A SQL query was executed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        /// </summary>
        public static readonly EventId ExecutingSqlQuery = MakeQueryId(Id.ExecutingSqlQuery);

        /// <summary>
        ///     <para>
        ///         ReadItem was executed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        /// </summary>
        public static readonly EventId ExecutingReadItem = MakeQueryId(Id.ExecutingReadItem);
    }
}
