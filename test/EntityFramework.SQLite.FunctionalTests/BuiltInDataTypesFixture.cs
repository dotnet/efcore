// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;


namespace Microsoft.Data.Entity.SQLite.FunctionalTests
{
    public class BuiltInDataTypesFixture : BuiltInDataTypesFixtureBase, IDisposable
    {
        private DbContext _context;

        public override DbContext CreateContext()
        {
            var testDatabase = SQLiteTestDatabase.Scratch().Result;

            var options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseSQLite(testDatabase.Connection.ConnectionString);

            _context = new DbContext(options);
            _context.Database.EnsureCreated();
            return _context;
        }

        void IDisposable.Dispose()
        {
            if (_context != null)
            {
                _context.Dispose();
            }
        }
    }
}
