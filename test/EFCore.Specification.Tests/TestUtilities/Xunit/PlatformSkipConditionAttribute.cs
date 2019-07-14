// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public sealed class PlatformSkipConditionAttribute : Attribute, ITestCondition
    {
        private readonly TestPlatform _excludedPlatforms;

        public PlatformSkipConditionAttribute(TestPlatform excludedPlatforms)
        {
            _excludedPlatforms = excludedPlatforms;
        }

        public ValueTask<bool> IsMetAsync() => new ValueTask<bool>(CanRunOnThisPlatform(_excludedPlatforms));

        public string SkipReason { get; set; } = "Test cannot run on this platform.";

        private static bool CanRunOnThisPlatform(TestPlatform excludedFrameworks)
        {
            if (excludedFrameworks == TestPlatform.None)
            {
                return true;
            }

            if (excludedFrameworks.HasFlag(TestPlatform.Windows)
                && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return false;
            }

            if (excludedFrameworks.HasFlag(TestPlatform.Linux)
                && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return false;
            }

            return !excludedFrameworks.HasFlag(TestPlatform.Mac)
                   || !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }
    }
}
