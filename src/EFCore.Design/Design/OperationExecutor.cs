// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     <para>
    ///         A facade for design-time operations.
    ///     </para>
    ///     <para>
    ///         Use the <c>CreateInstance</c> overloads on <see cref="AppDomain" /> and <see cref="Activator" /> with the
    ///         nested types to execute operations.
    ///     </para>
    /// </summary>
    public class OperationExecutor : MarshalByRefObject
    {
        private readonly LazyRef<DbContextOperations> _contextOperations;
        private readonly LazyRef<DatabaseOperations> _databaseOperations;
        private readonly LazyRef<MigrationsOperations> _migrationsOperations;
        private readonly string _projectDir;

        /// <summary>
        ///     <para>Initializes a new instance of the <see cref="OperationExecutor" /> class.</para>
        ///     <para>The arguments supported by <paramref name="args" /> are:</para>
        ///     <para><c>targetName</c>--The assembly name of the target project.</para>
        ///     <para><c>startupTargetName</c>--The assembly name of the startup project.</para>
        ///     <para><c>projectDir</c>--The target project's root directory.</para>
        ///     <para><c>rootNamespace</c>--The target project's root namespace.</para>
        /// </summary>
        /// <param name="reportHandler"> The <see cref="IOperationReportHandler" />. </param>
        /// <param name="args"> The executor arguments. </param>
        public OperationExecutor([NotNull] object reportHandler, [NotNull] IDictionary args)
        {
            Check.NotNull(reportHandler, nameof(reportHandler));
            Check.NotNull(args, nameof(args));

            var unwrappedReportHandler = ForwardingProxy.Unwrap<IOperationReportHandler>(reportHandler);
            var reporter = new OperationReporter(unwrappedReportHandler);

            var targetName = (string)args["targetName"];
            var startupTargetName = (string)args["startupTargetName"];
            _projectDir = (string)args["projectDir"];
            var rootNamespace = (string)args["rootNamespace"];
            var language = (string)args["language"];
            var toolsVersion = (string)args["toolsVersion"];

            // TODO: Flow in from tools (issue #8332)
            var designArgs = Array.Empty<string>();

            var runtimeVersion = ProductInfo.GetVersion();
            if (toolsVersion != null
                && new SemanticVersionComparer().Compare(toolsVersion, runtimeVersion) < 0)
            {
                reporter.WriteWarning(DesignStrings.VersionMismatch(toolsVersion, runtimeVersion));
            }

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
                    designArgs));
            _databaseOperations = new LazyRef<DatabaseOperations>(
                () => new DatabaseOperations(
                    reporter,
                    assembly.Value,
                    startupAssembly.Value,
                    _projectDir,
                    rootNamespace,
                    language,
                    designArgs));
            _migrationsOperations = new LazyRef<MigrationsOperations>(
                () => new MigrationsOperations(
                    reporter,
                    assembly.Value,
                    startupAssembly.Value,
                    _projectDir,
                    rootNamespace,
                    language,
                    designArgs));
        }

        /// <summary>
        ///     Represents an operation to add a new migration.
        /// </summary>
        public class AddMigration : OperationBase
        {
            /// <summary>
            ///     <para>Initializes a new instance of the <see cref="AddMigration" /> class.</para>
            ///     <para>The arguments supported by <paramref name="args" /> are:</para>
            ///     <para><c>name</c>--The name of the migration.</para>
            ///     <para>
            ///         <c>outputDir</c>--The directory (and sub-namespace) to use. Paths are relative to the project directory. Defaults to
            ///         "Migrations".
            ///     </para>
            ///     <para><c>contextType</c>--The <see cref="DbContext" /> type to use.</para>
            /// </summary>
            /// <param name="executor"> The operation executor. </param>
            /// <param name="resultHandler"> The <see cref="IOperationResultHandler" />. </param>
            /// <param name="args"> The operation arguments. </param>
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

        /// <summary>
        ///     Represents an operation to get information about a <see cref="DbContext" /> type.
        /// </summary>
        public class GetContextInfo : OperationBase
        {
            /// <summary>
            ///     <para>Initializes a new instance of the <see cref="GetContextInfo" /> class.</para>
            ///     <para>The arguments supported by <paramref name="args" /> are:</para>
            ///     <para><c>contextType</c>--The <see cref="DbContext" /> type to use.</para>
            /// </summary>
            /// <param name="executor"> The operation executor. </param>
            /// <param name="resultHandler"> The <see cref="IOperationResultHandler" />. </param>
            /// <param name="args"> The operation arguments. </param>
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
            var info = _contextOperations.Value.GetContextInfo(contextType);
            return new Hashtable
            {
                ["ProviderName"] = info.ProviderName,
                ["DatabaseName"] = info.DatabaseName,
                ["DataSource"] = info.DataSource,
                ["Options"] = info.Options
            };
        }

        /// <summary>
        ///     Represents an operation to update the database to a specified migration.
        /// </summary>
        public class UpdateDatabase : OperationBase
        {
            /// <summary>
            ///     <para>Initializes a new instance of the <see cref="UpdateDatabase" /> class.</para>
            ///     <para>The arguments supported by <paramref name="args" /> are:</para>
            ///     <para>
            ///         <c>targetMigration</c>--The target <see cref="Migration" />. If <see cref="Migration.InitialDatabase" />, all migrations will be
            ///         reverted. Defaults to the last migration.
            ///     </para>
            ///     <para><c>contextType</c>--The <see cref="DbContext" /> to use.</para>
            /// </summary>
            /// <param name="executor"> The operation executor. </param>
            /// <param name="resultHandler"> The <see cref="IOperationResultHandler" />. </param>
            /// <param name="args"> The operation arguments. </param>
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

        /// <summary>
        ///     Represents an operation to generate a SQL script from migrations.
        /// </summary>
        public class ScriptMigration : OperationBase
        {
            /// <summary>
            ///     <para>Initializes a new instance of the <see cref="ScriptMigration" /> class.</para>
            ///     <para>The arguments supported by <paramref name="args" /> are:</para>
            ///     <para><c>fromMigration</c>--The starting migration. Defaults to <see cref="Migration.InitialDatabase" />.</para>
            ///     <para><c>toMigration</c>--The ending migration. Defaults to the last migration.</para>
            ///     <para><c>idempotent</c>--Generate a script that can be used on a database at any migration.</para>
            ///     <para><c>contextType</c>--The <see cref="DbContext" /> to use.</para>
            /// </summary>
            /// <param name="executor"> The operation executor. </param>
            /// <param name="resultHandler"> The <see cref="IOperationResultHandler" />. </param>
            /// <param name="args"> The operation arguments. </param>
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

        /// <summary>
        ///     Represents an operation to remove the last migration.
        /// </summary>
        public class RemoveMigration : OperationBase
        {
            /// <summary>
            ///     <para>Initializes a new instance of the <see cref="RemoveMigration" /> class.</para>
            ///     <para>The arguments supported by <paramref name="args" /> are:</para>
            ///     <para><c>contextType</c>--The <see cref="DbContext" /> to use.</para>
            ///     <para><c>force</c>--Don't check to see if the migration has been applied to the database.</para>
            /// </summary>
            /// <param name="executor"> The operation executor. </param>
            /// <param name="resultHandler"> The <see cref="IOperationResultHandler" />. </param>
            /// <param name="args"> The operation arguments. </param>
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

        private IDictionary RemoveMigrationImpl([CanBeNull] string contextType, bool force)
        {
            var files = _migrationsOperations.Value
                .RemoveMigration(contextType, force);

            return new Hashtable
            {
                ["MigrationFile"] = files.MigrationFile,
                ["MetadataFile"] = files.MetadataFile,
                ["SnapshotFile"] = files.SnapshotFile
            };
        }

        /// <summary>
        ///     Represents an operation to list available <see cref="DbContext" /> types.
        /// </summary>
        public class GetContextTypes : OperationBase
        {
            /// <summary>
            ///     <para>Initializes a new instance of the <see cref="GetContextTypes" /> class.</para>
            ///     <para>No arguments are currently supported by <paramref name="args" />.</para>
            /// </summary>
            /// <param name="executor"> The operation executor. </param>
            /// <param name="resultHandler"> The <see cref="IOperationResultHandler" />. </param>
            /// <param name="args"> The operation arguments. </param>
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

        /// <summary>
        ///     Represents an operation to list available migrations.
        /// </summary>
        public class GetMigrations : OperationBase
        {
            /// <summary>
            ///     <para>Initializes a new instance of the <see cref="GetMigrations" /> class.</para>
            ///     <para>The arguments supported by <paramref name="args" /> are:</para>
            ///     <para><c>contextType</c>--The <see cref="DbContext" /> to use.</para>
            /// </summary>
            /// <param name="executor"> The operation executor. </param>
            /// <param name="resultHandler"> The <see cref="IOperationResultHandler" />. </param>
            /// <param name="args"> The operation arguments. </param>
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

        /// <summary>
        ///     Represents an operation to scaffold a <see cref="DbContext" /> and entity types for a database.
        /// </summary>
        public class ScaffoldContext : OperationBase
        {
            /// <summary>
            ///     <para>Initializes a new instance of the <see cref="ScaffoldContext" /> class.</para>
            ///     <para>The arguments supported by <paramref name="args" /> are:</para>
            ///     <para><c>connectionString</c>--The connection string to the database.</para>
            ///     <para><c>provider</c>--The provider to use.</para>
            ///     <para><c>outputDir</c>--The directory to put files in. Paths are relative to the project directory.</para>
            ///     <para><c>outputDbContextDir</c>--The directory to put DbContext file in. Paths are relative to the project directory.</para>
            ///     <para><c>dbContextClassName</c>--The name of the DbContext to generate.</para>
            ///     <para><c>schemaFilters</c>--The schemas of tables to generate entity types for.</para>
            ///     <para><c>tableFilters</c>--The tables to generate entity types for.</para>
            ///     <para><c>useDataAnnotations</c>--Use attributes to configure the model (where possible). If false, only the fluent API is used.</para>
            ///     <para><c>overwriteFiles</c>--Overwrite existing files.</para>
            ///     <para><c>useDatabaseNames</c>--Use table and column names directly from the database.</para>
            /// </summary>
            /// <param name="executor"> The operation executor. </param>
            /// <param name="resultHandler"> The <see cref="IOperationResultHandler" />. </param>
            /// <param name="args"> The operation arguments. </param>
            public ScaffoldContext([NotNull] OperationExecutor executor, [NotNull] object resultHandler, [NotNull] IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(args, nameof(args));

                var connectionString = (string)args["connectionString"];
                var provider = (string)args["provider"];
                var outputDir = (string)args["outputDir"];
                var outputDbContextDir = (string)args["outputDbContextDir"];
                var dbContextClassName = (string)args["dbContextClassName"];
                var schemaFilters = (IEnumerable<string>)args["schemaFilters"];
                var tableFilters = (IEnumerable<string>)args["tableFilters"];
                var useDataAnnotations = (bool)args["useDataAnnotations"];
                var overwriteFiles = (bool)args["overwriteFiles"];
                var useDatabaseNames = (bool)args["useDatabaseNames"];

                Execute(
                    () => executor.ScaffoldContextImpl(
                        provider, connectionString, outputDir, outputDbContextDir, dbContextClassName,
                        schemaFilters, tableFilters, useDataAnnotations, overwriteFiles, useDatabaseNames));
            }
        }

        private IDictionary ScaffoldContextImpl(
            [NotNull] string provider,
            [NotNull] string connectionString,
            [CanBeNull] string outputDir,
            [CanBeNull] string outputDbContextDir,
            [CanBeNull] string dbContextClassName,
            [NotNull] IEnumerable<string> schemaFilters,
            [NotNull] IEnumerable<string> tableFilters,
            bool useDataAnnotations,
            bool overwriteFiles,
            bool useDatabaseNames)
        {
            Check.NotNull(provider, nameof(provider));
            Check.NotNull(connectionString, nameof(connectionString));
            Check.NotNull(schemaFilters, nameof(schemaFilters));
            Check.NotNull(tableFilters, nameof(tableFilters));

            var files = _databaseOperations.Value.ScaffoldContext(
                provider, connectionString, outputDir, outputDbContextDir, dbContextClassName,
                schemaFilters, tableFilters, useDataAnnotations, overwriteFiles, useDatabaseNames);

            return new Hashtable
            {
                ["ContextFile"] = files.ContextFile,
                ["EntityTypeFiles"] = files.AdditionalFiles.ToArray()
            };
        }

        /// <summary>
        ///     Represents an operation to drop the database.
        /// </summary>
        public class DropDatabase : OperationBase
        {
            /// <summary>
            ///     <para>Initializes a new instance of the <see cref="DropDatabase" /> class.</para>
            ///     <para>The arguments supported by <paramref name="args" /> are:</para>
            ///     <para><c>contextType</c>--The <see cref="DbContext" /> to use.</para>
            /// </summary>
            /// <param name="executor"> The operation executor. </param>
            /// <param name="resultHandler"> The <see cref="IOperationResultHandler" />. </param>
            /// <param name="args"> The operation arguments. </param>
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

        /// <summary>
        ///     Represents an operation.
        /// </summary>
        public abstract class OperationBase : MarshalByRefObject
        {
            private readonly IOperationResultHandler _resultHandler;

            /// <summary>
            ///     Initializes a new instance of the <see cref="OperationBase" /> class.
            /// </summary>
            /// <param name="resultHandler"> The <see cref="IOperationResultHandler" />. </param>
            protected OperationBase([NotNull] object resultHandler)
            {
                Check.NotNull(resultHandler, nameof(resultHandler));

                _resultHandler = ForwardingProxy.Unwrap<IOperationResultHandler>(resultHandler);
            }

            /// <summary>
            ///     Executes an action passing exceptions to the <see cref="IOperationResultHandler" />.
            /// </summary>
            /// <param name="action"> The action to execute. </param>
            protected virtual void Execute([NotNull] Action action)
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

            /// <summary>
            ///     Executes an action passing the result or exceptions to the <see cref="IOperationResultHandler" />.
            /// </summary>
            /// <typeparam name="T"> The result type. </typeparam>
            /// <param name="action"> The action to execute. </param>
            protected virtual void Execute<T>([NotNull] Func<T> action)
            {
                Check.NotNull(action, nameof(action));

                Execute(() => _resultHandler.OnResult(action()));
            }

            /// <summary>
            ///     Executes an action passing results or exceptions to the <see cref="IOperationResultHandler" />.
            /// </summary>
            /// <typeparam name="T"> The type of results. </typeparam>
            /// <param name="action"> The action to execute. </param>
            protected virtual void Execute<T>([NotNull] Func<IEnumerable<T>> action)
            {
                Check.NotNull(action, nameof(action));

                Execute(() => _resultHandler.OnResult(action().ToArray()));
            }
        }
    }
}
