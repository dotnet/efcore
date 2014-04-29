// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Update;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerDataStore : RelationalDataStore
    {
        public SqlServerDataStore(
            [NotNull] ContextConfiguration configuration,
            [NotNull] SqlServerConnection connection,
            [NotNull] DatabaseBuilder databaseBuilder,
            [NotNull] CommandBatchPreparer batchPreparer,
            [NotNull] SqlServerBatchExecutor batchExecutor)
            : base(configuration, connection, databaseBuilder, batchPreparer, batchExecutor)
        {
        }
    }
}
