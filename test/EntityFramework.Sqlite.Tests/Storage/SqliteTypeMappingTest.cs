// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace Microsoft.Data.Entity.Storage
{
    public class SqliteTypeMappingTest : RelationalTypeMappingTest
    {
        protected override DbCommand CreateTestCommand()
            => new SqliteCommand();

        protected override DbType DefaultParameterType
            => DbType.String;
    }
}
