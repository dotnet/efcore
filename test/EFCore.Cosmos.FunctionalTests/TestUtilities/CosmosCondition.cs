// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

[Flags]
public enum CosmosCondition
{
    UsesTokenCredential = 1 << 0,
    DoesNotUseTokenCredential = 1 << 1,
    IsEmulator = 1 << 2,
    IsNotEmulator = 1 << 3,
}
