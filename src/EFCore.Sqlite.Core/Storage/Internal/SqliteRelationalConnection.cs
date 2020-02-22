// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class SqliteRelationalConnection : RelationalConnection, ISqliteRelationalConnection
    {
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Infrastructure> _logger;
        private readonly bool _loadSpatialite;
        private readonly int? _commandTimeout;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteRelationalConnection(
            [NotNull] RelationalConnectionDependencies dependencies,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Infrastructure> logger)
            : base(dependencies)
        {
            Check.NotNull(rawSqlCommandBuilder, nameof(rawSqlCommandBuilder));

            _rawSqlCommandBuilder = rawSqlCommandBuilder;
            _logger = logger;

            var optionsExtension = dependencies.ContextOptions.Extensions.OfType<SqliteOptionsExtension>().FirstOrDefault();
            if (optionsExtension != null)
            {
                _loadSpatialite = optionsExtension.LoadSpatialite;

                var relationalOptions = RelationalOptionsExtension.Extract(dependencies.ContextOptions);
                _commandTimeout = relationalOptions.CommandTimeout;

                if (relationalOptions.Connection != null)
                {
                    InitializeDbConnection(relationalOptions.Connection);
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override DbConnection CreateDbConnection()
        {
            var connection = new SqliteConnection(GetCheckedConnectionString());
            InitializeDbConnection(connection);

            return connection;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ISqliteRelationalConnection CreateReadOnlyConnection()
        {
            var connectionStringBuilder =
                new SqliteConnectionStringBuilder(GetCheckedConnectionString()) { Mode = SqliteOpenMode.ReadOnly };

            var contextOptions = new DbContextOptionsBuilder().UseSqlite(connectionStringBuilder.ToString()).Options;

            return new SqliteRelationalConnection(Dependencies.With(contextOptions), _rawSqlCommandBuilder, _logger);
        }

        private void InitializeDbConnection(DbConnection connection)
        {
            if (_loadSpatialite)
            {
                SpatialiteLoader.Load(connection);
            }

            if (connection is SqliteConnection sqliteConnection)
            {
                if (_commandTimeout.HasValue)
                {
                    sqliteConnection.DefaultTimeout = _commandTimeout.Value;
                }

                sqliteConnection.CreateFunction<object, object, object>(
                    "ef_mod",
                    (dividend, divisor) =>
                    {
                        if (dividend == null
                            || divisor == null)
                        {
                            return null;
                        }

                        if (dividend is string s)
                        {
                            return decimal.Parse(s, CultureInfo.InvariantCulture)
                                % Convert.ToDecimal(divisor, CultureInfo.InvariantCulture);
                        }

                        return Convert.ToDouble(dividend, CultureInfo.InvariantCulture)
                            % Convert.ToDouble(divisor, CultureInfo.InvariantCulture);
                    });

                CreateEfCompareFunctions(sqliteConnection);
            }
            else
            {
                _logger.UnexpectedConnectionTypeWarning(connection.GetType());
            }
        }

        private void CreateEfCompareFunctions(SqliteConnection sqliteConnection)
        {
            var functions = new[]
            {
                ("ef_compare_gt", Comparer.Operator.GreaterThan),
                ("ef_compare_geq", Comparer.Operator.GreaterThanOrEqual),
                ("ef_compare_lt", Comparer.Operator.LessThan),
                ("ef_compare_leq", Comparer.Operator.LessThanOrEqual),
                ("ef_compare_eq", Comparer.Operator.Equal)
            };

            foreach (var function in functions)
            {
                sqliteConnection.CreateFunction<object, object, object>(
                    function.Item1,
                    (left, right) =>
                    {
                        if (left == null
                            || right == null)
                        {
                            return null;
                        }

                        var leftSide = left is string leftAsString
                            ? decimal.Parse(leftAsString, CultureInfo.CurrentCulture)
                            : Convert.ToDecimal(left, CultureInfo.CurrentCulture);
                        var rightSide = right is string rightAsString
                            ? decimal.Parse(rightAsString, CultureInfo.CurrentCulture)
                            : Convert.ToDecimal(right, CultureInfo.CurrentCulture);

                        return Comparer.IsTrue(
                            Convert.ToDecimal(leftSide, CultureInfo.CurrentCulture), function.Item2,
                            Convert.ToDecimal(rightSide, CultureInfo.CurrentCulture));
                    });
            }
        }

        internal static class Comparer
        {
            public static bool IsTrue<T, U>(T value1, Operator comparisonOperator, U value2)
                where T : U
                where U : IComparable
            {
                switch (comparisonOperator)
                {
                    case Operator.GreaterThan:
                        return value1.CompareTo(value2) > 0;
                    case Operator.GreaterThanOrEqual:
                        return value1.CompareTo(value2) >= 0;
                    case Operator.LessThan:
                        return value1.CompareTo(value2) < 0;
                    case Operator.LessThanOrEqual:
                        return value1.CompareTo(value2) <= 0;
                    case Operator.Equal:
                        return value1.CompareTo(value2) == 0;
                    default:
                        return false;
                }
            }

            internal enum Operator
            {
                GreaterThan = 1,
                GreaterThanOrEqual = 2,
                LessThan = 3,
                LessThanOrEqual = 4,
                Equal = 5
            }
        }
    }
}
