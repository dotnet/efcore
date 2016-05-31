// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.PlatformAbstractions;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities
{
    public static class TestPlatformHelper
    {
        public static bool IsMono => PlatformServices.Default.Runtime.RuntimeType.Equals("Mono", StringComparison.OrdinalIgnoreCase);
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static bool IsMac => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }
}
