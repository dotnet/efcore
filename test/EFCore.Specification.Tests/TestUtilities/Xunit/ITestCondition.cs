// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit
{
    public interface ITestCondition
    {
        ValueTask<bool> IsMetAsync();

        string SkipReason { get; }
    }
}
