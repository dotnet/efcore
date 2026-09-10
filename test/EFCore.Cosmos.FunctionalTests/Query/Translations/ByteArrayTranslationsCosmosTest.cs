// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class ByteArrayTranslationsCosmosTest : ByteArrayTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public ByteArrayTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Length()
        => AssertTranslationFailed(() => base.Length());

    public override Task Index()
        => AssertTranslationFailed(() => base.Index());

    public override Task First()
        => AssertTranslationFailed(() => base.First());

    public override Task Contains_with_constant()
        => AssertTranslationFailed(() => base.Contains_with_constant());

    public override Task Contains_with_parameter()
        => AssertTranslationFailed(() => base.Contains_with_parameter());

    public override Task Contains_with_column()
        => AssertTranslationFailed(() => base.Contains_with_column());

    public override Task SequenceEqual()
        => AssertTranslationFailed(() => base.SequenceEqual());

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
