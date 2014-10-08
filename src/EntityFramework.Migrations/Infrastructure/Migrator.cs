// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public class Migrator
    {
        public const string InitialDatabase = "0";

        private readonly DbContextConfiguration _contextConfiguration;
        private readonly HistoryRepository _historyRepository;
        private readonly MigrationAssembly _migrationAssembly;
        private readonly ModelDiffer _modelDiffer;
        private readonly IMigrationOperationSqlGeneratorFactory _ddlSqlGeneratorFactory;
        private readonly SqlGenerator _dmlSqlGenerator;
        private readonly SqlStatementExecutor _sqlExecutor;
        private readonly LazyRef<ILogger> _logger;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected Migrator()
        {
        }

        public Migrator(
            [NotNull] DbContextConfiguration contextConfiguration,
            [NotNull] HistoryRepository historyRepository,
            [NotNull] MigrationAssembly migrationAssembly,
            [NotNull] ModelDiffer modelDiffer,
            [NotNull] IMigrationOperationSqlGeneratorFactory ddlSqlGeneratorFactory,
            [NotNull] SqlGenerator dmlSqlGenerator,
            [NotNull] SqlStatementExecutor sqlExecutor)
        {
            Check.NotNull(contextConfiguration, "contextConfiguration");
            Check.NotNull(historyRepository, "historyRepository");
            Check.NotNull(migrationAssembly, "migrationAssembly");
            Check.NotNull(modelDiffer, "modelDiffer");
            Check.NotNull(ddlSqlGeneratorFactory, "ddlSqlGeneratorFactory");
            Check.NotNull(dmlSqlGenerator, "dmlSqlGenerator");
            Check.NotNull(sqlExecutor, "sqlExecutor");

            _contextConfiguration = contextConfiguration;
            _historyRepository = historyRepository;
            _migrationAssembly = migrationAssembly;
            _modelDiffer = modelDiffer;
            _ddlSqlGeneratorFactory = ddlSqlGeneratorFactory;
            _dmlSqlGenerator = dmlSqlGenerator;
            _sqlExecutor = sqlExecutor;
            _logger = new LazyRef<ILogger>(() => ContextConfiguration.LoggerFactory.Create(GetType().Name));
        }

        protected virtual DbContextConfiguration ContextConfiguration
        {
            get { return _contextConfiguration; }
        }

        public virtual HistoryRepository HistoryRepository
        {
            get { return _historyRepository; }
        }

        public virtual MigrationAssembly MigrationAssembly
        {
            get { return _migrationAssembly; }
        }

        public virtual ModelDiffer ModelDiffer
        {
            get { return _modelDiffer; }
        }

        protected virtual IMigrationOperationSqlGeneratorFactory DdlSqlGeneratorFactory
        {
            get { return _ddlSqlGeneratorFactory; }
        }

        protected virtual SqlGenerator DmlSqlGenerator
        {
            get { return _dmlSqlGenerator; }
        }

        protected virtual SqlStatementExecutor SqlExecutor
        {
            get { return _sqlExecutor; }
        }

        protected virtual ILogger Logger
        {
            get { return _logger.Value; }
        }

        public virtual IReadOnlyList<IMigrationMetadata> GetLocalMigrations()
        {
            return MigrationAssembly.Migrations;
        }

        public virtual IReadOnlyList<IMigrationMetadata> GetDatabaseMigrations()
        {
            bool historyRepositoryExists;
            return GetDatabaseMigrations(out historyRepositoryExists);
        }

        public virtual IReadOnlyList<IMigrationMetadata> GetPendingMigrations()
        {
            var migrationPairs = PairMigrations(GetLocalMigrations(), GetDatabaseMigrations());

            return
                migrationPairs
                    .Where(p => p.DatabaseMigration == null)
                    .Select(p => p.LocalMigration)
                    .ToArray();
        }

        protected virtual IReadOnlyList<IMigrationMetadata> GetDatabaseMigrations(out bool historyTableExists)
        {
            IReadOnlyList<IMigrationMetadata> migrations;
            try
            {
                migrations = HistoryRepository.Migrations;
                historyTableExists = true;
            }
            catch (DbException)
            {
                // TODO: Log the exception message.
                migrations = new IMigrationMetadata[0];
                historyTableExists = false;
            }

            return migrations;
        }

        public virtual void UpdateDatabase()
        {
            UpdateDatabase(GetLocalMigrations().Count - 1, simulate: false);
        }

        public virtual void UpdateDatabase([NotNull] string targetMigrationName)
        {
            Check.NotEmpty(targetMigrationName, "targetMigrationName");

            UpdateDatabase(GetTargetMigrationIndex(targetMigrationName), simulate: false);
        }

        public virtual IReadOnlyList<SqlStatement> GenerateUpdateDatabaseSql()
        {
            return UpdateDatabase(GetLocalMigrations().Count - 1, simulate: true);
        }

        public virtual IReadOnlyList<SqlStatement> GenerateUpdateDatabaseSql([NotNull] string targetMigrationName)
        {
            Check.NotEmpty(targetMigrationName, "targetMigrationName");

            return UpdateDatabase(GetTargetMigrationIndex(targetMigrationName), simulate: true);
        }

        protected virtual IReadOnlyList<SqlStatement> UpdateDatabase(int targetMigrationIndex, bool simulate)
        {
            bool historyTableExists;
            var migrationPairs = PairMigrations(GetLocalMigrations(), GetDatabaseMigrations(out historyTableExists));
            var downgradeIndexes
                = GetLocalMigrations()
                    .Select((m, i) => i)
                    .Skip(targetMigrationIndex + 1)
                    .Where(i => migrationPairs[i].DatabaseMigration != null)
                    .Reverse()
                    .ToArray();
            var upgradeIndexes
                = GetLocalMigrations()
                    .Select((m, i) => i)
                    .Take(targetMigrationIndex + 1)
                    .Where(i => migrationPairs[i].DatabaseMigration == null)
                    .ToArray();
            var database = (RelationalDatabase)ContextConfiguration.Database;
            var statements = new List<SqlStatement>();

            if (!simulate && !database.Exists())
            {
                database.Create();
            }

            if (upgradeIndexes.Any() && !historyTableExists)
            {
                statements.AddRange(CreateHistoryTable(simulate));
            }

            statements.AddRange(downgradeIndexes.SelectMany(i => RevertMigration(i, simulate)));

            statements.AddRange(upgradeIndexes.SelectMany(i => ApplyMigration(i, simulate)));

            if (targetMigrationIndex == -1 && historyTableExists)
            {
                statements.AddRange(DropHistoryTable(simulate));
            }

            return statements;
        }

        protected virtual IReadOnlyList<SqlStatement> CreateHistoryTable(bool simulate)
        {
            var targetDatabase = ModelDiffer.DatabaseBuilder.GetDatabase(HistoryRepository.HistoryModel);
            var ddlSqlGenerator = DdlSqlGeneratorFactory.Create();

            var statements = ddlSqlGenerator.Generate(ModelDiffer.CreateSchema(targetDatabase)).ToArray();

            if (simulate)
            {
                return statements;
            }

            Logger.CreatingHistoryTable();

            ExecuteStatements(statements);

            return statements;
        }

        protected virtual IReadOnlyList<SqlStatement> DropHistoryTable(bool simulate)
        {
            var sourceDatabase = ModelDiffer.DatabaseBuilder.GetDatabase(HistoryRepository.HistoryModel);
            var ddlSqlGenerator = DdlSqlGeneratorFactory.Create(sourceDatabase);

            var statements = ddlSqlGenerator.Generate(ModelDiffer.DropSchema(sourceDatabase)).ToArray();

            if (simulate)
            {
                return statements;
            }

            Logger.DroppingHistoryTable();

            ExecuteStatements(statements);

            return statements;
        }

        protected virtual IReadOnlyList<SqlStatement> ApplyMigration(int index, bool simulate)
        {
            var migration = GetLocalMigrations()[index];
            var sourceDatabase = ModelDiffer.DatabaseBuilder.GetDatabase(GetSourceModel(index));
            var ddlSqlGenerator = DdlSqlGeneratorFactory.Create(sourceDatabase);

            var statements = ddlSqlGenerator.Generate(migration.UpgradeOperations)
                .Concat(HistoryRepository.GenerateInsertMigrationSql(migration, DmlSqlGenerator))
                .ToArray();

            if (simulate)
            {
                return statements;
            }

            Logger.ApplyingMigration(migration.MigrationId);

            ExecuteStatements(statements);

            return statements;
        }

        protected virtual IReadOnlyList<SqlStatement> RevertMigration(int index, bool simulate)
        {
            var migration = GetLocalMigrations()[index];
            var sourceDatabase = ModelDiffer.DatabaseBuilder.GetDatabase(migration.TargetModel);
            var ddlSqlGenerator = DdlSqlGeneratorFactory.Create(sourceDatabase);

            var statements = ddlSqlGenerator.Generate(migration.DowngradeOperations)
                .Concat(HistoryRepository.GenerateDeleteMigrationSql(migration, DmlSqlGenerator))
                .ToArray();

            if (simulate)
            {
                return statements;
            }

            Logger.RevertingMigration(migration.MigrationId);

            ExecuteStatements(statements);

            return statements;
        }

        protected virtual IReadOnlyList<MigrationPair> PairMigrations(
            IReadOnlyList<IMigrationMetadata> localMigrations,
            IReadOnlyList<IMigrationMetadata> databaseMigrations)
        {
            Check.NotNull(localMigrations, "localMigrations");
            Check.NotNull(databaseMigrations, "databaseMigrations");

            var pairs = new List<MigrationPair>(localMigrations.Count);
            var i = 0;
            var j = 0;

            while (i < localMigrations.Count
                   && j < databaseMigrations.Count)
            {
                var compareResult = string.CompareOrdinal(
                    localMigrations[i].MigrationId, databaseMigrations[j].MigrationId);

                if (compareResult == 0)
                {
                    pairs.Add(new MigrationPair(localMigrations[i++], databaseMigrations[j++]));
                }
                else if (compareResult < 0)
                {
                    pairs.Add(new MigrationPair(localMigrations[i++], null));
                }
                else
                {
                    break;
                }
            }

            if (j < databaseMigrations.Count)
            {
                throw new InvalidOperationException(Strings.FormatLocalMigrationNotFound(
                    databaseMigrations[j].MigrationId));
            }

            while (i < localMigrations.Count)
            {
                pairs.Add(new MigrationPair(localMigrations[i++], null));
            }

            return pairs;
        }

        protected virtual int GetTargetMigrationIndex([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            int index;

            if (name == InitialDatabase)
            {
                index = -1;
            }
            else
            {
                index = GetLocalMigrations().IndexOf(m => m.GetMigrationName() == name);
                if (index < 0)
                {
                    throw new InvalidOperationException(Strings.FormatTargetMigrationNotFound(name));
                }
            }

            return index;
        }

        protected virtual IModel GetSourceModel(int index)
        {
            return index == 0 ? new Metadata.Model() : GetLocalMigrations()[index - 1].TargetModel;
        }

        protected virtual void ExecuteStatements([NotNull] IEnumerable<SqlStatement> sqlStatements)
        {
            Check.NotNull(sqlStatements, "sqlStatements");

            var connection = ((RelationalDatabase)ContextConfiguration.Database).Connection;
            var pendingStatements = new List<SqlStatement>();

            foreach (var statement in sqlStatements.Where(s => !string.IsNullOrEmpty(s.Sql)))
            {
                if (!statement.SuppressTransaction)
                {
                    pendingStatements.Add(statement);

                    continue;
                }

                if (pendingStatements.Any())
                {
                    ExecuteStatementsWithinTransaction(pendingStatements, connection);

                    pendingStatements.Clear();
                }

                ExecuteStatementsWithoutTransaction(new[] { statement }, connection);
            }

            if (pendingStatements.Any())
            {
                ExecuteStatementsWithinTransaction(pendingStatements, connection);
            }
        }

        protected virtual void ExecuteStatementsWithinTransaction(
            [NotNull] IEnumerable<SqlStatement> sqlStatements, [NotNull] RelationalConnection connection)
        {
            Check.NotNull(sqlStatements, "sqlStatements");
            Check.NotNull(connection, "connection");

            RelationalTransaction transaction = null;
            try
            {
                transaction = connection.BeginTransaction(IsolationLevel.Serializable);

                SqlExecutor.ExecuteNonQuery(connection.DbConnection, transaction.DbTransaction, sqlStatements);

                transaction.Commit();
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }

        protected virtual void ExecuteStatementsWithoutTransaction(
            [NotNull] IEnumerable<SqlStatement> sqlStatements, [NotNull] RelationalConnection connection)
        {
            Check.NotNull(sqlStatements, "sqlStatements");
            Check.NotNull(connection, "connection");

            SqlExecutor.ExecuteNonQuery(connection.DbConnection, null, sqlStatements);
        }

        protected struct MigrationPair
        {
            public readonly IMigrationMetadata LocalMigration;
            public readonly IMigrationMetadata DatabaseMigration;

            public MigrationPair(
                [NotNull] IMigrationMetadata localMigration,
                [CanBeNull] IMigrationMetadata databaseMigration)
            {
                Check.NotNull(localMigration, "localMigration");

                LocalMigration = localMigration;
                DatabaseMigration = databaseMigration;
            }
        }
    }
}
