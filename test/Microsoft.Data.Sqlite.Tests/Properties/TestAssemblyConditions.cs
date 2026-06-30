// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

// Skip the entire assembly if the SQLite native library could not be initialized
[assembly: ConditionalAssembly(typeof(SqliteTestEnvironment), nameof(SqliteTestEnvironment.IsAvailable))]
