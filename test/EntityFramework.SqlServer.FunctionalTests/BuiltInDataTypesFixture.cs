// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class BuiltInDataTypesFixture : BuiltInDataTypesFixtureBase
    {
        public override DbContext CreateContext()
        {
            var testDatabase = SqlServerTestDatabase.Scratch().Result;

            var options
                = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseSqlServer(testDatabase.Connection.ConnectionString);

            var context = new DbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
