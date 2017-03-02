// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Values that are used as the eventId when logging messages from the SQLite provider via <see cref="ILogger" />.
    /// </summary>
    public enum SqliteEventId
    {
        /// <summary>
        ///     A schema was configured for an entity type
        /// </summary>
        SchemaConfiguredWarning = 1,

        /// <summary>
        ///     A sequence was configured
        /// </summary>
        SequenceWarning = 2
    }
}
