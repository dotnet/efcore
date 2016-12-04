// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Values that are used as the eventId when logging messages from the SQLite Design Entity Framework Core
    ///     components.
    /// </summary>
    public enum SqliteDesignEventId
    {
        /// <summary>
        ///     Column name empty on index.
        /// </summary>
        IndexMissingColumnNameWarning = 1,

        /// <summary>
        ///     Principal column not found.
        /// </summary>
        ForeignKeyReferencesMissingColumn,

        /// <summary>
        ///     Using schema selections warning.
        /// </summary>
        SchemasNotSupportedWarning
    }
}
