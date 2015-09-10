// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Sqlite.Tests
{
    public class SqliteFactoryTest
    {
        [Fact]
        public void Factory_creates_expected_types()
        {
            Assert.IsType<SqliteConnection>(SqliteFactory.Instance.CreateConnection());
            Assert.IsType<SqliteConnectionStringBuilder>(SqliteFactory.Instance.CreateConnectionStringBuilder());
            Assert.IsType<SqliteCommand>(SqliteFactory.Instance.CreateCommand());
            Assert.IsType<SqliteParameter>(SqliteFactory.Instance.CreateParameter());
        }
    }
}
