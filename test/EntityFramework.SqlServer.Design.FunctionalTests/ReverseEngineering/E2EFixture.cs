// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.SqlServer.FunctionalTests;

namespace EntityFramework.SqlServer.Design.ReverseEngineering.FunctionalTests
{
    public class E2EFixture
    {
        public E2EFixture()
        {
            SqlServerTestStore.CreateDatabase(
                "SqlServerReverseEngineerTestE2E", @"ReverseEngineering\E2E.sql", true);
        }
    }
}
