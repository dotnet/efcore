// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Design.Utilities;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Design
{
    public abstract class MigrationScaffolder
    {
        private readonly DbContextConfiguration _contextConfiguration;
        private readonly MigrationAssembly _migrationAssembly;
        private readonly ModelDiffer _modelDiffer;
        private readonly MigrationCodeGenerator _migrationCodeGenerator;

        public MigrationScaffolder(
            [NotNull] DbContextConfiguration contextConfiguration,
            [NotNull] MigrationAssembly migrationAssembly,
            [NotNull] ModelDiffer modelDiffer,
            [NotNull] MigrationCodeGenerator migrationCodeGenerator)
        {
            Check.NotNull(contextConfiguration, "contextConfiguration");
            Check.NotNull(migrationAssembly, "migrationAssembly");
            Check.NotNull(modelDiffer, "modelDiffer");
            Check.NotNull(migrationCodeGenerator, "migrationCodeGenerator");

            _contextConfiguration = contextConfiguration;
            _migrationAssembly = migrationAssembly;
            _modelDiffer = modelDiffer;
            _migrationCodeGenerator = migrationCodeGenerator;
        }

        protected virtual DbContextConfiguration ContextConfiguration
        {
            get { return _contextConfiguration; }
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

        public virtual string Namespace
        {
            get { return ContextConfiguration.GetMigrationNamespace(); }
        }

        public virtual void ScaffoldMigration([NotNull] string migrationName)
        {
            Check.NotEmpty(migrationName, "migrationName");

            if (MigrationAssembly.Migrations.Any(m => m.Name == migrationName))
            {
                throw new InvalidOperationException(Strings.FormatDuplicateMigrationName(migrationName));
            }

            var migration = CreateMigration(migrationName);

            ScaffoldMigration(migration);
            ScaffoldModel(migration.TargetModel);
        }

        public virtual void ScaffoldMigration([NotNull] IMigrationMetadata migration)
        {
            Check.NotNull(migration, "migration");

            var className = GetClassName(migration);
            var stringBuilder = new IndentedStringBuilder();
            var metadataStringBuilder = new IndentedStringBuilder();

            MigrationCodeGenerator.GenerateMigrationClass(Namespace, className, migration, stringBuilder);
            MigrationCodeGenerator.GenerateMigrationMetadataClass(Namespace, className, migration, metadataStringBuilder);

            OnMigrationScaffolded(className, stringBuilder.ToString(), metadataStringBuilder.ToString());
        }

        protected virtual IMigrationMetadata CreateMigration([NotNull] string migrationName)
        {
            Check.NotEmpty(migrationName, "migrationName");

            var sourceModel = MigrationAssembly.Model;
            var targetModel = ContextConfiguration.Model;

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
                new MigrationMetadata(migrationName, CreateMigrationTimestamp())
                    {
                        TargetModel = targetModel,
                        UpgradeOperations = upgradeOperations,
                        DowngradeOperations = downgradeOperations
                    };
        }

        protected virtual string CreateMigrationTimestamp()
        {
            return DateTime.UtcNow.ToString("yyyyMMddHHmmssf", CultureInfo.InvariantCulture);
        }

        protected virtual void ScaffoldModel([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            var className = GetClassName(model);
            var stringBuilder = new IndentedStringBuilder();

            MigrationCodeGenerator.ModelCodeGenerator.GenerateModelSnapshotClass(Namespace, className, model, stringBuilder);

            OnModelScaffolded(className, stringBuilder.ToString());
        }

        protected virtual string GetClassName([NotNull] IMigrationMetadata migration)
        {
            Check.NotNull(migration, "migration");

            return migration.Name;
        }

        protected virtual string GetClassName([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            return _contextConfiguration.Context.GetType().Name + "ModelSnapshot";
        }

        protected abstract void OnMigrationScaffolded(string className, string migrationClass, string migrationMetadataClass);

        protected abstract void OnModelScaffolded(string className, string modelSnapshotClass);
    }
}
