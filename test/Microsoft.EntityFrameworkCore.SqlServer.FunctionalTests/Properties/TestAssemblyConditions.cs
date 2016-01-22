// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestUtilities.Xunit;
using Xunit;

[assembly: TestFramework("ConditionalTestFramework", "Microsoft.EntityFrameworkCore.FunctionalTests")]

// Skip the entire assembly if not on Windows and no external SQL Server is configured
[assembly: SqlServerConfiguredCondition]

[assembly: FrameworkSkipCondition(RuntimeFrameworks.Mono, SkipReason = "SqlClient on Mono is partially implemented. SQL Server functional tests are ineffective on Mono")]
