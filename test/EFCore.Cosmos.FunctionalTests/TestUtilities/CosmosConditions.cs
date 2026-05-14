// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

/// <summary>
///     Static helpers consumed by Arcade's <see cref="Xunit.ConditionalFactAttribute" /> /
///     <see cref="Xunit.ConditionalTheoryAttribute" /> via <c>typeof(CosmosConditions)</c> +
///     <c>nameof(...)</c> to skip Cosmos-specific tests when the target account / emulator
///     does not match. Universal helpers are re-exported so that a single
///     <c>typeof(CosmosConditions)</c> can cover combined skips.
/// </summary>
public static class CosmosConditions
{
    public static bool UsesTokenCredential
        => TestEnvironment.UseTokenCredential;

    public static bool DoesNotUseTokenCredential
        => !TestEnvironment.UseTokenCredential;

    public static bool IsEmulator
        => TestEnvironment.IsEmulator;

    public static bool IsNotEmulator
        => !TestEnvironment.IsEmulator;

    public static bool IsNotLinuxEmulator
        => !TestEnvironment.IsLinuxEmulator;

    // Re-exported universal helpers so that combined skips can use a single calleeType.
    public static bool NotOnHelix
        => TestConditions.NotOnHelix;

    public static bool NotOnCI
        => TestConditions.NotOnCI;

    public static bool NotOnMac
        => TestConditions.NotOnMac;

    public static bool NotOnLinux
        => TestConditions.NotOnLinux;

    public static bool NotOnWindows
        => TestConditions.NotOnWindows;

    public static bool NotOnLinuxOrMac
        => TestConditions.NotOnLinuxOrMac;
}
