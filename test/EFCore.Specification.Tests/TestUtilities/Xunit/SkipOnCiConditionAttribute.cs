// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
public sealed class SkipOnCiConditionAttribute : Attribute, ITestCondition
{
    public ValueTask<bool> IsMetAsync()
        => new(
            Environment.GetEnvironmentVariable("PIPELINE_WORKSPACE") == null
            && Environment.GetEnvironmentVariable("GITHUB_RUN_ID") == null);

    public string SkipReason { get; set; } = "Tests not reliable on C.I.";
}
