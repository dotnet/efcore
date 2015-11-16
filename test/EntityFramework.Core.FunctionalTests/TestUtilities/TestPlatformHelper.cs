// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.PlatformAbstractions;

namespace Microsoft.Data.Entity.FunctionalTests.TestUtilities
{
    public static class TestPlatformHelper
    {
        public static bool IsMono => PlatformServices.Default.Runtime.RuntimeType.Equals("Mono", StringComparison.OrdinalIgnoreCase);
        public static bool IsWindows => PlatformServices.Default.Runtime.OperatingSystem.Equals("Windows", StringComparison.OrdinalIgnoreCase);
        public static bool IsLinux => PlatformServices.Default.Runtime.OperatingSystem.Equals("Linux", StringComparison.OrdinalIgnoreCase);
        public static bool IsMac => PlatformServices.Default.Runtime.OperatingSystem.Equals("Darwin", StringComparison.OrdinalIgnoreCase);
    }
}
