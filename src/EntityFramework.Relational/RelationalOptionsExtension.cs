// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class RelationalOptionsExtension : DbContextOptionsExtension
    {
        private const string ConnectionStringKey = "ConnectionString";
        private const string CommandTimeoutKey = "CommandTimeout";
        private const string MaxBatchSizeKey = "MaxBatchSize";

        private string _connectionString;
        private DbConnection _connection;
        private int? _commandTimeout;
        private int? _maxBatchSize;

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
                    && value <= 0)
                {
                    throw new InvalidOperationException(Strings.InvalidCommandTimeout);
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
                    && value <= 0)
                {
                    throw new InvalidOperationException(Strings.InvalidMaxBatchSize);
                }

                _maxBatchSize = value;
            }
        }

        public virtual string MigrationsAssembly { get; [param: CanBeNull] set; }

        protected override void Configure(IReadOnlyDictionary<string, string> rawOptions)
        {
            base.Configure(rawOptions);

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                var connectionString = GetSetting<string>(ConnectionStringKey);
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    ConnectionString = connectionString;
                }
            }

            if (!_commandTimeout.HasValue)
            {
                CommandTimeout = GetSetting<int?>(CommandTimeoutKey);
            }

            if (!_maxBatchSize.HasValue)
            {
                MaxBatchSize = GetSetting<int?>(MaxBatchSizeKey);
            }
        }

        public static RelationalOptionsExtension Extract([NotNull] IDbContextOptions options)
        {
            Check.NotNull(options, nameof(options));

            var storeConfigs = options.Extensions
                .OfType<RelationalOptionsExtension>()
                .ToArray();

            if (storeConfigs.Length == 0)
            {
                throw new InvalidOperationException(Strings.NoDataStoreConfigured);
            }

            if (storeConfigs.Length > 1)
            {
                throw new InvalidOperationException(Strings.MultipleDataStoresConfigured);
            }

            return storeConfigs[0];
        }
    }
}
