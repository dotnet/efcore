// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;
using Microsoft.Data.Entity.SqlServer.FunctionalTests.TestModels;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerOptimisticConcurrencyTest : OptimisticConcurrencyTestBase<SqlServerTestStore>
    {
        public override Task<SqlServerTestStore> CreateTestStoreAsync()
        {
            return SqlServerF1Context.GetSharedStoreAsync();
        }

        public override F1Context CreateF1Context(SqlServerTestStore testStore)
        {
            return SqlServerF1Context.Create(testStore);
        }
    }
}
