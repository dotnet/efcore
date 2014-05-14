// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Update;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteDataStore : RelationalDataStore
    {
        public SQLiteDataStore(
            [NotNull] DbContextConfiguration configuration,
            [NotNull] SQLiteConnectionConnection connection,
            [NotNull] CommandBatchPreparer batchPreparer,
            [NotNull] SQLiteBatchExecutor batchExecutor)
            : base(configuration, connection, batchPreparer, batchExecutor)
        {
        }
    }
}
