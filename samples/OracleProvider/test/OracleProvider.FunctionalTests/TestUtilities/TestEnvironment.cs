// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public static class TestEnvironment
    {
        public static IConfiguration Config { get; }

        static TestEnvironment()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: true)
                .AddJsonFile("config.test.json", optional: true)
                .AddEnvironmentVariables();

            Config = configBuilder.Build()
                .GetSection("Test:Oracle");
        }

        private const string DefaultConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;Connect Timeout=30";

        public static string DefaultConnection => Config["DefaultConnection"] ?? DefaultConnectionString;

        public static bool IsTeamCity => Environment.GetEnvironmentVariable("TEAMCITY_VERSION") != null;

        public static string ElasticPoolName => Config["ElasticPoolName"];

        public static bool? GetFlag(string key)
        {
            return bool.TryParse(Config[key], out var flag) ? flag : (bool?)null;
        }

        public static int? GetInt(string key)
        {
            return int.TryParse(Config[key], out var value) ? value : (int?)null;
        }
    }
}
