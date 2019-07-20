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
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    /// <summary>
    ///     Used to scaffold new migrations.
    /// </summary>
    public class MigrationsScaffolder : IMigrationsScaffolder
    {
        private readonly Type _contextType;
        private readonly string _activeProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MigrationsScaffolder" /> class.
        /// </summary>
        /// <param name="dependencies"> The dependencies. </param>
        public MigrationsScaffolder([NotNull] MigrationsScaffolderDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            _contextType = dependencies.CurrentContext.Context.GetType();
            _activeProvider = dependencies.DatabaseProvider.Name;
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual MigrationsScaffolderDependencies Dependencies { get; }

        /// <summary>
        ///     Scaffolds a new migration.
        /// </summary>
        /// <param name="migrationName"> The migration's name. </param>
        /// <param name="rootNamespace"> The project's root namespace. </param>
        /// <param name="subNamespace"> The migration's sub-namespace. </param>
        /// <returns> The scaffolded migration. </returns>
        public virtual ScaffoldedMigration ScaffoldMigration(
            [NotNull] string migrationName,
            [NotNull] string rootNamespace,
            [CanBeNull] string subNamespace)
            => ScaffoldMigration(migrationName, rootNamespace, subNamespace, language: null);

        /// <summary>
        ///     Scaffolds a new migration.
        /// </summary>
        /// <param name="migrationName"> The migration's name. </param>
        /// <param name="rootNamespace"> The project's root namespace. </param>
        /// <param name="subNamespace"> The migration's sub-namespace. </param>
        /// <param name="language"> The project's language. </param>
        /// <returns> The scaffolded migration. </returns>
        public virtual ScaffoldedMigration ScaffoldMigration(
            string migrationName,
            string rootNamespace,
            string subNamespace = null,
            string language = null)
        {
            Check.NotEmpty(migrationName, nameof(migrationName));
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));

            if (Dependencies.MigrationsAssembly.FindMigrationId(migrationName) != null)
            {
                throw new OperationException(DesignStrings.DuplicateMigrationName(migrationName));
            }

            var subNamespaceDefaulted = false;
            if (string.IsNullOrEmpty(subNamespace))
            {
                subNamespaceDefaulted = true;
                subNamespace = "Migrations";
            }

            var lastMigration = Dependencies.MigrationsAssembly.Migrations.LastOrDefault();

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
                        builder.Append(sanitizedContextName, 0, sanitizedContextName.Length - 7);
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
                    Dependencies.OperationReporter.WriteWarning(DesignStrings.ForeignMigrations(migrationNamespace));
                }
            }

            var modelSnapshot = Dependencies.MigrationsAssembly.ModelSnapshot;
            var lastModel = Dependencies.SnapshotModelProcessor.Process(modelSnapshot?.Model);
            var upOperations = Dependencies.MigrationsModelDiffer.GetDifferences(lastModel, Dependencies.Model);
            var downOperations = upOperations.Count > 0
                ? Dependencies.MigrationsModelDiffer.GetDifferences(Dependencies.Model, lastModel)
                : new List<MigrationOperation>();
            var migrationId = Dependencies.MigrationsIdGenerator.GenerateId(migrationName);
            var modelSnapshotNamespace = GetNamespace(modelSnapshot?.GetType(), migrationNamespace);

            var modelSnapshotName = sanitizedContextName + "ModelSnapshot";
            if (modelSnapshot != null)
            {
                var lastModelSnapshotName = modelSnapshot.GetType().Name;
                if (lastModelSnapshotName != modelSnapshotName)
                {
                    Dependencies.OperationReporter.WriteVerbose(DesignStrings.ReusingSnapshotName(lastModelSnapshotName));

                    modelSnapshotName = lastModelSnapshotName;
                }
            }

            if (upOperations.Any(o => o.IsDestructiveChange))
            {
                Dependencies.OperationReporter.WriteWarning(DesignStrings.DestructiveOperation);
            }

            var codeGenerator = Dependencies.MigrationsCodeGeneratorSelector.Select(language);
            var migrationCode = codeGenerator.GenerateMigration(
                migrationNamespace,
                migrationName,
                upOperations,
                downOperations);
            var migrationMetadataCode = codeGenerator.GenerateMetadata(
                migrationNamespace,
                _contextType,
                migrationName,
                migrationId,
                Dependencies.Model);
            var modelSnapshotCode = codeGenerator.GenerateSnapshot(
                modelSnapshotNamespace,
                _contextType,
                modelSnapshotName,
                Dependencies.Model);

            return new ScaffoldedMigration(
                codeGenerator.FileExtension,
                lastMigration.Key,
                migrationCode,
                migrationId,
                migrationMetadataCode,
                GetSubNamespace(rootNamespace, migrationNamespace),
                modelSnapshotCode,
                modelSnapshotName,
                GetSubNamespace(rootNamespace, modelSnapshotNamespace));
        }

        /// <summary>
        ///     Gets a sub-namespace.
        /// </summary>
        /// <param name="rootNamespace"> The root namespace. </param>
        /// <param name="namespace"> The full namespace. </param>
        /// <returns> The sub-namespace. </returns>
        protected virtual string GetSubNamespace([NotNull] string rootNamespace, [NotNull] string @namespace) =>
            @namespace == rootNamespace
                ? string.Empty
                : @namespace.StartsWith(rootNamespace + '.', StringComparison.Ordinal)
                    ? @namespace.Substring(rootNamespace.Length + 1)
                    : @namespace;

        /// <summary>
        ///     Removes the previous migration.
        /// </summary>
        /// <param name="projectDir"> The project's root directory. </param>
        /// <param name="rootNamespace"> The project's root namespace. </param>
        /// <param name="force"> Don't check to see if the migration has been applied to the database. </param>
        /// <returns> The removed migration files. </returns>
        public virtual MigrationFiles RemoveMigration([NotNull] string projectDir, [NotNull] string rootNamespace, bool force)
            => RemoveMigration(projectDir, rootNamespace, force, language: null);

        /// <summary>
        ///     Removes the previous migration.
        /// </summary>
        /// <param name="projectDir"> The project's root directory. </param>
        /// <param name="rootNamespace"> The project's root namespace. </param>
        /// <param name="force"> Don't check to see if the migration has been applied to the database. </param>
        /// <param name="language"> The project's language. </param>
        /// <returns> The removed migration files. </returns>
        // TODO: DRY (file names)
        public virtual MigrationFiles RemoveMigration(
            string projectDir,
            string rootNamespace,
            bool force,
            string language)
        {
            Check.NotEmpty(projectDir, nameof(projectDir));
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));

            var files = new MigrationFiles();

            var modelSnapshot = Dependencies.MigrationsAssembly.ModelSnapshot;
            if (modelSnapshot == null)
            {
                throw new OperationException(DesignStrings.NoSnapshot);
            }

            var codeGenerator = Dependencies.MigrationsCodeGeneratorSelector.Select(language);

            IModel model = null;
            var migrations = Dependencies.MigrationsAssembly.Migrations
                .Select(m => Dependencies.MigrationsAssembly.CreateMigration(m.Value, _activeProvider))
                .ToList();
            if (migrations.Count != 0)
            {
                var migration = migrations[migrations.Count - 1];
                model = migration.TargetModel;

                if (!Dependencies.MigrationsModelDiffer.HasDifferences(model, Dependencies.SnapshotModelProcessor.Process(modelSnapshot.Model)))
                {
                    var applied = false;
                    try
                    {
                        applied = Dependencies.HistoryRepository.GetAppliedMigrations().Any(
                            e => e.MigrationId.Equals(migration.GetId(), StringComparison.OrdinalIgnoreCase));
                    }
                    catch (Exception ex) when (force)
                    {
                        Dependencies.OperationReporter.WriteVerbose(ex.ToString());
                        Dependencies.OperationReporter.WriteWarning(
                            DesignStrings.ForceRemoveMigration(migration.GetId(), ex.Message));
                    }

                    if (applied)
                    {
                        if (force)
                        {
                            Dependencies.Migrator.Migrate(
                                migrations.Count > 1
                                    ? migrations[migrations.Count - 2].GetId()
                                    : Migration.InitialDatabase);
                        }
                        else
                        {
                            throw new OperationException(DesignStrings.RevertMigration(migration.GetId()));
                        }
                    }

                    var migrationFileName = migration.GetId() + codeGenerator.FileExtension;
                    var migrationFile = TryGetProjectFile(projectDir, migrationFileName);
                    if (migrationFile != null)
                    {
                        Dependencies.OperationReporter.WriteInformation(DesignStrings.RemovingMigration(migration.GetId()));
                        File.Delete(migrationFile);
                        files.MigrationFile = migrationFile;
                    }
                    else
                    {
                        Dependencies.OperationReporter.WriteWarning(
                            DesignStrings.NoMigrationFile(migrationFileName, migration.GetType().ShortDisplayName()));
                    }

                    var migrationMetadataFileName = migration.GetId() + ".Designer" + codeGenerator.FileExtension;
                    var migrationMetadataFile = TryGetProjectFile(projectDir, migrationMetadataFileName);
                    if (migrationMetadataFile != null)
                    {
                        File.Delete(migrationMetadataFile);
                        files.MetadataFile = migrationMetadataFile;
                    }
                    else
                    {
                        Dependencies.OperationReporter.WriteVerbose(
                            DesignStrings.NoMigrationMetadataFile(migrationMetadataFile));
                    }

                    model = migrations.Count > 1
                        ? migrations[migrations.Count - 2].TargetModel
                        : null;
                }
                else
                {
                    Dependencies.OperationReporter.WriteVerbose(DesignStrings.ManuallyDeleted);
                }
            }

            var modelSnapshotName = modelSnapshot.GetType().Name;
            var modelSnapshotFileName = modelSnapshotName + codeGenerator.FileExtension;
            var modelSnapshotFile = TryGetProjectFile(projectDir, modelSnapshotFileName);
            if (model == null)
            {
                if (modelSnapshotFile != null)
                {
                    Dependencies.OperationReporter.WriteInformation(DesignStrings.RemovingSnapshot);
                    File.Delete(modelSnapshotFile);
                    files.SnapshotFile = modelSnapshotFile;
                }
                else
                {
                    Dependencies.OperationReporter.WriteWarning(
                        DesignStrings.NoSnapshotFile(
                            modelSnapshotFileName,
                            modelSnapshot.GetType().ShortDisplayName()));
                }
            }
            else
            {
                var modelSnapshotNamespace = modelSnapshot.GetType().Namespace;
                Debug.Assert(!string.IsNullOrEmpty(modelSnapshotNamespace));
                var modelSnapshotCode = codeGenerator.GenerateSnapshot(
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

                Dependencies.OperationReporter.WriteInformation(DesignStrings.RevertingSnapshot);
                File.WriteAllText(modelSnapshotFile, modelSnapshotCode, Encoding.UTF8);
            }

            return files;
        }

        /// <summary>
        ///     Saves a scaffolded migration to files.
        /// </summary>
        /// <param name="projectDir"> The project's root directory. </param>
        /// <param name="migration"> The scaffolded migration. </param>
        /// <param name="outputDir"> The directory to put files in. Paths are relative to the project directory. </param>
        /// <returns> The saved migrations files. </returns>
        public virtual MigrationFiles Save(string projectDir, ScaffoldedMigration migration, string outputDir)
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

            Dependencies.OperationReporter.WriteVerbose(DesignStrings.WritingMigration(migrationFile));
            Directory.CreateDirectory(migrationDirectory);
            File.WriteAllText(migrationFile, migration.MigrationCode, Encoding.UTF8);
            File.WriteAllText(migrationMetadataFile, migration.MetadataCode, Encoding.UTF8);

            Dependencies.OperationReporter.WriteVerbose(DesignStrings.WritingSnapshot(modelSnapshotFile));
            Directory.CreateDirectory(modelSnapshotDirectory);
            File.WriteAllText(modelSnapshotFile, migration.SnapshotCode, Encoding.UTF8);

            return new MigrationFiles
            {
                MigrationFile = migrationFile,
                MetadataFile = migrationMetadataFile,
                SnapshotFile = modelSnapshotFile
            };
        }

        /// <summary>
        ///     Gets the namespace of a sibling type. If none, the default namespace is used.
        /// </summary>
        /// <param name="siblingType"> The sibling type. </param>
        /// <param name="defaultNamespace"> The default namespace. </param>
        /// <returns> The namespace. </returns>
        protected virtual string GetNamespace([CanBeNull] Type siblingType, [NotNull] string defaultNamespace)
        {
            if (siblingType != null)
            {
                var lastNamespace = siblingType.Namespace;
                if (lastNamespace != defaultNamespace)
                {
                    Dependencies.OperationReporter.WriteVerbose(DesignStrings.ReusingNamespace(siblingType.ShortDisplayName()));

                    return lastNamespace;
                }
            }

            return defaultNamespace;
        }

        /// <summary>
        ///     Gets the directory of a sibling file. If none, the directory corresponding to the sub-namespace is used.
        /// </summary>
        /// <param name="projectDir"> The project's root directory. </param>
        /// <param name="siblingFileName"> The sibling file's name. </param>
        /// <param name="subnamespace"> The sub-namespace. </param>
        /// <returns> The directory path. </returns>
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
                        Dependencies.OperationReporter.WriteVerbose(DesignStrings.ReusingNamespace(siblingFileName));

                        return lastDirectory;
                    }
                }
            }

            return defaultDirectory;
        }

        /// <summary>
        ///     Tries to find a file under the project directory.
        /// </summary>
        /// <param name="projectDir"> The project directory. </param>
        /// <param name="fileName"> The filename. </param>
        /// <returns> The file path or null if none. </returns>
        protected virtual string TryGetProjectFile([NotNull] string projectDir, [NotNull] string fileName) =>
            Directory.EnumerateFiles(projectDir, fileName, SearchOption.AllDirectories).FirstOrDefault();

        private bool ContainsForeignMigrations(string migrationsNamespace)
            => (from t in Dependencies.MigrationsAssembly.Assembly.GetConstructibleTypes()
                where t.Namespace == migrationsNamespace
                      && t.IsSubclassOf(typeof(Migration))
                let contextTypeAttribute = t.GetCustomAttribute<DbContextAttribute>()
                where contextTypeAttribute != null
                      && contextTypeAttribute.ContextType != _contextType
                select t).Any();
    }
}
