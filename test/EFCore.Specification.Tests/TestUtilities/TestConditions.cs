// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

/// <summary>
///     Static helpers consumed by Arcade's <see cref="Xunit.ConditionalFactAttribute" /> /
///     <see cref="Xunit.ConditionalTheoryAttribute" /> via <c>typeof(TestConditions)</c> +
///     <c>nameof(...)</c>. Each member returns a <see langword="bool" />; the conditional
///     attributes skip the test when any referenced member evaluates to <see langword="false" />.
/// </summary>
public static class TestConditions
{
    public static bool NotOnHelix
        => Environment.GetEnvironmentVariable("HELIX_WORKITEM_ROOT") is null;

    public static bool NotOnCI
        => Environment.GetEnvironmentVariable("PIPELINE_WORKSPACE") is null
            && Environment.GetEnvironmentVariable("GITHUB_RUN_ID") is null;

    public static bool NotOnMac
        => !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool NotOnLinux
        => !RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static bool NotOnWindows
        => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool NotOnLinuxOrMac
        => NotOnLinux && NotOnMac;
}
