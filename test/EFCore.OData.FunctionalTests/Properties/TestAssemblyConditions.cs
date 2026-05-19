// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

// Skip the entire assembly if SQL Server is not available
[assembly: ConditionalAssembly(typeof(SqlServerTestEnvironment), nameof(SqlServerTestEnvironment.SqlServerAvailable))]
