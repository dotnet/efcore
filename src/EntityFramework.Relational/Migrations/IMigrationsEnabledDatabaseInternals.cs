// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;

namespace Microsoft.Data.Entity.Relational.Migrations
{
    /// <summary>
    ///     This interface provides access to the underlying services of the
    ///     <see cref="MigrationsEnabledDatabase" /> facade API such that extension methods written
    ///     for this service can use the underlying services without these details showing up in the
    ///     API used by application developers.
    /// </summary>
    public interface IMigrationsEnabledDatabaseInternals : IDatabaseInternals
    {
        Migrator Migrator { get; }
    }
}
