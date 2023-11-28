// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

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
    SupportsTemporalTablesCascadeDelete = 1 << 8,
    SupportsUtf8 = 1 << 9,
    SupportsJsonPathExpressions = 1 << 10,
    SupportsSqlClr = 1 << 11,
    SupportsFunctions2017 = 1 << 12,
    SupportsFunctions2019 = 1 << 13,
    SupportsFunctions2022 = 1 << 14,
}
