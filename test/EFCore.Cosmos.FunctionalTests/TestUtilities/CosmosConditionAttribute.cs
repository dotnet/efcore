// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class CosmosConditionAttribute(CosmosCondition conditions) : Attribute, ITestCondition
{
    public CosmosCondition Conditions { get; set; } = conditions;

    public ValueTask<bool> IsMetAsync()
    {
        var isMet = true;

        if (Conditions.HasFlag(CosmosCondition.UsesTokenCredential))
        {
            isMet &= TestEnvironment.UseTokenCredential;
        }

        if (Conditions.HasFlag(CosmosCondition.DoesNotUseTokenCredential))
        {
            isMet &= !TestEnvironment.UseTokenCredential;
        }

        return ValueTask.FromResult(isMet);
    }

    public string SkipReason
        => string.Format(
                "The test Cosmos account does not meet these conditions: '{0}'",
                string.Join(
                    ", ", Enum.GetValues(typeof(CosmosCondition))
                        .Cast<Enum>()
                        .Where(Conditions.HasFlag)
                        .Select(f => Enum.GetName(typeof(CosmosCondition), f))));
}
