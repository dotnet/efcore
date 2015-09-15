// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !DNXCORE50

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Design.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Design
{
    public class OperationExecutor : MarshalByRefObject
    {
        private readonly LazyRef<DbContextOperations> _contextOperations;
        private readonly LazyRef<DatabaseOperations> _databaseOperations;
        private readonly LazyRef<MigrationsOperations> _migrationsOperations;

        public OperationExecutor([NotNull] object logHandler, [NotNull] IDictionary args)
        {
            Check.NotNull(logHandler, nameof(logHandler));
            Check.NotNull(args, nameof(args));

            var unwrappedLogHandler = ForwardingProxy.Unwrap<IOperationLogHandler>(logHandler);
            var loggerProvider = new LoggerProvider(name => new CommandLoggerAdapter(name, unwrappedLogHandler));

            var targetPath = (string)args["targetPath"];
            var startupTargetPath = (string)args["startupTargetPath"];

            var projectDir = (string)args["projectDir"];
            var rootNamespace = (string)args["rootNamespace"];

            var startupAssemblyName = AssemblyName.GetAssemblyName(startupTargetPath).Name;

            var assemblyName = AssemblyName.GetAssemblyName(targetPath);
            var assembly = Assembly.Load(assemblyName);

            _contextOperations = new LazyRef<DbContextOperations>(
                () => new DbContextOperations(
                    loggerProvider,
                    assembly,
                    startupAssemblyName));
            _databaseOperations = new LazyRef<DatabaseOperations>(
                () => new DatabaseOperations(
                    loggerProvider,
                    assembly,
                    startupAssemblyName,
                    projectDir,
                    rootNamespace));
            _migrationsOperations = new LazyRef<MigrationsOperations>(
                () => new MigrationsOperations(
                    loggerProvider,
                    assembly,
                    startupAssemblyName,
                    projectDir,
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
                var contextType = (string)args["contextType"];

                Execute(() => executor.AddMigrationImpl(name, contextType));
            }
        }

        private IEnumerable<string> AddMigrationImpl(
            [NotNull] string name,
            [CanBeNull] string contextType)
        {
            Check.NotEmpty(name, nameof(name));

            var files = _migrationsOperations.Value.AddMigration(
                name,
                contextType);

            // NOTE: First file will be opened in VS
            yield return files.MigrationFile;
            yield return files.MetadataFile;
            yield return files.SnapshotFile;
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

                Execute(() => executor.RemoveMigrationImpl(contextType));
            }
        }

        private IEnumerable<string> RemoveMigrationImpl([CanBeNull] string contextType)
        {
            var files = _migrationsOperations.Value
                .RemoveMigration(contextType);

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

                Execute(() => executor.GetContextTypesImpl());
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
            var migrations = _migrationsOperations.Value.GetMigrations(contextType);
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

        public class ReverseEngineer : OperationBase
        {
            public ReverseEngineer([NotNull] OperationExecutor executor, [NotNull] object resultHandler, [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var connectionString = (string)args["connectionString"];
                var provider = (string)args["provider"];
                var relativeOutputDir = (string)args["relativeOutputDir"];
                var useFluentApiOnly = (bool)args["useFluentApiOnly"];

                Execute(() => executor.ReverseEngineerImpl(provider, connectionString, relativeOutputDir, useFluentApiOnly));
            }
        }

        private IEnumerable<string> ReverseEngineerImpl(
            [NotNull] string provider,
            [NotNull] string connectionString,
            [CanBeNull] string outputDir,
            bool useFluentApiOnly)
        {
            Check.NotNull(provider, nameof(provider));
            Check.NotNull(connectionString, nameof(connectionString));

            var files = _databaseOperations.Value.ReverseEngineerAsync(
                    provider, connectionString, outputDir, useFluentApiOnly).Result;

            // NOTE: First file will be opened in VS
            yield return files.ContextFile;

            foreach (var file in files.EntityTypeFiles)
            {
                yield return file;
            }
        }

        public abstract class OperationBase : MarshalByRefObject
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
}

#endif
