// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    [Flags]
    public enum SqlServerCondition
    {
        IsSqlAzure = 1 << 0,
        IsNotSqlAzure = 1 << 1,
        SupportsMemoryOptimized = 1 << 2,
        SupportsAttach = 1 << 3,
        SupportsHiddenColumns = 1 << 4,
        IsNotCI = 1 << 5,
        SupportsFullTextSearch = 1 << 6,
        SupportsOnlineIndexes = 1 << 7,
    }
}
