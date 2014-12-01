// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Commands.Migrations
{
    public class MigrationScaffolder
    {
        private readonly DbContext _context;
        private readonly IDbContextOptions _options;
        private readonly IModel _model;
        private readonly MigrationAssembly _migrationAssembly;
        private readonly ModelDiffer _modelDiffer;
        private readonly MigrationCodeGenerator _migrationCodeGenerator;

        public MigrationScaffolder(
            [NotNull] DbContext context,
            [NotNull] IDbContextOptions options,
            [NotNull] IModel model,
            [NotNull] MigrationAssembly migrationAssembly,
            [NotNull] ModelDiffer modelDiffer,
            [NotNull] MigrationCodeGenerator migrationCodeGenerator)
        {
            Check.NotNull(context, "context");
            Check.NotNull(options, "options");
            Check.NotNull(model, "model");
            Check.NotNull(migrationAssembly, "migrationAssembly");
            Check.NotNull(modelDiffer, "modelDiffer");
            Check.NotNull(migrationCodeGenerator, "migrationCodeGenerator");

            _context = context;
            _options = options;
            _model = model;
            _migrationAssembly = migrationAssembly;
            _modelDiffer = modelDiffer;
            _migrationCodeGenerator = migrationCodeGenerator;
        }

        protected MigrationAssembly MigrationAssembly
        {
            get { return _migrationAssembly; }
        }

        protected ModelDiffer ModelDiffer
        {
            get { return _modelDiffer; }
        }

        protected virtual MigrationCodeGenerator MigrationCodeGenerator
        {
            get { return _migrationCodeGenerator; }
        }

        public virtual ScaffoldedMigration ScaffoldMigration(
            [NotNull] string migrationName,
            [NotNull] string rootNamespace)
        {
            Check.NotEmpty(migrationName, "migrationName");

            var existingMigrations = MigrationAssembly.Migrations.OrderBy(m => m.GetMigrationId()).ToList();

            if (existingMigrations.Any(m => m.GetMigrationName() == migrationName))
            {
                throw new InvalidOperationException(Strings.DuplicateMigrationName(migrationName));
            }

            var migrationNamespace = existingMigrations.Any()
                ? existingMigrations.Last().GetType().Namespace
                : rootNamespace + ".Migrations";
            var modelSnapshotNamespace = MigrationAssembly.ModelSnapshot?.GetType().Namespace
                ?? migrationNamespace;
            var migration = CreateMigration(migrationName);
            var contextType = _context.GetType();

            var migrationCode = new IndentedStringBuilder();
            var migrationMetadataCode = new IndentedStringBuilder();
            var snapshotModelCode = new IndentedStringBuilder();

            ScaffoldMigration(migrationNamespace, migration, contextType, migrationCode, migrationMetadataCode);
            ScaffoldSnapshotModel(modelSnapshotNamespace, migration.TargetModel, contextType, snapshotModelCode);

            return
                new ScaffoldedMigration(migration.MigrationId)
                {
                    Directory = GetDirectory(rootNamespace, migrationNamespace),
                    ModelSnapshotDirectory = GetDirectory(rootNamespace, modelSnapshotNamespace),
                    SnapshotModelClass = GetClassName(migration.TargetModel),
                    Language = _migrationCodeGenerator.Language,
                    MigrationCode = migrationCode.ToString(),
                    MigrationMetadataCode = migrationMetadataCode.ToString(),
                    SnapshotModelCode = snapshotModelCode.ToString()
                };
        }

        protected virtual MigrationInfo CreateMigration([NotNull] string migrationName)
        {
            Check.NotEmpty(migrationName, "migrationName");

            var sourceModel = MigrationAssembly.ModelSnapshot?.Model;
            var targetModel = _model;

            IReadOnlyList<MigrationOperation> upgradeOperations, downgradeOperations;
            if (sourceModel != null)
            {
                upgradeOperations = ModelDiffer.Diff(sourceModel, targetModel);
                downgradeOperations = ModelDiffer.Diff(targetModel, sourceModel);
            }
            else
            {
                upgradeOperations = ModelDiffer.CreateSchema(targetModel);
                downgradeOperations = ModelDiffer.DropSchema(targetModel);
            }

            return
                new MigrationInfo(CreateMigrationId(migrationName))
                {
                    TargetModel = targetModel,
                    UpgradeOperations = upgradeOperations,
                    DowngradeOperations = downgradeOperations
                };
        }

        protected virtual string CreateMigrationId(string migrationName)
        {
            return MigrationMetadataExtensions.CreateMigrationId(migrationName);
        }

        protected virtual void ScaffoldMigration(
            [NotNull] string migrationNamespace,
            [NotNull] MigrationInfo migration,
            [NotNull] Type contextType,
            [NotNull] IndentedStringBuilder migrationCode,
            [NotNull] IndentedStringBuilder migrationMetadataCode)
        {
            Check.NotEmpty(migrationNamespace, "migrationNamespace");
            Check.NotNull(migration, "migration");
            Check.NotNull(migrationCode, "migrationCode");
            Check.NotNull(migrationMetadataCode, "migrationMetadataCode");

            var className = GetClassName(migration);

            MigrationCodeGenerator.GenerateMigrationClass(migrationNamespace, className, migration, migrationCode);
            MigrationCodeGenerator.GenerateMigrationMetadataClass(migrationNamespace, className, migration, contextType, migrationMetadataCode);
        }

        protected virtual void ScaffoldSnapshotModel(
            [NotNull] string modelSnapshotNamespace,
            [NotNull] IModel model,
            [NotNull] Type contextType,
            [NotNull] IndentedStringBuilder snapshotModelCode)
        {
            Check.NotEmpty(modelSnapshotNamespace, "modelSnapshotNamespace");
            Check.NotNull(model, "model");
            Check.NotNull(contextType, "contextType");
            Check.NotNull(snapshotModelCode, "snapshotModelCode");

            var className = GetClassName(model);

            MigrationCodeGenerator.ModelCodeGenerator.GenerateModelSnapshotClass(modelSnapshotNamespace, className, model, contextType, snapshotModelCode);
        }

        protected virtual string GetClassName([NotNull] MigrationInfo migration)
        {
            Check.NotNull(migration, "migration");

            // TODO: Generate valid C# class name from migration name.
            return migration.GetMigrationName();
        }

        protected virtual string GetClassName([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            return _context.GetType().Name + "ModelSnapshot";
        }

        // Internal for testing
        internal static string GetDirectory(string rootNamespace, string migrationNamespace)
        {
            var directory = migrationNamespace.StartsWith(rootNamespace + '.')
                ? migrationNamespace.Substring(rootNamespace.Length + 1)
                : migrationNamespace;

            return directory.Replace('.', Path.DirectorySeparatorChar);
        }
    }
}
