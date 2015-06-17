// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class RelationalDatabaseExtensionsTest
    {
        [Fact]
        public void GetDbConnection_returns_the_current_connection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            var connectionMock = new Mock<IRelationalConnection>();
            connectionMock.SetupGet(m => m.DbConnection).Returns(dbConnectionMock.Object);

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddInstance(connectionMock.Object));

            Assert.Same(dbConnectionMock.Object, context.Database.GetDbConnection());
        }

        [Fact]
        public void Relational_specific_methods_throws_when_non_relational_provider_is_in_use()
        {
            var context = RelationalTestHelpers.Instance.CreateContext();

            Assert.Equal(
                Strings.RelationalNotInUse,
                Assert.Throws<InvalidOperationException>(() => context.Database.GetDbConnection()).Message);
        }
    }
}
