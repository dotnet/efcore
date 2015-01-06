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

        private string _connectionString;
        private DbConnection _connection;
        private int? _commandTimeout;

        public virtual string ConnectionString
        {
            get { return _connectionString; }

            [param: NotNull]
            set
            {
                Check.NotEmpty(value, "value");

                _connectionString = value;
            }
        }

        public virtual DbConnection Connection
        {
            get { return _connection; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _connection = value;
            }
        }

        public virtual int? CommandTimeout
        {
            get { return _commandTimeout; }
            set
            {
                if (value.HasValue && value < 0)
                {
                    throw new ArgumentException(Strings.InvalidCommandTimeout);
                }

                _commandTimeout = value;
            }
        }

        protected override void Configure(IReadOnlyDictionary<string, string> rawOptions)
        {
            Check.NotNull(rawOptions, "rawOptions");

            if (string.IsNullOrEmpty(_connectionString))
            {
                rawOptions.TryGetValue(ConnectionStringKey, out _connectionString);
            }
        }

        public static RelationalOptionsExtension Extract([NotNull] IDbContextOptions options)
        {
            Check.NotNull(options, "options");

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
