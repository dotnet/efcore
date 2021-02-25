// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

#nullable enable

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
            // These events are actually in Event in `DbLoggerCategory.Database.Command`.
            // Leaving the ID unchanged to avoid changing it after release.
            ExecutingSqlQuery = CoreEventId.ProviderBaseId + 100,
            ExecutingReadItem
        }

        private static readonly string _commandPrefix = DbLoggerCategory.Database.Command.Name + ".";

        /// <summary>
        ///     <para>
        ///         A SQL query was executed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
        ///     </para>
        /// </summary>
        public static readonly EventId ExecutingSqlQuery
            = new((int)Id.ExecutingSqlQuery, _commandPrefix + Id.ExecutingSqlQuery);

        /// <summary>
        ///     <para>
        ///         ReadItem was executed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
        ///     </para>
        /// </summary>
        public static readonly EventId ExecutingReadItem
                = new((int)Id.ExecutingReadItem, _commandPrefix + Id.ExecutingReadItem);
    }
}
