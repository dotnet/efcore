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
    public class MigrationTool
    {
        public class Constants
        {
            public const string ConfigFile = "ConfigFile";
            public const string ContextAssembly = "ContextAssembly";
            public const string ContextType = "ContextType";
            public const string MigrationName = "MigrationName";
            public const string MigrationSource = "MigrationSource";
            public const string MigrationAssembly = "MigrationAssembly";
            public const string MigrationNamespace = "MigrationNamespace";
            public const string MigrationDirectory = "MigrationDirectory";
            public const string TargetMigration = "TargetMigration";
            public const string References = "References";
        }

        private readonly IConfigurationSourceContainer _configuration;

        public MigrationTool([NotNull] IConfigurationSourceContainer configuration)
        {
            Check.NotNull(configuration, "configuration");

            _configuration = configuration;
        }

        public MigrationTool()
            : this(new Configuration())
        {
        }

        public virtual IConfigurationSourceContainer Configuration
        {
            get { return _configuration; }
        }

        public virtual void CommitConfiguration()
        {
            var configFile = Configuration.Get(Constants.ConfigFile);
            if (string.IsNullOrEmpty(configFile))
            {
                configFile = "migration.ini";
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
                var value = Configuration.Get(key);
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
                        Constants.ContextAssembly,
                        Constants.ContextType,
                        Constants.MigrationAssembly,
                        Constants.MigrationNamespace,
                        Constants.MigrationDirectory,
                        Constants.References
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

        public virtual ScaffoldedMigration CreateMigration()
        {
            var migrationName = Configuration.Get(Constants.MigrationName);
            if (string.IsNullOrEmpty(migrationName))
            {
                throw new InvalidOperationException(Strings.MigrationNameNotSpecified);
            }

            var migrationDirectory = Configuration.Get(Constants.MigrationDirectory);
            if (string.IsNullOrEmpty(migrationDirectory))
            {
                migrationDirectory = string.Empty;
            }

            using (var context = LoadContext())
            {
                ConfigureContext(context);

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
                    new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator()))
                    {
                        MigrationDirectory = migrationDirectory
                    };
        }

        protected virtual void WriteMigration(string migrationDirectory, ScaffoldedMigration scaffoldedMigration)
        {
            migrationDirectory = ResolvePath(migrationDirectory);

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

        public virtual IReadOnlyList<IMigrationMetadata> GetMigrations()
        {
            var source = Configuration.Get(Constants.MigrationSource);
            Func<Migrator, IReadOnlyList<IMigrationMetadata>> getMigrationsFunc;

            if (string.IsNullOrEmpty(source)
                || source.Equals("database", StringComparison.OrdinalIgnoreCase))
            {
                getMigrationsFunc = (m => m.GetDatabaseMigrations());
            }
            else if (source.Equals("local", StringComparison.OrdinalIgnoreCase))
            {
                getMigrationsFunc = (m => m.GetLocalMigrations());
            }
            else if (source.Equals("pending", StringComparison.OrdinalIgnoreCase))
            {
                getMigrationsFunc = (m => m.GetPendingMigrations());
            }
            else
            {
                throw new InvalidOperationException(Strings.InvalidMigrationSource);
            }

            using (var context = LoadContext())
            {
                ConfigureContext(context);

                return getMigrationsFunc(GetMigrator(context.Configuration));
            }
        }

        protected virtual Migrator GetMigrator(DbContextConfiguration contextConfiguration)
        {
            return contextConfiguration.Services.ServiceProvider.GetService<Migrator>();
        }

        // TODO: Add support for --SourceMigration
        public virtual IReadOnlyList<SqlStatement> GenerateScript()
        {
            var targetMigrationName = Configuration.Get(Constants.TargetMigration);

            using (var context = LoadContext())
            {
                ConfigureContext(context);

                var migrator = GetMigrator(context.Configuration);

                return
                    string.IsNullOrEmpty(targetMigrationName)
                        ? migrator.GenerateUpdateDatabaseSql()
                        : migrator.GenerateUpdateDatabaseSql(targetMigrationName);
            }
        }

        // TODO: Add support for --SourceMigration
        // TODO: Add support for --Verbose
        public virtual void UpdateDatabase()
        {
            var targetMigrationName = Configuration.Get(Constants.TargetMigration);

            using (var context = LoadContext())
            {
                ConfigureContext(context);

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
        protected internal virtual DbContext LoadContext()
        {
            var contextAssemblyRef = Configuration.Get(Constants.ContextAssembly);
            if (string.IsNullOrEmpty(contextAssemblyRef))
            {
                throw new InvalidOperationException(Strings.ContextAssemblyNotSpecified);
            }

            var contextAssembly = LoadAssembly(contextAssemblyRef);

            LoadReferences();

            var contextTypeName = Configuration.Get(Constants.ContextType);
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
                    .Where(t => !typeof(DbContext).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
                    .ToArray();
        }

        protected virtual void LoadReferences()
        {
            var references = Configuration.Get(Constants.References);
            if (string.IsNullOrEmpty(references))
            {
                return;
            }

            foreach (var assemblyFile in references.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                LoadAssembly(assemblyFile);
            }
        }

        protected virtual Assembly LoadAssembly(string assemblyRef)
        {
            // TODO: Figure out what we want to do about loading the context assembly and its references:
            // 1. Assembly.LoadFile or Assembly.Load?
            // 2. AppDomain.AssemblyResolve?
            // 3. New AppDomain with ApplicationBase, DynamicBase, PrivateBinPath?
            // 4. Something else?

            return Assembly.LoadFile(ResolvePath(assemblyRef));
        }

        protected virtual DbContext CreateContext(Type contextType)
        {
            return (DbContext)Activator.CreateInstance(contextType);
        }

        protected virtual void ConfigureContext(DbContext context)
        {
            var extension = RelationalOptionsExtension.Extract(context.Configuration);

            var migrationAssemblyFile = Configuration.Get(Constants.MigrationAssembly);
            if (!string.IsNullOrEmpty(migrationAssemblyFile))
            {
                extension.MigrationAssembly = LoadAssembly(migrationAssemblyFile);
            }

            var migrationNamespace = Configuration.Get(Constants.MigrationNamespace);
            if (!string.IsNullOrEmpty(migrationNamespace))
            {
                extension.MigrationNamespace = migrationNamespace;
            }
        }

        public virtual string ResolvePath(string path)
        {
            return Path.IsPathRooted(path) ? path : Path.Combine(Directory.GetCurrentDirectory(), path);
        }
    }
}
