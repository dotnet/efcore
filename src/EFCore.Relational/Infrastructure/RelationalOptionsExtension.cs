// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Represents options managed by the relational database providers.
    ///         These options are set using <see cref="DbContextOptionsBuilder" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are designed to be immutable. To change an option, call one of the 'With...'
    ///         methods to obtain a new instance with the option changed.
    ///     </para>
    /// </summary>
    public abstract class RelationalOptionsExtension : IDbContextOptionsExtension
    {
        // NB: When adding new options, make sure to update the copy constructor below.

        private string _connectionString;
        private DbConnection _connection;
        private int? _commandTimeout;
        private int? _maxBatchSize;
        private int? _minBatchSize;
        private bool _useRelationalNulls;
        private string _migrationsAssembly;
        private string _migrationsHistoryTableName;
        private string _migrationsHistoryTableSchema;
        private Func<ExecutionStrategyDependencies, IExecutionStrategy> _executionStrategyFactory;

        /// <summary>
        ///     Creates a new set of options with everything set to default values.
        /// </summary>
        protected RelationalOptionsExtension()
        {
        }

        /// <summary>
        ///     Called by a derived class constructor when implementing the <see cref="Clone" /> method.
        /// </summary>
        /// <param name="copyFrom"> The instance that is being cloned. </param>
        protected RelationalOptionsExtension([NotNull] RelationalOptionsExtension copyFrom)
        {
            Check.NotNull(copyFrom, nameof(copyFrom));

            _connectionString = copyFrom._connectionString;
            _connection = copyFrom._connection;
            _commandTimeout = copyFrom._commandTimeout;
            _maxBatchSize = copyFrom._maxBatchSize;
            _minBatchSize = copyFrom._minBatchSize;
            _useRelationalNulls = copyFrom._useRelationalNulls;
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
        /// <returns> A clone of this instance, which can be modified before being returned as immutable. </returns>
        protected abstract RelationalOptionsExtension Clone();

        /// <summary>
        ///     The connection string, or <c>null</c> if a <see cref="DbConnection" /> was used instead of
        ///     a connection string.
        /// </summary>
        public virtual string ConnectionString => _connectionString;

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="connectionString"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual RelationalOptionsExtension WithConnectionString([NotNull] string connectionString)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));

            var clone = Clone();

            clone._connectionString = connectionString;

            return clone;
        }

        /// <summary>
        ///     The <see cref="DbConnection" />, or <c>null</c> if a connection string was used instead of
        ///     the full connection object.
        /// </summary>
        public virtual DbConnection Connection => _connection;

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="connection"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual RelationalOptionsExtension WithConnection([NotNull] DbConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

            var clone = Clone();

            clone._connection = connection;

            return clone;
        }

        /// <summary>
        ///     The command timeout, or <c>null</c> if none has been set.
        /// </summary>
        public virtual int? CommandTimeout => _commandTimeout;

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="commandTimeout"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual RelationalOptionsExtension WithCommandTimeout(int? commandTimeout)
        {
            if (commandTimeout.HasValue
                && commandTimeout <= 0)
            {
                throw new InvalidOperationException(RelationalStrings.InvalidCommandTimeout);
            }

            var clone = Clone();

            clone._commandTimeout = commandTimeout;

            return clone;
        }

        /// <summary>
        ///     The maximum number of statements that will be included in commands sent to the database
        ///     during <see cref="DbContext.SaveChanges()" /> or <c>null</c> if none has been set.
        /// </summary>
        public virtual int? MaxBatchSize => _maxBatchSize;

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="maxBatchSize"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual RelationalOptionsExtension WithMaxBatchSize(int? maxBatchSize)
        {
            if (maxBatchSize.HasValue
                && maxBatchSize <= 0)
            {
                throw new InvalidOperationException(RelationalStrings.InvalidMaxBatchSize);
            }

            var clone = Clone();

            clone._maxBatchSize = maxBatchSize;

            return clone;
        }

        /// <summary>
        ///     The minimum number of statements that are needed for a multi-statement command sent to the database
        ///     during <see cref="DbContext.SaveChanges()" /> or <c>null</c> if none has been set.
        /// </summary>
        public virtual int? MinBatchSize => _minBatchSize;

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="minBatchSize"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual RelationalOptionsExtension WithMinBatchSize(int? minBatchSize)
        {
            if (minBatchSize.HasValue
                && minBatchSize <= 0)
            {
                throw new InvalidOperationException(RelationalStrings.InvalidMinBatchSize);
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
        public virtual bool UseRelationalNulls => _useRelationalNulls;

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="useRelationalNulls"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual RelationalOptionsExtension WithUseRelationalNulls(bool useRelationalNulls)
        {
            var clone = Clone();

            clone._useRelationalNulls = useRelationalNulls;

            return clone;
        }

        /// <summary>
        ///     The name of the assembly that contains migrations, or <c>null</c> if none has been set.
        /// </summary>
        public virtual string MigrationsAssembly => _migrationsAssembly;

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="migrationsAssembly"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual RelationalOptionsExtension WithMigrationsAssembly([CanBeNull] string migrationsAssembly)
        {
            var clone = Clone();

            clone._migrationsAssembly = migrationsAssembly;

            return clone;
        }

        /// <summary>
        ///     The table name to use for the migrations history table, or <c>null</c> if none has been set.
        /// </summary>
        public virtual string MigrationsHistoryTableName => _migrationsHistoryTableName;

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="migrationsHistoryTableName"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual RelationalOptionsExtension WithMigrationsHistoryTableName([CanBeNull] string migrationsHistoryTableName)
        {
            var clone = Clone();

            clone._migrationsHistoryTableName = migrationsHistoryTableName;

            return clone;
        }

        /// <summary>
        ///     The schema to use for the migrations history table, or <c>null</c> if none has been set.
        /// </summary>
        public virtual string MigrationsHistoryTableSchema => _migrationsHistoryTableSchema;

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="migrationsHistoryTableSchema"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual RelationalOptionsExtension WithMigrationsHistoryTableSchema([CanBeNull] string migrationsHistoryTableSchema)
        {
            var clone = Clone();

            clone._migrationsHistoryTableSchema = migrationsHistoryTableSchema;

            return clone;
        }

        /// <summary>
        ///     A factory for creating the default <see cref="IExecutionStrategy" />, or <c>null</c> if none has been
        ///     configured.
        /// </summary>
        public virtual Func<ExecutionStrategyDependencies, IExecutionStrategy> ExecutionStrategyFactory => _executionStrategyFactory;

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="executionStrategyFactory"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual RelationalOptionsExtension WithExecutionStrategyFactory(
            [CanBeNull] Func<ExecutionStrategyDependencies, IExecutionStrategy> executionStrategyFactory)
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
        /// <param name="options"> The context options to look in. </param>
        /// <returns> The extension. </returns>
        public static RelationalOptionsExtension Extract([NotNull] IDbContextOptions options)
        {
            Check.NotNull(options, nameof(options));

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
        /// <param name="services"> The collection to add services to. </param>
        public abstract void ApplyServices(IServiceCollection services);

        /// <summary>
        ///     Gives the extension a chance to validate that all options in the extension are valid.
        ///     Most extensions do not have invalid combinations and so this will be a no-op.
        ///     If options are invalid, then an exception should be thrown.
        /// </summary>
        /// <param name="options"> The options being validated. </param>
        public virtual void Validate(IDbContextOptions options)
        {
        }

        /// <summary>
        ///     Information/metadata for a <see cref="RelationalOptionsExtension" />.
        /// </summary>
        protected abstract class RelationalExtensionInfo : DbContextOptionsExtensionInfo
        {
            private string _logFragment;

            /// <summary>
            ///     Creates a new <see cref="RelationalOptionsExtension.RelationalExtensionInfo" /> instance containing
            ///     info/metadata for the given extension.
            /// </summary>
            /// <param name="extension"> The extension. </param>
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
            public override bool IsDatabaseProvider => true;

            /// <summary>
            ///     Returns a hash code created from any options that would cause a new <see cref="IServiceProvider" />
            ///     to be needed. Most extensions do not have any such options and should return zero.
            /// </summary>
            /// <returns> A hash over options that require a new service provider when changed. </returns>
            public override long GetServiceProviderHashCode() => 0;

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
}
