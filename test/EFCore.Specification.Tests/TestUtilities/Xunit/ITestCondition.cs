// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

public interface ITestCondition
{
    ValueTask<bool> IsMetAsync(XunitTestCase testCase);

    string SkipReason { get; }
}
