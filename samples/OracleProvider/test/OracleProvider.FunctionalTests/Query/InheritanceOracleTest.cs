// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceOracleTest : InheritanceRelationalTestBase<InheritanceOracleFixture>
    {
        public InheritanceOracleTest(InheritanceOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());
    }
}
