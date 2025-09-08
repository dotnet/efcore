// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Migrations.Internal
{
    public class XGHistoryRepository : HistoryRepository
    {
        private const string MigrationsScript = nameof(MigrationsScript);

        private readonly XGSqlGenerationHelper _sqlGenerationHelper;

        public XGHistoryRepository([NotNull] HistoryRepositoryDependencies dependencies)
            : base(dependencies)
        {
            _sqlGenerationHelper = (XGSqlGenerationHelper)dependencies.SqlGenerationHelper;
        }

        public override LockReleaseBehavior LockReleaseBehavior
            => LockReleaseBehavior.Connection;

        public override IMigrationsDatabaseLock AcquireDatabaseLock()
        {
            Dependencies.MigrationsLogger.AcquiringMigrationLock();

            Dependencies.RawSqlCommandBuilder
                .Build(GetAcquireLockCommandSql())
                .ExecuteNonQuery(CreateRelationalCommandParameters());

            return CreateMigrationDatabaseLock();
        }

        public override async Task<IMigrationsDatabaseLock> AcquireDatabaseLockAsync(CancellationToken cancellationToken = default)
        {
            await Dependencies.RawSqlCommandBuilder
                .Build(GetAcquireLockCommandSql())
                .ExecuteNonQueryAsync(CreateRelationalCommandParameters(), cancellationToken)
                .ConfigureAwait(false);

            return CreateMigrationDatabaseLock();
        }

        /// <summary>
        ///     Returns the name of the database-wide for migrations. Currently, this is actully a database *server*-wide lock, so the lock
        ///     should contain the database name to make it more database specific.
        /// </summary>
        protected virtual string GetDatabaseLockName(string databaseName)
            => $"__{databaseName}_EFMigrationsLock";

        // We cannot use LOCK TABLES/UNLOCK TABLES, because we would need to know *all* the table we want to access by name beforehand,
        // since after the LOCK TABLES statement has run, only the tables specified can be access and access to any other table results in
        // an error.
        // We use GET_LOCK()/RELEASE_LOCK() for now. We would like to not specify a timeout, because we cannot know how long the migration
        // operations are supposed to take. However, while MySQL interprets negative timeout values as infinite, MariaDB does not. We
        // therefore specify a very large timeout in seconds instead (currently 72 hours). If RELEASE_LOCK() is never called, the lock is automatically released
        // when the session ends or is killed. This function pair is not bound to a database, but is a database server wide global mutex. We
        // therefore explicitly use the database name as part of the lock name.
        // If it turns out, that users want a replication-save method later, we could implement a locking table mechanism as Sqlite does.
        private string GetAcquireLockCommandSql()
            => $"SELECT GET_LOCK('{GetDatabaseLockName(Dependencies.Connection.DbConnection.Database)}', {60 * 60 * 24 * 3})";

        private RelationalCommandParameterObject CreateRelationalCommandParameters()
            => new(
                Dependencies.Connection,
                null,
                null,
                Dependencies.CurrentContext.Context,
                Dependencies.CommandLogger, CommandSource.Migrations);

        private XGMigrationDatabaseLock CreateMigrationDatabaseLock()
            => new(
                this,
                CreateReleaseLockCommand(),
                CreateRelationalCommandParameters());

        private IRelationalCommand CreateReleaseLockCommand()
            => Dependencies.RawSqlCommandBuilder.Build($"SELECT RELEASE_LOCK('{GetDatabaseLockName(Dependencies.Connection.DbConnection.Database)}')");

        protected override void ConfigureTable([NotNull] EntityTypeBuilder<HistoryRow> history)
        {
            base.ConfigureTable(history);

            history.HasCharSet(CharSet.Utf8Mb4);
        }

        protected override string ExistsSql
        {
            get
            {
                var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

                var builder = new StringBuilder();

                builder.Append("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE ");

                builder
                    .Append("TABLE_SCHEMA=")
                    .Append(
                        stringTypeMapping.GenerateSqlLiteral(
                            _sqlGenerationHelper.GetSchemaName(TableName, TableSchema) ??
                            Dependencies.Connection.DbConnection.Database))
                    .Append(" AND TABLE_NAME=")
                    .Append(
                        stringTypeMapping.GenerateSqlLiteral(
                            _sqlGenerationHelper.GetObjectName(TableName, TableSchema)))
                    .Append(";");

                return builder.ToString();
            }
        }

        protected override bool InterpretExistsResult(object value) => value != null;

        public override string GetCreateIfNotExistsScript()
        {
            var script = GetCreateScript();
            return script.Insert(script.IndexOf("CREATE TABLE", StringComparison.Ordinal) + 12, " IF NOT EXISTS");
        }

        /// <summary>
        ///     Overridden by database providers to generate a SQL Script that will `BEGIN` a block
        ///     of SQL if and only if the migration with the given identifier does not already exist in the history table.
        /// </summary>
        /// <param name="migrationId"> The migration identifier. </param>
        /// <returns> The generated SQL. </returns>
        public override string GetBeginIfNotExistsScript(string migrationId) => GetBeginIfScript(migrationId, true);

        /// <summary>
        ///     Overridden by database providers to generate a SQL Script that will `BEGIN` a block
        ///     of SQL if and only if the migration with the given identifier already exists in the history table.
        /// </summary>
        /// <param name="migrationId"> The migration identifier. </param>
        /// <returns> The generated SQL. </returns>
        public override string GetBeginIfExistsScript(string migrationId) => GetBeginIfScript(migrationId, false);

        /// <summary>
        ///     Overridden by database providers to generate a SQL script to `END` the SQL block.
        /// </summary>
        /// <returns> The generated SQL. </returns>
        public virtual string GetBeginIfScript(string migrationId, bool notExists) => $@"DROP PROCEDURE IF EXISTS {MigrationsScript};
DELIMITER //
CREATE PROCEDURE {MigrationsScript}()
BEGIN
    IF{(notExists ? " NOT" : null)} EXISTS(SELECT 1 FROM {SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema)} WHERE {SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName)} = '{migrationId}') THEN
";

        /// <summary>
        ///     Overridden by database providers to generate a SQL script to `END` the SQL block.
        /// </summary>
        /// <returns> The generated SQL. </returns>
        public override string GetEndIfScript() => $@"
    END IF;
END //
DELIMITER ;
CALL {MigrationsScript}();
DROP PROCEDURE {MigrationsScript};
";

        public virtual void ConfigureModel(ModelBuilder modelBuilder)
            => modelBuilder.HasCharSet(null, DelegationModes.ApplyToDatabases);

        #region Necessary implementation because we cannot directly override EnsureModel

        private IModel _model;
        private string _migrationIdColumnName;
        private string _productVersionColumnName;

        // Customized implementation.
        protected virtual IModel EnsureModel()
        {
            if (_model == null)
            {
                var conventionSet = Dependencies.ConventionSetBuilder.CreateConventionSet();

                // Use public API to remove the convention, issue #214
                ConventionSet.Remove(conventionSet.ModelInitializedConventions, typeof(DbSetFindingConvention));
                ConventionSet.Remove(conventionSet.ModelInitializedConventions, typeof(RelationalDbFunctionAttributeConvention));

                var modelBuilder = new ModelBuilder(conventionSet);

                #region Custom implementation

                ConfigureModel(modelBuilder);

                #endregion

                modelBuilder.Entity<HistoryRow>(
                    x =>
                    {
                        ConfigureTable(x);
                        x.ToTable(TableName, TableSchema);
                    });

                _model = Dependencies.ModelRuntimeInitializer.Initialize(modelBuilder.FinalizeModel(), designTime: true, validationLogger: null);
            }

            return _model;
        }

        // Original implementation.
        public override string GetCreateScript()
        {
            var model = EnsureModel();

            var operations = Dependencies.ModelDiffer.GetDifferences(null, model.GetRelationalModel());
            var commandList = Dependencies.MigrationsSqlGenerator.Generate(operations, model);

            return string.Concat(commandList.Select(c => c.CommandText));
        }

        // Original implementation.
        protected override string MigrationIdColumnName
            => _migrationIdColumnName ??= EnsureModel()
                .FindEntityType(typeof(HistoryRow))!
                .FindProperty(nameof(HistoryRow.MigrationId))!
                .GetColumnName();

        // Original implementation.
        protected override string ProductVersionColumnName
            => _productVersionColumnName ??= EnsureModel()
                .FindEntityType(typeof(HistoryRow))!
                .FindProperty(nameof(HistoryRow.ProductVersion))!
                .GetColumnName();

        #endregion Necessary implementation because we cannot directly override EnsureModel

        private sealed class XGMigrationDatabaseLock(
            XGHistoryRepository historyRepository,
            IRelationalCommand releaseLockCommand,
            RelationalCommandParameterObject relationalCommandParameters,
            CancellationToken cancellationToken = default)
            : IMigrationsDatabaseLock
        {
            public IHistoryRepository HistoryRepository => historyRepository;

            public void Dispose()
                => releaseLockCommand.ExecuteScalar(relationalCommandParameters);

            public async ValueTask DisposeAsync()
                => await releaseLockCommand.ExecuteScalarAsync(relationalCommandParameters, cancellationToken).ConfigureAwait(false);
        }
    }
}
