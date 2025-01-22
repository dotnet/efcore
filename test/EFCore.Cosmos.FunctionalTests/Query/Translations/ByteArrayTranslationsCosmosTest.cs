// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class ByteArrayTranslationsCosmosTest : ByteArrayTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public ByteArrayTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Length(bool async)
        => AssertTranslationFailed(() => base.Length(async));

    public override Task Index(bool async)
        => AssertTranslationFailed(() => base.Index(async));

    public override Task First(bool async)
        => AssertTranslationFailed(() => base.First(async));

    public override Task Contains_with_constant(bool async)
        => AssertTranslationFailed(() => base.Contains_with_constant(async));

    public override Task Contains_with_parameter(bool async)
        => AssertTranslationFailed(() => base.Contains_with_parameter(async));

    public override Task Contains_with_column(bool async)
        => AssertTranslationFailed(() => base.Contains_with_column(async));

    public override Task SequenceEqual(bool async)
        => AssertTranslationFailed(() => base.SequenceEqual(async));

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
