// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit
{
    [Flags]
    public enum TestPlatform
    {
        None = 0,
        Windows = 1 << 0,
        Linux = 1 << 1,
        Mac = 1 << 2
    }
}
