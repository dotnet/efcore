// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Migrations.Infrastructure
{
    public class Migrator
    {
        public const string InitialDatabase = "0";

        private readonly HistoryRepository _historyRepository;
        private readonly MigrationAssembly _migrationAssembly;
        private readonly ModelDiffer _modelDiffer;
        private readonly IMigrationOperationSqlGeneratorFactory _ddlSqlGeneratorFactory;
        private readonly SqlGenerator _dmlSqlGenerator;
        private readonly SqlStatementExecutor _sqlExecutor;
        private readonly RelationalDataStoreCreator _storeCreator;
        private readonly RelationalConnection _connection;
        private readonly DbContextService<ILogger> _logger;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected Migrator()
        {
        }

        public Migrator(
            [NotNull] HistoryRepository historyRepository,
            [NotNull] MigrationAssembly migrationAssembly,
            [NotNull] ModelDiffer modelDiffer,
            [NotNull] IMigrationOperationSqlGeneratorFactory ddlSqlGeneratorFactory,
            [NotNull] SqlGenerator dmlSqlGenerator,
            [NotNull] SqlStatementExecutor sqlExecutor,
            [NotNull] RelationalDataStoreCreator storeCreator,
            [NotNull] RelationalConnection connection,
            [NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(historyRepository, "historyRepository");
            Check.NotNull(migrationAssembly, "migrationAssembly");
            Check.NotNull(modelDiffer, "modelDiffer");
            Check.NotNull(ddlSqlGeneratorFactory, "ddlSqlGeneratorFactory");
            Check.NotNull(dmlSqlGenerator, "dmlSqlGenerator");
            Check.NotNull(sqlExecutor, "sqlExecutor");
            Check.NotNull(storeCreator, "storeCreator");
            Check.NotNull(connection, "connection");
            Check.NotNull(loggerFactory, "loggerFactory");

            _historyRepository = historyRepository;
            _migrationAssembly = migrationAssembly;
            _modelDiffer = modelDiffer;
            _ddlSqlGeneratorFactory = ddlSqlGeneratorFactory;
            _dmlSqlGenerator = dmlSqlGenerator;
            _sqlExecutor = sqlExecutor;
            _storeCreator = storeCreator;
            _connection = connection;
            _logger = new DbContextService<ILogger>(loggerFactory.Create<Migrator>);
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
            get { return _logger.Service; }
        }

        public virtual IReadOnlyList<Migration> GetLocalMigrations()
        {
            return MigrationAssembly.Migrations;
        }

        public virtual IReadOnlyList<Migration> GetDatabaseMigrations()
        {
            var migrationPairs = PairMigrations(MigrationAssembly.Migrations, HistoryRepository.Rows);

            return
                migrationPairs
                    .Where(p => p.HistoryRow != null)
                    .Select(p => p.LocalMigration)
                    .ToList();
        }

        public virtual IReadOnlyList<Migration> GetPendingMigrations()
        {
            var migrationPairs = PairMigrations(MigrationAssembly.Migrations, HistoryRepository.Rows);

            return
                migrationPairs
                    .Where(p => p.HistoryRow == null)
                    .Select(p => p.LocalMigration)
                    .ToList();
        }

        public virtual void ApplyMigrations()
        {
            ApplyMigrations(MigrationAssembly.Migrations.Count - 1, simulate: false);
        }

        public virtual void ApplyMigrations([NotNull] string targetMigrationName)
        {
            Check.NotEmpty(targetMigrationName, "targetMigrationName");

            ApplyMigrations(GetTargetMigrationIndex(targetMigrationName), simulate: false);
        }

        public virtual IReadOnlyList<SqlBatch> ScriptMigrations()
        {
            return ApplyMigrations(MigrationAssembly.Migrations.Count - 1, simulate: true);
        }

        public virtual IReadOnlyList<SqlBatch> ScriptMigrations([NotNull] string targetMigrationName)
        {
            Check.NotEmpty(targetMigrationName, "targetMigrationName");

            return ApplyMigrations(GetTargetMigrationIndex(targetMigrationName), simulate: true);
        }

        protected virtual IReadOnlyList<SqlBatch> ApplyMigrations(int targetMigrationIndex, bool simulate)
        {
            bool historyTableExists;
            var migrationPairs = PairMigrations(MigrationAssembly.Migrations, HistoryRepository.GetRows(out historyTableExists));
            var downgradeIndexes
                = MigrationAssembly.Migrations
                    .Select((m, i) => i)
                    .Skip(targetMigrationIndex + 1)
                    .Where(i => migrationPairs[i].HistoryRow != null)
                    .Reverse()
                    .ToList();
            var upgradeIndexes
                = MigrationAssembly.Migrations
                    .Select((m, i) => i)
                    .Take(targetMigrationIndex + 1)
                    .Where(i => migrationPairs[i].HistoryRow == null)
                    .ToList();

            if (!simulate
                && !_storeCreator.Exists())
            {
                _storeCreator.Create();
            }

            var batches = new List<SqlBatch>();

            if (upgradeIndexes.Any()
                && !historyTableExists)
            {
                batches.AddRange(CreateHistoryTable(simulate));
            }

            batches.AddRange(downgradeIndexes.SelectMany(i => RevertMigration(i, simulate)));

            batches.AddRange(upgradeIndexes.SelectMany(i => ApplyMigration(i, simulate)));

            if (targetMigrationIndex == -1 && historyTableExists)
            {
                batches.AddRange(DropHistoryTable(simulate));
            }

            if (batches.Count == 0)
            {
                Logger.UpToDate();
            }

            return batches;
        }

        protected virtual IReadOnlyList<SqlBatch> CreateHistoryTable(bool simulate)
        {
            var targetModel = HistoryRepository.HistoryModel;
            var ddlSqlGenerator = DdlSqlGeneratorFactory.Create(targetModel);

            var batches = ddlSqlGenerator.Generate(ModelDiffer.CreateSchema(targetModel)).ToArray();

            if (simulate)
            {
                return batches;
            }

            Logger.CreatingHistoryTable();

            ExecuteSqlBatches(batches);

            return batches;
        }

        protected virtual IReadOnlyList<SqlBatch> DropHistoryTable(bool simulate)
        {
            var sourceModel = HistoryRepository.HistoryModel;
            var ddlSqlGenerator = DdlSqlGeneratorFactory.Create();

            var batches = ddlSqlGenerator.Generate(ModelDiffer.DropSchema(sourceModel)).ToArray();

            if (simulate)
            {
                return batches;
            }

            Logger.DroppingHistoryTable();

            ExecuteSqlBatches(batches);

            return batches;
        }

        protected virtual IReadOnlyList<SqlBatch> ApplyMigration(int index, bool simulate)
        {
            var migration = MigrationAssembly.Migrations[index];
            var targetModel = migration.GetTargetModel();
            var ddlSqlGenerator = DdlSqlGeneratorFactory.Create(targetModel);

            var statements = ddlSqlGenerator.Generate(migration.GetUpgradeOperations())
                .Concat(HistoryRepository.GenerateInsertMigrationSql(migration.GetMetadata(), DmlSqlGenerator))
                .ToList();

            if (simulate)
            {
                return statements;
            }

            Logger.ApplyingMigration(migration.GetMigrationId());

            ExecuteSqlBatches(statements);

            return statements;
        }

        protected virtual IReadOnlyList<SqlBatch> RevertMigration(int index, bool simulate)
        {
            var migration = MigrationAssembly.Migrations[index];
            var targetModel = GetSourceModel(index);
            var ddlSqlGenerator = DdlSqlGeneratorFactory.Create(targetModel);

            var batches = ddlSqlGenerator.Generate(migration.GetDowngradeOperations())
                .Concat(HistoryRepository.GenerateDeleteMigrationSql(migration.GetMetadata(), DmlSqlGenerator))
                .ToList();

            if (simulate)
            {
                return batches;
            }

            Logger.RevertingMigration(migration.GetMigrationId());

            ExecuteSqlBatches(batches);

            return batches;
        }

        protected virtual IReadOnlyList<MigrationPair> PairMigrations(
            IReadOnlyList<Migration> localMigrations,
            IReadOnlyList<HistoryRow> historyRows)
        {
            Check.NotNull(localMigrations, "localMigrations");
            Check.NotNull(historyRows, "historyRows");

            var pairs = new List<MigrationPair>(localMigrations.Count);
            var i = 0;
            var j = 0;

            while (i < localMigrations.Count
                   && j < historyRows.Count)
            {
                var compareResult = string.CompareOrdinal(
                    localMigrations[i].GetMigrationId(), historyRows[j].MigrationId);

                if (compareResult == 0)
                {
                    pairs.Add(new MigrationPair(localMigrations[i++], historyRows[j++]));
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

            if (j < historyRows.Count)
            {
                throw new InvalidOperationException(Strings.LocalMigrationNotFound(
                    historyRows[j].MigrationId));
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
                index = MigrationAssembly.Migrations.IndexOf(m => m.GetMigrationName() == name);
                if (index < 0)
                {
                    throw new InvalidOperationException(Strings.TargetMigrationNotFound(name));
                }
            }

            return index;
        }

        protected virtual IModel GetSourceModel(int index)
        {
            return index == 0 ? new Model() : MigrationAssembly.Migrations[index - 1].GetTargetModel();
        }

        protected virtual void ExecuteSqlBatches([NotNull] IEnumerable<SqlBatch> sqlBatches)
        {
            Check.NotNull(sqlBatches, "sqlBatches");

            var pendingBatches = new List<SqlBatch>();

            foreach (var sqlBatch in sqlBatches.Where(b => !string.IsNullOrEmpty(b.Sql)))
            {
                if (!sqlBatch.SuppressTransaction)
                {
                    pendingBatches.Add(sqlBatch);

                    continue;
                }

                if (pendingBatches.Any())
                {
                    ExecuteStatementsWithinTransaction(pendingBatches, _connection);

                    pendingBatches.Clear();
                }

                ExecuteStatementsWithoutTransaction(new[] { sqlBatch }, _connection);
            }

            if (pendingBatches.Any())
            {
                ExecuteStatementsWithinTransaction(pendingBatches, _connection);
            }
        }

        protected virtual void ExecuteStatementsWithinTransaction(
            [NotNull] IEnumerable<SqlBatch> sqlStatements, [NotNull] RelationalConnection connection)
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
            [NotNull] IEnumerable<SqlBatch> sqlStatements, [NotNull] RelationalConnection connection)
        {
            Check.NotNull(sqlStatements, "sqlStatements");
            Check.NotNull(connection, "connection");

            SqlExecutor.ExecuteNonQuery(connection.DbConnection, null, sqlStatements);
        }

        protected struct MigrationPair
        {
            public readonly Migration LocalMigration;
            public readonly HistoryRow HistoryRow;

            public MigrationPair(
                [NotNull] Migration localMigration,
                [CanBeNull] HistoryRow historyRow)
            {
                Check.NotNull(localMigration, "localMigration");

                LocalMigration = localMigration;
                HistoryRow = historyRow;
            }
        }
    }
}
