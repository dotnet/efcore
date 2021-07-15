// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

// Skip the entire assembly if not on Windows and no external SQL Server is configured
[assembly: SqlServerConfiguredCondition]

[assembly: LoggingIsBrokenCondition]

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class LoggingIsBrokenConditionAttribute : Attribute, ITestCondition
{
    public ValueTask<bool> IsMetAsync()
        => ValueTask.FromResult(true);

    public string SkipReason
        => "Missing method exceptions in Logging";
}
