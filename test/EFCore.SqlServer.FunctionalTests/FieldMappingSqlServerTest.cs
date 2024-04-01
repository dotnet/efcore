// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class FieldMappingSqlServerTest(FieldMappingSqlServerTest.FieldMappingSqlServerFixture fixture) : FieldMappingTestBase<FieldMappingSqlServerTest.FieldMappingSqlServerFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class FieldMappingSqlServerFixture : FieldMappingFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
