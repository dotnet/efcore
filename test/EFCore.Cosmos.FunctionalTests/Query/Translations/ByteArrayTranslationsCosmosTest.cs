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
        => AssertTranslationFailed(base.Length);

    public override Task Index()
        => AssertTranslationFailed(base.Index);

    public override Task First()
        => AssertTranslationFailed(base.First);

    public override Task Contains_with_constant()
        => AssertTranslationFailed(base.Contains_with_constant);

    public override Task Contains_with_parameter()
        => AssertTranslationFailed(base.Contains_with_parameter);

    public override Task Contains_with_column()
        => AssertTranslationFailed(base.Contains_with_column);

    public override Task Contains_with_parameter_above_ascii_range()
        => AssertTranslationFailed(base.Contains_with_parameter_above_ascii_range);

    public override Task Any()
        => AssertTranslationFailed(base.Any);

    public override Task SequenceEqual()
        => AssertTranslationFailed(base.SequenceEqual);

    public override Task Concat_with_parameter()
        => AssertTranslationFailed(base.Concat_with_parameter);

    public override Task Concat_with_inline_array()
        => AssertTranslationFailed(base.Concat_with_inline_array);

    public override Task Concat_with_empty_array()
        => AssertTranslationFailed(base.Concat_with_empty_array);

    public override Task Concat_with_column()
        => AssertTranslationFailed(base.Concat_with_column);

    public override Task Concat_chained()
        => AssertTranslationFailed(base.Concat_chained);

    public override Task Concat_in_projection()
        => AssertTranslationFailed(base.Concat_in_projection);

    public override Task Concat_SequenceEqual()
        => AssertTranslationFailed(base.Concat_SequenceEqual);

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
