// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Values that are used as the eventId when logging messages from the SQL Server provider via <see cref="ILogger" />.
    /// </summary>
    public enum SqlServerEventId
    {
        /// <summary>
        ///     No explicit type for a decimal column
        /// </summary>
        DefaultDecimalTypeWarning = 1
    }
}
