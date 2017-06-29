// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class LoggingSqliteTest : LoggingRelationalTest<SqliteDbContextOptionsBuilder, SqliteOptionsExtension>
    {
        [Fact]
        public void Logs_context_initialization_no_FKs()
        {
            Assert.Equal(
                ExpectedMessage("SuppressForeignKeyEnforcement " + DefaultOptions),
                ActualMessage(CreateOptionsBuilder(b => ((SqliteDbContextOptionsBuilder)b).SuppressForeignKeyEnforcement())));
        }

        protected override DbContextOptionsBuilder CreateOptionsBuilder(
            Action<RelationalDbContextOptionsBuilder<SqliteDbContextOptionsBuilder, SqliteOptionsExtension>> relationalAction)
            => new DbContextOptionsBuilder().UseSqlite("Data Source=LoggingSqliteTest.db", relationalAction);

        protected override string ProviderName => "Microsoft.EntityFrameworkCore.Sqlite";
    }
}
