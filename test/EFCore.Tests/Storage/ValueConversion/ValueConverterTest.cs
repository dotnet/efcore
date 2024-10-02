// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class ValueConverterTest
{
    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Value_converters_are_run_for_in_memory_database(bool async)
    {
        using (var context = new InMemoryConvertersContext())
        {
            await context.AddAsync(
                new Person
                {
                    Id = async ? 1 : 2,
                    ConvertedGoingIn = new DateTime(2015, 1, 10, 8, 8, 8, DateTimeKind.Local),
                    ConvertedComingOut = new DateTime(2015, 1, 10, 9, 9, 9, DateTimeKind.Local)
                });

            Assert.Equal(1, async ? await context.SaveChangesAsync() : context.SaveChanges());
        }

        using (var context = new InMemoryConvertersContext())
        {
            var person = context.Set<Person>().Find(async ? 1L : 2L);

            Assert.Equal(DateTimeKind.Utc, person.ConvertedGoingIn.Kind);
            Assert.Equal(new DateTime(2015, 1, 10, 8, 8, 8, DateTimeKind.Utc), person.ConvertedGoingIn);

            Assert.Equal(DateTimeKind.Utc, person.ConvertedComingOut.Kind);
            Assert.Equal(new DateTime(2015, 1, 10, 9, 9, 9, DateTimeKind.Utc), person.ConvertedComingOut);
        }
    }

    private class InMemoryConvertersContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(nameof(ValueComparerTest));

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Person>(
                b =>
                {
                    b.Property(o => o.ConvertedComingOut)
                        .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

                    b.Property(o => o.ConvertedGoingIn)
                        .HasConversion(v => DateTime.SpecifyKind(v, DateTimeKind.Utc), v => v);
                });
    }

    private class Person
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public DateTime ConvertedGoingIn { get; set; }
        public DateTime ConvertedComingOut { get; set; }
    }

    private static readonly ValueConverter<uint, int> _uIntToInt
        = new CastingConverter<uint, int>();

    [ConditionalFact]
    public void Can_access_raw_converters()
    {
        Assert.Same(_uIntToInt.ConvertFromProviderExpression, ((ValueConverter)_uIntToInt).ConvertFromProviderExpression);
        Assert.Same(_uIntToInt.ConvertToProviderExpression, ((ValueConverter)_uIntToInt).ConvertToProviderExpression);

        Assert.Equal(1, _uIntToInt.ConvertToProviderExpression.Compile()(1));
        Assert.Equal((uint)1, _uIntToInt.ConvertFromProviderExpression.Compile()(1));

        Assert.Equal(-1, _uIntToInt.ConvertToProviderExpression.Compile()(uint.MaxValue));
        Assert.Equal(uint.MaxValue, _uIntToInt.ConvertFromProviderExpression.Compile()(-1));
    }

    [ConditionalFact]
    public void Can_convert_exact_types_with_non_nullable_converter()
    {
        Assert.Equal(1, _uIntToInt.ConvertToProvider((uint)1));
        Assert.Equal((uint)1, _uIntToInt.ConvertFromProvider(1));

        Assert.Equal(-1, _uIntToInt.ConvertToProvider(uint.MaxValue));
        Assert.Equal(uint.MaxValue, _uIntToInt.ConvertFromProvider(-1));
    }

    [ConditionalFact]
    public void Can_convert_nullable_types_with_non_nullable_converter()
    {
        Assert.Equal(1, _uIntToInt.ConvertToProvider((uint?)1));
        Assert.Equal((uint)1, _uIntToInt.ConvertFromProvider((int?)1));

        Assert.Equal(-1, _uIntToInt.ConvertToProvider((uint?)uint.MaxValue));
        Assert.Equal(uint.MaxValue, _uIntToInt.ConvertFromProvider((int?)-1));
    }

    [ConditionalFact]
    public void Can_convert_non_exact_types_with_non_nullable_converter()
    {
        Assert.Equal(1, _uIntToInt.ConvertToProvider((ushort)1));
        Assert.Equal((uint)1, _uIntToInt.ConvertFromProvider((short)1));

        Assert.Equal(1, _uIntToInt.ConvertToProvider((ulong)1));
        Assert.Equal((uint)1, _uIntToInt.ConvertFromProvider((long)1));

        Assert.Equal(1, _uIntToInt.ConvertToProvider(1));
        Assert.Equal((uint)1, _uIntToInt.ConvertFromProvider(1));
    }

    [ConditionalFact]
    public void Can_convert_non_exact_nullable_types_with_non_nullable_converter()
    {
        Assert.Equal(1, _uIntToInt.ConvertToProvider((ushort?)1));
        Assert.Equal((uint)1, _uIntToInt.ConvertFromProvider((short?)1));

        Assert.Equal(1, _uIntToInt.ConvertToProvider((ulong?)1));
        Assert.Equal((uint)1, _uIntToInt.ConvertFromProvider((long?)1));

        Assert.Equal(1, _uIntToInt.ConvertToProvider((int?)1));
        Assert.Equal((uint)1, _uIntToInt.ConvertFromProvider((int?)1));
    }

    [ConditionalFact]
    public void Can_handle_nulls_with_non_nullable_converter()
    {
        Assert.Null(_uIntToInt.ConvertToProvider(null));
        Assert.Null(_uIntToInt.ConvertFromProvider(null));
    }

    private static readonly ValueConverter<uint?, int?> _nullableUIntToNullableInt
        = new CastingConverter<uint?, int?>();

    [ConditionalFact]
    public void Can_convert_exact_types_with_nullable_converter()
    {
        Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToProvider((uint?)1));
        Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromProvider((int?)1));

        Assert.Equal((int?)-1, _nullableUIntToNullableInt.ConvertToProvider((uint?)uint.MaxValue));
        Assert.Equal((uint?)uint.MaxValue, _nullableUIntToNullableInt.ConvertFromProvider((int?)-1));
    }

    [ConditionalFact]
    public void Can_convert_non_nullable_types_with_nullable_converter()
    {
        Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToProvider((uint?)1));
        Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromProvider((int?)1));

        Assert.Equal((int?)-1, _nullableUIntToNullableInt.ConvertToProvider((uint?)uint.MaxValue));
        Assert.Equal((uint?)uint.MaxValue, _nullableUIntToNullableInt.ConvertFromProvider((int?)-1));
    }

    [ConditionalFact]
    public void Can_convert_non_exact_types_with_nullable_converter()
    {
        Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToProvider((ushort?)1));
        Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromProvider((short?)1));

        Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToProvider((ulong?)1));
        Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromProvider((long?)1));

        Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToProvider((int?)1));
        Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromProvider((int?)1));
    }

    [ConditionalFact]
    public void Can_convert_non_exact_nullable_types_with_nullable_converter()
    {
        Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToProvider((ushort)1));
        Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromProvider((short)1));

        Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToProvider((ulong)1));
        Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromProvider((long)1));

        Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToProvider(1));
        Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromProvider(1));
    }

    [ConditionalFact]
    public void Can_handle_nulls_with_nullable_converter()
    {
        Assert.Null(_nullableUIntToNullableInt.ConvertToProvider(null));
        Assert.Null(_nullableUIntToNullableInt.ConvertFromProvider(null));
    }

    [ConditionalFact]
    public void Can_cast_between_numeric_types()
    {
        var types = new[]
        {
            typeof(sbyte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(byte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(char),
            typeof(double),
            typeof(float),
            typeof(decimal),
            typeof(sbyte?),
            typeof(short?),
            typeof(int?),
            typeof(long?),
            typeof(byte?),
            typeof(ushort?),
            typeof(uint?),
            typeof(ulong?),
            typeof(char?),
            typeof(double?),
            typeof(float?),
            typeof(decimal?)
        };

        foreach (var fromType in types)
        {
            foreach (var toType in types)
            {
                var converter = (ValueConverter)Activator.CreateInstance(
                    typeof(CastingConverter<,>).MakeGenericType(fromType, toType),
                    [null]);

                var resultToProvider = Expression.Lambda<Func<object>>(
                        Expression.Convert(
                            Expression.Invoke(
                                converter.ConvertToProviderExpression,
                                Expression.Convert(
                                    Expression.Constant(1), fromType)),
                            typeof(object)))
                    .Compile()();

                Assert.Same(toType.UnwrapNullableType(), resultToProvider.GetType());

                var resultFromProvider = Expression.Lambda<Func<object>>(
                        Expression.Convert(
                            Expression.Invoke(
                                converter.ConvertFromProviderExpression,
                                Expression.Convert(
                                    Expression.Constant(1), toType)),
                            typeof(object)))
                    .Compile()();

                Assert.Same(fromType.UnwrapNullableType(), resultFromProvider.GetType());
            }
        }
    }

    private static readonly ValueConverter<int, string> _intToString
        = new(v => v.ToString(), v => ConvertToInt(v));

    private static int ConvertToInt(string v)
        => int.TryParse(v, out var result) ? result : 0;

    private static readonly ValueConverter<Beatles, int> _enumToNumber
        = new EnumToNumberConverter<Beatles, int>();

    [ConditionalFact]
    public void Can_convert_compose_to_strings()
    {
        var converter
            = ((ValueConverter<Beatles, string>)_enumToNumber.ComposeWith(_intToString))
            .ConvertToProviderExpression.Compile();

        Assert.Equal("7", converter(Beatles.John));
        Assert.Equal("4", converter(Beatles.Paul));
        Assert.Equal("1", converter(Beatles.George));
        Assert.Equal("-1", converter(Beatles.Ringo));
        Assert.Equal("77", converter((Beatles)77));
        Assert.Equal("0", converter(default));
    }

    [ConditionalFact]
    public void Can_convert_compose_to_strings_object()
    {
        var converter = _enumToNumber.ComposeWith(_intToString).ConvertToProvider;

        Assert.Equal("7", converter(Beatles.John));
        Assert.Equal("4", converter(Beatles.Paul));
        Assert.Equal("1", converter(Beatles.George));
        Assert.Equal("-1", converter(Beatles.Ringo));
        Assert.Equal("77", converter((Beatles)77));
        Assert.Equal("0", converter(default(Beatles)));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_compose_to_enums()
    {
        var converter
            = ((ValueConverter<Beatles, string>)_enumToNumber.ComposeWith(_intToString))
            .ConvertFromProviderExpression.Compile();

        Assert.Equal(Beatles.John, converter("7"));
        Assert.Equal(Beatles.Paul, converter("4"));
        Assert.Equal(Beatles.George, converter("1"));
        Assert.Equal(Beatles.Ringo, converter("-1"));
        Assert.Equal((Beatles)77, converter("77"));
        Assert.Equal(default, converter("0"));
    }

    [ConditionalFact]
    public void Can_convert_compose_to_enums_object()
    {
        var converter = _enumToNumber.ComposeWith(_intToString).ConvertFromProvider;

        Assert.Equal(Beatles.John, converter("7"));
        Assert.Equal(Beatles.Paul, converter("4"));
        Assert.Equal(Beatles.George, converter("1"));
        Assert.Equal(Beatles.Ringo, converter("-1"));
        Assert.Equal((Beatles)77, converter("77"));
        Assert.Equal(default(Beatles), converter("0"));
        Assert.Null(converter(null));
    }

    private enum Beatles
    {
        John = 7,
        Paul = 4,
        George = 1,
        Ringo = -1
    }

    [ConditionalFact]
    public void Cannot_compose_converters_with_mismatched_types()
        => Assert.Equal(
            CoreStrings.ConvertersCannotBeComposed("Beatles", "int", "uint", "int"),
            Assert.Throws<ArgumentException>(
                () => _enumToNumber.ComposeWith(_uIntToInt)).Message);

#pragma warning disable xUnit1013 // Public method should be marked as test
    public static void OrderingTest<TModel, TProvider>(
#pragma warning restore xUnit1013 // Public method should be marked as test
        ValueConverter<TModel, TProvider> converter,
        params TModel[] values)
    {
        var convertToProvider = converter.ConvertToProviderExpression.Compile();
        var convertFromProvider = converter.ConvertFromProviderExpression.Compile();

        Assert.Equal(
            values,
            values.Select(v => convertToProvider(v))
                .OrderBy(v => v).ToList()
                .Select(v => convertFromProvider(v))
                .ToArray());
    }

#pragma warning disable xUnit1013 // Public method should be marked as test
    public static void OrderingTest<TModel>(
#pragma warning restore xUnit1013 // Public method should be marked as test
        ValueConverter<TModel, byte[]> converter,
        params TModel[] values)
    {
        var convertToProvider = converter.ConvertToProviderExpression.Compile();
        var convertFromProvider = converter.ConvertFromProviderExpression.Compile();

        Assert.Equal(
            values,
            values.Select(v => convertToProvider(v))
                .OrderBy(v => v, new BytesComparer()).ToList()
                .Select(v => convertFromProvider(v))
                .ToArray());
    }

    private class BytesComparer : IComparer<byte[]>
    {
        public int Compare(byte[] x, byte[] y)
            => StructuralComparisons.StructuralComparer.Compare(x, y);
    }
}
