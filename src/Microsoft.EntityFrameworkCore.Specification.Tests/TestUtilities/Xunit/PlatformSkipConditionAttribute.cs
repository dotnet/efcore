// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public class PlatformSkipConditionAttribute : Attribute, ITestCondition
    {
        private readonly TestPlatform _excludedPlatforms;

        public PlatformSkipConditionAttribute(TestPlatform excludedPlatforms)
        {
            _excludedPlatforms = excludedPlatforms;
        }

        public bool IsMet => CanRunOnThisPlatform(_excludedPlatforms);

        public string SkipReason { get; set; } = "Test cannot run on this platform.";

        private static bool CanRunOnThisPlatform(TestPlatform excludedFrameworks)
        {
            if (excludedFrameworks == TestPlatform.None)
            {
                return true;
            }

            if (excludedFrameworks.HasFlag(TestPlatform.Windows)
                && TestPlatformHelper.IsWindows)
            {
                return false;
            }

            if (excludedFrameworks.HasFlag(TestPlatform.Linux)
                && TestPlatformHelper.IsLinux)
            {
                return false;
            }

            if (excludedFrameworks.HasFlag(TestPlatform.Mac)
                && TestPlatformHelper.IsMac)
            {
                return false;
            }

            return true;
        }
    }
}
