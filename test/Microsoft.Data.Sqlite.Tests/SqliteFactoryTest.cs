// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Sqlite
{
    public class SqliteFactoryTest
    {
        [Fact]
        public void CreateConnection_works()
        {
            Assert.IsType<SqliteConnection>(SqliteFactory.Instance.CreateConnection());
        }

        [Fact]
        public void CreateConnectionStringBuilder_works()
        {
            Assert.IsType<SqliteConnectionStringBuilder>(SqliteFactory.Instance.CreateConnectionStringBuilder());
        }

        [Fact]
        public void CreateCommand_works()
        {
            Assert.IsType<SqliteCommand>(SqliteFactory.Instance.CreateCommand());
        }

        [Fact]
        public void CreateParameter_works()
        {
            Assert.IsType<SqliteParameter>(SqliteFactory.Instance.CreateParameter());
        }
    }
}
