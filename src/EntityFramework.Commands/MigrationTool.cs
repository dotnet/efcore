// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Migrations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Commands
{
    // TODO: Centralize filename logic #DRY
    public class MigrationTool
    {
        private readonly ILoggerProvider _loggerProvider;
        private readonly LazyRef<ILogger> _logger;
        private readonly Assembly _assembly;

        public MigrationTool([NotNull] ILoggerProvider loggerProvider, [NotNull] Assembly assembly)
        {
            Check.NotNull(loggerProvider, "loggerProvider");
            Check.NotNull(assembly, "assembly");

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(loggerProvider);

            _loggerProvider = loggerProvider;
            _logger = new LazyRef<ILogger>(() => loggerFactory.Create<MigrationTool>());
            _assembly = assembly;
        }

        public virtual IEnumerable<string> AddMigration(
            [NotNull] string migrationName,
            [CanBeNull] string contextTypeName,
            [NotNull] string rootNamespace,
            [NotNull] string projectDir)
        {
            Check.NotEmpty(migrationName, "migrationName");
            Check.NotEmpty(rootNamespace, "rootNamespace");
            Check.NotEmpty(projectDir, "projectDir");

            var contextType = GetContextType(contextTypeName);
            using (var context = CreateContext(contextType))
            {
                var scaffolder = CreateScaffolder(context);
                var migration = scaffolder.ScaffoldMigration(migrationName, rootNamespace);

                return WriteMigration(projectDir, migration, rootNamespace);
            }
        }

        // TODO: Move to MigrationScaffolder
        private IEnumerable<string> WriteMigration(
            string projectDir,
            ScaffoldedMigration migration,
            string rootNamespace)
        {
            Check.NotEmpty(projectDir, "projectDir");
            Check.NotNull(migration, "migration");
            Check.NotEmpty(rootNamespace, "rootNamespace");

            var migrationDir = GetMigrationDirectory(projectDir, migration, rootNamespace);
            Directory.CreateDirectory(migrationDir);

            var userCodeFile = Path.Combine(migrationDir, migration.MigrationId + migration.Language);
            File.WriteAllText(userCodeFile, migration.MigrationCode);
            yield return userCodeFile;

            var designerCodeFile = Path.Combine(migrationDir, migration.MigrationId + ".Designer" + migration.Language);
            File.WriteAllText(designerCodeFile, migration.MigrationMetadataCode);
            yield return designerCodeFile;

            var modelShapshotDir = GetSnapshotDirectory(projectDir, migration, rootNamespace);
            var modelSnapshotFile = Path.Combine(
                modelShapshotDir,
                migration.SnapshotModelClass + migration.Language);
            File.WriteAllText(modelSnapshotFile, migration.SnapshotModelCode);
            yield return modelSnapshotFile;
        }

        public virtual IEnumerable<Migration> GetMigrations([CanBeNull] string contextTypeName)
        {
            var contextType = GetContextType(contextTypeName);

            return MigrationAssembly.LoadMigrations(GetMigrationTypes(), contextType);
        }

        public virtual string ScriptMigration(
            [CanBeNull] string fromMigrationName,
            [CanBeNull] string toMigrationName,
            bool idempotent,
            [CanBeNull] string contextTypeName)
        {
            var contextType = GetContextType(contextTypeName);
            using (var context = CreateContext(contextType))
            {
                var migrator = CreateMigrator(context);

                // TODO: Use fromMigrationName
                // TODO: Use idempotent
                var statements = string.IsNullOrEmpty(toMigrationName)
                    ? migrator.ScriptMigrations()
                    : migrator.ScriptMigrations(toMigrationName);

                // TODO: Use SuppressTransaction somehow?
                return string.Join(Environment.NewLine, statements.Select(s => s.Sql));
            }
        }

        public virtual void ApplyMigration([CanBeNull] string migrationName, [CanBeNull] string contextTypeName)
        {
            var contextType = GetContextType(contextTypeName);
            using (var context = CreateContext(contextType))
            {
                var migrator = CreateMigrator(context);

                if (string.IsNullOrEmpty(migrationName))
                {
                    migrator.ApplyMigrations();
                }
                else
                {
                    migrator.ApplyMigrations(migrationName);
                }
            }
        }

        // TODO: Move to MigrationScaffolder
        public virtual IEnumerable<string> RemoveMigration(
            [CanBeNull] string contextTypeName,
            [NotNull] string rootNamespace,
            [NotNull] string projectDir)
        {
            Check.NotEmpty(rootNamespace, "rootNamespace");
            Check.NotEmpty(projectDir, "projectDir");

            var filesToDelete = new List<string>();

            var contextType = GetContextType(contextTypeName);
            using (var context = CreateContext(contextType))
            {
                var migrator = CreateMigrator(context);
                var snapshot = migrator.MigrationAssembly.ModelSnapshot;
                if (snapshot == null)
                {
                    throw new InvalidOperationException(Strings.NoSnapshot);
                }

                var codeGenerator = CreateCodeGenerator();
                var language = codeGenerator.Language;

                IModel model = null;
                var migrations = migrator.GetLocalMigrations();
                if (migrations.Count != 0)
                {
                    var migration = migrations.Last();
                    model = migration.GetMetadata().TargetModel;

                    if (!migrator.ModelDiffer.Diff(snapshot.Model, model).Any())
                    {
                        if (migrator.GetDatabaseMigrations().Contains(migration))
                        {
                            throw new InvalidOperationException(
                                Strings.UnapplyMigration(migration.GetMigrationName()));
                        }

                        var migrationFileName = migration.GetMigrationId() + language;
                        var migrationFile = FindProjectFile(projectDir, migrationFileName);
                        if (migrationFile != null)
                        {
                            filesToDelete.Add(migrationFile);
                            _logger.Value.WriteInformation(Strings.RemovingMigration(migration.GetMigrationName()));
                        }
                        else
                        {
                            var migrationClass = migration.GetType().FullName;
                            _logger.Value.WriteWarning(Strings.NoMigrationFile(migrationFileName, migrationClass));
                        }

                        var migrationMetadataFileName = migration.GetMigrationId() + ".Designer" + language;
                        var migrationMetadataFile = FindProjectFile(projectDir, migrationMetadataFileName);
                        if (migrationMetadataFile != null)
                        {
                            filesToDelete.Add(migrationMetadataFile);
                        }
                        else
                        {
                            _logger.Value.WriteVerbose(Strings.NoMigrationMetadataFile(migrationMetadataFileName));
                        }

                        model = migrations.Count > 1
                            ? migrations[migrations.Count - 2].GetMetadata().TargetModel
                            : null;
                    }
                    else
                    {
                        _logger.Value.WriteVerbose(Strings.ManuallyDeleted);
                    }
                }

                var snapshotFileName = snapshot.GetType().Name + language;
                var snapshotFile = FindProjectFile(projectDir, snapshotFileName);
                if (model == null)
                {
                    if (snapshotFile != null)
                    {
                        filesToDelete.Add(snapshotFile);
                        _logger.Value.WriteInformation(Strings.RemovingSnapshot);
                    }
                    else
                    {
                        var snapshotClass = snapshot.GetType().FullName;
                        _logger.Value.WriteWarning(Strings.NoSnapshotFile(snapshotFileName, snapshotClass));
                    }
                }
                else
                {
                    var snapshotNamespace = snapshot.GetType().Namespace;
                    if (snapshotFile == null)
                    {
                        snapshotFile = Path.Combine(
                            GetDirectoryFromNamespace(snapshotNamespace, rootNamespace),
                            snapshotFileName);
                    }

                    var scaffolder = CreateScaffolder(context);
                    var snapshotModelCode = new IndentedStringBuilder();
                    scaffolder.ScaffoldSnapshotModel(
                        snapshotNamespace,
                        model,
                        contextType,
                        snapshotModelCode);
                    File.WriteAllText(snapshotFile, snapshotModelCode.ToString());
                    _logger.Value.WriteInformation(Strings.RevertingSnapshot);
                }
            }

            return filesToDelete;
        }

        public virtual Type GetContextType([CanBeNull] string name)
        {
            var contextType = ContextTool.SelectType(GetContextTypes(), name);
            _logger.Value.WriteVerbose(Strings.LogUseContext(contextType.Name));

            return contextType;
        }

        public virtual IEnumerable<Type> GetContextTypes()
        {
            return ContextTool.GetContextTypes(_assembly)
                .Concat(
                    GetMigrationTypes()
                        .Select(MigrationAssembly.TryGetContextType)
                        .Where(t => t != null))
                .Distinct();
        }

        private DbContext CreateContext(Type type)
        {
            var context = ContextTool.CreateContext(type);

            var scopedServiceProvider = ((IDbContextServices)context).ScopedServiceProvider;
            var options = scopedServiceProvider.GetRequiredService<DbContextService<IDbContextOptions>>();

            var loggerFactory = scopedServiceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddProvider(_loggerProvider);

            var extension = MigrationsOptionsExtension.Extract(options.Service);
            if (extension == null || extension.MigrationAssembly == null)
            {
                options.Service.AddOrUpdateExtension<MigrationsOptionsExtension>(
                    x => x.MigrationAssembly = _assembly);
            }

            return context;
        }

        private Migrator CreateMigrator(DbContext context)
        {
            return ((IDbContextServices)context).ScopedServiceProvider.GetRequiredService<DbContextService<Migrator>>().Service;
        }

        private MigrationScaffolder CreateScaffolder(DbContext context)
        {
            var scopedServiceProvider = ((IDbContextServices)context).ScopedServiceProvider;
            var options = scopedServiceProvider.GetRequiredService<DbContextService<IDbContextOptions>>();
            var model = scopedServiceProvider.GetRequiredService<DbContextService<IModel>>();
            var migrator = CreateMigrator(context);

            return new MigrationScaffolder(
                context,
                options.Service,
                model.Service,
                migrator.MigrationAssembly,
                migrator.ModelDiffer,
                CreateCodeGenerator());
        }

        private MigrationCodeGenerator CreateCodeGenerator()
        {
            // TODO: Allow users to override #1283
            return new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator());
        }

        private IEnumerable<Type> GetMigrationTypes()
        {
            return MigrationAssembly.GetMigrationTypes(_assembly);
        }

        private static string GetMigrationDirectory(string projectDir, ScaffoldedMigration migration, string rootNamespace)
        {
            if (migration.LastMigration != null)
            {
                var lastMigrationFile = FindProjectFile(
                    projectDir,
                    migration.LastMigration.GetMigrationId() + migration.Language);
                if (lastMigrationFile != null)
                {
                    return Path.GetDirectoryName(lastMigrationFile);
                }
            }

            var migrationDirectory = GetDirectoryFromNamespace(migration.MigrationNamespace, rootNamespace);

            return Path.Combine(projectDir, migrationDirectory);
        }

        private static string GetSnapshotDirectory(string projectDir, ScaffoldedMigration migration, string rootNamespace)
        {
            if (migration.LastModelSnapshot != null)
            {
                var lastSnapshotFile = FindProjectFile(
                    projectDir,
                    migration.LastModelSnapshot.GetType().Name + migration.Language);
                if (lastSnapshotFile != null)
                {
                    return Path.GetDirectoryName(lastSnapshotFile);
                }
            }

            var snapshotDirectory = GetDirectoryFromNamespace(migration.ModelSnapshotNamespace, rootNamespace);

            return Path.Combine(projectDir, snapshotDirectory);
        }

        private static string FindProjectFile(string projectDir, string fileName)
        {
            return Directory.EnumerateFiles(projectDir, fileName, SearchOption.AllDirectories).FirstOrDefault();
        }

        private static string GetDirectoryFromNamespace(string @namespace, string rootNamespace)
        {
            var directory = @namespace.StartsWith(rootNamespace + '.')
                ? @namespace.Substring(rootNamespace.Length + 1)
                : @namespace;

            return directory.Replace('.', Path.DirectorySeparatorChar);
        }
    }
}
