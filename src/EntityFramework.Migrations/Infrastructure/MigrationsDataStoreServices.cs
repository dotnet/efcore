// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public abstract class MigrationsDataStoreServices : DataStoreServices
    {
        public abstract Migrator Migrator { get; }

        public static Func<IServiceProvider, LazyRef<Migrator>> MigratorFactory
        {
            get { return p => new LazyRef<Migrator>(() => ((MigrationsDataStoreServices)GetStoreServices(p)).Migrator); }
        }
    }
}
