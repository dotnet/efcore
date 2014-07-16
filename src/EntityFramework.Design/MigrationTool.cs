// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Design.Utilities;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Design
{
    // TODO: Add logging.
    // TODO: Consider moving most of the Configuration code out of this class
    public class MigrationTool
    {
        public static class Constants
        {
            public const string ConfigFileOption = "ConfigFile";
            public const string ContextAssemblyOption = "ContextAssembly";
            public const string ContextTypeOption = "ContextType";
            public const string MigrationNameOption = "MigrationName";
            public const string MigrationSourceOption = "MigrationSource";
            public const string MigrationAssemblyOption = "MigrationAssembly";
            public const string MigrationNamespaceOption = "MigrationNamespace";
            public const string MigrationDirectoryOption = "MigrationDirectory";
            public const string TargetMigrationOption = "TargetMigration";
            public const string ReferencesOption = "References";
            public const string MigrationSourceDatabase = "Database";
            public const string MigrationSourceLocal = "Local";
            public const string MigrationSourcePending = "Pending";
            public const string DefaultConfigFile = "migration.ini";
            public const string DefaultMigrationDirectory = "Migrations";
        }

        public virtual void CommitConfiguration([NotNull] IConfigurationSourceContainer configuration)
        {
            Check.NotNull(configuration, "configuration");

            var configFile = configuration.Get(Constants.ConfigFileOption);
            if (string.IsNullOrEmpty(configFile))
            {
                configFile = Constants.DefaultConfigFile;
            }

            configFile = ResolvePath(configFile);

            var iniFileConfigurationSource = CreateIniFileConfigurationSource(configFile);
            if (!File.Exists(configFile))
            {
                WriteFile(configFile, CreateEmptyConfiguration(), true);
            }

            iniFileConfigurationSource.Load();

            foreach (var key in GetCommitableSettings())
            {
                var value = configuration.Get(key);
                if (!string.IsNullOrEmpty(value))
                {
                    iniFileConfigurationSource.Set(key, value);
                }
            }

            iniFileConfigurationSource.Commit();
        }

        protected virtual IniFileConfigurationSource CreateIniFileConfigurationSource(string configFile)
        {
            return new IniFileConfigurationSource(configFile);
        }

        protected virtual IEnumerable<string> GetCommitableSettings()
        {
            return
                new[]
                    {
                        Constants.ContextAssemblyOption,
                        Constants.ContextTypeOption,
                        Constants.MigrationAssemblyOption,
                        Constants.MigrationNamespaceOption,
                        Constants.MigrationDirectoryOption,
                        Constants.ReferencesOption
                    };
        }

        protected virtual string CreateEmptyConfiguration()
        {
            var builder = new StringBuilder();

            foreach (var key in GetCommitableSettings())
            {
                builder.Append(key);
                builder.AppendLine("=");
            }

            return builder.ToString();
        }

        public virtual ScaffoldedMigration CreateMigration([NotNull] IConfigurationSourceContainer configuration)
        {
            Check.NotNull(configuration, "configuration");

            var migrationName = configuration.Get(Constants.MigrationNameOption);
            if (string.IsNullOrEmpty(migrationName))
            {
                throw new InvalidOperationException(Strings.MigrationNameNotSpecified);
            }

            var migrationDirectory = configuration.Get(Constants.MigrationDirectoryOption);

            var contextAssemblyFile = configuration.Get(Constants.ContextAssemblyOption);
            if (string.IsNullOrEmpty(contextAssemblyFile))
            {
                throw new InvalidOperationException(Strings.ContextAssemblyNotSpecified);
            }

            var references = configuration.Get(Constants.ReferencesOption);
            var contextTypeName = configuration.Get(Constants.ContextTypeOption);
            var migrationAssemblyFile = configuration.Get(Constants.MigrationAssemblyOption);
            var migrationNamespace = configuration.Get(Constants.MigrationNamespaceOption);

            return CreateMigration(
                migrationName,
                contextAssemblyFile,
                migrationDirectory,
                references,
                contextTypeName,
                migrationAssemblyFile,
                migrationNamespace);
        }

        public virtual ScaffoldedMigration CreateMigration(
            [NotNull] string migrationName,
            [NotNull] string contextAssemblyFile,
            [CanBeNull] string migrationDirectory = null,
            [CanBeNull] string references = null,
            [CanBeNull] string contextTypeName = null,
            [CanBeNull] string migrationAssemblyFile = null,
            [CanBeNull] string migrationNamespace = null)
        {
            Check.NotEmpty(migrationName, "migrationName");
            Check.NotEmpty(contextAssemblyFile, "contextAssemblyFile");

            if (string.IsNullOrEmpty(migrationDirectory))
            {
                migrationDirectory = Constants.DefaultMigrationDirectory;
            }

            using (var context = LoadContext(contextAssemblyFile, references, contextTypeName))
            {
                ConfigureContext(context, migrationAssemblyFile, migrationNamespace);

                var scaffolder = CreateScaffolder(context.Configuration, migrationDirectory);
                var scaffoldedMigration = scaffolder.ScaffoldMigration(migrationName);

                WriteMigration(migrationDirectory, scaffoldedMigration);

                return scaffoldedMigration;
            }
        }

        protected virtual MigrationScaffolder CreateScaffolder(
            DbContextConfiguration contextConfiguration, string migrationDirectory)
        {
            return
                new MigrationScaffolder(
                    contextConfiguration,
                    contextConfiguration.Services.ServiceProvider.GetService<MigrationAssembly>(),
                    contextConfiguration.Services.ServiceProvider.GetService<ModelDiffer>(),
                    new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator()));
        }

        protected virtual void WriteMigration(string migrationDirectory, ScaffoldedMigration scaffoldedMigration)
        {
            migrationDirectory = ResolvePath(migrationDirectory);
            Directory.CreateDirectory(migrationDirectory);

            scaffoldedMigration.MigrationFile = Path.Combine(migrationDirectory, scaffoldedMigration.MigrationClass + ".cs");
            scaffoldedMigration.MigrationMetadataFile = Path.Combine(migrationDirectory, scaffoldedMigration.MigrationClass + ".Designer.cs");
            scaffoldedMigration.SnapshotModelFile = Path.Combine(migrationDirectory, scaffoldedMigration.SnapshotModelClass + ".cs");

            WriteFile(scaffoldedMigration.MigrationFile, scaffoldedMigration.MigrationCode, overwrite: false);
            WriteFile(scaffoldedMigration.MigrationMetadataFile, scaffoldedMigration.MigrationMetadataCode, overwrite: false);
            WriteFile(scaffoldedMigration.SnapshotModelFile, scaffoldedMigration.SnapshotModelCode, overwrite: true);
        }

        protected virtual void WriteFile(string path, string content, bool overwrite)
        {
            var fileMode = overwrite ? FileMode.Create : FileMode.CreateNew;

            using (var stream = new FileStream(path, fileMode, FileAccess.Write))
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.Write(content);
                }
            }
        }

        public virtual IReadOnlyList<IMigrationMetadata> GetMigrations([NotNull] IConfigurationSourceContainer configuration)
        {
            Check.NotNull(configuration, "configuration");

            var source = configuration.Get(Constants.MigrationSourceOption);

            var contextAssemblyFile = configuration.Get(Constants.ContextAssemblyOption);
            if (string.IsNullOrEmpty(contextAssemblyFile))
            {
                throw new InvalidOperationException(Strings.ContextAssemblyNotSpecified);
            }

            var references = configuration.Get(Constants.ReferencesOption);
            var contextTypeName = configuration.Get(Constants.ContextTypeOption);
            var migrationAssemblyFile = configuration.Get(Constants.MigrationAssemblyOption);
            var migrationNamespace = configuration.Get(Constants.MigrationNamespaceOption);

            return GetMigrations(
                contextAssemblyFile,
                source,
                references,
                contextTypeName,
                migrationAssemblyFile,
                migrationNamespace);
        }

        public virtual IReadOnlyList<IMigrationMetadata> GetMigrations(
            [NotNull] string contextAssemblyFile,
            [CanBeNull] string source = null,
            [CanBeNull] string references = null,
            [CanBeNull] string contextTypeName = null,
            [CanBeNull] string migrationAssemblyFile = null,
            [CanBeNull] string migrationNamespace = null)
        {
            Check.NotEmpty(contextAssemblyFile, "contextAssemblyFile");

            Func<Migrator, IReadOnlyList<IMigrationMetadata>> getMigrationsFunc;

            if (string.IsNullOrEmpty(source)
                || source.Equals(Constants.MigrationSourceDatabase, StringComparison.OrdinalIgnoreCase))
            {
                getMigrationsFunc = (m => m.GetDatabaseMigrations());
            }
            else if (source.Equals(Constants.MigrationSourceLocal, StringComparison.OrdinalIgnoreCase))
            {
                getMigrationsFunc = (m => m.GetLocalMigrations());
            }
            else if (source.Equals(Constants.MigrationSourcePending, StringComparison.OrdinalIgnoreCase))
            {
                getMigrationsFunc = (m => m.GetPendingMigrations());
            }
            else
            {
                throw new InvalidOperationException(Strings.InvalidMigrationSource);
            }

            using (var context = LoadContext(contextAssemblyFile, references, contextTypeName))
            {
                ConfigureContext(context, migrationAssemblyFile, migrationNamespace);

                return getMigrationsFunc(GetMigrator(context.Configuration));
            }
        }

        protected virtual Migrator GetMigrator(DbContextConfiguration contextConfiguration)
        {
            return contextConfiguration.Services.ServiceProvider.GetService<Migrator>();
        }

        // TODO: Add support for --SourceMigration
        public virtual IReadOnlyList<SqlStatement> GenerateScript([NotNull] IConfigurationSourceContainer configuration)
        {
            Check.NotNull(configuration, "configuration");

            var targetMigrationName = configuration.Get(Constants.TargetMigrationOption);

            var contextAssemblyFile = configuration.Get(Constants.ContextAssemblyOption);
            if (string.IsNullOrEmpty(contextAssemblyFile))
            {
                throw new InvalidOperationException(Strings.ContextAssemblyNotSpecified);
            }

            var references = configuration.Get(Constants.ReferencesOption);
            var contextTypeName = configuration.Get(Constants.ContextTypeOption);
            var migrationAssemblyFile = configuration.Get(Constants.MigrationAssemblyOption);
            var migrationNamespace = configuration.Get(Constants.MigrationNamespaceOption);

            return GenerateScript(
                contextAssemblyFile,
                targetMigrationName,
                references,
                contextTypeName,
                migrationAssemblyFile,
                migrationNamespace);
        }

        public virtual IReadOnlyList<SqlStatement> GenerateScript(
            [NotNull] string contextAssemblyFile,
            [CanBeNull] string targetMigrationName = null,
            [CanBeNull] string references = null,
            [CanBeNull] string contextTypeName = null,
            [CanBeNull] string migrationAssemblyFile = null,
            [CanBeNull] string migrationNamespace = null)
        {
            Check.NotEmpty(contextAssemblyFile, "contextAssemblyFile");

            using (var context = LoadContext(contextAssemblyFile, references, contextTypeName))
            {
                ConfigureContext(context, migrationAssemblyFile, migrationNamespace);

                var migrator = GetMigrator(context.Configuration);

                return
                    string.IsNullOrEmpty(targetMigrationName)
                        ? migrator.GenerateUpdateDatabaseSql()
                        : migrator.GenerateUpdateDatabaseSql(targetMigrationName);
            }
        }

        // TODO: Add support for --SourceMigration
        // TODO: Add support for --Verbose
        public virtual void UpdateDatabase([NotNull] IConfigurationSourceContainer configuration)
        {
            Check.NotNull(configuration, "configuration");

            var targetMigrationName = configuration.Get(Constants.TargetMigrationOption);
            var contextAssemblyFile = configuration.Get(Constants.ContextAssemblyOption);
            if (string.IsNullOrEmpty(contextAssemblyFile))
            {
                throw new InvalidOperationException(Strings.ContextAssemblyNotSpecified);
            }

            var references = configuration.Get(Constants.ReferencesOption);
            var contextTypeName = configuration.Get(Constants.ContextTypeOption);
            var migrationAssemblyFile = configuration.Get(Constants.MigrationAssemblyOption);
            var migrationNamespace = configuration.Get(Constants.MigrationNamespaceOption);

            UpdateDatabase(
                contextAssemblyFile,
                targetMigrationName,
                references,
                contextTypeName,
                migrationAssemblyFile,
                migrationNamespace);
        }

        public virtual void UpdateDatabase(
            [NotNull] string contextAssemblyFile,
            [CanBeNull] string targetMigrationName = null,
            [CanBeNull] string references = null,
            [CanBeNull] string contextTypeName = null,
            [CanBeNull] string migrationAssemblyFile = null,
            [CanBeNull] string migrationNamespace = null)
        {
            Check.NotEmpty(contextAssemblyFile, "contextAssemblyFile");

            using (var context = LoadContext(contextAssemblyFile, references, contextTypeName))
            {
                ConfigureContext(context, migrationAssemblyFile, migrationNamespace);

                var migrator = GetMigrator(context.Configuration);

                if (string.IsNullOrEmpty(targetMigrationName))
                {
                    migrator.UpdateDatabase();
                }
                else
                {
                    migrator.UpdateDatabase(targetMigrationName);
                }
            }
        }

        // Internal for testing.
        protected internal virtual DbContext LoadContext(IConfigurationSourceContainer configuration)
        {
            var contextAssemblyFile = configuration.Get(Constants.ContextAssemblyOption);
            if (string.IsNullOrEmpty(contextAssemblyFile))
            {
                throw new InvalidOperationException(Strings.ContextAssemblyNotSpecified);
            }

            var references = configuration.Get(Constants.ReferencesOption);
            var contextTypeName = configuration.Get(Constants.ContextTypeOption);

            return LoadContext(contextAssemblyFile, references, contextTypeName);
        }

        protected virtual DbContext LoadContext(
            [NotNull] string contextAssemblyFile,
            [CanBeNull] string references = null,
            [CanBeNull] string contextTypeName = null)
        {
            Check.NotEmpty(contextAssemblyFile, "contextAssemblyFile");

            var contextAssembly = LoadAssembly(contextAssemblyFile);

            LoadReferences(references);

            var contextType
                = string.IsNullOrEmpty(contextTypeName)
                    ? FindContextType(contextAssembly)
                    : GetContextType(contextTypeName, contextAssembly);

            return CreateContext(contextType);
        }

        protected virtual Type GetContextType(string contextTypeName, Assembly contextAssembly)
        {
            var contextType = contextAssembly.GetType(contextTypeName);
            if (contextType == null)
            {
                throw new InvalidOperationException(Strings.FormatAssemblyDoesNotContainType(contextAssembly.FullName, contextTypeName));
            }

            if (!typeof(DbContext).GetTypeInfo().IsAssignableFrom(contextType.GetTypeInfo()))
            {
                throw new InvalidOperationException(Strings.FormatTypeIsNotDbContext(contextTypeName));
            }

            return contextType;
        }

        protected virtual Type FindContextType(Assembly contextAssembly)
        {
            var contextTypes = GetContextTypes(contextAssembly);

            if (contextTypes.Count == 0)
            {
                throw new InvalidOperationException(Strings.FormatAssemblyDoesNotContainDbContext(contextAssembly.FullName));
            }

            if (contextTypes.Count > 1)
            {
                throw new InvalidOperationException(Strings.FormatAssemblyContainsMultipleDbContext(contextAssembly.FullName));
            }

            return contextTypes[0];
        }

        protected virtual IReadOnlyList<Type> GetContextTypes(Assembly contextAssembly)
        {
            return
                contextAssembly.GetAccessibleTypes()
                    .Where(t => typeof(DbContext).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
                    .ToArray();
        }

        protected virtual void LoadReferences(string references)
        {
            if (string.IsNullOrEmpty(references))
            {
                return;
            }

            foreach (var assemblyFile in references.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                LoadAssembly(assemblyFile);
            }
        }

        protected virtual Assembly LoadAssembly(string assemblyFile)
        {
            return Assembly.LoadFile(ResolvePath(assemblyFile));
        }

        protected virtual DbContext CreateContext(Type contextType)
        {
            return (DbContext)Activator.CreateInstance(contextType);
        }

        protected virtual void ConfigureContext(
            DbContext context,
            [CanBeNull] string migrationAssemblyFile = null,
            [CanBeNull] string migrationNamespace = null)
        {
            var extension = RelationalOptionsExtension.Extract(context.Configuration);

            if (!string.IsNullOrEmpty(migrationAssemblyFile))
            {
                extension.MigrationAssembly = LoadAssembly(migrationAssemblyFile);
            }

            if (!string.IsNullOrEmpty(migrationNamespace))
            {
                extension.MigrationNamespace = migrationNamespace;
            }
        }

        public virtual string ResolvePath([NotNull] string path)
        {
            Check.NotNull(path, "path");

            return Path.IsPathRooted(path) ? path : Path.Combine(Directory.GetCurrentDirectory(), path);
        }
    }
}
