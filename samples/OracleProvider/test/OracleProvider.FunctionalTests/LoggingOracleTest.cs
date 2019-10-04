// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Oracle.Infrastructure.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class LoggingOracleTest : LoggingRelationalTestBase<OracleDbContextOptionsBuilder, OracleOptionsExtension>
    {
        protected override DbContextOptionsBuilder CreateOptionsBuilder(
            Action<RelationalDbContextOptionsBuilder<OracleDbContextOptionsBuilder, OracleOptionsExtension>> relationalAction)
            => new DbContextOptionsBuilder().UseOracle(OracleTestStore.CreateConnectionString("LoggingOracleTest"), relationalAction);

        protected override string ProviderName => "Microsoft.EntityFrameworkCore.Oracle";
    }
}
