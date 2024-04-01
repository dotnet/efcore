// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Represents options managed by the relational database providers.
///     These options are set using <see cref="DbContextOptionsBuilder" />.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are designed to be immutable. To change an option, call one of the 'With...'
///         methods to obtain a new instance with the option changed.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public abstract class RelationalOptionsExtension : IDbContextOptionsExtension
{
    // NB: When adding new options, make sure to update the copy constructor below.

    private string? _connectionString;
    private DbConnection? _connection;
    private bool _connectionOwned;
    private int? _commandTimeout;
    private int? _maxBatchSize;
    private int? _minBatchSize;
    private bool _useRelationalNulls;
    private QuerySplittingBehavior? _querySplittingBehavior;
    private string? _migrationsAssembly;
    private Assembly? _migrationsAssemblyObject;

    private string? _migrationsHistoryTableName;
    private string? _migrationsHistoryTableSchema;
    private Func<ExecutionStrategyDependencies, IExecutionStrategy>? _executionStrategyFactory;

    /// <summary>
    ///     Creates a new set of options with everything set to default values.
    /// </summary>
    protected RelationalOptionsExtension()
    {
    }

    /// <summary>
    ///     Called by a derived class constructor when implementing the <see cref="Clone" /> method.
    /// </summary>
    /// <param name="copyFrom">The instance that is being cloned.</param>
    protected RelationalOptionsExtension(RelationalOptionsExtension copyFrom)
    {
        _connectionString = copyFrom._connectionString;
        _connection = copyFrom._connection;
        _connectionOwned = copyFrom._connectionOwned;
        _commandTimeout = copyFrom._commandTimeout;
        _maxBatchSize = copyFrom._maxBatchSize;
        _minBatchSize = copyFrom._minBatchSize;
        _useRelationalNulls = copyFrom._useRelationalNulls;
        _querySplittingBehavior = copyFrom._querySplittingBehavior;
        _migrationsAssembly = copyFrom._migrationsAssembly;
        _migrationsHistoryTableName = copyFrom._migrationsHistoryTableName;
        _migrationsHistoryTableSchema = copyFrom._migrationsHistoryTableSchema;
        _executionStrategyFactory = copyFrom._executionStrategyFactory;
    }

    /// <summary>
    ///     Information/metadata about the extension.
    /// </summary>
    public abstract DbContextOptionsExtensionInfo Info { get; }

    /// <summary>
    ///     Override this method in a derived class to ensure that any clone created is also of that class.
    /// </summary>
    /// <returns>A clone of this instance, which can be modified before being returned as immutable.</returns>
    protected abstract RelationalOptionsExtension Clone();

    /// <summary>
    ///     The connection string, or <see langword="null" /> if a <see cref="DbConnection" /> was used instead of
    ///     a connection string.
    /// </summary>
    public virtual string? ConnectionString
        => _connectionString;

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="connectionString">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual RelationalOptionsExtension WithConnectionString(string? connectionString)
    {
        var clone = Clone();

        clone._connectionString = connectionString;
        if (connectionString is not null)
        {
            clone._connection = null;
        }

        return clone;
    }

    /// <summary>
    ///     The <see cref="DbConnection" />, or <see langword="null" /> if a connection string was used instead of
    ///     the full connection object.
    /// </summary>
    public virtual DbConnection? Connection
        => _connection;

    /// <summary>
    ///     <see langword="true" /> if the <see cref="Connection" /> is owned by the context and should be disposed appropriately.
    /// </summary>
    public virtual bool IsConnectionOwned
        => _connectionOwned;

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="connection">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual RelationalOptionsExtension WithConnection(DbConnection? connection)
        => WithConnection(connection, owned: false);

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="connection">The option to change.</param>
    /// <param name="owned">
    ///     If <see langword="true" />, then the connection will become owned by the context, and will be disposed in the same way
    ///     that a connection created by the context is disposed.
    /// </param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual RelationalOptionsExtension WithConnection(DbConnection? connection, bool owned)
    {
        var clone = Clone();

        clone._connection = connection;
        clone._connectionOwned = owned;
        if (connection is not null)
        {
            clone._connectionString = null;
        }

        return clone;
    }

    /// <summary>
    ///     The command timeout, or <see langword="null" /> if none has been set.
    /// </summary>
    public virtual int? CommandTimeout
        => _commandTimeout;

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="commandTimeout">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual RelationalOptionsExtension WithCommandTimeout(int? commandTimeout)
    {
        if (commandTimeout is < 0)
        {
            throw new InvalidOperationException(RelationalStrings.InvalidCommandTimeout(commandTimeout));
        }

        var clone = Clone();

        clone._commandTimeout = commandTimeout;

        return clone;
    }

    /// <summary>
    ///     The maximum number of statements that will be included in commands sent to the database
    ///     during <see cref="DbContext.SaveChanges()" /> or <see langword="null" /> if none has been set.
    /// </summary>
    public virtual int? MaxBatchSize
        => _maxBatchSize;

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="maxBatchSize">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual RelationalOptionsExtension WithMaxBatchSize(int? maxBatchSize)
    {
        if (maxBatchSize.HasValue
            && maxBatchSize <= 0)
        {
            throw new InvalidOperationException(RelationalStrings.InvalidMaxBatchSize(maxBatchSize));
        }

        var clone = Clone();

        clone._maxBatchSize = maxBatchSize;

        return clone;
    }

    /// <summary>
    ///     The minimum number of statements that are needed for a multi-statement command sent to the database
    ///     during <see cref="DbContext.SaveChanges()" /> or <see langword="null" /> if none has been set.
    /// </summary>
    public virtual int? MinBatchSize
        => _minBatchSize;

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="minBatchSize">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual RelationalOptionsExtension WithMinBatchSize(int? minBatchSize)
    {
        if (minBatchSize.HasValue
            && minBatchSize <= 0)
        {
            throw new InvalidOperationException(RelationalStrings.InvalidMinBatchSize(minBatchSize));
        }

        var clone = Clone();

        clone._minBatchSize = minBatchSize;

        return clone;
    }

    /// <summary>
    ///     Indicates whether or not to use relational database semantics when comparing null values. By default,
    ///     Entity Framework will use C# semantics for null values, and generate SQL to compensate for differences
    ///     in how the database handles nulls.
    /// </summary>
    public virtual bool UseRelationalNulls
        => _useRelationalNulls;

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="useRelationalNulls">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual RelationalOptionsExtension WithUseRelationalNulls(bool useRelationalNulls)
    {
        var clone = Clone();

        clone._useRelationalNulls = useRelationalNulls;

        return clone;
    }

    /// <summary>
    ///     The <see cref="QuerySplittingBehavior" /> to use when loading related collections in a query.
    /// </summary>
    public virtual QuerySplittingBehavior? QuerySplittingBehavior
        => _querySplittingBehavior;

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="querySplittingBehavior">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual RelationalOptionsExtension WithUseQuerySplittingBehavior(QuerySplittingBehavior querySplittingBehavior)
    {
        var clone = Clone();

        clone._querySplittingBehavior = querySplittingBehavior;

        return clone;
    }

    /// <summary>
    ///     The name of the assembly that contains migrations, or <see langword="null" /> if none has been set.
    /// </summary>
    public virtual string? MigrationsAssembly
        => _migrationsAssembly;

    /// <summary>
    ///     The assembly that contains migrations, or <see langword="null" /> if none has been set.
    /// </summary>
    public virtual Assembly? MigrationsAssemblyObject
        => _migrationsAssemblyObject;

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="migrationsAssembly">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual RelationalOptionsExtension WithMigrationsAssembly(string? migrationsAssembly)
    {
        var clone = Clone();

        clone._migrationsAssembly = migrationsAssembly;

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="migrationsAssembly">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual RelationalOptionsExtension WithMigrationsAssembly(Assembly migrationsAssembly)
    {
        var clone = Clone();

        clone._migrationsAssemblyObject = migrationsAssembly;

        return clone;
    }

    /// <summary>
    ///     The table name to use for the migrations history table, or <see langword="null" /> if none has been set.
    /// </summary>
    public virtual string? MigrationsHistoryTableName
        => _migrationsHistoryTableName;

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="migrationsHistoryTableName">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual RelationalOptionsExtension WithMigrationsHistoryTableName(string? migrationsHistoryTableName)
    {
        var clone = Clone();

        clone._migrationsHistoryTableName = migrationsHistoryTableName;

        return clone;
    }

    /// <summary>
    ///     The schema to use for the migrations history table, or <see langword="null" /> if none has been set.
    /// </summary>
    public virtual string? MigrationsHistoryTableSchema
        => _migrationsHistoryTableSchema;

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="migrationsHistoryTableSchema">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual RelationalOptionsExtension WithMigrationsHistoryTableSchema(string? migrationsHistoryTableSchema)
    {
        var clone = Clone();

        clone._migrationsHistoryTableSchema = migrationsHistoryTableSchema;

        return clone;
    }

    /// <summary>
    ///     A factory for creating the default <see cref="IExecutionStrategy" />, or <see langword="null" /> if none has been
    ///     configured.
    /// </summary>
    public virtual Func<ExecutionStrategyDependencies, IExecutionStrategy>? ExecutionStrategyFactory
        => _executionStrategyFactory;

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="executionStrategyFactory">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual RelationalOptionsExtension WithExecutionStrategyFactory(
        Func<ExecutionStrategyDependencies, IExecutionStrategy>? executionStrategyFactory)
    {
        var clone = Clone();

        clone._executionStrategyFactory = executionStrategyFactory;

        return clone;
    }

    /// <summary>
    ///     Finds an existing <see cref="RelationalOptionsExtension" /> registered on the given options
    ///     or throws if none has been registered. This is typically used to find some relational
    ///     configuration when it is known that a relational provider is being used.
    /// </summary>
    /// <param name="options">The context options to look in.</param>
    /// <returns>The extension.</returns>
    public static RelationalOptionsExtension Extract(IDbContextOptions options)
    {
        var relationalOptionsExtensions
            = options.Extensions
                .OfType<RelationalOptionsExtension>()
                .ToList();

        if (relationalOptionsExtensions.Count == 0)
        {
            throw new InvalidOperationException(RelationalStrings.NoProviderConfigured);
        }

        if (relationalOptionsExtensions.Count > 1)
        {
            throw new InvalidOperationException(RelationalStrings.MultipleProvidersConfigured);
        }

        return relationalOptionsExtensions[0];
    }

    /// <summary>
    ///     Adds the services required to make the selected options work. This is used when there
    ///     is no external <see cref="IServiceProvider" /> and EF is maintaining its own service
    ///     provider internally. This allows database providers (and other extensions) to register their
    ///     required services when EF is creating an service provider.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    public abstract void ApplyServices(IServiceCollection services);

    /// <summary>
    ///     Gives the extension a chance to validate that all options in the extension are valid.
    ///     Most extensions do not have invalid combinations and so this will be a no-op.
    ///     If options are invalid, then an exception should be thrown.
    /// </summary>
    /// <param name="options">The options being validated.</param>
    public virtual void Validate(IDbContextOptions options)
    {
    }

    /// <summary>
    ///     Adds default <see cref="WarningBehavior" /> for relational events.
    /// </summary>
    /// <param name="coreOptionsExtension">The core options extension.</param>
    /// <returns>The new core options extension.</returns>
    public static CoreOptionsExtension WithDefaultWarningConfiguration(CoreOptionsExtension coreOptionsExtension)
        => coreOptionsExtension.WithWarningsConfiguration(
            coreOptionsExtension.WarningsConfiguration
                .TryWithExplicit(RelationalEventId.AmbientTransactionWarning, WarningBehavior.Throw)
                .TryWithExplicit(RelationalEventId.IndexPropertiesBothMappedAndNotMappedToTable, WarningBehavior.Throw)
                .TryWithExplicit(RelationalEventId.IndexPropertiesMappedToNonOverlappingTables, WarningBehavior.Throw)
                .TryWithExplicit(RelationalEventId.ForeignKeyPropertiesMappedToUnrelatedTables, WarningBehavior.Throw)
                .TryWithExplicit(RelationalEventId.StoredProcedureConcurrencyTokenNotMapped, WarningBehavior.Throw));

    /// <summary>
    ///     Information/metadata for a <see cref="RelationalOptionsExtension" />.
    /// </summary>
    protected abstract class RelationalExtensionInfo : DbContextOptionsExtensionInfo
    {
        private string? _logFragment;

        /// <summary>
        ///     Creates a new <see cref="RelationalExtensionInfo" /> instance containing
        ///     info/metadata for the given extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        protected RelationalExtensionInfo(IDbContextOptionsExtension extension)
            : base(extension)
        {
        }

        /// <summary>
        ///     The extension for which this instance contains metadata.
        /// </summary>
        public new virtual RelationalOptionsExtension Extension
            => (RelationalOptionsExtension)base.Extension;

        /// <summary>
        ///     True, since this is a database provider base class.
        /// </summary>
        public override bool IsDatabaseProvider
            => true;

        /// <summary>
        ///     Returns a hash code created from any options that would cause a new <see cref="IServiceProvider" />
        ///     to be needed. For example, if the options affect a singleton service. However most extensions do not
        ///     have any such options and should return zero.
        /// </summary>
        /// <returns>A hash over options that require a new service provider when changed.</returns>
        public override int GetServiceProviderHashCode()
            => 0;

        /// <summary>
        ///     Returns a value indicating whether all of the options used in <see cref="GetServiceProviderHashCode" />
        ///     are the same as in the given extension.
        /// </summary>
        /// <param name="other">The other extension.</param>
        /// <returns>A value indicating whether all of the options that require a new service provider are the same.</returns>
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is RelationalExtensionInfo;

        /// <summary>
        ///     A message fragment for logging typically containing information about
        ///     any useful non-default options that have been configured.
        /// </summary>
        public override string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder();

                    if (Extension._commandTimeout != null)
                    {
                        builder.Append("CommandTimeout=").Append(Extension._commandTimeout).Append(' ');
                    }

                    if (Extension._maxBatchSize != null)
                    {
                        builder.Append("MaxBatchSize=").Append(Extension._maxBatchSize).Append(' ');
                    }

                    if (Extension._useRelationalNulls)
                    {
                        builder.Append("UseRelationalNulls ");
                    }

                    if (Extension._querySplittingBehavior != null)
                    {
                        builder.Append("QuerySplittingBehavior=").Append(Extension._querySplittingBehavior).Append(' ');
                    }

                    if (Extension._migrationsAssembly != null)
                    {
                        builder.Append("MigrationsAssembly=").Append(Extension._migrationsAssembly).Append(' ');
                    }

                    if (Extension._migrationsHistoryTableName != null
                        || Extension._migrationsHistoryTableSchema != null)
                    {
                        builder.Append("MigrationsHistoryTable=");

                        if (Extension._migrationsHistoryTableSchema != null)
                        {
                            builder.Append(Extension._migrationsHistoryTableSchema).Append('.');
                        }

                        builder.Append(Extension._migrationsHistoryTableName ?? HistoryRepository.DefaultTableName).Append(' ');
                    }

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }
    }
}
