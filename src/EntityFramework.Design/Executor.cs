// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Design.Utilities;

namespace Microsoft.Data.Entity.Design
{
    // TODO: Validate arguments
    public class Executor : MarshalByRefObject
    {
        private readonly string _targetPath;

        public Executor([NotNull] IDictionary args)
        {
            Check.NotNull(args, "args");

            _targetPath = (string)args["targetPath"];
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
                var migrationsDir = (string)args["migrationsDir"];

                Execute(() => executor.CreateMigrationImpl(migrationName, contextName, migrationsDir));
            }
        }

        public virtual IEnumerable<string> CreateMigrationImpl(
            [NotNull] string migrationName,
            [CanBeNull] string contextName,
            [CanBeNull] string migrationsDir)
        {
            Check.NotNull(migrationName, "migrationName");

            var migration = new MigrationTool().CreateMigration(
                migrationName,
                _targetPath,
                migrationDirectory: migrationsDir,
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

                var targetMigration = (string)args["targetMigration"];
                var contextName = (string)args["contextName"];

                Execute(() => executor.PublishMigrationImpl(targetMigration, contextName));
            }
        }

        public virtual void PublishMigrationImpl([CanBeNull] string targetMigration, [CanBeNull] string contextName)
        {
            new MigrationTool().UpdateDatabase(
                _targetPath,
                targetMigrationName: targetMigration,
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

                var targetMigration = (string)args["targetMigration"];
                var contextName = (string)args["contextName"];

                Execute(() => executor.CreateMigrationScriptImpl(targetMigration, contextName));
            }
        }

        public virtual string CreateMigrationScriptImpl(
            [CanBeNull] string targetMigration,
            [CanBeNull] string contextName)
        {
            var statements = new MigrationTool().GenerateScript(
                _targetPath,
                targetMigrationName: targetMigration,
                contextTypeName: contextName);

            return string.Join(Environment.NewLine, statements.Select(s => s.Sql));
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
