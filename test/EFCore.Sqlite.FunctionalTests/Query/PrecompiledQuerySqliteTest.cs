// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

public class PrecompiledQuerySqliteTest(
    PrecompiledQuerySqliteTest.PrecompiledQuerySqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : PrecompiledQueryRelationalTestBase(fixture, testOutputHelper),
        IClassFixture<PrecompiledQuerySqliteTest.PrecompiledQuerySqliteFixture>
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    [ConditionalFact]
    public virtual Task Glob()
        => Test("""_ = context.Blogs.Where(b => EF.Functions.Glob(b.Name, "*foo*")).ToList();""");

    [ConditionalFact]
    public virtual Task Regexp()
        => Test("""_ = context.Blogs.Where(b => Regex.IsMatch(b.Name, "^foo")).ToList();""");

    public class PrecompiledQuerySqliteFixture : PrecompiledQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
            => SqlitePrecompiledQueryTestHelpers.Instance;
    }
}
