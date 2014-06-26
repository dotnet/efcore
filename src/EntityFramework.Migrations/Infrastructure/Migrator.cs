// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;

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

        protected virtual IReadOnlyList<IMigrationMetadata> GetDatabaseMigrations(out bool historyRepositoryExists)
        {
            IReadOnlyList<IMigrationMetadata> migrations;
            try
            {
                migrations = HistoryRepository.Migrations;
                historyRepositoryExists = true;
            }
            catch (DbException)
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
            var dbConnection = ((RelationalConnection)ContextConfiguration.Connection).DbConnection;

            SqlExecutor.ExecuteNonQuery(dbConnection, sqlStatements);
        }

        public virtual IReadOnlyList<SqlStatement> GenerateUpdateDatabaseSql()
        {
            bool historyRepositoryExists;
            var migrationPairs = PairMigrations(GetLocalMigrations(), GetDatabaseMigrations(out historyRepositoryExists));

            return                
                GenerateUpdateDatabaseSql(
                    new IMigrationMetadata[0],
                    migrationPairs
                        .Where(p => p.DatabaseMigration == null)
                        .Select(p => p.LocalMigration)
                        .ToArray(),
                    historyRepositoryExists,
                    removeHistoryRepository: false);
        }

        public virtual IReadOnlyList<SqlStatement> GenerateUpdateDatabaseSql([NotNull] string targetMigrationName)
        {
            Check.NotEmpty(targetMigrationName, "targetMigrationName");

            bool historyRepositoryExists;
            var removeHistoryRepository = false;

            var migrationPairs = PairMigrations(GetLocalMigrations(), GetDatabaseMigrations(out historyRepositoryExists));

            int index;
            if (targetMigrationName == InitialDatabase)
            {
                index = -1;
                removeHistoryRepository = true;
            }
            else
            {
                index = migrationPairs.IndexOf(p => p.LocalMigration.Name == targetMigrationName);
                if (index < 0)
                {
                    throw new InvalidOperationException(Strings.FormatTargetMigrationNotFound(targetMigrationName));
                }
            }

            return
                GenerateUpdateDatabaseSql(                    
                    migrationPairs
                        .Skip(index + 1)
                        .Where(p => p.DatabaseMigration != null)
                        .Select(p => p.LocalMigration)
                        .Reverse()
                        .ToArray(),
                    migrationPairs
                        .Take(index + 1)
                        .Where(p => p.DatabaseMigration == null)
                        .Select(p => p.LocalMigration)
                        .ToArray(),
                    historyRepositoryExists,
                    removeHistoryRepository);
        }

        protected virtual IReadOnlyList<SqlStatement> GenerateUpdateDatabaseSql(
            IReadOnlyList<IMigrationMetadata> downgradeMigrations,
            IReadOnlyList<IMigrationMetadata> upgradeMigrations,
            bool historyRepositoryExists,
            bool removeHistoryRepository)
        {
            var sqlStatements = new List<SqlStatement>();

            if (!historyRepositoryExists && upgradeMigrations.Count > 0)
            {
                var database = ModelDiffer.DatabaseBuilder.GetDatabase(HistoryRepository.HistoryModel);
                var ddlSqlGenerator = DdlSqlGeneratorFactory.Create(database);

                sqlStatements.AddRange(ddlSqlGenerator.Generate(ModelDiffer.CreateSchema(database)));
            }

            foreach (var migration in downgradeMigrations)
            {
                var database = ModelDiffer.DatabaseBuilder.GetDatabase(migration.TargetModel);
                var ddlSqlGenerator = DdlSqlGeneratorFactory.Create(database);

                sqlStatements.AddRange(ddlSqlGenerator.Generate(migration.DowngradeOperations));

                sqlStatements.AddRange(
                    HistoryRepository.GenerateDeleteMigrationSql(migration, DmlSqlGenerator));
            }

            foreach (var migration in upgradeMigrations)
            {
                var database = ModelDiffer.DatabaseBuilder.GetDatabase(migration.TargetModel);
                var ddlSqlGenerator = DdlSqlGeneratorFactory.Create(database);

                sqlStatements.AddRange(ddlSqlGenerator.Generate(migration.UpgradeOperations));

                sqlStatements.AddRange(
                    HistoryRepository.GenerateInsertMigrationSql(migration, DmlSqlGenerator));
            }

            if (historyRepositoryExists && removeHistoryRepository)
            {
                var database = ModelDiffer.DatabaseBuilder.GetDatabase(HistoryRepository.HistoryModel);
                var ddlSqlGenerator = DdlSqlGeneratorFactory.Create(new DatabaseModel());

                sqlStatements.AddRange(ddlSqlGenerator.Generate(ModelDiffer.DropSchema(database)));
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
                var compareResult = MigrationMetadataComparer.Instance.Compare(
                    localMigrations[i], databaseMigrations[j]);

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
                    databaseMigrations[j].Name, databaseMigrations[j].Timestamp));
            }

            while (i < localMigrations.Count)
            {
                pairs.Add(new MigrationPair(localMigrations[i++], null));
            }

            return pairs;
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
