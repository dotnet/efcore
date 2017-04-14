// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
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

        private static readonly string _transactionPrefix = LoggerCategory.Database.Transaction.Name + ".";
        private static EventId MakeTransactionId(Id id) => new EventId((int)id, _transactionPrefix + id);

        /// <summary>
        ///     Changes were saved to the database.
        ///     This event is in the <see cref="LoggerCategory.Database.Transaction" /> category.
        /// </summary>
        public static readonly EventId TransactionIgnoredWarning = MakeTransactionId(Id.TransactionIgnoredWarning);

        private static readonly string _updatePrefix = LoggerCategory.Update.Name + ".";
        private static EventId MakeUpdateId(Id id) => new EventId((int)id, _updatePrefix + id);

        /// <summary>
        ///     A transaction operation was requested, but ignored because in-memory does not support transactions.
        ///     This event is in the <see cref="LoggerCategory.Update" /> category.
        /// </summary>
        public static readonly EventId ChangesSaved = MakeUpdateId(Id.ChangesSaved);
    }
}
