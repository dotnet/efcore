// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

[Flags]
public enum TestPlatform
{
    None = 0,
    Windows = 1 << 0,
    Linux = 1 << 1,
    Mac = 1 << 2
}
