// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !DNXCORE50

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Commands
{
    public class Executor : MarshalByRefObject
    {
        private readonly string _projectDir;
        private readonly string _rootNamespace;
        private readonly string _startupAssemblyName;
        private readonly LoggerProvider _loggerProvider;
        private readonly Assembly _migrationTargetAssembly;

        public Executor([NotNull] object logHandler, [NotNull] IDictionary args)
        {
            Check.NotNull(logHandler, nameof(logHandler));
            Check.NotNull(args, nameof(args));

            var unwrappedLogHandler = logHandler as ILogHandler
                                      ?? new ForwardingProxy<ILogHandler>(logHandler).GetTransparentProxy();
            _loggerProvider = new LoggerProvider(name => new CommandLoggerAdapter(name, unwrappedLogHandler));

            var targetPath = (string)args["targetPath"];
            var startupTargetPath = (string)args["startupTargetPath"];

            _projectDir = (string)args["projectDir"];
            _rootNamespace = (string)args["rootNamespace"];

            _startupAssemblyName = AssemblyName.GetAssemblyName(startupTargetPath).Name;

            var assemblyName = AssemblyName.GetAssemblyName(targetPath);
            _migrationTargetAssembly = Assembly.Load(assemblyName);
        }

        // Load MigrationTool and DatabaseTool lazily to avoid loading their dependencies
        // when the other one is being run (this prevents problems loading Microsoft.AspNet.Razor
        // which is not available on netcore50).
        public virtual MigrationTool CreateMigrationTool()
        {
            return new MigrationTool(_loggerProvider, _migrationTargetAssembly);
        }

        public virtual DatabaseTool CreateDatabaseTool()
        {
            return new DatabaseTool(null, _loggerProvider);
        }

        public class GetContextType : OperationBase
        {
            public GetContextType([NotNull] Executor executor, [NotNull] object resultHandler, [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var name = (string)args["name"];

                Execute(() => executor.GetContextTypeImpl(name));
            }
        }

        public virtual string GetContextTypeImpl([CanBeNull] string name) =>
            CreateMigrationTool().GetContextType(name).AssemblyQualifiedName;

        public class AddMigration : OperationBase
        {
            public AddMigration([NotNull] Executor executor, [NotNull] object resultHandler, [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var migrationName = (string)args["migrationName"];
                var contextTypeName = (string)args["contextTypeName"];

                Execute(() => executor.AddMigrationImpl(migrationName, contextTypeName));
            }
        }

        public virtual IEnumerable<string> AddMigrationImpl(
            [NotNull] string migrationName,
            [CanBeNull] string contextTypeName)
        {
            Check.NotEmpty(migrationName, nameof(migrationName));

            var files = CreateMigrationTool().AddMigration(
                migrationName,
                contextTypeName,
                _startupAssemblyName,
                _rootNamespace,
                _projectDir);

            // NOTE: First file will be opened in VS
            yield return files.MigrationFile;
            yield return files.MigrationMetadataFile;
            yield return files.ModelSnapshotFile;
        }

        public class ApplyMigration : OperationBase
        {
            public ApplyMigration([NotNull] Executor executor, [NotNull] object resultHandler, [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var migrationName = (string)args["migrationName"];
                var contextTypeName = (string)args["contextTypeName"];

                Execute(() => executor.ApplyMigrationImpl(migrationName, contextTypeName));
            }
        }

        public virtual void ApplyMigrationImpl([CanBeNull] string migrationName, [CanBeNull] string contextTypeName) =>
            CreateMigrationTool().ApplyMigration(migrationName, contextTypeName, _startupAssemblyName);

        public class ScriptMigration : OperationBase
        {
            public ScriptMigration(
                [NotNull] Executor executor,
                [NotNull] object resultHandler,
                [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var fromMigrationName = (string)args["fromMigrationName"];
                var toMigrationName = (string)args["toMigrationName"];
                var idempotent = (bool)args["idempotent"];
                var contextTypeName = (string)args["contextTypeName"];

                Execute(() => executor.ScriptMigrationImpl(fromMigrationName, toMigrationName, idempotent, contextTypeName));
            }
        }

        public virtual string ScriptMigrationImpl(
            [CanBeNull] string fromMigrationName,
            [CanBeNull] string toMigrationName,
            bool idempotent,
            [CanBeNull] string contextTypeName)
            => CreateMigrationTool().ScriptMigration(
                fromMigrationName,
                toMigrationName,
                idempotent,
                contextTypeName,
                _startupAssemblyName);

        public class RemoveMigration : OperationBase
        {
            public RemoveMigration(
                [NotNull] Executor executor,
                [NotNull] object resultHandler,
                [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var contextTypeName = (string)args["contextTypeName"];

                Execute(() => executor.RemoveMigrationImpl(contextTypeName));
            }
        }

        public virtual IEnumerable<string> RemoveMigrationImpl([CanBeNull] string contextTypeName)
        {
            var files = CreateMigrationTool()
                .RemoveMigration(contextTypeName, _startupAssemblyName, _rootNamespace, _projectDir);

            if (files.MigrationFile != null)
            {
                yield return files.MigrationFile;
            }

            if (files.MigrationMetadataFile != null)
            {
                yield return files.MigrationMetadataFile;
            }

            if (files.ModelSnapshotFile != null)
            {
                yield return files.ModelSnapshotFile;
            }
        }

        public class GetContextTypes : OperationBase
        {
            public GetContextTypes([NotNull] Executor executor, [NotNull] object resultHandler, [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                Execute(() => executor.GetContextTypesImpl());
            }
        }

        public virtual IEnumerable<IDictionary> GetContextTypesImpl()
        {
            var contextTypes = CreateMigrationTool().GetContextTypes().ToArray();
            var nameGroups = contextTypes.GroupBy(t => t.Name).ToArray();
            var fullNameGroups = contextTypes.GroupBy(t => t.FullName).ToArray();

            return contextTypes.Select(
                t =>
                    {
                        var result = new Hashtable();
                        result["AssemblyQualifiedName"] = t.AssemblyQualifiedName;
                        result["FullName"] = t.FullName;
                        result["Name"] = t.Name;
                        result["SafeName"] = nameGroups.Count(g => g.Key == t.Name) == 1
                            ? t.Name
                            : fullNameGroups.Count(g => g.Key == t.FullName) == 1
                                ? t.FullName
                                : t.AssemblyQualifiedName;

                        return result;
                    });
        }

        public class GetMigrations : OperationBase
        {
            public GetMigrations([NotNull] Executor executor, [NotNull] object resultHandler, [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var contextTypeName = (string)args["contextTypeName"];

                Execute(() => executor.GetMigrationsImpl(contextTypeName));
            }
        }

        public virtual IEnumerable<IDictionary> GetMigrationsImpl([CanBeNull] string contextTypeName) =>
            CreateMigrationTool().GetMigrations(contextTypeName, _startupAssemblyName).Select(
                id => new Hashtable {["MigrationId"] = id,["MigrationName"] = id,["SafeName"] = id });

        public class ReverseEngineer : OperationBase
        {
            public ReverseEngineer([NotNull] Executor executor, [NotNull] object resultHandler, [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var connectionString = (string)args["connectionString"];
                var providerAssemblyName = (string)args["provider"];
                var relativeOutputDir = (string)(args.Contains("relativeOutputDir") ? args["relativeOutputDir"] : null);
                var useFluentApiOnly = (bool)args["useFluentApiOnly"];

                Execute(() => executor
                    .ReverseEngineerImplAsync(providerAssemblyName, connectionString, relativeOutputDir, useFluentApiOnly)
                    .GetAwaiter().GetResult());
            }
        }

        public virtual Task<IReadOnlyList<string>> ReverseEngineerImplAsync(
            [NotNull] string providerAssemblyName,
            [NotNull] string connectionString,
            [CanBeNull] string relativeOutputDir,
            bool useFluentApiOnly,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return CreateDatabaseTool().ReverseEngineerAsync(
                providerAssemblyName, connectionString, _rootNamespace,
                _projectDir, relativeOutputDir, useFluentApiOnly, cancellationToken);
        }

        public abstract class OperationBase : MarshalByRefObject
        {
            private readonly IResultHandler _resultHandler;

            protected OperationBase([NotNull] object resultHandler)
            {
                Check.NotNull(resultHandler, nameof(resultHandler));

                _resultHandler = resultHandler as IResultHandler
                                 ?? new ForwardingProxy<IResultHandler>(resultHandler).GetTransparentProxy();
            }

            public virtual IResultHandler ResultHandler => _resultHandler;

            public virtual void Execute([NotNull] Action action)
            {
                Check.NotNull(action, nameof(action));

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _resultHandler.OnError(ex.GetType().AssemblyQualifiedName, ex.Message, ex.ToString());
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
