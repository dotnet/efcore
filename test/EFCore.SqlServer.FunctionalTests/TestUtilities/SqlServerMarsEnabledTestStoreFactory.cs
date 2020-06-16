// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class SqlServerMarsEnabledTestStoreFactory : SqlServerTestStoreFactory
    {
        public static new SqlServerMarsEnabledTestStoreFactory Instance => new SqlServerMarsEnabledTestStoreFactory();

        protected SqlServerMarsEnabledTestStoreFactory()
        {
        }

        public override TestStore GetOrCreate(string storeName)
            => SqlServerTestStore.GetOrCreate(storeName, scriptPath: null, multipleActiveResultSets: true);

    }
}
