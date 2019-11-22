// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.Data.SqlClient;
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
            .GetSection("Test:SqlServer");

        public static string DefaultConnection { get; } = Config["DefaultConnection"]
            ?? "Data Source=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;Connect Timeout=60;ConnectRetryCount=0";

        private static readonly string _dataSource = new SqlConnectionStringBuilder(DefaultConnection).DataSource;

        public static bool IsConfigured { get; } = !string.IsNullOrEmpty(_dataSource);

        public static bool IsLocalDb { get; } = _dataSource.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase);

        public static bool IsSqlAzure { get; } = _dataSource.Contains("database.windows.net");

        public static bool IsCI { get; } = Environment.GetEnvironmentVariable("PIPELINE_WORKSPACE") != null
            || Environment.GetEnvironmentVariable("TEAMCITY_VERSION") != null;

        private static bool? _fullTextInstalled;

        public static bool IsFullTestSearchSupported
        {
            get
            {
                if (!IsConfigured)
                {
                    return false;
                }

                if (_fullTextInstalled.HasValue)
                {
                    return _fullTextInstalled.Value;
                }

                using (var sqlConnection = new SqlConnection(SqlServerTestStore.CreateConnectionString("master")))
                {
                    sqlConnection.Open();

                    using (var command = new SqlCommand(
                        "SELECT FULLTEXTSERVICEPROPERTY('IsFullTextInstalled')", sqlConnection))
                    {
                        var result = (int)command.ExecuteScalar();

                        _fullTextInstalled = result == 1;

                        if (_fullTextInstalled.Value)
                        {
                            var flag = GetFlag("SupportsFullTextSearch");

                            if (flag.HasValue)
                            {
                                return flag.Value;
                            }
                        }
                    }
                }

                _fullTextInstalled = false;
                return false;
            }
        }

        public static string ElasticPoolName { get; } = Config["ElasticPoolName"];

        public static bool? GetFlag(string key)
            => bool.TryParse(Config[key], out var flag) ? flag : (bool?)null;

        public static int? GetInt(string key)
            => int.TryParse(Config[key], out var value) ? value : (int?)null;
    }
}
