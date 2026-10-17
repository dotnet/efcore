// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class BasicTypesQueryFixtureBase : SharedStoreFixtureBase<BasicTypesContext>, IQueryFixtureBase
{
    private BasicTypesData? _expectedData;

    protected override string StoreName
        => "BasicTypesTest";

    public Func<DbContext> GetContextCreator()
        => CreateContext;

    protected override Task SeedAsync(BasicTypesContext context)
    {
        var data = new BasicTypesData();
        context.AddRange(data.BasicTypesEntities);
        context.AddRange(data.NullableBasicTypesEntities);
        return context.SaveChangesAsync();
    }

    public virtual ISetSource GetExpectedData()
        => _expectedData ??= new BasicTypesData();

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object?, object?>>
    {
        { typeof(BasicTypesEntity), e => ((BasicTypesEntity?)e)?.Id },
        { typeof(NullableBasicTypesEntity), e => ((NullableBasicTypesEntity?)e)?.Id }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object?, object?>>
    {
        {
            typeof(BasicTypesEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (BasicTypesEntity)e!;
                    var aa = (BasicTypesEntity)a;

                    Assert.Equal(ee.Id, aa.Id);

                    Assert.Equal(ee.Byte, aa.Byte);
                    Assert.Equal(ee.Short, aa.Short);
                    Assert.Equal(ee.Int, aa.Int);
                    Assert.Equal(ee.Long, aa.Long);
                    Assert.Equal(ee.Float, aa.Float);
                    Assert.Equal(ee.Double, aa.Double);
                    Assert.Equal(ee.Decimal, aa.Decimal);

                    Assert.Equal(ee.String, aa.String);

                    Assert.Equal(ee.DateTime, aa.DateTime);
                    Assert.Equal(ee.DateOnly, aa.DateOnly);
                    Assert.Equal(ee.TimeOnly, aa.TimeOnly);
                    Assert.Equal(ee.DateTimeOffset, aa.DateTimeOffset);
                    Assert.Equal(ee.TimeSpan, aa.TimeSpan);

                    Assert.Equal(ee.Bool, aa.Bool);
                    Assert.Equal(ee.Guid, aa.Guid);
                    Assert.Equivalent(ee.ByteArray, aa.ByteArray);

                    Assert.Equal(ee.Enum, aa.Enum);
                    Assert.Equal(ee.FlagsEnum, aa.FlagsEnum);
                }
            }
        },
        {
            typeof(NullableBasicTypesEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (NullableBasicTypesEntity)e!;
                    var aa = (NullableBasicTypesEntity)a;

                    Assert.Equal(ee.Id, aa.Id);

                    Assert.Equal(ee.Byte, aa.Byte);
                    Assert.Equal(ee.Short, aa.Short);
                    Assert.Equal(ee.Int, aa.Int);
                    Assert.Equal(ee.Long, aa.Long);
                    Assert.Equal(ee.Float, aa.Float);
                    Assert.Equal(ee.Double, aa.Double);
                    Assert.Equal(ee.Decimal, aa.Decimal);

                    Assert.Equal(ee.String, aa.String);

                    Assert.Equal(ee.DateTime, aa.DateTime);
                    Assert.Equal(ee.DateOnly, aa.DateOnly);
                    Assert.Equal(ee.TimeOnly, aa.TimeOnly);
                    Assert.Equal(ee.DateTimeOffset, aa.DateTimeOffset);
                    Assert.Equal(ee.TimeSpan, aa.TimeSpan);

                    Assert.Equal(ee.Bool, aa.Bool);
                    Assert.Equal(ee.Guid, aa.Guid);
                    Assert.Equivalent(ee.ByteArray, aa.ByteArray);

                    Assert.Equal(ee.Enum, aa.Enum);
                    Assert.Equal(ee.FlagsEnum, aa.FlagsEnum);
                }
            }
        }
    }.ToDictionary(e => e.Key, e => (object)e.Value);
}
