// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Relational.Update;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerBatchExecutor : BatchExecutor
    {
        public SqlServerBatchExecutor(
            [NotNull] SqlServerSqlGenerator sqlGenerator,
            [NotNull] SqlServerConnection connection)
            : base(sqlGenerator, connection)
        {
        }
    }
}
