// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    ///     Represents the caching modes that can be used when creating a new <see cref="SqliteConnection" />.
    /// </summary>
    public enum SqliteCacheMode
    {
        /// <summary>
        ///     Default mode.
        /// </summary>
        Default,

        /// <summary>
        ///     Private-cache mode. Each connection uses a private cache.
        /// </summary>
        Private,

        /// <summary>
        ///     Shared-cache mode. Connections share a cache. This mode can change the behavior of transaction and table
        ///     locking.
        /// </summary>
        /// <seealso href="https://www.sqlite.org/sharedcache.html">SQLite Shared-Cache Mode</seealso>
        Shared
    }
}
