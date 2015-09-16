// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Internal;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Strings = Microsoft.Data.Entity.Design.Internal.Strings;

namespace Microsoft.Data.Entity.Migrations.Design
{
    public class MigrationsScaffolder
    {
        private readonly Type _contextType;
        private readonly IModel _model;
        private readonly IMigrationsAssembly _migrationsAssembly;
        private readonly IMigrationsModelDiffer _modelDiffer;
        private readonly IMigrationsIdGenerator _idGenerator;
        private readonly MigrationsCodeGenerator _migrationCodeGenerator;
        private readonly IHistoryRepository _historyRepository;
        private readonly LazyRef<ILogger> _logger;
        private readonly string _activeProvider;

        public MigrationsScaffolder(
            [NotNull] DbContext context,
            [NotNull] IModel model,
            [NotNull] IMigrationsAssembly migrationsAssembly,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsIdGenerator idGenerator,
            [NotNull] MigrationsCodeGenerator migrationCodeGenerator,
            [NotNull] IHistoryRepository historyRepository,
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IDatabaseProviderServices providerServices)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(model, nameof(model));
            Check.NotNull(migrationsAssembly, nameof(migrationsAssembly));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(idGenerator, nameof(idGenerator));
            Check.NotNull(migrationCodeGenerator, nameof(migrationCodeGenerator));
            Check.NotNull(historyRepository, nameof(historyRepository));
            Check.NotNull(loggerFactory, nameof(loggerFactory));
            Check.NotNull(providerServices, nameof(providerServices));

            _contextType = context.GetType();
            _model = model;
            _migrationsAssembly = migrationsAssembly;
            _modelDiffer = modelDiffer;
            _idGenerator = idGenerator;
            _migrationCodeGenerator = migrationCodeGenerator;
            _historyRepository = historyRepository;
            _logger = new LazyRef<ILogger>(loggerFactory.CreateLogger<MigrationsScaffolder>);
            _activeProvider = providerServices.InvariantName;
        }

        public virtual ScaffoldedMigration ScaffoldMigration(
            [NotNull] string migrationName,
            [NotNull] string rootNamespace)
        {
            Check.NotEmpty(migrationName, nameof(migrationName));
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));

            if (_migrationsAssembly.FindMigrationId(migrationName) != null)
            {
                throw new InvalidOperationException(Strings.DuplicateMigrationName(migrationName));
            }

            var lastMigration = _migrationsAssembly.Migrations.LastOrDefault();
            var migrationNamespace = GetNamespace(lastMigration.Value?.AsType(), rootNamespace + ".Migrations");
            var modelSnapshot = _migrationsAssembly.ModelSnapshot;
            var lastModel = modelSnapshot?.Model;
            var upOperations = _modelDiffer.GetDifferences(lastModel, _model);
            var downOperations = upOperations.Any()
                ? _modelDiffer.GetDifferences(_model, lastModel)
                : new List<MigrationOperation>();
            var migrationId = _idGenerator.GenerateId(migrationName);
            var modelSnapshotNamespace = GetNamespace(modelSnapshot?.GetType(), migrationNamespace);

            var modelSnapshotName = _contextType.Name + "ModelSnapshot";
            if (modelSnapshot != null)
            {
                var lastModelSnapshotName = modelSnapshot.GetType().Name;
                if (lastModelSnapshotName != modelSnapshotName)
                {
                    _logger.Value.LogVerbose(Strings.ReusingSnapshotName(lastModelSnapshotName));

                    modelSnapshotName = lastModelSnapshotName;
                }
            }

            if (upOperations.Any(o => o.IsDestructiveChange))
            {
                _logger.Value.LogWarning(Strings.DestructiveOperation);
            }

            var migrationCode = _migrationCodeGenerator.GenerateMigration(
                migrationNamespace,
                migrationName,
                upOperations,
                downOperations);
            var migrationMetadataCode = _migrationCodeGenerator.GenerateMetadata(
                migrationNamespace,
                _contextType,
                migrationName,
                migrationId,
                _model);
            var modelSnapshotCode = _migrationCodeGenerator.GenerateSnapshot(
                modelSnapshotNamespace,
                _contextType,
                modelSnapshotName,
                _model);

            return new ScaffoldedMigration(
                _migrationCodeGenerator.FileExtension,
                lastMigration.Key,
                migrationCode,
                migrationId,
                migrationMetadataCode,
                GetSubnamespace(rootNamespace, migrationNamespace),
                modelSnapshotCode,
                modelSnapshotName,
                GetSubnamespace(rootNamespace, modelSnapshotNamespace));
        }

        protected virtual string GetSubnamespace([NotNull] string rootNamespace, [NotNull] string @namespace) =>
            @namespace == rootNamespace
                ? string.Empty
                : @namespace.StartsWith(rootNamespace + '.')
                    ? @namespace.Substring(rootNamespace.Length + 1)
                    : @namespace;

        // TODO: DRY (file names)
        public virtual MigrationFiles RemoveMigration([NotNull] string projectDir, [NotNull] string rootNamespace)
        {
            Check.NotEmpty(projectDir, nameof(projectDir));
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));

            var files = new MigrationFiles();

            var modelSnapshot = _migrationsAssembly.ModelSnapshot;
            if (modelSnapshot == null)
            {
                throw new InvalidOperationException(Strings.NoSnapshot);
            }

            var language = _migrationCodeGenerator.FileExtension;

            IModel model = null;
            var migrations = _migrationsAssembly.Migrations
                .Select(m => _migrationsAssembly.CreateMigration(m.Value, _activeProvider))
                .ToList();
            if (migrations.Count != 0)
            {
                var migration = migrations[migrations.Count - 1];
                model = migration.TargetModel;

                if (!_modelDiffer.HasDifferences(model, modelSnapshot.Model))
                {
                    if (_historyRepository.GetAppliedMigrations().Any(
                        e => e.MigrationId.Equals(migration.GetId(), StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new InvalidOperationException(Strings.UnapplyMigration(migration.GetId()));
                    }

                    var migrationFileName = migration.GetId() + language;
                    var migrationFile = TryGetProjectFile(projectDir, migrationFileName);
                    if (migrationFile != null)
                    {
                        _logger.Value.LogInformation(Strings.RemovingMigration(migration.GetId()));
                        File.Delete(migrationFile);
                        files.MigrationFile = migrationFile;
                    }
                    else
                    {
                        _logger.Value.LogWarning(Strings.NoMigrationFile(migrationFileName, migration.GetType().FullName));
                    }

                    var migrationMetadataFileName = migration.GetId() + ".Designer" + language;
                    var migrationMetadataFile = TryGetProjectFile(projectDir, migrationMetadataFileName);
                    if (migrationMetadataFile != null)
                    {
                        File.Delete(migrationMetadataFile);
                        files.MetadataFile = migrationMetadataFile;
                    }
                    else
                    {
                        _logger.Value.LogVerbose(Strings.NoMigrationMetadataFile(migrationMetadataFileName));
                    }

                    model = migrations.Count > 1
                        ? migrations[migrations.Count - 2].TargetModel
                        : null;
                }
                else
                {
                    _logger.Value.LogVerbose(Strings.ManuallyDeleted);
                }
            }

            var modelSnapshotName = modelSnapshot.GetType().Name;
            var modelSnapshotFileName = modelSnapshotName + language;
            var modelSnapshotFile = TryGetProjectFile(projectDir, modelSnapshotFileName);
            if (model == null)
            {
                if (modelSnapshotFile != null)
                {
                    _logger.Value.LogInformation(Strings.RemovingSnapshot);
                    File.Delete(modelSnapshotFile);
                    files.SnapshotFile = modelSnapshotFile;
                }
                else
                {
                    _logger.Value.LogWarning(
                        Strings.NoSnapshotFile(modelSnapshotFileName, modelSnapshot.GetType().FullName));
                }
            }
            else
            {
                var modelSnapshotNamespace = modelSnapshot.GetType().Namespace;
                var modelSnapshotCode = _migrationCodeGenerator.GenerateSnapshot(
                    modelSnapshotNamespace,
                    _contextType,
                    modelSnapshotName,
                    model);

                if (modelSnapshotFile == null)
                {
                    modelSnapshotFile = Path.Combine(
                        GetDirectory(projectDir, null, GetSubnamespace(rootNamespace, modelSnapshotNamespace)),
                        modelSnapshotFileName);
                }

                _logger.Value.LogInformation(Strings.RevertingSnapshot);
                File.WriteAllText(modelSnapshotFile, modelSnapshotCode);
            }

            return files;
        }

        public virtual MigrationFiles Save([NotNull] string projectDir, [NotNull] ScaffoldedMigration migration)
        {
            Check.NotEmpty(projectDir, nameof(projectDir));
            Check.NotNull(migration, nameof(migration));

            var lastMigrationFileName = migration.PreviousMigrationId + migration.FileExtension;
            var migrationDirectory = GetDirectory(projectDir, lastMigrationFileName, migration.MigrationSubNamespace);
            var migrationFile = Path.Combine(migrationDirectory, migration.MigrationId + migration.FileExtension);
            var migrationMetadataFile = Path.Combine(migrationDirectory, migration.MigrationId + ".Designer" + migration.FileExtension);
            var modelSnapshotFileName = migration.SnapshotName + migration.FileExtension;
            var modelSnapshotDirectory = GetDirectory(projectDir, modelSnapshotFileName, migration.SnapshotSubnamespace);
            var modelSnapshotFile = Path.Combine(modelSnapshotDirectory, modelSnapshotFileName);

            _logger.Value.LogVerbose(Strings.WritingMigration(migrationFile));
            Directory.CreateDirectory(migrationDirectory);
            File.WriteAllText(migrationFile, migration.MigrationCode);
            File.WriteAllText(migrationMetadataFile, migration.MetadataCode);

            _logger.Value.LogVerbose(Strings.WritingSnapshot(modelSnapshotFile));
            Directory.CreateDirectory(modelSnapshotDirectory);
            File.WriteAllText(modelSnapshotFile, migration.SnapshotCode);

            return new MigrationFiles
            {
                MigrationFile = migrationFile,
                MetadataFile = migrationMetadataFile,
                SnapshotFile = modelSnapshotFile
            };
        }

        protected virtual string GetNamespace([CanBeNull] Type siblingType, [NotNull] string defaultNamespace)
        {
            if (siblingType != null)
            {
                var lastNamespace = siblingType.Namespace;
                if (lastNamespace != defaultNamespace)
                {
                    _logger.Value.LogVerbose(Strings.ReusingNamespace(siblingType.Name));

                    return lastNamespace;
                }
            }

            return defaultNamespace;
        }

        protected virtual string GetDirectory(
            [NotNull] string projectDir,
            [CanBeNull] string siblingFileName,
            [NotNull] string subnamespace)
        {
            Check.NotEmpty(projectDir, nameof(projectDir));
            Check.NotNull(subnamespace, nameof(subnamespace));

            var defaultDirectory = Path.Combine(projectDir, Path.Combine(subnamespace.Split('.')));

            if (siblingFileName != null)
            {
                var siblingPath = TryGetProjectFile(projectDir, siblingFileName);
                if (siblingPath != null)
                {
                    var lastDirectory = Path.GetDirectoryName(siblingPath);
                    if (!defaultDirectory.Equals(lastDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.Value.LogVerbose(Strings.ReusingDirectory(siblingFileName));

                        return lastDirectory;
                    }
                }
            }

            return defaultDirectory;
        }

        protected virtual string TryGetProjectFile([NotNull] string projectDir, [NotNull] string fileName) =>
            Directory.EnumerateFiles(projectDir, fileName, SearchOption.AllDirectories).FirstOrDefault();
    }
}
