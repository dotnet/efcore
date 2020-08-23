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

        public static bool IsCI { get; } = Environment.GetEnvironmentVariable("PIPELINE_WORKSPACE") != null;

        private static bool? _isAzureSqlDb;

        private static bool? _fullTextInstalled;

        private static bool? _supportsHiddenColumns;

        private static bool? _supportsOnlineIndexing;

        private static bool? _supportsMemoryOptimizedTables;

        private static byte? _productMajorVersion;

        private static int? _engineEdition;

        public static bool IsSqlAzure
        {
            get
            {
                if (!IsConfigured)
                {
                    return false;
                }

                if (_isAzureSqlDb.HasValue)
                {
                    return _isAzureSqlDb.Value;
                }

                try
                {
                    _engineEdition = GetEngineEdition();

                    _isAzureSqlDb = (_engineEdition == 5 || _engineEdition == 8);
                }
                catch (PlatformNotSupportedException)
                {
                    _isAzureSqlDb = false;
                }

                return _isAzureSqlDb.Value;
            }
        }

        public static bool IsLocalDb { get; } = _dataSource.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase);

        public static bool IsFullTextSearchSupported
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

                try
                {
                    using var sqlConnection = new SqlConnection(SqlServerTestStore.CreateConnectionString("master"));
                    sqlConnection.Open();

                    using var command = new SqlCommand(
                        "SELECT FULLTEXTSERVICEPROPERTY('IsFullTextInstalled')", sqlConnection);
                    var result = (int)command.ExecuteScalar();

                    _fullTextInstalled = result == 1;
                }
                catch (PlatformNotSupportedException)
                {
                    _fullTextInstalled = false;
                }

                return _fullTextInstalled.Value;
            }
        }

        public static bool IsHiddenColumnsSupported
        {
            get
            {
                if (!IsConfigured)
                {
                    return false;
                }

                if (_supportsHiddenColumns.HasValue)
                {
                    return _supportsHiddenColumns.Value;
                }

                try
                {
                    _engineEdition = GetEngineEdition();
                    _productMajorVersion = GetProductMajorVersion();

                    _supportsHiddenColumns = (_productMajorVersion >= 13 && _engineEdition != 6) || IsSqlAzure;
                }
                catch (PlatformNotSupportedException)
                {
                    _supportsHiddenColumns = false;
                }

                return _supportsHiddenColumns.Value;
            }
        }

        public static bool IsOnlineIndexingSupported
        {
            get
            {
                if (!IsConfigured)
                {
                    return false;
                }

                if (_supportsOnlineIndexing.HasValue)
                {
                    return _supportsOnlineIndexing.Value;
                }

                try
                {
                    _engineEdition = GetEngineEdition();

                    _supportsOnlineIndexing = _engineEdition == 3 || IsSqlAzure;
                }
                catch (PlatformNotSupportedException)
                {
                    _supportsOnlineIndexing = false;
                }

                return _supportsOnlineIndexing.Value;
            }
        }

        public static bool IsMemoryOptimizedTablesSupported
        {
            get
            {
                if (!IsConfigured)
                {
                    return false;
                }

                var supported = GetFlag(nameof(SqlServerCondition.SupportsMemoryOptimized));
                if (supported.HasValue)
                {
                    _supportsMemoryOptimizedTables = supported.Value;
                }

                if (_supportsMemoryOptimizedTables.HasValue)
                {
                    return _supportsMemoryOptimizedTables.Value;
                }

                try
                {
                    using var sqlConnection = new SqlConnection(SqlServerTestStore.CreateConnectionString("master"));
                    sqlConnection.Open();

                    using var command = new SqlCommand(
                        "SELECT SERVERPROPERTY('IsXTPSupported');", sqlConnection);
                    var result = command.ExecuteScalar();
                    _supportsMemoryOptimizedTables = (result != null ? Convert.ToInt32(result) : 0) == 1 && !IsSqlAzure && !IsLocalDb;
                }
                catch (PlatformNotSupportedException)
                {
                    _supportsMemoryOptimizedTables = false;
                }

                return _supportsMemoryOptimizedTables.Value;
            }
        }

        public static string ElasticPoolName { get; } = Config["ElasticPoolName"];

        public static bool? GetFlag(string key)
            => bool.TryParse(Config[key], out var flag) ? flag : (bool?)null;

        public static int? GetInt(string key)
            => int.TryParse(Config[key], out var value) ? value : (int?)null;

        private static int GetEngineEdition()
        {
            if (_engineEdition.HasValue)
            {
                return _engineEdition.Value;
            }

            using var sqlConnection = new SqlConnection(SqlServerTestStore.CreateConnectionString("master"));
            sqlConnection.Open();

            using var command = new SqlCommand(
                "SELECT SERVERPROPERTY('EngineEdition');", sqlConnection);
            _engineEdition = (int)command.ExecuteScalar();

            return _engineEdition.Value;
        }

        private static byte GetProductMajorVersion()
        {
            if (_productMajorVersion.HasValue)
            {
                return _productMajorVersion.Value;
            }

            using var sqlConnection = new SqlConnection(SqlServerTestStore.CreateConnectionString("master"));
            sqlConnection.Open();

            using var command = new SqlCommand(
                "SELECT SERVERPROPERTY('ProductVersion');", sqlConnection);
            _productMajorVersion = (byte)Version.Parse((string)command.ExecuteScalar()).Major;

            return _productMajorVersion.Value;
        }
    }
}
