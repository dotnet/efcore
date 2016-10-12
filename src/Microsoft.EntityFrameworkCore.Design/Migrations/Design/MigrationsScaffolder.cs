// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
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
        private readonly ILogger _logger;
        private readonly string _activeProvider;

        public MigrationsScaffolder(
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IModel model,
            [NotNull] IMigrationsAssembly migrationsAssembly,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsIdGenerator idGenerator,
            [NotNull] MigrationsCodeGenerator migrationCodeGenerator,
            [NotNull] IHistoryRepository historyRepository,
            [NotNull] ILogger<MigrationsScaffolder> logger,
            [NotNull] IDatabaseProviderServices providerServices)
        {
            Check.NotNull(currentContext, nameof(currentContext));
            Check.NotNull(model, nameof(model));
            Check.NotNull(migrationsAssembly, nameof(migrationsAssembly));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(idGenerator, nameof(idGenerator));
            Check.NotNull(migrationCodeGenerator, nameof(migrationCodeGenerator));
            Check.NotNull(historyRepository, nameof(historyRepository));
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(providerServices, nameof(providerServices));

            _contextType = currentContext.Context.GetType();
            _model = model;
            _migrationsAssembly = migrationsAssembly;
            _modelDiffer = modelDiffer;
            _idGenerator = idGenerator;
            _migrationCodeGenerator = migrationCodeGenerator;
            _historyRepository = historyRepository;
            _logger = logger;
            _activeProvider = providerServices.InvariantName;
        }

        public virtual ScaffoldedMigration ScaffoldMigration(
            [NotNull] string migrationName,
            [NotNull] string rootNamespace,
            [CanBeNull] string subNamespace = null)
        {
            Check.NotEmpty(migrationName, nameof(migrationName));
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));

            if (_migrationsAssembly.FindMigrationId(migrationName) != null)
            {
                throw new OperationException(DesignStrings.DuplicateMigrationName(migrationName));
            }

            var subNamespaceDefaulted = false;
            if (string.IsNullOrEmpty(subNamespace))
            {
                subNamespaceDefaulted = true;
                subNamespace = "Migrations";
            }

            var lastMigration = _migrationsAssembly.Migrations.LastOrDefault();

            var migrationNamespace = rootNamespace + "." + subNamespace;
            if (subNamespaceDefaulted)
            {
                migrationNamespace = GetNamespace(lastMigration.Value?.AsType(), migrationNamespace);
            }

            var sanitizedContextName = _contextType.Name;
            var genericMarkIndex = sanitizedContextName.IndexOf('`');
            if (genericMarkIndex != -1)
            {
                sanitizedContextName = sanitizedContextName.Substring(0, genericMarkIndex);
            }

            if (ContainsForeignMigrations(migrationNamespace))
            {
                if (subNamespaceDefaulted)
                {
                    var builder = new StringBuilder()
                        .Append(rootNamespace)
                        .Append(".Migrations.");

                    if (sanitizedContextName.EndsWith("Context", StringComparison.Ordinal))
                    {
                        builder.Append(sanitizedContextName.Substring(0, sanitizedContextName.Length - 7));
                    }
                    else
                    {
                        builder
                            .Append(sanitizedContextName)
                            .Append("Migrations");
                    }

                    migrationNamespace = builder.ToString();
                }
                else
                {
                    _logger.LogWarning(
                        DesignEventId.ForeignMigrations,
                        () => DesignStrings.ForeignMigrations(migrationNamespace));
                }
            }

            var modelSnapshot = _migrationsAssembly.ModelSnapshot;
            var lastModel = modelSnapshot?.Model;
            var upOperations = _modelDiffer.GetDifferences(lastModel, _model);
            var downOperations = upOperations.Any()
                ? _modelDiffer.GetDifferences(_model, lastModel)
                : new List<MigrationOperation>();
            var migrationId = _idGenerator.GenerateId(migrationName);
            var modelSnapshotNamespace = GetNamespace(modelSnapshot?.GetType(), migrationNamespace);

            var modelSnapshotName = sanitizedContextName + "ModelSnapshot";
            if (modelSnapshot != null)
            {
                var lastModelSnapshotName = modelSnapshot.GetType().Name;
                if (lastModelSnapshotName != modelSnapshotName)
                {
                    _logger.LogDebug(
                        DesignEventId.ReusingSnapshotName,
                        () => DesignStrings.ReusingSnapshotName(lastModelSnapshotName));

                    modelSnapshotName = lastModelSnapshotName;
                }
            }

            if (upOperations.Any(o => o.IsDestructiveChange))
            {
                _logger.LogWarning(
                    DesignEventId.DestructiveOperation,
                    () => DesignStrings.DestructiveOperation);
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
                GetSubNamespace(rootNamespace, migrationNamespace),
                modelSnapshotCode,
                modelSnapshotName,
                GetSubNamespace(rootNamespace, modelSnapshotNamespace));
        }

        protected virtual string GetSubNamespace([NotNull] string rootNamespace, [NotNull] string @namespace) =>
            @namespace == rootNamespace
                ? string.Empty
                : @namespace.StartsWith(rootNamespace + '.', StringComparison.Ordinal)
                    ? @namespace.Substring(rootNamespace.Length + 1)
                    : @namespace;

        // TODO: DRY (file names)
        public virtual MigrationFiles RemoveMigration([NotNull] string projectDir, [NotNull] string rootNamespace, bool force)
        {
            Check.NotEmpty(projectDir, nameof(projectDir));
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));

            var files = new MigrationFiles();

            var modelSnapshot = _migrationsAssembly.ModelSnapshot;
            if (modelSnapshot == null)
            {
                throw new OperationException(DesignStrings.NoSnapshot);
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
                    if (force)
                    {
                        _logger.LogWarning(
                            DesignEventId.ForceRemoveMigration,
                            () => DesignStrings.ForceRemoveMigration(migration.GetId()));
                    }
                    else if (_historyRepository.GetAppliedMigrations().Any(
                        e => e.MigrationId.Equals(migration.GetId(), StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new OperationException(DesignStrings.UnapplyMigration(migration.GetId()));
                    }

                    var migrationFileName = migration.GetId() + language;
                    var migrationFile = TryGetProjectFile(projectDir, migrationFileName);
                    if (migrationFile != null)
                    {
                        _logger.LogInformation(
                            DesignEventId.RemovingMigration,
                            () => DesignStrings.RemovingMigration(migration.GetId()));
                        File.Delete(migrationFile);
                        files.MigrationFile = migrationFile;
                    }
                    else
                    {
                        _logger.LogWarning(
                            DesignEventId.NoMigrationFile,
                            () => DesignStrings.NoMigrationFile(migrationFileName, migration.GetType().ShortDisplayName()));
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
                        _logger.LogDebug(
                            DesignEventId.NoMigrationMetadataFile,
                            () => DesignStrings.NoMigrationMetadataFile(migrationMetadataFileName));
                    }

                    model = migrations.Count > 1
                        ? migrations[migrations.Count - 2].TargetModel
                        : null;
                }
                else
                {
                    _logger.LogDebug(
                        DesignEventId.ManuallyDeleted,
                        () => DesignStrings.ManuallyDeleted);
                }
            }

            var modelSnapshotName = modelSnapshot.GetType().Name;
            var modelSnapshotFileName = modelSnapshotName + language;
            var modelSnapshotFile = TryGetProjectFile(projectDir, modelSnapshotFileName);
            if (model == null)
            {
                if (modelSnapshotFile != null)
                {
                    _logger.LogInformation(
                        DesignEventId.RemovingSnapshot,
                        () => DesignStrings.RemovingSnapshot);
                    File.Delete(modelSnapshotFile);
                    files.SnapshotFile = modelSnapshotFile;
                }
                else
                {
                    _logger.LogWarning(
                        DesignEventId.NoSnapshotFile,
                        () => DesignStrings.NoSnapshotFile(modelSnapshotFileName, modelSnapshot.GetType().ShortDisplayName()));
                }
            }
            else
            {
                var modelSnapshotNamespace = modelSnapshot.GetType().Namespace;
                Debug.Assert(!string.IsNullOrEmpty(modelSnapshotNamespace));
                var modelSnapshotCode = _migrationCodeGenerator.GenerateSnapshot(
                    modelSnapshotNamespace,
                    _contextType,
                    modelSnapshotName,
                    model);

                if (modelSnapshotFile == null)
                {
                    modelSnapshotFile = Path.Combine(
                        GetDirectory(projectDir, null, GetSubNamespace(rootNamespace, modelSnapshotNamespace)),
                        modelSnapshotFileName);
                }

                _logger.LogInformation(
                    DesignEventId.RevertingSnapshot,
                    () => DesignStrings.RevertingSnapshot);
                File.WriteAllText(modelSnapshotFile, modelSnapshotCode, Encoding.UTF8);
            }

            return files;
        }

        public virtual MigrationFiles Save(
            [NotNull] string projectDir,
            [NotNull] ScaffoldedMigration migration,
            [CanBeNull] string outputDir)
        {
            Check.NotEmpty(projectDir, nameof(projectDir));
            Check.NotNull(migration, nameof(migration));

            var lastMigrationFileName = migration.PreviousMigrationId + migration.FileExtension;
            var migrationDirectory = outputDir ?? GetDirectory(projectDir, lastMigrationFileName, migration.MigrationSubNamespace);
            var migrationFile = Path.Combine(migrationDirectory, migration.MigrationId + migration.FileExtension);
            var migrationMetadataFile = Path.Combine(migrationDirectory, migration.MigrationId + ".Designer" + migration.FileExtension);
            var modelSnapshotFileName = migration.SnapshotName + migration.FileExtension;
            var modelSnapshotDirectory = outputDir ?? GetDirectory(projectDir, modelSnapshotFileName, migration.SnapshotSubnamespace);
            var modelSnapshotFile = Path.Combine(modelSnapshotDirectory, modelSnapshotFileName);

            _logger.LogDebug(
                DesignEventId.WritingMigration,
                () => DesignStrings.WritingMigration(migrationFile));
            Directory.CreateDirectory(migrationDirectory);
            File.WriteAllText(migrationFile, migration.MigrationCode, Encoding.UTF8);
            File.WriteAllText(migrationMetadataFile, migration.MetadataCode, Encoding.UTF8);

            _logger.LogDebug(
                DesignEventId.WritingSnapshot,
                () => DesignStrings.WritingSnapshot(modelSnapshotFile));
            Directory.CreateDirectory(modelSnapshotDirectory);
            File.WriteAllText(modelSnapshotFile, migration.SnapshotCode, Encoding.UTF8);

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
                    _logger.LogDebug(
                        DesignEventId.ReusingNamespace,
                        () => DesignStrings.ReusingNamespace(siblingType.ShortDisplayName()));

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
                        _logger.LogDebug(
                            DesignEventId.ReusingDirectory,
                            () => DesignStrings.ReusingDirectory(siblingFileName));

                        return lastDirectory;
                    }
                }
            }

            return defaultDirectory;
        }

        protected virtual string TryGetProjectFile([NotNull] string projectDir, [NotNull] string fileName) =>
            Directory.EnumerateFiles(projectDir, fileName, SearchOption.AllDirectories).FirstOrDefault();

        private bool ContainsForeignMigrations(string migrationsNamespace)
            => (from t in _migrationsAssembly.Assembly.GetConstructableTypes()
                where t.Namespace == migrationsNamespace
                      && t.IsSubclassOf(typeof(Migration))
                let contextTypeAttribute = t.GetCustomAttribute<DbContextAttribute>()
                where contextTypeAttribute != null
                      && contextTypeAttribute.ContextType != _contextType
                select t).Any();
    }
}
