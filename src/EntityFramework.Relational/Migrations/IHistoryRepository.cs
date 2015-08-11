// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Migrations
{
    // TODO: Consider upgrade scenarios
    public interface IHistoryRepository
    {
        bool Exists();
        IReadOnlyList<HistoryRow> GetAppliedMigrations();
        string GetCreateScript();
        string GetCreateIfNotExistsScript();
        string GetInsertScript([NotNull] HistoryRow row);
        string GetDeleteScript([NotNull] string migrationId);
        string GetBeginIfNotExistsScript([NotNull] string migrationId);
        string GetBeginIfExistsScript([NotNull] string migrationId);
        string GetEndIfScript();
    }
}
