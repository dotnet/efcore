// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Migrations
{
    public interface IMigrator
    {
        IReadOnlyList<Migration> GetUnappliedMigrations();

        bool HasPendingModelChanges();

        Task ApplyMigrationsAsync([CanBeNull] string targetMigration = null);

        void ApplyMigrations([CanBeNull] string targetMigration = null);

        string ScriptMigrations(
            [CanBeNull] string fromMigrationName,
            [CanBeNull] string toMigrationName,
            bool idempotent = false);
    }
}
