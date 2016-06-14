// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Values that are used as the eventId when logging messages from the in-memory database provider via <see cref="ILogger" />.
    /// </summary>
    public enum InMemoryEventId
    {
        /// <summary>
        ///     Changes were saved to the database.
        /// </summary>
        SavedChanges = 1,

        /// <summary>
        ///     A transaction operation was requested, but ignored because in-memory does not support transactions.
        /// </summary>
        TransactionIgnoredWarning
    }
}
