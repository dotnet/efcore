// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     A version-resilient, AppDomain-and-reflection-friendly facade for command operations.
    /// </summary>
    public partial class OperationExecutor
    {
        private readonly LazyRef<DbContextOperations> _contextOperations;
        private readonly LazyRef<DatabaseOperations> _databaseOperations;
        private readonly LazyRef<MigrationsOperations> _migrationsOperations;
        private readonly string _projectDir;

        public OperationExecutor([NotNull] object reportHandler, [NotNull] IDictionary args)
        {
            Check.NotNull(reportHandler, nameof(reportHandler));
            Check.NotNull(args, nameof(args));

            var unwrappedReportHandler = ForwardingProxy.Unwrap<IOperationReportHandler>(reportHandler);
            var reporter = new OperationReporter(unwrappedReportHandler);

            var targetName = (string)args["targetName"];
            var startupTargetName = (string)args["startupTargetName"];
            var environment = (string)args["environment"];
            _projectDir = (string)args["projectDir"];
            var contentRootPath = (string)args["contentRootPath"];
            var rootNamespace = (string)args["rootNamespace"];

            // NOTE: LazyRef is used so any exceptions get passed to the resultHandler
            var startupAssembly = new LazyRef<Assembly>(
                () => Assembly.Load(new AssemblyName(startupTargetName)));
            var assembly = new LazyRef<Assembly>(
                () =>
                    {
                        try
                        {
                            return Assembly.Load(new AssemblyName(targetName));
                        }
                        catch (Exception ex)
                        {
                            throw new OperationException(
                                DesignStrings.UnreferencedAssembly(targetName, startupTargetName),
                                ex);
                        }
                    });
            _contextOperations = new LazyRef<DbContextOperations>(
                () => new DbContextOperations(
                    reporter,
                    assembly.Value,
                    startupAssembly.Value,
                    environment,
                    contentRootPath));
            _databaseOperations = new LazyRef<DatabaseOperations>(
                () => new DatabaseOperations(
                    reporter,
                    startupAssembly.Value,
                    environment,
                    _projectDir,
                    contentRootPath,
                    rootNamespace));
            _migrationsOperations = new LazyRef<MigrationsOperations>(
                () => new MigrationsOperations(
                    reporter,
                    assembly.Value,
                    startupAssembly.Value,
                    environment,
                    _projectDir,
                    contentRootPath,
                    rootNamespace));
        }

        public class GetContextType : OperationBase
        {
            public GetContextType(
                [NotNull] OperationExecutor executor,
                [NotNull] object resultHandler,
                [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var name = (string)args["name"];

                Execute(() => executor.GetContextTypeImpl(name));
            }
        }

        private string GetContextTypeImpl([CanBeNull] string name) =>
            _contextOperations.Value.GetContextType(name).AssemblyQualifiedName;

        public class AddMigration : OperationBase
        {
            public AddMigration(
                [NotNull] OperationExecutor executor,
                [NotNull] object resultHandler,
                [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var name = (string)args["name"];
                var outputDir = (string)args["outputDir"];
                var contextType = (string)args["contextType"];

                Execute(() => executor.AddMigrationImpl(name, outputDir, contextType));
            }
        }

        private IDictionary AddMigrationImpl(
            [NotNull] string name,
            [CanBeNull] string outputDir,
            [CanBeNull] string contextType)
        {
            Check.NotEmpty(name, nameof(name));

            // In package manager console, relative outputDir is relative to project directory
            if (!string.IsNullOrWhiteSpace(outputDir)
                && !Path.IsPathRooted(outputDir))
            {
                outputDir = Path.GetFullPath(Path.Combine(_projectDir, outputDir));
            }

            var files = _migrationsOperations.Value.AddMigration(
                name,
                outputDir,
                contextType);

            return new Hashtable
            {
                ["MigrationFile"] = files.MigrationFile,
                ["MetadataFile"] = files.MetadataFile,
                ["SnapshotFile"] = files.SnapshotFile
            };
        }

        public class GetContextInfo : OperationBase
        {
            public GetContextInfo([NotNull] OperationExecutor executor, [NotNull] object resultHandler, [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var contextType = (string)args["contextType"];
                Execute(() => executor.GetContextInfoImpl(contextType));
            }
        }

        private IDictionary GetContextInfoImpl([CanBeNull] string contextType)
        {
            var databaseInfo = _contextOperations.Value.GetContextInfo(contextType);
            return new Hashtable
            {
                ["DatabaseName"] = databaseInfo.DatabaseName,
                ["DataSource"] = databaseInfo.DataSource
            };
        }

        public class UpdateDatabase : OperationBase
        {
            public UpdateDatabase([NotNull] OperationExecutor executor, [NotNull] object resultHandler, [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var targetMigration = (string)args["targetMigration"];
                var contextType = (string)args["contextType"];

                Execute(() => executor.UpdateDatabaseImpl(targetMigration, contextType));
            }
        }

        private void UpdateDatabaseImpl([CanBeNull] string targetMigration, [CanBeNull] string contextType) =>
            _migrationsOperations.Value.UpdateDatabase(targetMigration, contextType);

        public class ScriptMigration : OperationBase
        {
            public ScriptMigration(
                [NotNull] OperationExecutor executor,
                [NotNull] object resultHandler,
                [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var fromMigration = (string)args["fromMigration"];
                var toMigration = (string)args["toMigration"];
                var idempotent = (bool)args["idempotent"];
                var contextType = (string)args["contextType"];

                Execute(() => executor.ScriptMigrationImpl(fromMigration, toMigration, idempotent, contextType));
            }
        }

        private string ScriptMigrationImpl(
            [CanBeNull] string fromMigration,
            [CanBeNull] string toMigration,
            bool idempotent,
            [CanBeNull] string contextType)
            => _migrationsOperations.Value.ScriptMigration(
                fromMigration,
                toMigration,
                idempotent,
                contextType);

        public class RemoveMigration : OperationBase
        {
            public RemoveMigration(
                [NotNull] OperationExecutor executor,
                [NotNull] object resultHandler,
                [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var contextType = (string)args["contextType"];
                var force = (bool)args["force"];

                Execute(() => executor.RemoveMigrationImpl(contextType, force));
            }
        }

        private IEnumerable<string> RemoveMigrationImpl([CanBeNull] string contextType, bool force)
        {
            var files = _migrationsOperations.Value
                .RemoveMigration(contextType, force);

            if (files.MigrationFile != null)
            {
                yield return files.MigrationFile;
            }

            if (files.MetadataFile != null)
            {
                yield return files.MetadataFile;
            }

            if (files.SnapshotFile != null)
            {
                yield return files.SnapshotFile;
            }
        }

        public class GetContextTypes : OperationBase
        {
            public GetContextTypes([NotNull] OperationExecutor executor, [NotNull] object resultHandler, [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                Execute(executor.GetContextTypesImpl);
            }
        }

        private IEnumerable<IDictionary> GetContextTypesImpl()
        {
            var contextTypes = _contextOperations.Value.GetContextTypes().ToList();
            var nameGroups = contextTypes.GroupBy(t => t.Name).ToList();
            var fullNameGroups = contextTypes.GroupBy(t => t.FullName).ToList();

            return contextTypes.Select(
                t => new Hashtable
                {
                    ["AssemblyQualifiedName"] = t.AssemblyQualifiedName,
                    ["FullName"] = t.FullName,
                    ["Name"] = t.Name,
                    ["SafeName"] = nameGroups.Count(g => g.Key == t.Name) == 1
                        ? t.Name
                        : fullNameGroups.Count(g => g.Key == t.FullName) == 1
                            ? t.FullName
                            : t.AssemblyQualifiedName
                });
        }

        public class GetMigrations : OperationBase
        {
            public GetMigrations([NotNull] OperationExecutor executor, [NotNull] object resultHandler, [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var contextType = (string)args["contextType"];

                Execute(() => executor.GetMigrationsImpl(contextType));
            }
        }

        private IEnumerable<IDictionary> GetMigrationsImpl([CanBeNull] string contextType)
        {
            var migrations = _migrationsOperations.Value.GetMigrations(contextType).ToList();
            var nameGroups = migrations.GroupBy(m => m.Name).ToList();

            return migrations.Select(
                m => new Hashtable
                {
                    ["Id"] = m.Id,
                    ["Name"] = m.Name,
                    ["SafeName"] = nameGroups.Count(g => g.Key == m.Name) == 1
                        ? m.Name
                        : m.Id
                });
        }

        public class ScaffoldContext : OperationBase
        {
            public ScaffoldContext([NotNull] OperationExecutor executor, [NotNull] object resultHandler, [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var connectionString = (string)args["connectionString"];
                var provider = (string)args["provider"];
                var outputDir = (string)args["outputDir"];
                var dbContextClassName = (string)args["dbContextClassName"];
                var schemaFilters = (IEnumerable<string>)args["schemaFilters"];
                var tableFilters = (IEnumerable<string>)args["tableFilters"];
                var useDataAnnotations = (bool)args["useDataAnnotations"];
                var overwriteFiles = (bool)args["overwriteFiles"];

                Execute(() => executor.ScaffoldContextImpl(provider,
                    connectionString, outputDir, dbContextClassName,
                    schemaFilters, tableFilters, useDataAnnotations, overwriteFiles));
            }
        }

        private IEnumerable<string> ScaffoldContextImpl(
            [NotNull] string provider,
            [NotNull] string connectionString,
            [CanBeNull] string outputDir,
            [CanBeNull] string dbContextClassName,
            [NotNull] IEnumerable<string> schemaFilters,
            [NotNull] IEnumerable<string> tableFilters,
            bool useDataAnnotations,
            bool overwriteFiles)
        {
            Check.NotNull(provider, nameof(provider));
            Check.NotNull(connectionString, nameof(connectionString));
            Check.NotNull(schemaFilters, nameof(schemaFilters));
            Check.NotNull(tableFilters, nameof(tableFilters));

            var files = _databaseOperations.Value.ScaffoldContextAsync(
                provider, connectionString, outputDir, dbContextClassName,
                schemaFilters, tableFilters, useDataAnnotations, overwriteFiles).Result;

            // NOTE: First file will be opened in VS
            yield return files.ContextFile;

            foreach (var file in files.EntityTypeFiles)
            {
                yield return file;
            }
        }

        public class DropDatabase : OperationBase
        {
            public DropDatabase(
                [NotNull] OperationExecutor executor,
                [NotNull] object resultHandler,
                [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var contextType = (string)args["contextType"];

                Execute(() => executor.DropDatabaseImpl(contextType));
            }
        }

        private void DropDatabaseImpl(string contextType)
            => _contextOperations.Value.DropDatabase(contextType);

        public abstract partial class OperationBase
        {
            private readonly IOperationResultHandler _resultHandler;

            protected OperationBase([NotNull] object resultHandler)
            {
                Check.NotNull(resultHandler, nameof(resultHandler));

                _resultHandler = ForwardingProxy.Unwrap<IOperationResultHandler>(resultHandler);
            }

            public virtual void Execute([NotNull] Action action)
            {
                Check.NotNull(action, nameof(action));

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _resultHandler.OnError(ex.GetType().FullName, ex.Message, ex.ToString());
                }
            }

            public virtual void Execute<T>([NotNull] Func<T> action)
            {
                Check.NotNull(action, nameof(action));

                Execute(() => _resultHandler.OnResult(action()));
            }

            public virtual void Execute<T>([NotNull] Func<IEnumerable<T>> action)
            {
                Check.NotNull(action, nameof(action));

                Execute(() => _resultHandler.OnResult(action().ToArray()));
            }
        }
    }

#if NET451
    public partial class OperationExecutor : MarshalByRefObject
    {
        public partial class OperationBase : MarshalByRefObject
        {
        }
    }
#endif
}
