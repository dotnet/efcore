// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class SqliteServiceCollectionExtensionsTest : EntityFrameworkServiceCollectionExtensionsTest
    {
        [Fact]
        public void Calling_AddEntityFramework_explicitly_does_not_change_services()
        {
            var services1 = new ServiceCollection().AddEntityFrameworkSqlite();
            var services2 = new ServiceCollection().AddEntityFrameworkSqlite();

            ServiceCollectionRelationalProviderInfrastructure.TryAddDefaultRelationalServices(services2);

            AssertServicesSame(services1, services2);
        }

        public SqliteServiceCollectionExtensionsTest()
            : base(SqliteTestHelpers.Instance)
        {
        }
    }
}
