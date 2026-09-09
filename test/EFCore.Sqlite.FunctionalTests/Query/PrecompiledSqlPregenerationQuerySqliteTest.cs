// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class PrecompiledSqlPregenerationQuerySqlServerTest(
    PrecompiledSqlPregenerationQuerySqlServerTest.PrecompiledSqlPregenerationQuerySqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : PrecompiledSqlPregenerationQueryRelationalTestBase(fixture, testOutputHelper),
        IClassFixture<PrecompiledSqlPregenerationQuerySqlServerTest.PrecompiledSqlPregenerationQuerySqliteFixture>
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    public class PrecompiledSqlPregenerationQuerySqliteFixture : PrecompiledSqlPregenerationQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
            => SqlitePrecompiledQueryTestHelpers.Instance;
    }
}
