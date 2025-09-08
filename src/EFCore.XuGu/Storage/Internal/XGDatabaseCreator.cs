// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using XuguClient;
using Microsoft.EntityFrameworkCore.XuGu.Internal;
using System.Text.RegularExpressions;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGDatabaseCreator : RelationalDatabaseCreator
    {
        private readonly IXGRelationalConnection _relationalConnection;
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
        private readonly string _databaseName = "SYSTEM";

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XGDatabaseCreator(
            [NotNull] RelationalDatabaseCreatorDependencies dependencies,
            [NotNull] IXGRelationalConnection relationalConnection,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder)
            : base(dependencies)
        {
            if (relationalConnection.DbConnection.ConnectionString != null)
            {
                Match match = Regex.Match(relationalConnection.DbConnection.ConnectionString, @".*DB=([^;]+)");

                if (match.Success)
                {
                    _databaseName = match.Groups[1].Value;
                }
            }

            _relationalConnection = relationalConnection;
            ((XGConnection)_relationalConnection.DbConnection).ChangeDatabase(_databaseName, false);
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TimeSpan RetryTimeout { get; set; } = TimeSpan.FromMinutes(1);

        public override void Create()
        {
            using (var masterConnection = _relationalConnection.CreateMasterConnection())
            {
                Dependencies.MigrationCommandExecutor
                    .ExecuteNonQuery(CreateCreateOperations(), masterConnection);

                ClearPool();
            }

            Exists(retryOnNotExists: true);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override async Task CreateAsync(CancellationToken cancellationToken = default)
        {
            using (var masterConnection = _relationalConnection.CreateMasterConnection())
            {
                await Dependencies.MigrationCommandExecutor.ExecuteNonQueryAsync(CreateCreateOperations(), masterConnection, cancellationToken).ConfigureAwait(false);

                ClearPool();
            }

            await ExistsAsync(retryOnNotExists: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool HasTables()
            => Dependencies.ExecutionStrategy
                .Execute(
                    _relationalConnection,
                    connection => Convert.ToInt64(CreateHasTablesCommand() // XuGu returns a Int64
                                      .ExecuteScalar(
                                          new RelationalCommandParameterObject(
                                              connection,
                                              null,
                                              null,
                                              Dependencies.CurrentContext.Context,
                                              Dependencies.CommandLogger))) != 0);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Task<bool> HasTablesAsync(CancellationToken cancellationToken = default)
            => Dependencies.ExecutionStrategy
                .ExecuteAsync(
                    _relationalConnection,
                    async (connection, ct) => Convert.ToInt64(
                        await CreateHasTablesCommand() // XuGu returns a Int64
                            .ExecuteScalarAsync(
                                new RelationalCommandParameterObject(
                                    connection,
                                    null,
                                    null,
                                    Dependencies.CurrentContext.Context,
                                    Dependencies.CommandLogger),
                                cancellationToken: ct)
                            .ConfigureAwait(false)) != 0,
                    cancellationToken);

        private IRelationalCommand CreateHasTablesCommand()
            => _rawSqlCommandBuilder
                .Build(@"SELECT CASE WHEN COUNT(*) = 0 THEN FALSE ELSE TRUE END
FROM all_tables;");

        private IReadOnlyList<MigrationCommand> CreateCreateOperations()
        {
            var designTimeModel = Dependencies.CurrentContext.Context.GetService<IDesignTimeModel>().Model;

            return Dependencies.MigrationsSqlGenerator.Generate(
                new[]
                {
                    new XGCreateDatabaseOperation
                    {
                        Name = _relationalConnection.DbConnection.Database,
                        CharSet = designTimeModel.GetCharSet(),
                        TimeZone = designTimeModel.GetTimeZone(),
                    }
                });
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool Exists()
            => Exists(retryOnNotExists: false);

        private bool Exists(bool retryOnNotExists)
            => Dependencies.ExecutionStrategy.Execute(
                DateTime.UtcNow + RetryTimeout, giveUp =>
                {
                    while (true)
                    {
                        try
                        {
                            try
                            {
                                using (var masterConnection = _relationalConnection.CreateMasterConnection())
                                {
                                    masterConnection.Open();
                                    using (var cmd = masterConnection.DbConnection.CreateCommand())
                                    {
                                        cmd.CommandText = $"SELECT COUNT(*) FROM ALL_DATABASES WHERE DB_NAME = '{_relationalConnection.DbConnection.Database}';";
                                        long i = (long)cmd.ExecuteScalar();
                                        if (i > 0)
                                        {
                                            cmd.CommandText = $"USE `{_relationalConnection.DbConnection.Database}`";
                                            cmd.ExecuteNonQuery();
                                            return true;
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                if (!e.Message.Contains("1045")) // Access denied because credentials were lost
                                {
                                    throw;
                                }

                                _relationalConnection.Open(errorsExpected: true);

                                _relationalConnection.Close();
                            }
                            return true;
                        }
                        catch (Exception e)
                        {
                            if (!retryOnNotExists
                                && IsDoesNotExist(e))
                            {
                                return false;
                            }

                            if (DateTime.UtcNow > giveUp
                                || !RetryOnExistsFailure(e))
                            {
                                throw;
                            }

                            Thread.Sleep(RetryDelay);
                        }
                    }
                });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
            => ExistsAsync(retryOnNotExists: false, cancellationToken: cancellationToken);

        private Task<bool> ExistsAsync(bool retryOnNotExists, CancellationToken cancellationToken)
            => Dependencies.ExecutionStrategy.ExecuteAsync(
                DateTime.UtcNow + RetryTimeout, async (giveUp, ct) =>
                {
                    while (true)
                    {
                        try
                        {
                            try
                            {
                                using (var masterConnection = _relationalConnection.CreateMasterConnection())
                                {
                                    await masterConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
                                    using (var cmd = masterConnection.DbConnection.CreateCommand())
                                    {
                                        cmd.CommandText = $"SELECT COUNT(*) FROM ALL_DATABASES WHERE DB_NAME = '{_relationalConnection.DbConnection.Database}';";
                                        long i = (long)cmd.ExecuteScalar();
                                        if (i > 0)
                                        {
                                            cmd.CommandText = $"USE `{_relationalConnection.DbConnection.Database}`";
                                            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                                            return true;
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                if (!e.Message.Contains("1045")) // Access denied because credentials were lost
                                {
                                    throw;
                                }

                                await _relationalConnection.OpenAsync(ct, errorsExpected: true)
                                    .ConfigureAwait(false);

                                _relationalConnection.Close();
                            }
                            return true;
                        }
                        catch (Exception e)
                        {
                            if (!retryOnNotExists
                                && IsDoesNotExist(e))
                            {
                                return false;
                            }

                            if (DateTime.UtcNow > giveUp
                                || !RetryOnExistsFailure(e))
                            {
                                throw;
                            }

                            await Task.Delay(RetryDelay, ct).ConfigureAwait(false);
                        }
                    }
                }, cancellationToken);

        private static bool IsDoesNotExist(Exception exception) => exception.Message.Contains("E2016");

        private bool RetryOnExistsFailure(Exception exception)
        {
            if (exception.Message.Contains("1049"))
            {
                ClearPool();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void Delete()
        {
            ClearAllPools();

            using (var masterConnection = _relationalConnection.CreateMasterConnection())
            {
                masterConnection.Open();
                using (var cmd = masterConnection.DbConnection.CreateCommand())
                {
                    cmd.CommandText = $"USE `SYSTEM`;";
                    cmd.ExecuteNonQuery();
                }
                masterConnection.DbConnection.ChangeDatabase("SYSTEM");
                Dependencies.MigrationCommandExecutor
                    .ExecuteNonQuery(CreateDropCommands(), masterConnection);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override async Task DeleteAsync(CancellationToken cancellationToken = default)
        {
            ClearAllPools();

            using (var masterConnection = _relationalConnection.CreateMasterConnection())
            {
                await masterConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var cmd = masterConnection.DbConnection.CreateCommand())
                {
                    cmd.CommandText = $"USE `SYSTEM`;";
                    await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
                masterConnection.DbConnection.ChangeDatabase("SYSTEM");
                await Dependencies.MigrationCommandExecutor
                    .ExecuteNonQueryAsync(CreateDropCommands(), masterConnection, cancellationToken).ConfigureAwait(false);
            }
        }

        private IReadOnlyList<MigrationCommand> CreateDropCommands()
        {
            var databaseName = _relationalConnection.DbConnection.Database;
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new InvalidOperationException(XGStrings.NoInitialCatalog);
            }

            var operations = new MigrationOperation[]
            {
                // TODO Check DbConnection.Database always gives us what we want
                // Issue #775
                new XGDropDatabaseOperation { Name = _relationalConnection.DbConnection.Database }
            };

            return Dependencies.MigrationsSqlGenerator.Generate(operations);
        }

        // Clear connection pools in case there are active connections that are pooled
        private static void ClearAllPools() => global::XuguClient.XGConnection.ClearAllPools();

        // Clear connection pool for the database connection since after the 'create database' call, a previously
        // invalid connection may now be valid.
        private void ClearPool() => global::XuguClient.XGConnection.ClearPool((global::XuguClient.XGConnection)_relationalConnection.DbConnection);
    }
}
