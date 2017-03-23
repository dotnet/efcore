// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public abstract class RelationalOptionsExtension : IDbContextOptionsExtension
    {
        private string _connectionString;
        private DbConnection _connection;
        private int? _commandTimeout;
        private int? _maxBatchSize;
        private bool _useRelationalNulls;
        private string _migrationsAssembly;
        private string _migrationsHistoryTableName;
        private string _migrationsHistoryTableSchema;
        private Func<ExecutionStrategyContext, IExecutionStrategy> _executionStrategyFactory;

        protected RelationalOptionsExtension()
        {
        }

        // NB: When adding new options, make sure to update the copy ctor below.

        protected RelationalOptionsExtension([NotNull] RelationalOptionsExtension copyFrom)
        {
            Check.NotNull(copyFrom, nameof(copyFrom));

            _connectionString = copyFrom._connectionString;
            _connection = copyFrom._connection;
            _commandTimeout = copyFrom._commandTimeout;
            _maxBatchSize = copyFrom._maxBatchSize;
            _useRelationalNulls = copyFrom._useRelationalNulls;
            _migrationsAssembly = copyFrom._migrationsAssembly;
            _migrationsHistoryTableName = copyFrom._migrationsHistoryTableName;
            _migrationsHistoryTableSchema = copyFrom._migrationsHistoryTableSchema;
            _executionStrategyFactory = copyFrom._executionStrategyFactory;
        }

        protected abstract RelationalOptionsExtension Clone();

        public virtual string ConnectionString => _connectionString;

        public virtual RelationalOptionsExtension WithConnectionString([NotNull] string connectionString)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));

            var clone = Clone();

            clone._connectionString = connectionString;

            return clone;
        }

        public virtual DbConnection Connection => _connection;

        public virtual RelationalOptionsExtension WithConnection([NotNull] DbConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

            var clone = Clone();

            clone._connection = connection;

            return clone;
        }

        public virtual int? CommandTimeout => _commandTimeout;

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

        public virtual int? MaxBatchSize => _maxBatchSize;

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

        public virtual bool UseRelationalNulls => _useRelationalNulls;

        public virtual RelationalOptionsExtension WithUseRelationalNulls(bool useRelationalNulls)
        {
            var clone = Clone();

            clone._useRelationalNulls = useRelationalNulls;

            return clone;
        }

        public virtual string MigrationsAssembly => _migrationsAssembly;

        public virtual RelationalOptionsExtension WithMigrationsAssembly([CanBeNull] string migrationsAssembly)
        {
            var clone = Clone();

            clone._migrationsAssembly = migrationsAssembly;

            return clone;
        }

        public virtual string MigrationsHistoryTableName => _migrationsHistoryTableName;

        public virtual RelationalOptionsExtension WithMigrationsHistoryTableName([CanBeNull] string migrationsHistoryTableName)
        {
            var clone = Clone();

            clone._migrationsHistoryTableName = migrationsHistoryTableName;

            return clone;
        }

        public virtual string MigrationsHistoryTableSchema => _migrationsHistoryTableSchema;

        public virtual RelationalOptionsExtension WithMigrationsHistoryTableSchema([CanBeNull] string migrationsHistoryTableSchema)
        {
            var clone = Clone();

            clone._migrationsHistoryTableSchema = migrationsHistoryTableSchema;

            return clone;
        }

        public virtual Func<ExecutionStrategyContext, IExecutionStrategy> ExecutionStrategyFactory => _executionStrategyFactory;

        public virtual RelationalOptionsExtension WithExecutionStrategyFactory(
            [CanBeNull] Func<ExecutionStrategyContext, IExecutionStrategy> executionStrategyFactory)
        {
            var clone = Clone();

            clone._executionStrategyFactory = executionStrategyFactory;

            return clone;
        }

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

        public abstract bool ApplyServices(IServiceCollection services);

        public virtual long GetServiceProviderHashCode() => 0;

        public virtual void Validate(IDbContextOptions options)
        {
        }
    }
}
