// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     A facade for design-time operations.
/// </summary>
/// <remarks>
///     Use the <c>CreateInstance</c> overloads on <see cref="AppDomain" /> and <see cref="Activator" /> with the
///     nested types to execute operations.
/// </remarks>
public class OperationExecutor : MarshalByRefObject
{
    private readonly string _projectDir;
    private readonly string _targetAssemblyName;
    private readonly string _startupTargetAssemblyName;
    private readonly string? _rootNamespace;
    private readonly string? _language;
    private readonly bool _nullable;
    private readonly string[]? _designArgs;
    private readonly OperationReporter _reporter;

    private DbContextOperations? _contextOperations;
    private DatabaseOperations? _databaseOperations;
    private MigrationsOperations? _migrationsOperations;
    private Assembly? _assembly;
    private Assembly? _startupAssembly;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OperationExecutor" /> class
    /// </summary>
    /// <remarks>
    ///     <para>The arguments supported by <paramref name="args" /> are:</para>
    ///     <para><c>targetName</c>--The assembly name of the target project.</para>
    ///     <para><c>startupTargetName</c>--The assembly name of the startup project.</para>
    ///     <para><c>projectDir</c>--The target project's root directory.</para>
    ///     <para><c>rootNamespace</c>--The target project's root namespace.</para>
    ///     <para><c>language</c>--The programming language to be used to generate classes.</para>
    ///     <para><c>nullable</c>--A value indicating whether nullable reference types are enabled.</para>
    ///     <para><c>remainingArguments</c>--Extra arguments passed into the operation.</para>
    /// </remarks>
    /// <param name="reportHandler">The <see cref="IOperationReportHandler" />.</param>
    /// <param name="args">The executor arguments.</param>
    public OperationExecutor(IOperationReportHandler reportHandler, IDictionary args)
    {
        Check.NotNull(reportHandler, nameof(reportHandler));
        Check.NotNull(args, nameof(args));

        _reporter = new OperationReporter(reportHandler);
        _targetAssemblyName = (string)args["targetName"]!;
        _startupTargetAssemblyName = (string)args["startupTargetName"]!;
        _projectDir = (string)args["projectDir"]!;
        _rootNamespace = (string?)args["rootNamespace"];
        _language = (string?)args["language"];
        _nullable = (bool)(args["nullable"] ?? false);
        _designArgs = (string[]?)args["remainingArguments"];

        var toolsVersion = (string?)args["toolsVersion"];
        var runtimeVersion = ProductInfo.GetVersion();
        if (toolsVersion != null
            && new SemanticVersionComparer().Compare(toolsVersion, runtimeVersion) < 0)
        {
            _reporter.WriteWarning(DesignStrings.VersionMismatch(toolsVersion, runtimeVersion));
        }
    }

    private Assembly Assembly
    {
        get
        {
            Assembly Create()
            {
                try
                {
                    return Assembly.Load(new AssemblyName(_targetAssemblyName));
                }
                catch (Exception ex)
                {
                    throw new OperationException(
                        DesignStrings.UnreferencedAssembly(_targetAssemblyName, _startupTargetAssemblyName),
                        ex);
                }
            }

            return _assembly ??= Create();
        }
    }

    private Assembly StartupAssembly
        => _startupAssembly
            ??= Assembly.Load(new AssemblyName(_startupTargetAssemblyName));

    /// <summary>
    ///     Exposes the underlying operations for testing.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual MigrationsOperations MigrationsOperations
        => _migrationsOperations
            ??= new MigrationsOperations(
                _reporter,
                Assembly,
                StartupAssembly,
                _projectDir,
                _rootNamespace,
                _language,
                _nullable,
                _designArgs);

    /// <summary>
    ///     Exposes the underlying operations for testing.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DbContextOperations ContextOperations
        => _contextOperations
            ??= new DbContextOperations(
                _reporter,
                Assembly,
                StartupAssembly,
                _projectDir,
                _rootNamespace,
                _language,
                _nullable,
                _designArgs);

    /// <summary>
    ///     Exposes the underlying operations for testing.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DatabaseOperations DatabaseOperations
        => _databaseOperations
            ??= new DatabaseOperations(
                _reporter,
                Assembly,
                StartupAssembly,
                _projectDir,
                _rootNamespace,
                _language,
                _nullable,
                _designArgs);

    /// <summary>
    ///     Represents an operation to add a new migration.
    /// </summary>
    public class AddMigration : OperationBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AddMigration" /> class.
        /// </summary>
        /// <remarks>
        ///     <para>The arguments supported by <paramref name="args" /> are:</para>
        ///     <para><c>name</c>--The name of the migration.</para>
        ///     <para>
        ///         <c>outputDir</c>--The directory (and sub-namespace) to use. Paths are relative to the project directory. Defaults to
        ///         "Migrations".
        ///     </para>
        ///     <para><c>contextType</c>--The <see cref="DbContext" /> type to use.</para>
        /// </remarks>
        /// <param name="executor">The operation executor.</param>
        /// <param name="resultHandler">The <see cref="IOperationResultHandler" />.</param>
        /// <param name="args">The operation arguments.</param>
        public AddMigration(
            OperationExecutor executor,
            IOperationResultHandler resultHandler,
            IDictionary args)
            : base(resultHandler)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(args, nameof(args));

            var name = (string)args["name"]!;
            var outputDir = (string?)args["outputDir"];
            var contextType = (string?)args["contextType"];
            var @namespace = (string?)args["namespace"];
            var dryRun = (bool?)args["dryRun"]!;

            Execute(() => executor.AddMigrationImpl(name, outputDir, contextType, @namespace, dryRun == true));
        }
    }

    private IDictionary AddMigrationImpl(
        string name,
        string? outputDir,
        string? contextType,
        string? @namespace,
        bool dryRun)
    {
        Check.NotEmpty(name, nameof(name));

        var files = MigrationsOperations.AddMigration(name, outputDir, contextType, @namespace, dryRun);
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
        ///     Initializes a new instance of the <see cref="GetContextInfo" /> class.
        /// </summary>
        /// <remarks>
        ///     <para>The arguments supported by <paramref name="args" /> are:</para>
        ///     <para><c>contextType</c>--The <see cref="DbContext" /> type to use.</para>
        /// </remarks>
        /// <param name="executor">The operation executor.</param>
        /// <param name="resultHandler">The <see cref="IOperationResultHandler" />.</param>
        /// <param name="args">The operation arguments.</param>
        public GetContextInfo(
            OperationExecutor executor,
            IOperationResultHandler resultHandler,
            IDictionary args)
            : base(resultHandler)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(args, nameof(args));

            var contextType = (string?)args["contextType"];
            Execute(() => executor.GetContextInfoImpl(contextType));
        }
    }

    private IDictionary GetContextInfoImpl(string? contextType)
    {
        var info = ContextOperations.GetContextInfo(contextType);
        return new Hashtable
        {
            ["Type"] = info.Type,
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
        ///     Initializes a new instance of the <see cref="UpdateDatabase" /> class.
        /// </summary>
        /// <remarks>
        ///     <para>The arguments supported by <paramref name="args" /> are:</para>
        ///     <para>
        ///         <c>targetMigration</c>--The target <see cref="Migration" />. If <see cref="Migration.InitialDatabase" />, all migrations will be
        ///         reverted. Defaults to the last migration.
        ///     </para>
        ///     <para>
        ///         <c>connectionString</c>--The connection string to the database. Defaults to the one specified in
        ///         <see cref="O:Microsoft.Extensions.DependencyInjection.EntityFrameworkServiceCollectionExtensions.AddDbContext" /> or
        ///         <see cref="DbContext.OnConfiguring" />.
        ///     </para>
        ///     <para><c>contextType</c>--The <see cref="DbContext" /> to use.</para>
        /// </remarks>
        /// <param name="executor">The operation executor.</param>
        /// <param name="resultHandler">The <see cref="IOperationResultHandler" />.</param>
        /// <param name="args">The operation arguments.</param>
        public UpdateDatabase(
            OperationExecutor executor,
            IOperationResultHandler resultHandler,
            IDictionary args)
            : base(resultHandler)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(args, nameof(args));

            var targetMigration = (string?)args["targetMigration"];
            var connectionString = (string?)args["connectionString"];
            var contextType = (string?)args["contextType"];

            Execute(() => executor.UpdateDatabaseImpl(targetMigration, connectionString, contextType));
        }
    }

    private void UpdateDatabaseImpl(
        string? targetMigration,
        string? connectionString,
        string? contextType)
        => MigrationsOperations.UpdateDatabase(targetMigration, connectionString, contextType);

    /// <summary>
    ///     Represents an operation to generate a SQL script from migrations.
    /// </summary>
    public class ScriptMigration : OperationBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptMigration" /> class.
        /// </summary>
        /// <remarks>
        ///     <para>The arguments supported by <paramref name="args" /> are:</para>
        ///     <para><c>fromMigration</c>--The starting migration. Defaults to <see cref="Migration.InitialDatabase" />.</para>
        ///     <para><c>toMigration</c>--The ending migration. Defaults to the last migration.</para>
        ///     <para><c>idempotent</c>--Generate a script that can be used on a database at any migration.</para>
        ///     <para><c>noTransactions</c>--Don't generate SQL transaction statements.</para>
        ///     <para><c>contextType</c>--The <see cref="DbContext" /> to use.</para>
        /// </remarks>
        /// <param name="executor">The operation executor.</param>
        /// <param name="resultHandler">The <see cref="IOperationResultHandler" />.</param>
        /// <param name="args">The operation arguments.</param>
        public ScriptMigration(
            OperationExecutor executor,
            IOperationResultHandler resultHandler,
            IDictionary args)
            : base(resultHandler)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(args, nameof(args));

            var fromMigration = (string?)args["fromMigration"];
            var toMigration = (string?)args["toMigration"];
            var idempotent = (bool)args["idempotent"]!;
            var noTransactions = (bool)(args["noTransactions"] ?? false);
            var contextType = (string?)args["contextType"];

            Execute(() => executor.ScriptMigrationImpl(fromMigration, toMigration, idempotent, noTransactions, contextType));
        }
    }

    private string ScriptMigrationImpl(
        string? fromMigration,
        string? toMigration,
        bool idempotent,
        bool noTransactions,
        string? contextType)
    {
        var options = MigrationsSqlGenerationOptions.Default;
        if (idempotent)
        {
            options |= MigrationsSqlGenerationOptions.Idempotent;
        }

        if (noTransactions)
        {
            options |= MigrationsSqlGenerationOptions.NoTransactions;
        }

        return MigrationsOperations.ScriptMigration(
            fromMigration,
            toMigration,
            options,
            contextType);
    }

    /// <summary>
    ///     Represents an operation to remove the last migration.
    /// </summary>
    public class RemoveMigration : OperationBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RemoveMigration" /> class.
        /// </summary>
        /// <remarks>
        ///     <para>The arguments supported by <paramref name="args" /> are:</para>
        ///     <para><c>contextType</c>--The <see cref="DbContext" /> to use.</para>
        ///     <para><c>force</c>--Don't check to see if the migration has been applied to the database.</para>
        /// </remarks>
        /// <param name="executor">The operation executor.</param>
        /// <param name="resultHandler">The <see cref="IOperationResultHandler" />.</param>
        /// <param name="args">The operation arguments.</param>
        public RemoveMigration(
            OperationExecutor executor,
            IOperationResultHandler resultHandler,
            IDictionary args)
            : base(resultHandler)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(args, nameof(args));

            var contextType = (string?)args["contextType"];
            var force = (bool)args["force"]!;
            var dryRun = (bool?)args["dryRun"]!;

            Execute(() => executor.RemoveMigrationImpl(contextType, force, dryRun == true));
        }
    }

    private IDictionary RemoveMigrationImpl(string? contextType, bool force, bool dryRun)
    {
        var files = MigrationsOperations.RemoveMigration(contextType, force, dryRun);

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
        ///     Initializes a new instance of the <see cref="GetContextTypes" /> class.
        /// </summary>
        /// <remarks>
        ///     No arguments are currently supported by <paramref name="args" />.
        /// </remarks>
        /// <param name="executor">The operation executor.</param>
        /// <param name="resultHandler">The <see cref="IOperationResultHandler" />.</param>
        /// <param name="args">The operation arguments.</param>
        public GetContextTypes(
            OperationExecutor executor,
            IOperationResultHandler resultHandler,
            IDictionary args)
            : base(resultHandler)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(args, nameof(args));

            Execute(executor.GetContextTypesImpl);
        }
    }

    private IEnumerable<IDictionary> GetContextTypesImpl()
    {
        var contextTypes = ContextOperations.GetContextTypes().ToList();
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
        ///     Initializes a new instance of the <see cref="GetMigrations" /> class.
        /// </summary>
        /// <remarks>
        ///     <para>The arguments supported by <paramref name="args" /> are:</para>
        ///     <para><c>contextType</c>--The <see cref="DbContext" /> to use.</para>
        ///     <para>
        ///         <c>connectionString</c>--The connection string to the database. Defaults to the one specified in
        ///         <see cref="O:Microsoft.Extensions.DependencyInjection.EntityFrameworkServiceCollectionExtensions.AddDbContext" /> or
        ///         <see cref="DbContext.OnConfiguring" />.
        ///     </para>
        ///     <para><c>noConnect</c>--Don't connect to the database.</para>
        /// </remarks>
        /// <param name="executor">The operation executor.</param>
        /// <param name="resultHandler">The <see cref="IOperationResultHandler" />.</param>
        /// <param name="args">The operation arguments.</param>
        public GetMigrations(
            OperationExecutor executor,
            IOperationResultHandler resultHandler,
            IDictionary args)
            : base(resultHandler)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(args, nameof(args));

            var contextType = (string?)args["contextType"];
            var connectionString = (string?)args["connectionString"];
            var noConnect = (bool)(args["noConnect"] ?? true);

            Execute(() => executor.GetMigrationsImpl(contextType, connectionString, noConnect));
        }
    }

    private IEnumerable<IDictionary> GetMigrationsImpl(
        string? contextType,
        string? connectionString,
        bool noConnect)
    {
        var migrations = MigrationsOperations.GetMigrations(contextType, connectionString, noConnect).ToList();
        var nameGroups = migrations.GroupBy(m => m.Name).ToList();

        return migrations.Select(
            m => new Hashtable
            {
                ["Id"] = m.Id,
                ["Name"] = m.Name,
                ["SafeName"] = nameGroups.Count(g => g.Key == m.Name) == 1
                    ? m.Name
                    : m.Id,
                ["Applied"] = m.Applied
            });
    }

    /// <summary>
    ///     Represents an operation to generate a compiled model from the DbContext.
    /// </summary>
    public class OptimizeContext : OperationBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptimizeContext" /> class.
        /// </summary>
        /// <remarks>
        ///     <para>The arguments supported by <paramref name="args" /> are:</para>
        ///     <para><c>outputDir</c>--The directory to put files in. Paths are relative to the project directory.</para>
        ///     <para><c>modelNamespace</c>--Specify to override the namespace of the generated model.</para>
        ///     <para><c>contextType</c>--The <see cref="DbContext" /> to use.</para>
        /// </remarks>
        /// <param name="executor">The operation executor.</param>
        /// <param name="resultHandler">The <see cref="IOperationResultHandler" />.</param>
        /// <param name="args">The operation arguments.</param>
        public OptimizeContext(
            OperationExecutor executor,
            IOperationResultHandler resultHandler,
            IDictionary args)
            : base(resultHandler)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(args, nameof(args));

            var outputDir = (string?)args["outputDir"];
            var modelNamespace = (string?)args["modelNamespace"];
            var contextType = (string?)args["contextType"];

            Execute(() => executor.OptimizeContextImpl(outputDir, modelNamespace, contextType));
        }
    }

    private IReadOnlyList<string> OptimizeContextImpl(string? outputDir, string? modelNamespace, string? contextType)
        => ContextOperations.Optimize(outputDir, modelNamespace, contextType);

    /// <summary>
    ///     Represents an operation to scaffold a <see cref="DbContext" /> and entity types for a database.
    /// </summary>
    public class ScaffoldContext : OperationBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScaffoldContext" /> class.
        /// </summary>
        /// <remarks>
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
        ///     <para><c>modelNamespace</c>--Specify to override the namespace of the generated entity types.</para>
        ///     <para><c>contextNamespace</c>--Specify to override the namespace of the generated DbContext class.</para>
        ///     <para><c>noPluralize</c>--Don't use the pluralizer.</para>
        /// </remarks>
        /// <param name="executor">The operation executor.</param>
        /// <param name="resultHandler">The <see cref="IOperationResultHandler" />.</param>
        /// <param name="args">The operation arguments.</param>
        public ScaffoldContext(
            OperationExecutor executor,
            IOperationResultHandler resultHandler,
            IDictionary args)
            : base(resultHandler)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(args, nameof(args));

            var connectionString = (string)args["connectionString"]!;
            var provider = (string)args["provider"]!;
            var outputDir = (string?)args["outputDir"];
            var outputDbContextDir = (string?)args["outputDbContextDir"];
            var dbContextClassName = (string?)args["dbContextClassName"];
            var schemaFilters = (IEnumerable<string>)args["schemaFilters"]!;
            var tableFilters = (IEnumerable<string>)args["tableFilters"]!;
            var modelNamespace = (string?)args["modelNamespace"];
            var contextNamespace = (string?)args["contextNamespace"];
            var useDataAnnotations = (bool)args["useDataAnnotations"]!;
            var overwriteFiles = (bool)args["overwriteFiles"]!;
            var useDatabaseNames = (bool)args["useDatabaseNames"]!;
            var suppressOnConfiguring = (bool)(args["suppressOnConfiguring"] ?? false);
            var noPluralize = (bool)(args["noPluralize"] ?? false);

            Execute(
                () => executor.ScaffoldContextImpl(
                    provider, connectionString, outputDir, outputDbContextDir, dbContextClassName,
                    schemaFilters, tableFilters, modelNamespace, contextNamespace, useDataAnnotations,
                    overwriteFiles, useDatabaseNames, suppressOnConfiguring, noPluralize));
        }
    }

    private IDictionary ScaffoldContextImpl(
        string provider,
        string connectionString,
        string? outputDir,
        string? outputDbContextDir,
        string? dbContextClassName,
        IEnumerable<string> schemaFilters,
        IEnumerable<string> tableFilters,
        string? modelNamespace,
        string? contextNamespace,
        bool useDataAnnotations,
        bool overwriteFiles,
        bool useDatabaseNames,
        bool suppressOnConfiguring,
        bool noPluralize)
    {
        Check.NotNull(provider, nameof(provider));
        Check.NotNull(connectionString, nameof(connectionString));
        Check.NotNull(schemaFilters, nameof(schemaFilters));
        Check.NotNull(tableFilters, nameof(tableFilters));

        var files = DatabaseOperations.ScaffoldContext(
            provider, connectionString, outputDir, outputDbContextDir, dbContextClassName,
            schemaFilters, tableFilters, modelNamespace, contextNamespace, useDataAnnotations,
            overwriteFiles, useDatabaseNames, suppressOnConfiguring, noPluralize);

        return new Hashtable { ["ContextFile"] = files.ContextFile, ["EntityTypeFiles"] = files.AdditionalFiles.ToArray() };
    }

    /// <summary>
    ///     Represents an operation to drop the database.
    /// </summary>
    public class DropDatabase : OperationBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DropDatabase" /> class.
        /// </summary>
        /// <remarks>
        ///     <para>The arguments supported by <paramref name="args" /> are:</para>
        ///     <para><c>contextType</c>--The <see cref="DbContext" /> to use.</para>
        /// </remarks>
        /// <param name="executor">The operation executor.</param>
        /// <param name="resultHandler">The <see cref="IOperationResultHandler" />.</param>
        /// <param name="args">The operation arguments.</param>
        public DropDatabase(
            OperationExecutor executor,
            IOperationResultHandler resultHandler,
            IDictionary args)
            : base(resultHandler)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(args, nameof(args));

            var contextType = (string?)args["contextType"];

            Execute(() => executor.DropDatabaseImpl(contextType));
        }
    }

    private void DropDatabaseImpl(string? contextType)
        => ContextOperations.DropDatabase(contextType);

    /// <summary>
    ///     Represents an operation to generate a SQL script from the DbContext.
    /// </summary>
    public class ScriptDbContext : OperationBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptDbContext" /> class.
        /// </summary>
        /// <remarks>
        ///     <para>The arguments supported by <paramref name="args" /> are:</para>
        ///     <para><c>contextType</c>--The <see cref="DbContext" /> to use.</para>
        /// </remarks>
        /// <param name="executor">The operation executor.</param>
        /// <param name="resultHandler">The <see cref="IOperationResultHandler" />.</param>
        /// <param name="args">The operation arguments.</param>
        public ScriptDbContext(
            OperationExecutor executor,
            IOperationResultHandler resultHandler,
            IDictionary args)
            : base(resultHandler)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(args, nameof(args));

            var contextType = (string?)args["contextType"];

            Execute(() => executor.ScriptDbContextImpl(contextType));
        }
    }

    private string ScriptDbContextImpl(string? contextType)
        => ContextOperations.ScriptDbContext(contextType);

    /// <summary>
    ///     Represents an operation to check if there are any pending migrations.
    /// </summary>
    public class HasPendingModelChanges : OperationBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HasPendingModelChanges" /> class.
        /// </summary>
        /// <remarks>
        ///     <para>The arguments supported by <paramref name="args" /> are:</para>
        ///     <para><c>contextType</c>--The <see cref="DbContext" /> to use.</para>
        /// </remarks>
        /// <param name="executor">The operation executor.</param>
        /// <param name="resultHandler">The <see cref="IOperationResultHandler" />.</param>
        /// <param name="args">The operation arguments.</param>
        public HasPendingModelChanges(
            OperationExecutor executor,
            IOperationResultHandler resultHandler,
            IDictionary args)
            : base(resultHandler)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(args, nameof(args));

            var contextType = (string?)args["contextType"];

            Execute(() => executor.HasPendingModelChangesImpl(contextType));
        }
    }

    private void HasPendingModelChangesImpl(string? contextType)
        => MigrationsOperations.HasPendingModelChanges(contextType);

    /// <summary>
    ///     Represents an operation.
    /// </summary>
    public abstract class OperationBase : MarshalByRefObject
    {
        private readonly IOperationResultHandler _resultHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperationBase" /> class.
        /// </summary>
        /// <param name="resultHandler">The <see cref="IOperationResultHandler" />.</param>
        protected OperationBase(IOperationResultHandler resultHandler)
        {
            _resultHandler = resultHandler;
        }

        /// <summary>
        ///     Executes an action passing exceptions to the <see cref="IOperationResultHandler" />.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        protected virtual void Execute(Action action)
        {
            EF.IsDesignTime = true;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _resultHandler.OnError(ex.GetType().FullName!, ex.Message, ex.ToString());
            }
            finally
            {
                EF.IsDesignTime = false;
            }
        }

        /// <summary>
        ///     Executes an action passing the result or exceptions to the <see cref="IOperationResultHandler" />.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="action">The action to execute.</param>
        protected virtual void Execute<T>(Func<T> action)
            => Execute(() => _resultHandler.OnResult(action()));

        /// <summary>
        ///     Executes an action passing results or exceptions to the <see cref="IOperationResultHandler" />.
        /// </summary>
        /// <typeparam name="T">The type of results.</typeparam>
        /// <param name="action">The action to execute.</param>
        protected virtual void Execute<T>(Func<IEnumerable<T>> action)
            => Execute(() => _resultHandler.OnResult(action().ToArray()));
    }
}
