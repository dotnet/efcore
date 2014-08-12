// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET451

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Design.Utilities;
using Microsoft.Data.Entity.Migrations.Infrastructure;

namespace Microsoft.Data.Entity.Design
{
    // TODO: Move most of this implementation to MigrationTool. Only serialization and
    //       backwards compatibility should be handled here.
    public class Executor : MarshalByRefObject
    {
        private readonly string _targetDir;
        private readonly string _targetFileName;
        private readonly string _projectDir;
        private readonly string _rootNamespace;

        private readonly MigrationTool _migrationTool;

        public Executor([NotNull] IDictionary args)
        {
            Check.NotNull(args, "args");

            _targetDir = (string)args["targetDir"];
            _targetFileName = (string)args["targetFileName"];
            _projectDir = (string)args["projectDir"];
            _rootNamespace = (string)args["rootNamespace"];

            // TODO: Use _projectDir & _rootNamespace
            _migrationTool = new MigrationTool();
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
            var assemblyFile = Path.Combine(_targetDir, _targetFileName);
            var assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
            var assembly = Assembly.Load(assemblyName);

            var contextNames = GetContextNamesImpl().ToArray();
            if (contextNames.Length == 0)
            {
                throw new InvalidOperationException(Strings.FormatAssemblyDoesNotContainDbContext(assembly.FullName));
            }

            if (string.IsNullOrEmpty(contextName))
            {
                if (contextNames.Length == 1)
                {
                    return (string)contextNames[0]["FullName"];
                }

                throw new InvalidOperationException(
                    Strings.FormatAssemblyContainsMultipleDbContext(assembly.FullName));
            }

            var candidate = contextNames.Select(n => (string)n["FullName"])
                .FirstOrDefault(n => n.Equals(contextName, StringComparison.OrdinalIgnoreCase));
            if (candidate != null)
            {
                return candidate;
            }

            var candidates = contextNames
                .Where(n => ((string)n["Name"]).Equals(contextName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (candidates.Length == 1)
            {
                return (string)candidates[0]["FullName"];
            }
            if (candidates.Length == 0)
            {
                throw new InvalidOperationException(Strings.FormatSpecifiedContextNotFound(contextName));
            }

            throw new InvalidOperationException(Strings.FormatMultipleContextsFound(contextName));
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

            contextName = GetContextNameImpl(contextName);

            var migration = _migrationTool.CreateMigration(
                migrationName,
                Path.Combine(_targetDir, _targetFileName),
                migrationDirectory: Path.Combine(_projectDir, "Migrations"),
                contextTypeName: contextName);

            yield return migration.MigrationFile;
            yield return migration.MigrationMetadataFile;
            yield return migration.SnapshotModelFile;
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
            contextName = GetContextNameImpl(contextName);

            _migrationTool.UpdateDatabase(
                Path.Combine(_targetDir, _targetFileName),
                targetMigrationName: migrationName,
                contextTypeName: contextName);
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
            contextName = GetContextNameImpl(contextName);

            // TODO: Use fromMigration & idempotent
            var statements = _migrationTool.GenerateScript(
                Path.Combine(_targetDir, _targetFileName),
                targetMigrationName: toMigration,
                contextTypeName: contextName);

            return string.Join(Environment.NewLine, statements.Select(s => s.Sql));
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
            var assemblyFile = Path.Combine(_targetDir, _targetFileName);
            var assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
            var assembly = Assembly.Load(assemblyName);

            var contextTypes = _migrationTool.GetContextTypes(assembly);
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

        public virtual IEnumerable<IDictionary> GetMigrationNamesImpl([CanBeNull] string contextName)
        {
            contextName = GetContextNameImpl(contextName);

            var migrations = _migrationTool.GetMigrations(
                Path.Combine(_targetDir, _targetFileName),
                source: MigrationTool.Constants.MigrationSourceLocal,
                contextTypeName: contextName);
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
