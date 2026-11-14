// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using SQLitePCL;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite.Utilities;

internal static class IsBusyHelper
{
    public static bool IsBusy(int rc)
        => rc is SQLITE_LOCKED or SQLITE_BUSY or SQLITE_LOCKED_SHAREDCACHE;

    // SQLITE_BUSY_SNAPSHOT means that a connection holding a read transaction tried to write after another connection
    // committed. The read snapshot stays stale until the reading transaction ends, so retrying cannot succeed.
    // Use this overload when stepping a statement, which is where a write can be refused because of a stale snapshot.
    // Preparing a statement doesn't start a write transaction, so the retry loops for preparing and for connection
    // setup don't need it.
    public static bool IsBusy(int rc, sqlite3? db)
        => IsBusy(rc)
            && !(rc == SQLITE_BUSY
                && db is { IsInvalid: false }
                && sqlite3_extended_errcode(db) == SQLITE_BUSY_SNAPSHOT);
}
