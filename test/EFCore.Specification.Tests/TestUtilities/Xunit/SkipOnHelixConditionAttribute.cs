// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
public sealed class SkipOnHelixConditionAttribute : Attribute, ITestCondition
{
    public ValueTask<bool> IsMetAsync()
        => new(Environment.GetEnvironmentVariable("HELIX_WORKITEM_ROOT") is null);

    public string SkipReason { get; set; } = "Test does not run on Helix.";
}
