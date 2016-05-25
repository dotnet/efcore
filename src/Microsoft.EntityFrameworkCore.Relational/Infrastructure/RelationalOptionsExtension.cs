// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
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
        }

        public virtual string ConnectionString
        {
            get { return _connectionString; }
            [param: NotNull]
            set
            {
                Check.NotEmpty(value, nameof(value));

                _connectionString = value;
            }
        }

        public virtual DbConnection Connection
        {
            get { return _connection; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _connection = value;
            }
        }

        public virtual int? CommandTimeout
        {
            get { return _commandTimeout; }
            [param: CanBeNull]
            set
            {
                if (value.HasValue
                    && (value <= 0))
                {
                    throw new InvalidOperationException(RelationalStrings.InvalidCommandTimeout);
                }

                _commandTimeout = value;
            }
        }

        public virtual int? MaxBatchSize
        {
            get { return _maxBatchSize; }
            [param: CanBeNull]
            set
            {
                if (value.HasValue
                    && (value <= 0))
                {
                    throw new InvalidOperationException(RelationalStrings.InvalidMaxBatchSize);
                }

                _maxBatchSize = value;
            }
        }

        public virtual bool UseRelationalNulls
        {
            get { return _useRelationalNulls; }
            set { _useRelationalNulls = value; }
        }

        public virtual string MigrationsAssembly
        {
            get { return _migrationsAssembly; }
            [param: CanBeNull] set { _migrationsAssembly = value; }
        }

        public virtual string MigrationsHistoryTableName
        {
            get { return _migrationsHistoryTableName; }
            [param: CanBeNull] set { _migrationsHistoryTableName = value; }
        }

        public virtual string MigrationsHistoryTableSchema
        {
            get { return _migrationsHistoryTableSchema; }
            [param: CanBeNull] set { _migrationsHistoryTableSchema = value; }
        }

        public static RelationalOptionsExtension Extract([NotNull] IDbContextOptions options)
        {
            Check.NotNull(options, nameof(options));

            var relationalOptionsExtensions
                = options.Extensions
                    .OfType<RelationalOptionsExtension>()
                    .ToArray();

            if (relationalOptionsExtensions.Length == 0)
            {
                throw new InvalidOperationException(RelationalStrings.NoProviderConfigured);
            }

            if (relationalOptionsExtensions.Length > 1)
            {
                throw new InvalidOperationException(RelationalStrings.MultipleProvidersConfigured);
            }

            return relationalOptionsExtensions[0];
        }

        public abstract void ApplyServices(IServiceCollection services);
    }
}
