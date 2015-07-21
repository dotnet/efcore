// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Sql;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;
using Strings = Microsoft.Data.Entity.Relational.Internal.Strings;

namespace Microsoft.Data.Entity.Migrations.History
{
    // TODO: Leverage query pipeline for GetAppliedMigrations
    // TODO: Leverage update pipeline for GetInsertScript & GetDeleteScript
    public abstract class HistoryRepository : IHistoryRepository
    {
        public const string DefaultTableName = "__MigrationHistory";

        private readonly IRelationalDatabaseCreator _databaseCreator;
        private readonly ISqlStatementExecutor _executor;
        private readonly IRelationalConnection _connection;
        private readonly IDbContextOptions _options;
        private readonly IModelDiffer _modelDiffer;
        private readonly IMigrationSqlGenerator _migrationSqlGenerator;
        private readonly IUpdateSqlGenerator _updateSqlGenerator;
        private readonly IServiceProvider _serviceProvider;
        private readonly LazyRef<IModel> _model;
        private readonly LazyRef<string> _migrationIdColumnName;
        private readonly LazyRef<string> _productVersionColumnName;

        public HistoryRepository(
            [NotNull] IDatabaseCreator databaseCreator,
            [NotNull] ISqlStatementExecutor executor,
            [NotNull] IRelationalConnection connection,
            [NotNull] IMigrationModelFactory modelFactory,
            [NotNull] IDbContextOptions options,
            [NotNull] IModelDiffer modelDiffer,
            [NotNull] IMigrationSqlGenerator migrationSqlGenerator,
            [NotNull] IRelationalMetadataExtensionProvider annotations,
            [NotNull] IUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(databaseCreator, nameof(databaseCreator));
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(modelFactory, nameof(modelFactory));
            Check.NotNull(options, nameof(options));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(migrationSqlGenerator, nameof(migrationSqlGenerator));
            Check.NotNull(annotations, nameof(annotations));
            Check.NotNull(updateSqlGenerator, nameof(updateSqlGenerator));
            Check.NotNull(serviceProvider, nameof(serviceProvider));

            _databaseCreator = (IRelationalDatabaseCreator)databaseCreator;
            _executor = executor;
            _connection = connection;
            _options = options;
            _modelDiffer = modelDiffer;
            _migrationSqlGenerator = migrationSqlGenerator;
            _updateSqlGenerator = updateSqlGenerator;
            _serviceProvider = serviceProvider;

            var relationalOptions = RelationalOptionsExtension.Extract(options);
            TableName = relationalOptions?.MigrationsHistoryTableName ?? DefaultTableName;
            TableSchema = relationalOptions.MigrationsHistoryTableSchema;
            _model = new LazyRef<IModel>(
                () => modelFactory.Create(
                    mb => mb.Entity<HistoryRow>(
                        x =>
                        {
                            ConfigureTable(x);
                            x.ToTable(TableName, TableSchema);
                        })));
            var entityType = new LazyRef<IEntityType>(() => _model.Value.GetEntityType(typeof(HistoryRow)));
            _migrationIdColumnName = new LazyRef<string>(
                () => annotations.For(entityType.Value.FindProperty(nameof(HistoryRow.MigrationId))).ColumnName);
            _productVersionColumnName = new LazyRef<string>(
                () => annotations.For(entityType.Value.FindProperty(nameof(HistoryRow.ProductVersion))).ColumnName);
        }

        protected virtual string TableName { get; }
        protected virtual string TableSchema { get; }
        protected virtual string MigrationIdColumnName => _migrationIdColumnName.Value;
        protected virtual string ProductVersionColumnName => _productVersionColumnName.Value;

        protected abstract string ExistsSql { get; }

        public virtual bool Exists()
            => _databaseCreator.Exists() && Exists(_executor.ExecuteScalar(_connection, null, ExistsSql));

        protected abstract bool Exists(object value);

        public abstract string GetCreateIfNotExistsScript();

        public virtual string GetCreateScript()
        {
            var operations = _modelDiffer.GetDifferences(null, _model.Value);
            var batches = _migrationSqlGenerator.Generate(operations);
            if (batches.Count != 1 || batches[0].SuppressTransaction)
            {
                throw new InvalidOperationException(Strings.InvalidCreateScript);
            }

            return batches[0].Sql;
        }

        protected virtual void ConfigureTable(EntityTypeBuilder<HistoryRow> history)
        {
            history.ToTable(DefaultTableName);
            history.Key(h => h.MigrationId);
            history.Property(h => h.MigrationId).MaxLength(150);
            history.Property(h => h.ProductVersion).MaxLength(32);
        }

        public virtual IReadOnlyList<HistoryRow> GetAppliedMigrations()
        {
            var rows = new List<HistoryRow>();

            if (Exists())
            {
                _connection.Open();
                try
                {
                    using (var reader = _executor.ExecuteReader(_connection, null, GetAppliedMigrationsSql))
                    {
                        while (reader.Read())
                        {
                            rows.Add(new HistoryRow(reader.GetString(0), reader.GetString(1)));
                        }
                    }
                }
                finally
                {
                    _connection.Close();
                }
            }

            return rows;
        }

        protected virtual string GetAppliedMigrationsSql
            => new StringBuilder()
                .Append("SELECT ")
                .Append(_updateSqlGenerator.DelimitIdentifier(MigrationIdColumnName))
                .Append(", ")
                .AppendLine(_updateSqlGenerator.DelimitIdentifier(ProductVersionColumnName))
                .Append("FROM ")
                .AppendLine(_updateSqlGenerator.DelimitIdentifier(TableName, TableSchema))
                .Append("ORDER BY ")
                .Append(_updateSqlGenerator.DelimitIdentifier(MigrationIdColumnName))
                .Append(";")
                .ToString();

        public virtual string GetInsertScript([NotNull] HistoryRow row)
        {
            Check.NotNull(row, nameof(row));

            return new StringBuilder().Append("INSERT INTO ")
                .Append(_updateSqlGenerator.DelimitIdentifier(TableName, TableSchema))
                .Append(" (")
                .Append(_updateSqlGenerator.DelimitIdentifier(MigrationIdColumnName))
                .Append(", ")
                .Append(_updateSqlGenerator.DelimitIdentifier(ProductVersionColumnName))
                .AppendLine(")")
                .Append("VALUES ('")
                .Append(_updateSqlGenerator.EscapeLiteral(row.MigrationId))
                .Append("', '")
                .Append(_updateSqlGenerator.EscapeLiteral(row.ProductVersion))
                .Append("');")
                .ToString();
        }

        public virtual string GetDeleteScript([NotNull] string migrationId)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));

            return new StringBuilder().Append("DELETE FROM ")
                .AppendLine(_updateSqlGenerator.DelimitIdentifier(TableName, TableSchema))
                .Append("WHERE ")
                .Append(_updateSqlGenerator.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = '")
                .Append(_updateSqlGenerator.EscapeLiteral(migrationId))
                .Append("';")
                .ToString();
        }

        public abstract string GetBeginIfNotExistsScript(string migrationId);
        public abstract string GetBeginIfExistsScript(string migrationId);
        public abstract string GetEndIfScript();
    }
}
