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

        public class GetContextType : OperationBase
        {
            public GetContextType([NotNull] Executor executor, [NotNull] object handler, [NotNull] IDictionary args)
                : base(handler)
            {
                Check.NotNull(executor, "executor");
                Check.NotNull(args, "args");

                var name = (string)args["name"];

                Execute(() => executor.GetContextTypeImpl(name));
            }
        }

        public virtual string GetContextTypeImpl([CanBeNull] string name)
        {
            return _migrationTool.GetContextType(name).AssemblyQualifiedName;
        }

        public class AddMigration : OperationBase
        {
            public AddMigration([NotNull] Executor executor, [NotNull] object handler, [NotNull] IDictionary args)
                : base(handler)
            {
                Check.NotNull(executor, "executor");
                Check.NotNull(args, "args");

                var migrationName = (string)args["migrationName"];
                var contextTypeName = (string)args["contextTypeName"];

                Execute(() => executor.AddMigrationImpl(migrationName, contextTypeName));
            }
        }

        public virtual IEnumerable<string> AddMigrationImpl(
            [NotNull] string migrationName,
            [CanBeNull] string contextTypeName)
        {
            Check.NotEmpty(migrationName, "migrationName");

            var migration = _migrationTool.AddMigration(migrationName, _rootNamespace, contextTypeName);

            return _migrationTool.WriteMigration(_projectDir, migration);
        }

        public class ApplyMigration : OperationBase
        {
            public ApplyMigration([NotNull] Executor executor, [NotNull] object handler, [NotNull] IDictionary args)
                : base(handler)
            {
                Check.NotNull(executor, "executor");
                Check.NotNull(args, "args");

                var migrationName = (string)args["migrationName"];
                var contextTypeName = (string)args["contextTypeName"];

                Execute(() => executor.ApplyMigrationImpl(migrationName, contextTypeName));
            }
        }

        public virtual void ApplyMigrationImpl([CanBeNull] string migrationName, [CanBeNull] string contextTypeName)
        {
            _migrationTool.ApplyMigration(migrationName, contextTypeName);
        }

        public class ScriptMigration : OperationBase
        {
            public ScriptMigration(
                [NotNull] Executor executor,
                [NotNull] object handler,
                [NotNull] IDictionary args)
                : base(handler)
            {
                Check.NotNull(executor, "executor");
                Check.NotNull(args, "args");

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
        {
            return _migrationTool.ScriptMigration(fromMigrationName, toMigrationName, idempotent, contextTypeName);
        }

        public class GetContextTypes : OperationBase
        {
            public GetContextTypes([NotNull] Executor executor, [NotNull] object handler, [NotNull] IDictionary args)
                : base(handler)
            {
                Check.NotNull(executor, "executor");
                Check.NotNull(args, "args");

                Execute(() => executor.GetContextTypesImpl());
            }
        }

        public virtual IEnumerable<IDictionary> GetContextTypesImpl()
        {
            var contextTypes = _migrationTool.GetContextTypes().ToArray();
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
            public GetMigrations([NotNull] Executor executor, [NotNull] object handler, [NotNull] IDictionary args)
                : base(handler)
            {
                Check.NotNull(executor, "executor");
                Check.NotNull(args, "args");

                var contextTypeName = (string)args["contextTypeName"];

                Execute(() => executor.GetMigrationsImpl(contextTypeName));
            }
        }

        // TODO: DRY (See GetContextTypesImpl)
        public virtual IEnumerable<IDictionary> GetMigrationsImpl([CanBeNull] string contextTypeName)
        {
            var migrations = _migrationTool.GetMigrations(contextTypeName);
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
                    _handler.OnError(ex.GetType().AssemblyQualifiedName, ex.Message, ex.ToString());
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
