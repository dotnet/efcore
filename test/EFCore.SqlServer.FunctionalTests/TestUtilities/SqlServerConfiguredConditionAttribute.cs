// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
public sealed class SqlServerConfiguredConditionAttribute : Attribute, ITestCondition
{
    public ValueTask<bool> IsMetAsync(XunitTestCase testCase)
        => new(TestEnvironment.IsConfigured && (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !TestEnvironment.IsLocalDb));

    public string SkipReason
        => TestEnvironment.IsLocalDb
            ? "LocalDb is not accessible on this platform. An external SQL Server must be configured."
            : "No test SQL Server has been configured.";
}
