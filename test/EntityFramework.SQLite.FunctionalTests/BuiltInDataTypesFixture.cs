// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SQLite.FunctionalTests
{
    public class BuiltInDataTypesFixture : BuiltInDataTypesFixtureBase
    {
        public override DbContext CreateContext()
        {
            var testDatabase = SQLiteTestDatabase.Scratch().Result;

            var options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseSQLite(testDatabase.Connection.ConnectionString);

            var context = new DbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
