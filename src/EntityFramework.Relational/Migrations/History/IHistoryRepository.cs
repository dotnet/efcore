// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Operations;

namespace Microsoft.Data.Entity.Migrations.History
{
    // TODO: Consider upgrade scenarios
    public interface IHistoryRepository
    {
        bool Exists();
        IReadOnlyList<IHistoryRow> GetAppliedMigrations();
        string Create(bool ifNotExists);
        MigrationOperation GetInsertOperation([NotNull] IHistoryRow row);
        MigrationOperation GetDeleteOperation([NotNull] string migrationId);
        string BeginIfNotExists([NotNull] string migrationId);
        string BeginIfExists([NotNull] string migrationId);
        string EndIf();
    }
}
