// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class TestEnvironment
{
    public static IConfiguration Config { get; } = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("config.json", optional: true)
        .AddJsonFile("config.test.json", optional: true)
        .AddEnvironmentVariables()
        .Build()
        .GetSection("Test:SqlServer");

    public static string DefaultConnection { get; } = Config["DefaultConnection"]
        ?? "Data Source=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;Connect Timeout=60;ConnectRetryCount=0";
}
