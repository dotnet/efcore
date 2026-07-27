// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class SqlServerTestEnvironment
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

    public static bool IsCI { get; } = Environment.GetEnvironmentVariable("PIPELINE_WORKSPACE") != null
        || Environment.GetEnvironmentVariable("HELIX_CORRELATION_PAYLOAD") != null;

    private static bool? _isAzureSqlDb;

    private static bool? _fullTextInstalled;

    private static bool? _IsHiddenColumnsSupported;

    private static bool? _IsSqlClrSupported;

    private static bool? _supportsOnlineIndexing;

    private static bool? _IsMemoryOptimizedTablesSupportedTables;

    private static bool? _IsTemporalTablesCascadeDeleteSupported;

    private static bool? _IsUtf8Supported;

    private static bool? _supportsJsonPathExpressions;

    private static bool? _isJsonTypeSupported;

    private static bool? _isVectorTypeSupported;

    private static bool? _IsFunctions2017Supported;

    private static bool? _IsFunctions2019Supported;

    private static bool? _IsFunctions2022Supported;

    private static byte? _productMajorVersion;

    private static int? _compatibilityLevel;

    private static int? _engineEdition;

    public static bool IsAzureSql
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

    public static bool SqlServerAvailable { get; } = IsConfigured
        && (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !IsLocalDb);

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

            if (_IsHiddenColumnsSupported.HasValue)
            {
                return _IsHiddenColumnsSupported.Value;
            }

            try
            {
                _IsHiddenColumnsSupported = GetEngineEdition() != 6 && GetCompatibilityLevel() >= 130;
            }
            catch (PlatformNotSupportedException)
            {
                _IsHiddenColumnsSupported = false;
            }

            return _IsHiddenColumnsSupported.Value;
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

            if (_IsSqlClrSupported.HasValue)
            {
                return _IsSqlClrSupported.Value;
            }

            try
            {
                _IsSqlClrSupported = GetEngineEdition() != 9;
            }
            catch (PlatformNotSupportedException)
            {
                _IsSqlClrSupported = false;
            }

            return _IsSqlClrSupported.Value;
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
                _supportsOnlineIndexing = GetEngineEdition() == 3 || IsAzureSql;
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

            var supported = GetFlag(nameof(IsMemoryOptimizedTablesSupported));
            if (supported.HasValue)
            {
                _IsMemoryOptimizedTablesSupportedTables = supported.Value;
            }

            if (_IsMemoryOptimizedTablesSupportedTables.HasValue)
            {
                return _IsMemoryOptimizedTablesSupportedTables.Value;
            }

            try
            {
                using var sqlConnection = new SqlConnection(SqlServerTestStore.CreateConnectionString("master"));
                sqlConnection.Open();

                using var command = new SqlCommand(
                    "SELECT SERVERPROPERTY('IsXTPSupported');", sqlConnection);
                var result = command.ExecuteScalar();
                _IsMemoryOptimizedTablesSupportedTables = (result != null ? Convert.ToInt32(result) : 0) == 1 && !IsLocalDb;
            }
            catch (PlatformNotSupportedException)
            {
                _IsMemoryOptimizedTablesSupportedTables = false;
            }

            return _IsMemoryOptimizedTablesSupportedTables.Value;
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

            if (_IsTemporalTablesCascadeDeleteSupported.HasValue)
            {
                return _IsTemporalTablesCascadeDeleteSupported.Value;
            }

            try
            {
                _IsTemporalTablesCascadeDeleteSupported = GetCompatibilityLevel() >= 140;
            }
            catch (PlatformNotSupportedException)
            {
                _IsTemporalTablesCascadeDeleteSupported = false;
            }

            return _IsTemporalTablesCascadeDeleteSupported.Value;
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

            if (_IsUtf8Supported.HasValue)
            {
                return _IsUtf8Supported.Value;
            }

            try
            {
                _IsUtf8Supported = GetCompatibilityLevel() >= 150;
            }
            catch (PlatformNotSupportedException)
            {
                _IsUtf8Supported = false;
            }

            return _IsUtf8Supported.Value;
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
                _supportsJsonPathExpressions = GetCompatibilityLevel() >= 140;
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

            if (_IsFunctions2017Supported.HasValue)
            {
                return _IsFunctions2017Supported.Value;
            }

            try
            {
                _IsFunctions2017Supported = GetCompatibilityLevel() >= 140;
            }
            catch (PlatformNotSupportedException)
            {
                _IsFunctions2017Supported = false;
            }

            return _IsFunctions2017Supported.Value;
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

            if (_IsFunctions2019Supported.HasValue)
            {
                return _IsFunctions2019Supported.Value;
            }

            try
            {
                _IsFunctions2019Supported = GetCompatibilityLevel() >= 150;
            }
            catch (PlatformNotSupportedException)
            {
                _IsFunctions2019Supported = false;
            }

            return _IsFunctions2019Supported.Value;
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

            if (_IsFunctions2022Supported.HasValue)
            {
                return _IsFunctions2022Supported.Value;
            }

            try
            {
                _IsFunctions2022Supported = GetCompatibilityLevel() >= 160;
            }
            catch (PlatformNotSupportedException)
            {
                _IsFunctions2022Supported = false;
            }

            return _IsFunctions2022Supported.Value;
        }
    }

    public static bool IsJsonTypeSupported
    {
        get
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (_isJsonTypeSupported.HasValue)
            {
                return _isJsonTypeSupported.Value;
            }

            try
            {
                _isJsonTypeSupported = GetCompatibilityLevel() >= 170;
            }
            catch (PlatformNotSupportedException)
            {
                _isJsonTypeSupported = false;
            }

            return _isJsonTypeSupported.Value;
        }
    }

    public static bool IsVectorTypeSupported
    {
        get
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (_isVectorTypeSupported.HasValue)
            {
                return _isVectorTypeSupported.Value;
            }

            try
            {
                _isVectorTypeSupported = !IsLocalDb && GetCompatibilityLevel() >= 170;
            }
            catch (PlatformNotSupportedException)
            {
                _isVectorTypeSupported = false;
            }

            return _isVectorTypeSupported.Value;
        }
    }

    public static byte SqlServerMajorVersion
        => GetProductMajorVersion();

    public static bool IsNotAzureSql => !IsAzureSql;

    public static bool SupportsAttach
    {
        get
        {
            var defaultConnection = new SqlConnectionStringBuilder(DefaultConnection);
            return defaultConnection.DataSource.Contains("(localdb)", StringComparison.OrdinalIgnoreCase)
                || defaultConnection.UserInstance;
        }
    }

    public static DbContextOptionsBuilder SetCompatibilityLevelFromEnvironment(DbContextOptionsBuilder builder)
    {
        builder.UseSqlServerCompatibilityLevel(GetCompatibilityLevel());

        return builder;
    }

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

    private static int GetCompatibilityLevel()
    {
        if (_compatibilityLevel.HasValue)
        {
            return _compatibilityLevel.Value;
        }

        using var sqlConnection = new SqlConnection(SqlServerTestStore.CreateConnectionString("master"));
        sqlConnection.Open();

        using var command = new SqlCommand(
            "SELECT compatibility_level FROM sys.databases WHERE [name] = 'model';", sqlConnection);
        _compatibilityLevel = (byte)command.ExecuteScalar();

        return _compatibilityLevel.Value;
    }
}
