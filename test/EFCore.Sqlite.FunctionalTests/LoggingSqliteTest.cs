// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class LoggingSqliteTest : LoggingRelationalTestBase<SqliteDbContextOptionsBuilder, SqliteOptionsExtension>
    {
        [Fact]
        public void Logs_context_initialization_no_FKs()
        {
            Assert.Equal(
                ExpectedMessage("SuppressForeignKeyEnforcement " + DefaultOptions),
                ActualMessage(s => CreateOptionsBuilder(s, b => ((SqliteDbContextOptionsBuilder)b).SuppressForeignKeyEnforcement())));
        }

        protected override DbContextOptionsBuilder CreateOptionsBuilder(
            IServiceCollection services,
            Action<RelationalDbContextOptionsBuilder<SqliteDbContextOptionsBuilder, SqliteOptionsExtension>> relationalAction)
            => new DbContextOptionsBuilder()
                .UseInternalServiceProvider(services.AddEntityFrameworkSqlite().BuildServiceProvider())
                .UseSqlite("Data Source=LoggingSqliteTest.db", relationalAction);

        protected override string ProviderName => "Microsoft.EntityFrameworkCore.Sqlite";
    }
}
