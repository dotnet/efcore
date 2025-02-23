// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

public class SkipSyncTestsAttribute : Attribute, ITestCondition
{
    public ValueTask<bool> IsMetAsync(XunitTestCase testCase)
    {
        if (testCase is not
            {
                Method: IMethodInfo method,
                TestMethodArguments: object[] arguments
            }
            || method.GetParameters()?.ToList() is not { Count: > 0 } parameters)
        {
            return new(true);
        }

        var asyncParameterIndex = parameters.FindIndex(
            p => p is { Name: "async", ParameterType: var parameterType }
                && parameterType.ToRuntimeType() == typeof(bool));

        return (asyncParameterIndex > -1
            && arguments.Length >= asyncParameterIndex + 1
            && arguments[asyncParameterIndex] is false)
            ? new(false)
            : new(true);
    }

    public string SkipReason
        => "Synchronous I/O test skipped.";
}
