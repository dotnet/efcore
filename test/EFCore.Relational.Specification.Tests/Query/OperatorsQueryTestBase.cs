// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Operators;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class OperatorsQueryTestBase : NonSharedModelTestBase
{
    protected OperatorsData ExpectedData { get; init; }

    protected OperatorsQueryTestBase(ITestOutputHelper testOutputHelper)
    {
        //TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        ExpectedData = OperatorsData.Instance;
    }

    protected override string StoreName
        => "OperatorsTest";

    protected virtual void Seed(OperatorsContext ctx)
    {
        ctx.Set<OperatorEntityString>().AddRange(ExpectedData.OperatorEntitiesString);
        ctx.Set<OperatorEntityInt>().AddRange(ExpectedData.OperatorEntitiesInt);
        ctx.Set<OperatorEntityNullableInt>().AddRange(ExpectedData.OperatorEntitiesNullableInt);
        ctx.Set<OperatorEntityLong>().AddRange(ExpectedData.OperatorEntitiesLong);
        ctx.Set<OperatorEntityBool>().AddRange(ExpectedData.OperatorEntitiesBool);
        ctx.Set<OperatorEntityNullableBool>().AddRange(ExpectedData.OperatorEntitiesNullableBool);
        ctx.Set<OperatorEntityDateTimeOffset>().AddRange(ExpectedData.OperatorEntitiesDateTimeOffset);

        ctx.SaveChanges();
    }

    [ConditionalFact(Skip = "issue #30245")]
    public virtual async Task Bitwise_and_on_expression_with_like_and_null_check_being_compared_to_false()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using var context = contextFactory.CreateContext();

        var expected = (from o1 in ExpectedData.OperatorEntitiesString
                        from o2 in ExpectedData.OperatorEntitiesString
                        from o3 in ExpectedData.OperatorEntitiesBool
                        where ((o2.Value == "B" || o3.Value) & (o1.Value != null)) != false
                        select new { Value1 = o1.Value, Value2 = o2.Value, Value3 = o3.Value }).ToList();

        var actual = (from o1 in context.Set<OperatorEntityString>()
                      from o2 in context.Set<OperatorEntityString>()
                      from o3 in context.Set<OperatorEntityBool>()
                      where ((EF.Functions.Like(o2.Value, "B") || o3.Value) & (o1.Value != null)) != false
                      select new { Value1 = o1.Value, Value2 = o2.Value, Value3 = o3.Value }).ToList();

        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Value1, actual[i].Value1);
            Assert.Equal(expected[i].Value2, actual[i].Value2);
            Assert.Equal(expected[i].Value3, actual[i].Value3);
        }
    }

    [ConditionalFact(Skip = "issue #30248")]
    public virtual async Task Complex_predicate_with_bitwise_and_modulo_and_negation()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using var context = contextFactory.CreateContext();

        var expected = (from e0 in ExpectedData.OperatorEntitiesLong
                        from e1 in ExpectedData.OperatorEntitiesLong
                        from e2 in ExpectedData.OperatorEntitiesLong
                        from e3 in ExpectedData.OperatorEntitiesLong
                        where ((((e1.Value % 2) / e0.Value) & (((e3.Value | e2.Value) - e0.Value) - (e2.Value * e2.Value))) >= (((e1.Value / ~(e3.Value)) % (long)((1 + 1))) % (~(e0.Value) + 1)))
                        select new { Value0 = e0.Value, Value1 = e1.Value, Value2 = e2.Value, Value3 = e3.Value }).ToList();

        var actual = (from e0 in context.Set<OperatorEntityLong>()
                      from e1 in context.Set<OperatorEntityLong>()
                      from e2 in context.Set<OperatorEntityLong>()
                      from e3 in context.Set<OperatorEntityLong>()
                      where ((((e1.Value % 2) / e0.Value) & (((e3.Value | e2.Value) - e0.Value) - (e2.Value * e2.Value))) >= (((e1.Value / ~(e3.Value)) % (long)((1 + 1))) % (~(e0.Value) + 1)))
                      select new { Value0 = e0.Value, Value1 = e1.Value, Value2 = e2.Value, Value3 = e3.Value }).ToList();


        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Value0, actual[i].Value0);
            Assert.Equal(expected[i].Value1, actual[i].Value1);
            Assert.Equal(expected[i].Value2, actual[i].Value2);
            Assert.Equal(expected[i].Value3, actual[i].Value3);
        }
    }

    [ConditionalFact(Skip = "issue #30248")]
    public virtual async Task Complex_predicate_with_bitwise_and_arithmetic_operations()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using var context = contextFactory.CreateContext();

        var expected = (from e0 in ExpectedData.OperatorEntitiesInt
                        from e1 in ExpectedData.OperatorEntitiesInt
                        from e2 in ExpectedData.OperatorEntitiesBool
                        where (((((e1.Value & (e0.Value + e0.Value)) & e0.Value) / 1) > (e1.Value & (int)((8 + 2)))) && e2.Value)
                        select new { Value0 = e0.Value, Value1 = e1.Value, Value2 = e2.Value }).ToList();

        var actual = (from e0 in context.Set<OperatorEntityInt>()
                      from e1 in context.Set<OperatorEntityInt>()
                      from e2 in context.Set<OperatorEntityBool>()
                      where (((((e1.Value & (e0.Value + e0.Value)) & e0.Value) / 1) > (e1.Value & (int)((8 + 2)))) && e2.Value)
                      select new { Value0 = e0.Value, Value1 = e1.Value, Value2 = e2.Value }).ToList();


        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Value0, actual[i].Value0);
            Assert.Equal(expected[i].Value1, actual[i].Value1);
            Assert.Equal(expected[i].Value2, actual[i].Value2);
        }
    }

    [ConditionalFact(Skip = "issue #30277")]
    public virtual async Task Projection_with_not_and_negation_on_integer()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using var context = contextFactory.CreateContext();

        var expected = (from e3 in ExpectedData.OperatorEntitiesLong
                        from e4 in ExpectedData.OperatorEntitiesLong
                        from e5 in ExpectedData.OperatorEntitiesLong
                        orderby e3.Id, e4.Id, e5.Id
                        select ((~(-(-((e5.Value + e3.Value) + 2))) % (-(e4.Value + e4.Value) - e3.Value)))).ToList();

        var actual = (from e3 in context.Set<OperatorEntityLong>()
                      from e4 in context.Set<OperatorEntityLong>()
                      from e5 in context.Set<OperatorEntityLong>()
                      orderby e3.Id, e4.Id, e5.Id
                      select ((~(-(-((e5.Value + e3.Value) + 2))) % (-(e4.Value + e4.Value) - e3.Value)))).ToList();
       
        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i], actual[i]);
        }
    }
}
