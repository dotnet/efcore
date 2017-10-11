// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class TypeConverterTest
    {
        private static readonly ValueConverter<uint, int> _uIntToInt
            = new ValueConverter<uint, int>(v => (int)v, v => (uint)v);

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

        private static readonly ValueConverter<uint?, int?> _nullableUIntToInt
            = new ValueConverter<uint?, int?>(v => (int?)v, v => (uint?)v);

        [Fact]
        public void Can_convert_exact_types_with_nullable_converter()
        {
            Assert.Equal((int?)1, _nullableUIntToInt.ConvertToStore((uint?)1));
            Assert.Equal((uint?)1, _nullableUIntToInt.ConvertFromStore((int?)1));

            Assert.Equal((int?)-1, _nullableUIntToInt.ConvertToStore((uint?)uint.MaxValue));
            Assert.Equal((uint?)uint.MaxValue, _nullableUIntToInt.ConvertFromStore((int?)-1));
        }

        [Fact]
        public void Can_convert_non_nullable_types_with_nullable_converter()
        {
            Assert.Equal((int?)1, _nullableUIntToInt.ConvertToStore((uint?)1));
            Assert.Equal((uint?)1, _nullableUIntToInt.ConvertFromStore((int?)1));

            Assert.Equal((int?)-1, _nullableUIntToInt.ConvertToStore((uint?)uint.MaxValue));
            Assert.Equal((uint?)uint.MaxValue, _nullableUIntToInt.ConvertFromStore((int?)-1));
        }

        [Fact]
        public void Can_convert_non_exact_types_with_nullable_converter()
        {
            Assert.Equal((int?)1, _nullableUIntToInt.ConvertToStore((ushort?)1));
            Assert.Equal((uint?)1, _nullableUIntToInt.ConvertFromStore((short?)1));

            Assert.Equal((int?)1, _nullableUIntToInt.ConvertToStore((ulong?)1));
            Assert.Equal((uint?)1, _nullableUIntToInt.ConvertFromStore((long?)1));

            Assert.Equal((int?)1, _nullableUIntToInt.ConvertToStore((int?)1));
            Assert.Equal((uint?)1, _nullableUIntToInt.ConvertFromStore((int?)1));
        }

        [Fact]
        public void Can_convert_non_exact_nullable_types_with_nullable_converter()
        {
            Assert.Equal((int?)1, _nullableUIntToInt.ConvertToStore((ushort)1));
            Assert.Equal((uint?)1, _nullableUIntToInt.ConvertFromStore((short)1));

            Assert.Equal((int?)1, _nullableUIntToInt.ConvertToStore((ulong)1));
            Assert.Equal((uint?)1, _nullableUIntToInt.ConvertFromStore((long)1));

            Assert.Equal((int?)1, _nullableUIntToInt.ConvertToStore(1));
            Assert.Equal((uint?)1, _nullableUIntToInt.ConvertFromStore(1));
        }

        [Fact]
        public void Can_handle_nulls_with_nullable_converter()
        {
            Assert.Null(_nullableUIntToInt.ConvertToStore(null));
            Assert.Null(_nullableUIntToInt.ConvertFromStore(null));
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
    }
}
