// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET451 || ASPNET50

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Migrations.Infrastructure;

namespace Microsoft.Data.Entity.Commands
{
    public class Executor : MarshalByRefObject
    {
        private readonly string _projectDir;
        private readonly string _rootNamespace;
        private readonly MigrationTool _migrationTool;

        public Executor([NotNull] IDictionary args)
        {
            Check.NotNull(args, "args");

            // TODO: Pass in targetPath
            var targetDir = (string)args["targetDir"];
            var targetFileName = (string)args["targetFileName"];
            var targetPath = Path.Combine(targetDir, targetFileName);

            _projectDir = (string)args["projectDir"];
            _rootNamespace = (string)args["rootNamespace"];

            var assemblyName = AssemblyName.GetAssemblyName(targetPath);
            var assembly = Assembly.Load(assemblyName);
            _migrationTool = new MigrationTool(assembly);
        }

        public class GetContextName : OperationBase
        {
            public GetContextName([NotNull] Executor executor, [NotNull] object handler, [NotNull] IDictionary args)
                : base(handler)
            {
                Check.NotNull(executor, "executor");
                Check.NotNull(args, "args");

                var contextName = (string)args["contextName"];

                Execute(() => executor.GetContextNameImpl(contextName));
            }
        }

        public virtual string GetContextNameImpl([CanBeNull] string contextName)
        {
            return _migrationTool.GetContextType(contextName).FullName;
        }

        public class CreateMigration : OperationBase
        {
            public CreateMigration([NotNull] Executor executor, [NotNull] object handler, [NotNull] IDictionary args)
                : base(handler)
            {
                Check.NotNull(executor, "executor");
                Check.NotNull(args, "args");

                var migrationName = (string)args["migrationName"];
                var contextName = (string)args["contextName"];

                Execute(() => executor.CreateMigrationImpl(migrationName, contextName));
            }
        }

        public virtual IEnumerable<string> CreateMigrationImpl(
            [NotNull] string migrationName,
            [CanBeNull] string contextName)
        {
            Check.NotEmpty(migrationName, "migrationName");

            var migration = _migrationTool.CreateMigration(migrationName, _rootNamespace, contextName);

            return _migrationTool.WriteMigration(_projectDir, migration);
        }

        public class PublishMigration : OperationBase
        {
            public PublishMigration([NotNull] Executor executor, [NotNull] object handler, [NotNull] IDictionary args)
                : base(handler)
            {
                Check.NotNull(executor, "executor");
                Check.NotNull(args, "args");

                var migrationName = (string)args["migrationName"];
                var contextName = (string)args["contextName"];

                Execute(() => executor.PublishMigrationImpl(migrationName, contextName));
            }
        }

        public virtual void PublishMigrationImpl([CanBeNull] string migrationName, [CanBeNull] string contextName)
        {
            _migrationTool.UpdateDatabase(migrationName, contextName);
        }

        public class CreateMigrationScript : OperationBase
        {
            public CreateMigrationScript(
                [NotNull] Executor executor,
                [NotNull] object handler,
                [NotNull] IDictionary args)
                : base(handler)
            {
                Check.NotNull(executor, "executor");
                Check.NotNull(args, "args");

                var fromMigration = (string)args["fromMigration"];
                var toMigration = (string)args["toMigration"];
                var idempotent = (bool)args["idempotent"];
                var contextName = (string)args["contextName"];

                Execute(() => executor.CreateMigrationScriptImpl(fromMigration, toMigration, idempotent, contextName));
            }
        }

        public virtual string CreateMigrationScriptImpl(
            [CanBeNull] string fromMigration,
            [CanBeNull] string toMigration,
            bool idempotent,
            [CanBeNull] string contextName)
        {
            return _migrationTool.GenerateScript(fromMigration, toMigration, idempotent, contextName);
        }

        public class GetContextNames : OperationBase
        {
            public GetContextNames([NotNull] Executor executor, [NotNull] object handler, [NotNull] IDictionary args)
                : base(handler)
            {
                Check.NotNull(executor, "executor");
                Check.NotNull(args, "args");

                Execute(() => executor.GetContextNamesImpl());
            }
        }

        public virtual IEnumerable<IDictionary> GetContextNamesImpl()
        {
            var contextTypes = _migrationTool.GetContextTypes().ToArray();
            var groups = contextTypes.GroupBy(t => t.Name).ToArray();

            return contextTypes.Select(
                t =>
                {
                    var result = new Hashtable();
                    result["FullName"] = t.FullName;
                    result["Name"] = t.Name;
                    result["SafeName"] = groups.Count(g => g.Key == t.Name) == 1 ? t.Name : t.FullName;

                    return result;
                });
        }

        public class GetMigrationNames : OperationBase
        {
            public GetMigrationNames([NotNull] Executor executor, [NotNull] object handler, [NotNull] IDictionary args)
                : base(handler)
            {
                Check.NotNull(executor, "executor");
                Check.NotNull(args, "args");

                var contextName = (string)args["contextName"];

                Execute(() => executor.GetMigrationNamesImpl(contextName));
            }
        }

        // TODO: DRY (See GetContextNamesImpl)
        public virtual IEnumerable<IDictionary> GetMigrationNamesImpl([CanBeNull] string contextName)
        {
            var migrations = _migrationTool.GetMigrations(contextName);
            var groups = migrations.GroupBy(m => m.GetMigrationName()).ToArray();

            return migrations.Select(
                m =>
                {
                    var migrationName = m.GetMigrationName();

                    var result = new Hashtable();
                    result["MigrationId"] = m.MigrationId;
                    result["MigrationName"] = migrationName;
                    result["SafeName"] = groups.Count(g => g.Key == migrationName) == 1 ? migrationName : m.MigrationId;

                    return result;
                });
        }

        public abstract class OperationBase : MarshalByRefObject
        {
            private readonly IHandler _handler;

            protected OperationBase([NotNull] object handler)
            {
                Check.NotNull(handler, "handler");

                _handler = handler as IHandler ?? new ForwardingProxy<IHandler>(handler).GetTransparentProxy();
            }

            public virtual IHandler Handler
            {
                get { return _handler; }
            }

            public virtual void Execute([NotNull] Action action)
            {
                Check.NotNull(action, "action");

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _handler.OnError(ex.GetType().FullName, ex.Message, ex.ToString());
                }
            }

            public virtual void Execute<T>([NotNull] Func<T> action)
            {
                Check.NotNull(action, "action");

                Execute(() => _handler.OnResult(action()));
            }

            public virtual void Execute<T>([NotNull] Func<IEnumerable<T>> action)
            {
                Check.NotNull(action, "action");

                Execute(() => _handler.OnResult(action().ToArray()));
            }
        }
    }
}

#endif
