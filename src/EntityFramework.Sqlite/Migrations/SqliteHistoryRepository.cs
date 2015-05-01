// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Operations;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteHistoryRepository : ISqliteHistoryRepository
    {
        public virtual string BeginIfExists(string migrationId)
        {
            throw new NotImplementedException();
        }

        public virtual string BeginIfNotExists(string migrationId)
        {
            throw new NotImplementedException();
        }

        public virtual string Create(bool ifNotExists)
        {
            throw new NotImplementedException();
        }

        public virtual string EndIf()
        {
            throw new NotImplementedException();
        }

        public virtual bool Exists()
        {
            throw new NotImplementedException();
        }

        public virtual IReadOnlyList<IHistoryRow> GetAppliedMigrations()
        {
            throw new NotImplementedException();
        }

        public virtual MigrationOperation GetDeleteOperation(string migrationId)
        {
            throw new NotImplementedException();
        }

        public virtual MigrationOperation GetInsertOperation(IHistoryRow row)
        {
            throw new NotImplementedException();
        }
    }
}
