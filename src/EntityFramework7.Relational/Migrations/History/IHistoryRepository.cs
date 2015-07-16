// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Operations;

namespace Microsoft.Data.Entity.Migrations.History
{
    // TODO: Consider upgrade scenarios
    public interface IHistoryRepository
    {
        bool Exists();
        Task<bool> ExistsAsync();
        IReadOnlyList<IHistoryRow> GetAppliedMigrations();
        Task<IReadOnlyList<IHistoryRow>> GetAppliedMigrationsAsync();
        string Create(bool ifNotExists);
        MigrationOperation GetInsertOperation([NotNull] IHistoryRow row);
        MigrationOperation GetDeleteOperation([NotNull] string migrationId);
        string BeginIfNotExists([NotNull] string migrationId);
        string BeginIfExists([NotNull] string migrationId);
        string EndIf();
    }
}
