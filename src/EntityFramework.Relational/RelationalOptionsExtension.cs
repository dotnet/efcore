// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class RelationalOptionsExtension : DbContextOptionsExtension
    {
        private const string ConnectionStringKey = "ConnectionString";

        private string _connectionString;
        private DbConnection _connection;
        private Assembly _migrationAssembly;
        private string _migrationNamespace;

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

        public virtual Assembly MigrationAssembly
        {
            get { return _migrationAssembly; }

            [param: NotNull] set { _migrationAssembly = Check.NotNull(value, "value"); }
        }

        public virtual string MigrationNamespace
        {
            get { return _migrationNamespace; }

            [param: NotNull] set { _migrationNamespace = Check.NotEmpty(value, "value"); }
        }

        protected override void Configure(IReadOnlyDictionary<string, string> rawOptions)
        {
            Check.NotNull(rawOptions, "rawOptions");

            if (string.IsNullOrEmpty(_connectionString))
            {
                rawOptions.TryGetValue(ConnectionStringKey, out _connectionString);
            }

            // TODO: Read other options.
        }

        public static RelationalOptionsExtension Extract([NotNull] IDbContextOptions options)
        {
            Check.NotNull(options, "options");

            var storeConfigs = options.Extensions
                .OfType<RelationalOptionsExtension>()
                .ToArray();

            if (storeConfigs.Length == 0)
            {
                throw new InvalidOperationException(Strings.FormatNoDataStoreConfigured());
            }

            if (storeConfigs.Length > 1)
            {
                throw new InvalidOperationException(Strings.FormatMultipleDataStoresConfigured());
            }

            return storeConfigs[0];
        }
    }
}
