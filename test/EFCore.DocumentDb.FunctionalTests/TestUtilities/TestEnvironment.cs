// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public static class TestEnvironment
    {
        private const string _defaultServiceEndPointString = "https://localhost:8081";
        private const string _defaultAuthKey
            = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        public static IConfiguration Config { get; }

        static TestEnvironment()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: true)
                .AddEnvironmentVariables();

            Config = configBuilder.Build().GetSection("Test:DocumentDb");
        }

        public static Uri DefaultServiceEndPoint
            => new Uri(Config["DefaultServiceEndPoint"] ?? _defaultServiceEndPointString);

        public static string DefaultAuthKey => Config["DefaultAuthKey"] ?? _defaultAuthKey;

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
