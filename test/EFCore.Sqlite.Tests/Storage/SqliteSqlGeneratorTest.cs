// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Relational.Tests.Storage;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Sqlite.Tests.Storage
{
    public class SqliteSqlGeneratorTest : SqlGeneratorTestBase
    {
        protected override ISqlGenerationHelper CreateSqlGenerationHelper()
            => new SqliteSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());
    }
}
