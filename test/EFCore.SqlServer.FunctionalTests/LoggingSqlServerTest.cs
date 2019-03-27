// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class LoggingSqlServerTest : LoggingRelationalTestBase<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>
    {
        [Fact]
        public void Logs_context_initialization_row_number_paging()
        {
            Assert.Equal(
                ExpectedMessage("RowNumberPaging " + DefaultOptions),
                ActualMessage(CreateOptionsBuilder(b => ((SqlServerDbContextOptionsBuilder)b).UseRowNumberForPaging())));
        }

        protected override DbContextOptionsBuilder CreateOptionsBuilder(
            Action<RelationalDbContextOptionsBuilder<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>> relationalAction)
            => new DbContextOptionsBuilder().UseSqlServer("Data Source=LoggingSqlServerTest.db", relationalAction);

        protected override string ProviderName => "Microsoft.EntityFrameworkCore.SqlServer";
    }
}
