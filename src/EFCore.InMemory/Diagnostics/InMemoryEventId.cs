// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Event IDs for in-memory events that correspond to messages logged to an <see cref="ILogger" />
    ///         and events sent to a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
    ///         behavior of warnings.
    ///     </para>
    /// </summary>
    public static class InMemoryEventId
    {
        // Warning: These values must not change between releases.
        // Only add new values to the end of sections, never in the middle.
        // Try to use <Noun><Verb> naming and be consistent with existing names.
        private enum Id
        {
            // Transaction events
            TransactionIgnoredWarning = CoreEventId.ProviderBaseId,

            // Update events
            ChangesSaved = CoreEventId.ProviderBaseId + 100
        }

        private static readonly string _transactionPrefix = DbLoggerCategory.Database.Transaction.Name + ".";
        private static EventId MakeTransactionId(Id id) => EventIdFactory.Create((int)id, _transactionPrefix + id);

        /// <summary>
        ///     <para>
        ///         Changes were saved to the database.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="EventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId TransactionIgnoredWarning = MakeTransactionId(Id.TransactionIgnoredWarning);

        private static readonly string _updatePrefix = DbLoggerCategory.Update.Name + ".";
        private static EventId MakeUpdateId(Id id) => EventIdFactory.Create((int)id, _updatePrefix + id);

        /// <summary>
        ///     <para>
        ///         A transaction operation was requested, but ignored because in-memory does not support transactions.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="SaveChangesEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ChangesSaved = MakeUpdateId(Id.ChangesSaved);

        private static class EventIdFactory
        {
            public static EventId Create(int id, string name)
            {
                if (AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Diagnostics.UseLegacyEventIds", out var isEnabled)
                    && isEnabled)
                {
                    if (id >= CoreEventId.ProviderDesignBaseId)
                    {
                        id = MassageId(id, CoreEventId.ProviderDesignBaseId);
                    }
                    else if (id >= CoreEventId.ProviderBaseId)
                    {
                        id = MassageId(id, CoreEventId.ProviderBaseId);
                    }
                    else if (id >= CoreEventId.RelationalBaseId)
                    {
                        id = MassageId(id, CoreEventId.RelationalBaseId);
                    }
                    else if (id >= CoreEventId.CoreBaseId)
                    {
                        id = MassageId(id, CoreEventId.CoreBaseId);
                    }
                }

                return new EventId(id, name);
            }

            private static int MassageId(int id, int baseId) => (id - baseId) + (baseId * 10);
        }
    }
}
