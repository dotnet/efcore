// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations
{
    // TODO: Log
    public class Migrator
    {
        private const string InitialDatabase = "0";

        private readonly MigrationAssembly _migrationAssembly;
        private readonly IHistoryRepository _historyRepository;
        private readonly IRelationalDataStoreCreator _dataStoreCreator;
        private readonly IMigrationSqlGenerator _migrationSqlGenerator;
        private readonly SqlStatementExecutor _executor;
        private readonly IRelationalConnection _connection;
        private readonly IModelDiffer _modelDiffer;
        private readonly IModel _model;
        private readonly MigrationIdGenerator _idGenerator;
        private readonly ISqlGenerator _sqlGenerator;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected Migrator()
        {
        }

        public Migrator(
            [NotNull] MigrationAssembly migrationAssembly,
            [NotNull] IHistoryRepository historyRepository,
            [NotNull] IDataStoreCreator dataStoreCreator,
            [NotNull] IMigrationSqlGenerator migrationSqlGenerator,
            [NotNull] SqlStatementExecutor executor,
            [NotNull] IRelationalConnection connection,
            [NotNull] IModelDiffer modelDiffer,
            [NotNull] IModel model,
            [NotNull] MigrationIdGenerator idGenerator,
            [NotNull] ISqlGenerator sqlGenerator)
        {
            Check.NotNull(migrationAssembly, nameof(migrationAssembly));
            Check.NotNull(historyRepository, nameof(historyRepository));
            Check.NotNull(dataStoreCreator, nameof(dataStoreCreator));
            Check.NotNull(migrationSqlGenerator, nameof(migrationSqlGenerator));
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(model, nameof(model));
            Check.NotNull(idGenerator, nameof(idGenerator));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            _migrationAssembly = migrationAssembly;
            _historyRepository = historyRepository;
            _dataStoreCreator = (IRelationalDataStoreCreator)dataStoreCreator;
            _migrationSqlGenerator = migrationSqlGenerator;
            _executor = executor;
            _connection = connection;
            _modelDiffer = modelDiffer;
            _model = model;
            _idGenerator = idGenerator;
            _sqlGenerator = sqlGenerator;
        }

        protected virtual string ProductVersion =>
            typeof(Migrator).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

        public virtual IReadOnlyList<Migration> GetUnappliedMigrations()
        {
            var appliedMigrations = _historyRepository.GetAppliedMigrations();

            return _migrationAssembly.Migrations.Where(
                m => !appliedMigrations.Any(
                    e => string.Equals(e.MigrationId, m.Id, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public virtual bool HasPendingModelChanges() =>
            _modelDiffer.HasDifferences(_migrationAssembly.ModelSnapshot?.Model, _model);

        public virtual void ApplyMigrations([CanBeNull] string targetMigration = null)
        {
            var migrations = _migrationAssembly.Migrations;
            var appliedMigrationEntries = _historyRepository.GetAppliedMigrations();

            var appliedMigrations = new List<Migration>();
            var unappliedMigrations = new List<Migration>();
            foreach (var migraion in migrations)
            {
                if (appliedMigrationEntries.Any(
                    e => string.Equals(e.MigrationId, migraion.Id, StringComparison.OrdinalIgnoreCase)))
                {
                    appliedMigrations.Add(migraion);
                }
                else
                {
                    unappliedMigrations.Add(migraion);
                }
            }

            IEnumerable<Migration> migrationsToApply;
            IEnumerable<Migration> migrationsToRevert;
            if (string.IsNullOrEmpty(targetMigration))
            {
                migrationsToApply = unappliedMigrations;
                migrationsToRevert = Enumerable.Empty<Migration>();
            }
            else if (targetMigration == InitialDatabase)
            {
                migrationsToApply = Enumerable.Empty<Migration>();
                migrationsToRevert = appliedMigrations;
            }
            else
            {
                targetMigration = _idGenerator.ResolveId(targetMigration, migrations);
                migrationsToApply = unappliedMigrations
                    .Where(m => string.Compare(m.Id, targetMigration, StringComparison.OrdinalIgnoreCase) <= 0);
                migrationsToRevert = appliedMigrations
                    .Where(m => string.Compare(m.Id, targetMigration, StringComparison.OrdinalIgnoreCase) > 0)
                    .OrderByDescending(m => m.Id);
            }

            bool first;
            var checkFirst = true;
            foreach (var migration in migrationsToApply)
            {
                var batches = ApplyMigration(migration).ToList();

                first = false;
                if (checkFirst)
                {
                    first = migration == migrations[0];
                    if (first && !_historyRepository.Exists())
                    {
                        // TODO: Consider removing check above and always using "if not exists"
                        batches.Insert(0, new SqlBatch(_historyRepository.Create(ifNotExists: false)));
                    }

                    checkFirst = false;
                }

                Execute(batches, first);
            }

            foreach (var migration in migrationsToRevert)
            {
                Execute(RevertMigration(migration));
            }
        }

        public virtual string ScriptMigrations(
            [CanBeNull] string fromMigrationName,
            [CanBeNull] string toMigrationName,
            bool idempotent = false)
        {
            var migrations = _migrationAssembly.Migrations;

            if (string.IsNullOrEmpty(fromMigrationName))
            {
                fromMigrationName = InitialDatabase;
            }
            else if (fromMigrationName != InitialDatabase)
            {
                fromMigrationName = _idGenerator.ResolveId(fromMigrationName, migrations);
            }

            if (string.IsNullOrEmpty(toMigrationName))
            {
                toMigrationName = migrations.Last().Id;
            }
            else if (toMigrationName != InitialDatabase)
            {
                toMigrationName = _idGenerator.ResolveId(toMigrationName, migrations);
            }

            var builder = new IndentedStringBuilder();

            // If going up...
            if (string.Compare(fromMigrationName, toMigrationName, StringComparison.OrdinalIgnoreCase) <= 0)
            {
                var migrationsToApply = migrations.Where(
                    m => string.Compare(m.Id, fromMigrationName, StringComparison.OrdinalIgnoreCase) > 0
                        && string.Compare(m.Id, toMigrationName, StringComparison.OrdinalIgnoreCase) <= 0);
                var checkFirst = true;
                foreach (var migration in migrationsToApply)
                {
                    if (checkFirst)
                    {
                        if (migration == migrations[0])
                        {
                            builder.AppendLine(_historyRepository.Create(ifNotExists: true));
                            builder.AppendLine(_sqlGenerator.BatchSeparator);
                            builder.AppendLine();
                        }

                        checkFirst = false;
                    }

                    foreach (var batch in ApplyMigration(migration))
                    {
                        if (idempotent)
                        {
                            builder.AppendLine(_historyRepository.BeginIfNotExists(migration.Id));
                            using (builder.Indent())
                            {
                                var lines = batch.Sql.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                                foreach (var line in lines)
                                {
                                    builder.AppendLine(line);
                                }
                            }
                            builder.AppendLine(_historyRepository.EndIf());
                        }
                        else
                        {
                            builder.Append(batch.Sql);
                        }

                        builder.AppendLine(_sqlGenerator.BatchSeparator);
                        builder.AppendLine();
                    }
                }
            }
            else // If going down...
            {
                var migrationsToRevert = migrations
                    .Where(
                        m => string.Compare(m.Id, toMigrationName, StringComparison.OrdinalIgnoreCase) > 0
                            && string.Compare(m.Id, fromMigrationName, StringComparison.OrdinalIgnoreCase) <= 0)
                    .OrderByDescending(m => m.Id);
                foreach (var migration in migrationsToRevert)
                {
                    foreach (var batch in RevertMigration(migration))
                    {
                        if (idempotent)
                        {
                            builder.AppendLine(_historyRepository.BeginIfExists(migration.Id));
                            using (builder.Indent())
                            {
                                var lines = batch.Sql.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                                foreach (var line in lines)
                                {
                                    builder.AppendLine(line);
                                }
                            }
                            builder.AppendLine(_historyRepository.EndIf());
                        }
                        else
                        {
                            builder.Append(batch.Sql);
                        }

                        builder.AppendLine(_sqlGenerator.BatchSeparator);
                        builder.AppendLine();
                    }
                }
            }

            return builder.ToString();
        }

        protected virtual IReadOnlyList<SqlBatch> ApplyMigration([NotNull] Migration migration)
        {
            Check.NotNull(migration, nameof(migration));

            var migrationBuilder = new MigrationBuilder();
            migration.Up(migrationBuilder);

            var operations = migrationBuilder.Operations.ToList();
            operations.Add(_historyRepository.GetInsertOperation(new HistoryRow(migration.Id, ProductVersion)));

            return _migrationSqlGenerator.Generate(operations, migration.Target);
        }

        protected virtual IReadOnlyList<SqlBatch> RevertMigration([NotNull] Migration migration)
        {
            Check.NotNull(migration, nameof(migration));

            var migrationBuilder = new MigrationBuilder();
            migration.Down(migrationBuilder);
            var operations = migrationBuilder.Operations.ToList();

            operations.Add(_historyRepository.GetDeleteOperation(migration.Id));

            // TODO: Pass source model?
            return _migrationSqlGenerator.Generate(operations);
        }

        protected virtual void Execute([NotNull] IEnumerable<SqlBatch> sqlBatches, bool ensureDatabase = false)
        {
            Check.NotNull(sqlBatches, nameof(sqlBatches));

            if (ensureDatabase && !_dataStoreCreator.Exists())
            {
                _dataStoreCreator.Create();
            }

            using (var transaction = _connection.BeginTransaction())
            {
                _executor.ExecuteNonQuery(_connection, transaction.DbTransaction, sqlBatches);
                transaction.Commit();
            }
        }
    }
}
