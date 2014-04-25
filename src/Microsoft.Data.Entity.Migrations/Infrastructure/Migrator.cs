// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public class Migrator
    {
        private readonly DbContextConfiguration _contextConfiguration;
        private readonly HistoryRepository _historyRepository;
        private readonly MigrationAssembly _migrationAssembly;
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly IMigrationOperationSqlGeneratorFactory _ddlSqlGeneratorFactory;
        private readonly SqlGenerator _dmlSqlGenerator;
        private readonly SqlStatementExecutor _sqlExecutor;        

        public Migrator(
            [NotNull] DbContextConfiguration contextConfiguration,
            [NotNull] HistoryRepository historyRepository,
            [NotNull] MigrationAssembly migrationAssembly,
            [NotNull] DatabaseBuilder databaseBuilder,
            [NotNull] IMigrationOperationSqlGeneratorFactory ddlSqlGeneratorFactory,
            [NotNull] SqlGenerator dmlSqlGenerator,
            [NotNull] SqlStatementExecutor sqlExecutor)
        {
            Check.NotNull(contextConfiguration, "contextConfiguration");
            Check.NotNull(historyRepository, "historyRepository");
            Check.NotNull(migrationAssembly, "migrationAssembly");
            Check.NotNull(databaseBuilder, "databaseBuilder");
            Check.NotNull(ddlSqlGeneratorFactory, "ddlSqlGeneratorFactory");
            Check.NotNull(dmlSqlGenerator, "dmlSqlGenerator");
            Check.NotNull(sqlExecutor, "sqlExecutor");

            _contextConfiguration = contextConfiguration;
            _historyRepository = historyRepository;
            _migrationAssembly = migrationAssembly;
            _databaseBuilder = databaseBuilder;
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

        protected virtual DatabaseBuilder DatabaseBuilder
        {
            get { return _databaseBuilder; }
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
            return HistoryRepository.Migrations;
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
            var migrationPairs = PairMigrations(GetLocalMigrations(), GetDatabaseMigrations());

            return 
                GenerateUpdateDatabaseSql(
                    new IMigrationMetadata[0],
                    migrationPairs
                        .Where(p => p.DatabaseMigration == null)
                        .Select(p => p.LocalMigration)
                        .ToArray());
        }

        public virtual IReadOnlyList<SqlStatement> GenerateUpdateDatabaseSql([NotNull] string targetMigrationName)
        {
            Check.NotEmpty(targetMigrationName, "targetMigrationName");

            var migrationPairs = PairMigrations(GetLocalMigrations(), GetDatabaseMigrations());

            var index = migrationPairs.IndexOf(p => p.LocalMigration.Name == targetMigrationName);
            if (index < 0)
            {
                throw new InvalidOperationException(Strings.FormatTargetMigrationNotFound(targetMigrationName));
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
                        .ToArray());
        }

        protected virtual IReadOnlyList<SqlStatement> GenerateUpdateDatabaseSql(
            IReadOnlyList<IMigrationMetadata> downgradeMigrations, 
            IReadOnlyList<IMigrationMetadata> upgradeMigrations)
        {
            var sqlStatements = new List<SqlStatement>();

            foreach (var migration in downgradeMigrations)
            {
                var database = DatabaseBuilder.GetDatabase(migration.TargetModel);
                var ddlSqlGenerator = DdlSqlGeneratorFactory.Create(database);

                sqlStatements.AddRange(
                    ddlSqlGenerator.Generate(
                        migration.DowngradeOperations, generateIdempotentSql: true));

                sqlStatements.AddRange(
                    HistoryRepository.GenerateDeleteMigrationSql(migration, DmlSqlGenerator));
            }
            
            foreach (var migration in upgradeMigrations)
            {
                var database = DatabaseBuilder.GetDatabase(migration.TargetModel);
                var ddlSqlGenerator = DdlSqlGeneratorFactory.Create(database);

                sqlStatements.AddRange(
                    ddlSqlGenerator.Generate(
                        migration.UpgradeOperations, generateIdempotentSql: true));

                sqlStatements.AddRange(
                    HistoryRepository.GenerateInsertMigrationSql(migration, DmlSqlGenerator));
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

            while (i < localMigrations.Count && j < databaseMigrations.Count)
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
