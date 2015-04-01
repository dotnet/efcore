// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Update;

namespace Microsoft.Data.Entity.Sqlite.Update
{
    public class SqliteModificationCommandBatchFactory : ModificationCommandBatchFactory, ISqliteModificationCommandBatchFactory
    {
        public SqliteModificationCommandBatchFactory([NotNull] ISqliteSqlGenerator sqlGenerator)
            : base(sqlGenerator)
        {
        }

        public override ModificationCommandBatch Create(IDbContextOptions options) =>
            new SqliteModificationCommandBatch(SqlGenerator);
    }
}
