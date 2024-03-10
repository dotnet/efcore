// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

    private static readonly string _dataSource = new SqlConnectionStringBuilder(DefaultConnection).DataSource;

    public static bool IsConfigured { get; } = !string.IsNullOrEmpty(_dataSource);

    public static bool IsCI { get; } = Environment.GetEnvironmentVariable("PIPELINE_WORKSPACE") != null;

    private static bool? _isAzureSqlDb;

    private static bool? _fullTextInstalled;

    private static bool? _supportsHiddenColumns;

    private static bool? _supportsSqlClr;

    private static bool? _supportsOnlineIndexing;

    private static bool? _supportsMemoryOptimizedTables;

    private static bool? _supportsTemporalTablesCascadeDelete;

    private static bool? _supportsUtf8;

    private static bool? _supportsJsonPathExpressions;

    private static bool? _supportsFunctions2017;

    private static bool? _supportsFunctions2019;

    private static bool? _supportsFunctions2022;

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
                _isAzureSqlDb = GetEngineEdition() is 5 or 8;
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
                _supportsHiddenColumns = (GetProductMajorVersion() >= 13 && GetEngineEdition() != 6) || IsSqlAzure;
            }
            catch (PlatformNotSupportedException)
            {
                _supportsHiddenColumns = false;
            }

            return _supportsHiddenColumns.Value;
        }
    }

    public static bool IsSqlClrSupported
    {
        get
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (_supportsSqlClr.HasValue)
            {
                return _supportsSqlClr.Value;
            }

            try
            {
                _supportsSqlClr = GetEngineEdition() != 9;
            }
            catch (PlatformNotSupportedException)
            {
                _supportsSqlClr = false;
            }

            return _supportsSqlClr.Value;
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
                _supportsOnlineIndexing = GetEngineEdition() == 3 || IsSqlAzure;
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

    public static bool IsTemporalTablesCascadeDeleteSupported
    {
        get
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (_supportsTemporalTablesCascadeDelete.HasValue)
            {
                return _supportsTemporalTablesCascadeDelete.Value;
            }

            try
            {
                _supportsTemporalTablesCascadeDelete = (GetProductMajorVersion() >= 14 /* && GetEngineEdition() != 6*/) || IsSqlAzure;
            }
            catch (PlatformNotSupportedException)
            {
                _supportsTemporalTablesCascadeDelete = false;
            }

            return _supportsTemporalTablesCascadeDelete.Value;
        }
    }

    public static bool IsUtf8Supported
    {
        get
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (_supportsUtf8.HasValue)
            {
                return _supportsUtf8.Value;
            }

            try
            {
                _supportsUtf8 = GetProductMajorVersion() >= 15 || IsSqlAzure;
            }
            catch (PlatformNotSupportedException)
            {
                _supportsUtf8 = false;
            }

            return _supportsUtf8.Value;
        }
    }

    public static bool SupportsJsonPathExpressions
    {
        get
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (_supportsJsonPathExpressions.HasValue)
            {
                return _supportsJsonPathExpressions.Value;
            }

            try
            {
                _supportsJsonPathExpressions = GetProductMajorVersion() >= 14 || IsSqlAzure;
            }
            catch (PlatformNotSupportedException)
            {
                _supportsJsonPathExpressions = false;
            }

            return _supportsJsonPathExpressions.Value;
        }
    }

    public static bool IsFunctions2017Supported
    {
        get
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (_supportsFunctions2017.HasValue)
            {
                return _supportsFunctions2017.Value;
            }

            try
            {
                _supportsFunctions2017 = GetProductMajorVersion() >= 14 || IsSqlAzure;
            }
            catch (PlatformNotSupportedException)
            {
                _supportsFunctions2017 = false;
            }

            return _supportsFunctions2017.Value;
        }
    }

    public static bool IsFunctions2019Supported
    {
        get
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (_supportsFunctions2019.HasValue)
            {
                return _supportsFunctions2019.Value;
            }

            try
            {
                _supportsFunctions2019 = GetProductMajorVersion() >= 15 || IsSqlAzure;
            }
            catch (PlatformNotSupportedException)
            {
                _supportsFunctions2019 = false;
            }

            return _supportsFunctions2019.Value;
        }
    }

    public static bool IsFunctions2022Supported
    {
        get
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (_supportsFunctions2022.HasValue)
            {
                return _supportsFunctions2022.Value;
            }

            try
            {
                _supportsFunctions2022 = GetProductMajorVersion() >= 16 || IsSqlAzure;
            }
            catch (PlatformNotSupportedException)
            {
                _supportsFunctions2022 = false;
            }

            return _supportsFunctions2022.Value;
        }
    }

    public static byte SqlServerMajorVersion
        => GetProductMajorVersion();

    public static string? ElasticPoolName { get; } = Config["ElasticPoolName"];

    public static bool? GetFlag(string key)
        => bool.TryParse(Config[key], out var flag) ? flag : null;

    public static int? GetInt(string key)
        => int.TryParse(Config[key], out var value) ? value : null;

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
