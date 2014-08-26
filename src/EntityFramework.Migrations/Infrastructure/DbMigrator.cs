// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public class DbMigrator
    {
        public const string InitialDatabase = "0";

        private readonly DbContextConfiguration _contextConfiguration;
        private readonly HistoryRepository _historyRepository;
        private readonly MigrationAssembly _migrationAssembly;
        private readonly ModelDiffer _modelDiffer;
        private readonly IMigrationOperationSqlGeneratorFactory _ddlSqlGeneratorFactory;
        private readonly SqlGenerator _dmlSqlGenerator;
        private readonly SqlStatementExecutor _sqlExecutor;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected DbMigrator()
        {
        }

        public DbMigrator(
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
        }

        protected virtual DbContextConfiguration ContextConfiguration
        {
            get { return _contextConfiguration; }
        }

        protected virtual HistoryRepository HistoryRepository
        {
            get { return _historyRepository; }
        }

        protected virtual MigrationAssembly MigrationAssembly
        {
            get { return _migrationAssembly; }
        }

        protected virtual ModelDiffer ModelDiffer
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

        protected virtual IReadOnlyList<IMigrationMetadata> GetDatabaseMigrations(out bool historyRepositoryExists)
        {
            IReadOnlyList<IMigrationMetadata> migrations;
            try
            {
                migrations = HistoryRepository.Migrations;
                historyRepositoryExists = true;
            }
            catch (DataStoreException)
            {
                // TODO: Log the exception message.
                migrations = new IMigrationMetadata[0];
                historyRepositoryExists = false;
            }

            return migrations;
        }

        public virtual void UpdateDatabase()
        {
            UpdateDatabase(GenerateUpdateDatabaseSql());
        }

        public virtual void UpdateDatabase([NotNull] string targetMigrationName)
        {
            Check.NotEmpty(targetMigrationName, "targetMigrationName");

            UpdateDatabase(GenerateUpdateDatabaseSql(targetMigrationName));
        }

        protected virtual void UpdateDatabase(IReadOnlyList<SqlStatement> sqlStatements)
        {
            var database = (RelationalDatabase)ContextConfiguration.Database;

            if (!database.Exists())
            {
                database.Create();
            }

            SqlExecutor.ExecuteNonQuery(database.Connection.DbConnection, database.Connection.DbTransaction, sqlStatements);
        }

        public virtual IReadOnlyList<SqlStatement> GenerateUpdateDatabaseSql()
        {
            bool historyRepositoryExists;
            var localMigrations = GetLocalMigrations();
            var migrationPairs = PairMigrations(localMigrations, GetDatabaseMigrations(out historyRepositoryExists));

            return
                GenerateUpdateDatabaseSql(
                    localMigrations,
                    new int[0],
                    localMigrations
                        .Select((p, i) => i)
                        .Where(i => migrationPairs[i].DatabaseMigration == null)
                        .ToArray(),
                    historyRepositoryExists,
                    removeHistoryRepository: false);
        }

        public virtual IReadOnlyList<SqlStatement> GenerateUpdateDatabaseSql([NotNull] string targetMigrationName)
        {
            Check.NotEmpty(targetMigrationName, "targetMigrationName");

            bool historyRepositoryExists;
            var localMigrations = GetLocalMigrations();
            var migrationPairs = PairMigrations(localMigrations, GetDatabaseMigrations(out historyRepositoryExists));

            int index;
            if (targetMigrationName == InitialDatabase)
            {
                index = -1;
            }
            else
            {
                index = localMigrations.IndexOf(m => m.GetMigrationName() == targetMigrationName);
                if (index < 0)
                {
                    throw new InvalidOperationException(Strings.FormatTargetMigrationNotFound(targetMigrationName));
                }
            }

            return
                GenerateUpdateDatabaseSql(
                    localMigrations,
                    localMigrations
                        .Select((m, i) => i)
                        .Skip(index + 1)
                        .Where(i => migrationPairs[i].DatabaseMigration != null)
                        .Reverse()
                        .ToArray(),
                    localMigrations
                        .Select((m, i) => i)
                        .Take(index + 1)
                        .Where(i => migrationPairs[i].DatabaseMigration == null)
                        .ToArray(),
                    historyRepositoryExists, index == -1);
        }

        protected virtual IReadOnlyList<SqlStatement> GenerateUpdateDatabaseSql(
            IReadOnlyList<IMigrationMetadata> migrations,
            IReadOnlyList<int> downgradeIndexes,
            IReadOnlyList<int> upgradeIndexes,
            bool historyRepositoryExists,
            bool removeHistoryRepository)
        {
            var sqlStatements = new List<SqlStatement>();

            if (!historyRepositoryExists
                && upgradeIndexes.Count > 0)
            {
                var targetDatabase = ModelDiffer.DatabaseBuilder.GetDatabase(HistoryRepository.HistoryModel);
                var ddlSqlGenerator = DdlSqlGeneratorFactory.Create();

                sqlStatements.AddRange(ddlSqlGenerator.Generate(ModelDiffer.CreateSchema(targetDatabase)));
            }

            foreach (var index in downgradeIndexes)
            {
                var migration = migrations[index];
                var sourceDatabase = ModelDiffer.DatabaseBuilder.GetDatabase(migration.TargetModel);
                var ddlSqlGenerator = DdlSqlGeneratorFactory.Create(sourceDatabase);

                sqlStatements.AddRange(ddlSqlGenerator.Generate(migration.DowngradeOperations));

                sqlStatements.AddRange(
                    HistoryRepository.GenerateDeleteMigrationSql(migration, DmlSqlGenerator));
            }

            foreach (var index in upgradeIndexes)
            {
                var migration = migrations[index];
                var sourceDatabase = ModelDiffer.DatabaseBuilder.GetDatabase(GetSourceModel(migrations, index));
                var ddlSqlGenerator = DdlSqlGeneratorFactory.Create(sourceDatabase);

                sqlStatements.AddRange(ddlSqlGenerator.Generate(migration.UpgradeOperations));

                sqlStatements.AddRange(
                    HistoryRepository.GenerateInsertMigrationSql(migration, DmlSqlGenerator));
            }

            if (historyRepositoryExists && removeHistoryRepository)
            {
                var sourceDatabase = ModelDiffer.DatabaseBuilder.GetDatabase(HistoryRepository.HistoryModel);
                var ddlSqlGenerator = DdlSqlGeneratorFactory.Create(sourceDatabase);

                sqlStatements.AddRange(ddlSqlGenerator.Generate(ModelDiffer.DropSchema(sourceDatabase)));
            }

            return sqlStatements;
        }

        protected virtual IReadOnlyList<MigrationPair> PairMigrations(
            IReadOnlyList<IMigrationMetadata> localMigrations,
            IReadOnlyList<IMigrationMetadata> databaseMigrations)
        {
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

        private static IModel GetSourceModel(IReadOnlyList<IMigrationMetadata> migrations, int index)
        {
            return index == 0 ? new Metadata.Model() : migrations[index - 1].TargetModel;
        }

        protected struct MigrationPair
        {
            public readonly IMigrationMetadata LocalMigration;
            public readonly IMigrationMetadata DatabaseMigration;

            public MigrationPair(
                IMigrationMetadata localMigration,
                IMigrationMetadata databaseMigration)
            {
                LocalMigration = localMigration;
                DatabaseMigration = databaseMigration;
            }
        }
    }
}
