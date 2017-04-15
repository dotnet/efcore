// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
    public class OsSkipConditionAttribute : Attribute, ITestCondition
    {
        private readonly TestPlatform _excludedOperatingSystem;
        private readonly IEnumerable<string> _excludedVersions;

        public OsSkipConditionAttribute(TestPlatform operatingSystem, params string[] versions)
        {
            _excludedOperatingSystem = operatingSystem;
            _excludedVersions = versions ?? Enumerable.Empty<string>();
        }

        public bool IsMet
        {
            get
            {
                var skip = _excludedOperatingSystem == GetCurrentOs();
                var osVersion = GetCurrentOsVersion();
                if (_excludedVersions.Any())
                {
                    skip = skip
                           && _excludedVersions.Any(ex => osVersion.StartsWith(ex, StringComparison.OrdinalIgnoreCase));
                }

                // Since a test would be excuted only if 'IsMet' is true, return false if we want to skip
                return !skip;
            }
        }

        public string SkipReason { get; set; } = "Test cannot run on this operating system.";

        private static TestPlatform GetCurrentOs()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return TestPlatform.Windows;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return TestPlatform.Linux;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return TestPlatform.Mac;
            }
            throw new PlatformNotSupportedException();
        }

        private static string GetCurrentOsVersion()
        {
            // currently not used on other OS's
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Microsoft.Extensions.Internal.RuntimeEnvironment.OperatingSystemVersion
                : string.Empty;
        }
    }
}
