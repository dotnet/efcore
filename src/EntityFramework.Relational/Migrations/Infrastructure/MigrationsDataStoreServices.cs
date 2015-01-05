// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Relational.Migrations.Infrastructure
{
    public abstract class MigrationsDataStoreServices : DataStoreServices
    {
        public abstract Migrator Migrator { get; }

        public static Func<IServiceProvider, DbContextService<Migrator>> MigratorFactory
        {
            get { return p => new DbContextService<Migrator>(() => ((MigrationsDataStoreServices)GetStoreServices(p)).Migrator); }
        }
    }
}
