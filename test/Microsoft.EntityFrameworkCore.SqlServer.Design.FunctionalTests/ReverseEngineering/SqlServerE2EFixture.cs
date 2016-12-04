// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design.FunctionalTests.ReverseEngineering
{
    public class SqlServerE2EFixture : IDisposable
    {
        private readonly SqlServerTestStore _testStore;

        public SqlServerE2EFixture()
        {
            _testStore = SqlServerTestStore.GetOrCreateShared(
                "SqlServerReverseEngineerTestE2E",
                () => SqlServerTestStore.ExecuteScript("SqlServerReverseEngineerTestE2E", "ReverseEngineering/E2E.sql"));
        }

        public void Dispose() => _testStore.Dispose();
    }
}
