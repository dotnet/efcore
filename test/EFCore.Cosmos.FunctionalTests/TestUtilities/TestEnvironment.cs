// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public static class TestEnvironment
    {
        public static IConfiguration Config { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", optional: true)
            .AddJsonFile("config.test.json", optional: true)
            .AddEnvironmentVariables()
            .Build()
            .GetSection("Test:Cosmos");

        public static string DefaultConnection { get; } = string.IsNullOrEmpty(Config["DefaultConnection"])
            ? "https://localhost:8081"
            : Config["DefaultConnection"];

        public static string AuthToken { get; } = string.IsNullOrEmpty(Config["AuthToken"])
            ? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
            : Config["AuthToken"];

        public static string ConnectionString { get; } = $"AccountEndpoint={DefaultConnection};AccountKey={AuthToken}";

        public static bool IsEmulator { get; } = DefaultConnection.StartsWith("https://localhost:8081", StringComparison.Ordinal);
    }
}
