// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class SqliteSqlGeneratorTest : SqlGeneratorTestBase
    {
        protected override ISqlGenerationHelper CreateSqlGenerationHelper()
            => new SqliteSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());
    }
}
