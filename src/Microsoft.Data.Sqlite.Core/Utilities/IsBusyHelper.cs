// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite.Utilities;

internal static class IsBusyHelper
{
    public static bool IsBusy(int rc)
        => rc is SQLITE_LOCKED or SQLITE_BUSY or SQLITE_LOCKED_SHAREDCACHE;
}
