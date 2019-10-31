// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class MigrationsOperations
    {
        private readonly IOperationReporter _reporter;
        private readonly Assembly _assembly;
        private readonly string _projectDir;
        private readonly string _rootNamespace;
        private readonly string _language;
        private readonly DesignTimeServicesBuilder _servicesBuilder;
        private readonly DbContextOperations _contextOperations;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public MigrationsOperations(
            [NotNull] IOperationReporter reporter,
            [NotNull] Assembly assembly,
            [NotNull] Assembly startupAssembly,
            [NotNull] string projectDir,
            [NotNull] string rootNamespace,
            [CanBeNull] string language,
            [NotNull] string[] args)
        {
            Check.NotNull(reporter, nameof(reporter));
            Check.NotNull(assembly, nameof(assembly));
            Check.NotNull(startupAssembly, nameof(startupAssembly));
            Check.NotNull(projectDir, nameof(projectDir));
            Check.NotNull(rootNamespace, nameof(rootNamespace));
            Check.NotNull(args, nameof(args));

            _reporter = reporter;
            _assembly = assembly;
            _projectDir = projectDir;
            _rootNamespace = rootNamespace;
            _language = language;
            _contextOperations = new DbContextOperations(
                reporter,
                assembly,
                startupAssembly,
                args);

            _servicesBuilder = new DesignTimeServicesBuilder(assembly, startupAssembly, reporter, args);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual MigrationFiles AddMigration(
            [NotNull] string name,
            [CanBeNull] string outputDir,
            [CanBeNull] string contextType)
        {
            Check.NotEmpty(name, nameof(name));

            if (outputDir != null)
            {
                outputDir = Path.GetFullPath(Path.Combine(_projectDir, outputDir));
            }

            var subNamespace = SubnamespaceFromOutputPath(outputDir);

            using (var context = _contextOperations.CreateContext(contextType))
            {
                var services = _servicesBuilder.Build(context);
                EnsureServices(services);
                EnsureMigrationsAssembly(services);

                var scaffolder = services.GetRequiredService<IMigrationsScaffolder>();
                var migration = scaffolder.ScaffoldMigration(name, _rootNamespace, subNamespace, _language);
                var files = scaffolder.Save(_projectDir, migration, outputDir);

                return files;
            }
        }

        // if outputDir is a subfolder of projectDir, then use each subfolder as a subnamespace
        // --output-dir $(projectFolder)/A/B/C
        // => "namespace $(rootnamespace).A.B.C"
        private string SubnamespaceFromOutputPath(string outputDir)
        {
            if (outputDir?.StartsWith(_projectDir, StringComparison.Ordinal) != true)
            {
                return null;
            }

            var subPath = outputDir.Substring(_projectDir.Length);

            return !string.IsNullOrWhiteSpace(subPath)
                ? string.Join(
                    ".",
                    subPath.Split(
                        new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                        StringSplitOptions.RemoveEmptyEntries))
                : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<MigrationInfo> GetMigrations(
            [CanBeNull] string contextType)
        {
            using (var context = _contextOperations.CreateContext(contextType))
            {
                var services = _servicesBuilder.Build(context);
                EnsureServices(services);

                var migrationsAssembly = services.GetRequiredService<IMigrationsAssembly>();
                var idGenerator = services.GetRequiredService<IMigrationsIdGenerator>();

                return from id in migrationsAssembly.Migrations.Keys
                       select new MigrationInfo { Id = id, Name = idGenerator.GetName(id) };
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string ScriptMigration(
            [CanBeNull] string fromMigration,
            [CanBeNull] string toMigration,
            bool idempotent,
            [CanBeNull] string contextType)
        {
            using (var context = _contextOperations.CreateContext(contextType))
            {
                var services = _servicesBuilder.Build(context);
                EnsureServices(services);

                var migrator = services.GetRequiredService<IMigrator>();

                return migrator.GenerateScript(fromMigration, toMigration, idempotent);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdateDatabase(
            [CanBeNull] string targetMigration,
            [CanBeNull] string contextType)
        {
            using (var context = _contextOperations.CreateContext(contextType))
            {
                var services = _servicesBuilder.Build(context);
                EnsureServices(services);

                var migrator = services.GetRequiredService<IMigrator>();

                migrator.Migrate(targetMigration);
            }

            _reporter.WriteInformation(DesignStrings.Done);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual MigrationFiles RemoveMigration(
            [CanBeNull] string contextType, bool force)
        {
            using (var context = _contextOperations.CreateContext(contextType))
            {
                var services = _servicesBuilder.Build(context);
                EnsureServices(services);
                EnsureMigrationsAssembly(services);

                var scaffolder = services.GetRequiredService<IMigrationsScaffolder>();

                var files = scaffolder.RemoveMigration(_projectDir, _rootNamespace, force, _language);

                _reporter.WriteInformation(DesignStrings.Done);

                return files;
            }
        }

        private static void EnsureServices(IServiceProvider services)
        {
            var migrator = services.GetService<IMigrator>();
            if (migrator == null)
            {
                var databaseProvider = services.GetService<IDatabaseProvider>();
                throw new OperationException(DesignStrings.NonRelationalProvider(databaseProvider?.Name ?? "Unknown"));
            }
        }

        private void EnsureMigrationsAssembly(IServiceProvider services)
        {
            var assemblyName = _assembly.GetName();
            var options = services.GetRequiredService<IDbContextOptions>();
            var contextType = services.GetRequiredService<ICurrentDbContext>().Context.GetType();
            var migrationsAssemblyName = RelationalOptionsExtension.Extract(options).MigrationsAssembly
                                         ?? contextType.GetTypeInfo().Assembly.GetName().Name;
            if (assemblyName.Name != migrationsAssemblyName
                && assemblyName.FullName != migrationsAssemblyName)
            {
                throw new OperationException(
                    DesignStrings.MigrationsAssemblyMismatch(assemblyName.Name, migrationsAssemblyName));
            }
        }
    }
}
