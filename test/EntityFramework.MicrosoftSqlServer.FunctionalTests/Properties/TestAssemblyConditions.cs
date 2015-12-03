// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.SqlServer.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestUtilities.Xunit;
using Xunit;

[assembly: TestFramework("ConditionalTestFramework", "EntityFramework.Core.FunctionalTests")]

// Skip the entire assembly if not on Windows and no external SQL Server is configured
[assembly: SqlServerConfiguredCondition]

[assembly: FrameworkSkipCondition(RuntimeFrameworks.Mono, SkipReason = "SqlClient on Mono is partially implemented. SQL Server functional tests are ineffective on Mono")]
