// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
public sealed class PlatformSkipConditionAttribute(TestPlatform excludedPlatforms) : Attribute, ITestCondition
{
    private readonly TestPlatform _excludedPlatforms = excludedPlatforms;

    public ValueTask<bool> IsMetAsync()
        => new(CanRunOnThisPlatform(_excludedPlatforms));

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
