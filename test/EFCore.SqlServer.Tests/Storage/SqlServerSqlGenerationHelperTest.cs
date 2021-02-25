// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class SqlServerSqlGenerationHelperTest
    {
        [ConditionalFact]
        public void BatchSeparator_returns_separator()
            => Assert.Equal("GO" + Environment.NewLine + Environment.NewLine, CreateSqlGenerationHelper().BatchTerminator);

        private ISqlGenerationHelper CreateSqlGenerationHelper()
            => new SqlServerSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());
    }
}
