// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Migrations
{
    public abstract class MigrationsEnabledDatabase : RelationalDatabase, IMigrationsEnabledDatabaseInternals
    {
        private readonly LazyRef<Migrator> _migrator;

        protected MigrationsEnabledDatabase(
            [NotNull] DbContextConfiguration configuration,
            [NotNull] ILoggerFactory loggerFactory)
            : base(configuration, loggerFactory)
        {
            // TODO: Decouple from DbContextConfiguration (Issue #641)
            _migrator = new LazyRef<Migrator>(() => ((MigrationsDataStoreServices)configuration.DataStoreServices).Migrator);
        }

        public virtual void ApplyMigrations()
        {
            Migrator.ApplyMigrations();
        }

        protected virtual Migrator Migrator
        {
            get { return _migrator.Value; }
        }

        Migrator IMigrationsEnabledDatabaseInternals.Migrator
        {
            get { return Migrator; }
        }
    }
}
