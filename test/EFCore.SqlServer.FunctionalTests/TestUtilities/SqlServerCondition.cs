// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    [Flags]
    public enum SqlServerCondition
    {
        SupportsSequences = 1 << 0,
        SupportsOffset = 1 << 1,
        IsSqlAzure = 1 << 2,
        IsNotSqlAzure = 1 << 3,
        SupportsMemoryOptimized = 1 << 4,
        SupportsAttach = 1 << 5,
        SupportsHiddenColumns = 1 << 6,
        IsNotCI = 1 << 7,
        SupportsFullTextSearch = 1 << 8
    }
}
