// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class ValueConverterTest
    {
        private static readonly ValueConverter<uint, int> _uIntToInt
            = new CastingConverter<uint, int>();

        [Fact]
        public void Can_access_raw_converters()
        {
            Assert.Same(_uIntToInt.ConvertFromStoreExpression, ((ValueConverter)_uIntToInt).ConvertFromStoreExpression);
            Assert.Same(_uIntToInt.ConvertToStoreExpression, ((ValueConverter)_uIntToInt).ConvertToStoreExpression);

            Assert.Equal(1, _uIntToInt.ConvertToStoreExpression.Compile()(1));
            Assert.Equal((uint)1, _uIntToInt.ConvertFromStoreExpression.Compile()(1));

            Assert.Equal(-1, _uIntToInt.ConvertToStoreExpression.Compile()(uint.MaxValue));
            Assert.Equal(uint.MaxValue, _uIntToInt.ConvertFromStoreExpression.Compile()(-1));
        }

        [Fact]
        public void Can_convert_exact_types_with_non_nullable_converter()
        {
            Assert.Equal(1, _uIntToInt.ConvertToStore((uint)1));
            Assert.Equal((uint)1, _uIntToInt.ConvertFromStore(1));

            Assert.Equal(-1, _uIntToInt.ConvertToStore(uint.MaxValue));
            Assert.Equal(uint.MaxValue, _uIntToInt.ConvertFromStore(-1));
        }

        [Fact]
        public void Can_convert_nullable_types_with_non_nullable_converter()
        {
            Assert.Equal(1, _uIntToInt.ConvertToStore((uint?)1));
            Assert.Equal((uint)1, _uIntToInt.ConvertFromStore((int?)1));

            Assert.Equal(-1, _uIntToInt.ConvertToStore((uint?)uint.MaxValue));
            Assert.Equal(uint.MaxValue, _uIntToInt.ConvertFromStore((int?)-1));
        }

        [Fact]
        public void Can_convert_non_exact_types_with_non_nullable_converter()
        {
            Assert.Equal(1, _uIntToInt.ConvertToStore((ushort)1));
            Assert.Equal((uint)1, _uIntToInt.ConvertFromStore((short)1));

            Assert.Equal(1, _uIntToInt.ConvertToStore((ulong)1));
            Assert.Equal((uint)1, _uIntToInt.ConvertFromStore((long)1));

            Assert.Equal(1, _uIntToInt.ConvertToStore(1));
            Assert.Equal((uint)1, _uIntToInt.ConvertFromStore(1));
        }

        [Fact]
        public void Can_convert_non_exact_nullable_types_with_non_nullable_converter()
        {
            Assert.Equal(1, _uIntToInt.ConvertToStore((ushort?)1));
            Assert.Equal((uint)1, _uIntToInt.ConvertFromStore((short?)1));

            Assert.Equal(1, _uIntToInt.ConvertToStore((ulong?)1));
            Assert.Equal((uint)1, _uIntToInt.ConvertFromStore((long?)1));

            Assert.Equal(1, _uIntToInt.ConvertToStore((int?)1));
            Assert.Equal((uint)1, _uIntToInt.ConvertFromStore((int?)1));
        }

        [Fact]
        public void Can_handle_nulls_with_non_nullable_converter()
        {
            Assert.Null(_uIntToInt.ConvertToStore(null));
            Assert.Null(_uIntToInt.ConvertFromStore(null));
        }

        private static readonly ValueConverter<uint?, int?> _nullableUIntToNullableInt
            = new CastingConverter<uint?, int?>();

        [Fact]
        public void Can_convert_exact_types_with_nullable_converter()
        {
            Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToStore((uint?)1));
            Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromStore((int?)1));

            Assert.Equal((int?)-1, _nullableUIntToNullableInt.ConvertToStore((uint?)uint.MaxValue));
            Assert.Equal((uint?)uint.MaxValue, _nullableUIntToNullableInt.ConvertFromStore((int?)-1));
        }

        [Fact]
        public void Can_convert_non_nullable_types_with_nullable_converter()
        {
            Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToStore((uint?)1));
            Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromStore((int?)1));

            Assert.Equal((int?)-1, _nullableUIntToNullableInt.ConvertToStore((uint?)uint.MaxValue));
            Assert.Equal((uint?)uint.MaxValue, _nullableUIntToNullableInt.ConvertFromStore((int?)-1));
        }

        [Fact]
        public void Can_convert_non_exact_types_with_nullable_converter()
        {
            Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToStore((ushort?)1));
            Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromStore((short?)1));

            Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToStore((ulong?)1));
            Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromStore((long?)1));

            Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToStore((int?)1));
            Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromStore((int?)1));
        }

        [Fact]
        public void Can_convert_non_exact_nullable_types_with_nullable_converter()
        {
            Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToStore((ushort)1));
            Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromStore((short)1));

            Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToStore((ulong)1));
            Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromStore((long)1));

            Assert.Equal((int?)1, _nullableUIntToNullableInt.ConvertToStore(1));
            Assert.Equal((uint?)1, _nullableUIntToNullableInt.ConvertFromStore(1));
        }

        [Fact]
        public void Can_handle_nulls_with_nullable_converter()
        {
            Assert.Null(_nullableUIntToNullableInt.ConvertToStore(null));
            Assert.Null(_nullableUIntToNullableInt.ConvertFromStore(null));
        }

        [Fact]
        public void Can_cast_between_numeric_types()
        {
            var types = new[]
            {
                typeof(sbyte), typeof(short), typeof(int), typeof(long),
                typeof(byte), typeof(ushort), typeof(uint), typeof(ulong),
                typeof(char), typeof(double), typeof(float), typeof(decimal),
                typeof(sbyte?), typeof(short?), typeof(int?), typeof(long?),
                typeof(byte?), typeof(ushort?), typeof(uint?), typeof(ulong?),
                typeof(char?), typeof(double?), typeof(float?), typeof(decimal?)
            };

            foreach (var fromType in types)
            {
                foreach (var toType in types)
                {
                    var converter = (ValueConverter)Activator.CreateInstance(
                        typeof(CastingConverter<,>).MakeGenericType(fromType, toType),
                        new object[] { null });

                    var resultToStore = Expression.Lambda<Func<object>>(
                            Expression.Convert(
                                Expression.Invoke(
                                    converter.ConvertToStoreExpression,
                                    Expression.Convert(
                                        Expression.Constant(1), fromType)),
                                typeof(object)))
                        .Compile()();

                    Assert.Same(toType.UnwrapNullableType(), resultToStore.GetType());

                    var resultFromStore = Expression.Lambda<Func<object>>(
                            Expression.Convert(
                                Expression.Invoke(
                                    converter.ConvertFromStoreExpression,
                                    Expression.Convert(
                                        Expression.Constant(1), toType)),
                                typeof(object)))
                        .Compile()();

                    Assert.Same(fromType.UnwrapNullableType(), resultFromStore.GetType());
                }
            }
        }

        private static readonly ValueConverter<string, int?> _stringConverter
            = new ValueConverter<string, int?>(
                v => v.Equals("<null>", StringComparison.OrdinalIgnoreCase)
                    ? null
                    : (int?)int.Parse(v, CultureInfo.InvariantCulture),
                v => v != null
                    ? v.Value.ToString(CultureInfo.InvariantCulture)
                    : "<null>");

        [Fact]
        public void Can_convert_nulls_to_non_nulls()
        {
            Assert.Equal(1234, _stringConverter.ConvertToStore("1234"));
            Assert.Equal("1234", _stringConverter.ConvertFromStore(1234));

            Assert.Null(_stringConverter.ConvertToStore("<null>"));
            Assert.Equal("<null>", _stringConverter.ConvertFromStore(null));
        }

        private static readonly ValueConverter<int, string> _intToString
            = new ValueConverter<int, string>(
                v => v.ToString(),
                v => ConvertToInt(v));

        private static int ConvertToInt(string v)
            => int.TryParse(v, out var result) ? result : 0;

        private static readonly ValueConverter<Beatles, int> _enumToNumber
            = new EnumToNumberConverter<Beatles, int>();

        [Fact]
        public void Can_convert_compose_to_strings()
        {
            var converter
                = ((ValueConverter<Beatles, string>)ValueConverter.Compose(_enumToNumber, _intToString))
                .ConvertToStoreExpression.Compile();

            Assert.Equal("7", converter(Beatles.John));
            Assert.Equal("4", converter(Beatles.Paul));
            Assert.Equal("1", converter(Beatles.George));
            Assert.Equal("-1", converter(Beatles.Ringo));
            Assert.Equal("77", converter((Beatles)77));
            Assert.Equal("0", converter(default));
        }

        [Fact]
        public void Can_convert_compose_to_strings_object()
        {
            var converter = ValueConverter.Compose(_enumToNumber, _intToString).ConvertToStore;

            Assert.Equal("7", converter(Beatles.John));
            Assert.Equal("4", converter(Beatles.Paul));
            Assert.Equal("1", converter(Beatles.George));
            Assert.Equal("-1", converter(Beatles.Ringo));
            Assert.Equal("77", converter((Beatles)77));
            Assert.Equal("0", converter(default(Beatles)));
            Assert.Null(converter(null));
        }

        [Fact]
        public void Can_convert_compose_to_enums()
        {
            var converter
                = ((ValueConverter<Beatles, string>)ValueConverter.Compose(_enumToNumber, _intToString))
                .ConvertFromStoreExpression.Compile();

            Assert.Equal(Beatles.John, converter("7"));
            Assert.Equal(Beatles.Paul, converter("4"));
            Assert.Equal(Beatles.George, converter("1"));
            Assert.Equal(Beatles.Ringo, converter("-1"));
            Assert.Equal((Beatles)77, converter("77"));
            Assert.Equal(default, converter("0"));
        }

        [Fact]
        public void Can_convert_compose_to_enums_object()
        {
            var converter = ValueConverter.Compose(_enumToNumber, _intToString).ConvertFromStore;

            Assert.Equal(Beatles.John, converter("7"));
            Assert.Equal(Beatles.Paul, converter("4"));
            Assert.Equal(Beatles.George, converter("1"));
            Assert.Equal(Beatles.Ringo, converter("-1"));
            Assert.Equal((Beatles)77, converter("77"));
            Assert.Equal(default(Beatles), converter("0"));
            Assert.Equal(default(Beatles), converter(null));
        }

        private enum Beatles
        {
            John = 7,
            Paul = 4,
            George = 1,
            Ringo = -1
        }

        [Fact]
        public void Cannot_compose_converters_with_mismatched_types()
        {
            Assert.Equal(
                CoreStrings.ConvertersCannotBeComposed("Beatles", "int", "uint", "int"),
                Assert.Throws<ArgumentException>(
                    () => ValueConverter.Compose(_enumToNumber, _uIntToInt)).Message);
        }

        public static void OrderingTest<TModel, TStore>(
            ValueConverter<TModel, TStore> converter,
            params TModel[] values)
        {
            var convertToStore = converter.ConvertToStoreExpression.Compile();
            var convertFromStore = converter.ConvertFromStoreExpression.Compile();

            Assert.Equal(
                values,
                values.Select(v => convertToStore(v))
                    .OrderBy(v => v).ToList()
                    .Select(v => convertFromStore(v))
                    .ToArray());
        }

        public static void OrderingTest<TModel>(
            ValueConverter<TModel, byte[]> converter,
            params TModel[] values)
        {
            var convertToStore = converter.ConvertToStoreExpression.Compile();
            var convertFromStore = converter.ConvertFromStoreExpression.Compile();

            Assert.Equal(
                values,
                values.Select(v => convertToStore(v))
                    .OrderBy(v => v, new BytesComparer()).ToList()
                    .Select(v => convertFromStore(v))
                    .ToArray());
        }

        private class BytesComparer : IComparer<byte[]>
        {
            public int Compare(byte[] x, byte[] y)
                => StructuralComparisons.StructuralComparer.Compare(x, y);
        }
    }
}
