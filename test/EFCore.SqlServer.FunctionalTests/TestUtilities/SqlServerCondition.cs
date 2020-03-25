// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    [Flags]
    public enum SqlServerCondition
    {
        SupportsSequences = 1 << 0,
        IsSqlAzure = 1 << 1,
        IsNotSqlAzure = 1 << 2,
        SupportsMemoryOptimized = 1 << 3,
        SupportsAttach = 1 << 4,
        SupportsHiddenColumns = 1 << 5,
        IsNotCI = 1 << 6,
        SupportsFullTextSearch = 1 << 7,
        SupportsOnlineIndexes = 1 << 8
    }
}
