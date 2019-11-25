// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class LoggingSqlServerTest : LoggingRelationalTestBase<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>
    {
        [ConditionalFact]
        public void Logs_context_initialization_row_number_paging()
        {
            Assert.Equal(
                ExpectedMessage("RowNumberPaging " + DefaultOptions),
#pragma warning disable 618
                ActualMessage(s => CreateOptionsBuilder(s, b => ((SqlServerDbContextOptionsBuilder)b).UseRowNumberForPaging())));
#pragma warning restore 618
        }

        protected override DbContextOptionsBuilder CreateOptionsBuilder(
            IServiceCollection services,
            Action<RelationalDbContextOptionsBuilder<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>> relationalAction)
            => new DbContextOptionsBuilder()
                .UseInternalServiceProvider(services.AddEntityFrameworkSqlServer().BuildServiceProvider())
                .UseSqlServer("Data Source=LoggingSqlServerTest.db", relationalAction);

        protected override string ProviderName => "Microsoft.EntityFrameworkCore.SqlServer";
    }
}
