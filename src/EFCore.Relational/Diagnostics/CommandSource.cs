// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     Enum used by <see cref="CommandEventData" />, and subclasses to indicate the
    ///     source of the <see cref="DbCommand" /> being used to execute the command.
    /// </summary>
    public enum CommandSource
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Linq Query
        /// </summary>
        LinqQuery,

        /// <summary>
        /// Save Changes
        /// </summary>
        SaveChanges,

        /// <summary>
        /// Migrations
        /// </summary>
        Migrations,

        /// <summary>
        /// FromSqlQuery
        /// </summary>
        FromSqlQuery,

        /// <summary>
        /// ExecuteSqlRaw
        /// </summary>
        ExecuteSqlRaw,

        /// <summary>
        /// ValueGenerator
        /// </summary>
        ValueGenerator,

        /// <summary>
        /// Scaffolding
        /// </summary>
        Scaffolding,

        /// <summary>
        /// BulkUpdate
        /// </summary>
        BulkUpdate
    }
}
