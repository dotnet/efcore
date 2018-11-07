// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class DateTimeConvertersTest
    {
        private static readonly DateTimeToTicksConverter _dateTimeToTicks
            = new DateTimeToTicksConverter();

        [Fact]
        public void Can_convert_DateTime_to_ticks()
        {
            var converter = _dateTimeToTicks.ConvertToProviderExpression.Compile();

            Assert.Equal(622514598150000000, converter(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Utc)));
            Assert.Equal(622514598150000000, converter(new DateTime(1973, 9, 3, 0, 10, 15)));
            Assert.Equal(0, converter(new DateTime()));
        }

        [Fact]
        public void Can_convert_ticks_to_DateTime()
        {
            var converter = _dateTimeToTicks.ConvertFromProviderExpression.Compile();

            // Kind is not preserved, but value is ticks
            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Unspecified), converter(622514598150000000));
            Assert.Equal(DateTimeKind.Unspecified, converter(622514598150000000).Kind);
            Assert.Equal(new DateTime(), converter(0));
        }

        private static readonly DateTimeToBinaryConverter _dateTimeToBinary
            = new DateTimeToBinaryConverter();

        [Fact]
        public void Can_convert_DateTime_to_binary()
        {
            var converter = _dateTimeToBinary.ConvertToProviderExpression.Compile();

            Assert.Equal(5234200616577387904, converter(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Utc)));
            Assert.Equal(622514598150000000, converter(new DateTime(1973, 9, 3, 0, 10, 15)));
            Assert.Equal(0, converter(new DateTime()));
        }

        [Fact]
        public void Can_convert_binary_to_DateTime()
        {
            var converter = _dateTimeToBinary.ConvertFromProviderExpression.Compile();

            // Kind is preserved, but value is not ticks, however value is ticks if kind is unspecified
            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Utc), converter(5234200616577387904));
            Assert.Equal(DateTimeKind.Utc, converter(5234200616577387904).Kind);
            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Unspecified), converter(622514598150000000));
            Assert.Equal(DateTimeKind.Unspecified, converter(622514598150000000).Kind);
            Assert.Equal(new DateTime(), converter(0));
        }

        private static readonly CompositeValueConverter<DateTime, long, ulong> _dateTimeToUTicks
            = (CompositeValueConverter<DateTime, long, ulong>)new DateTimeToTicksConverter().ComposeWith(
                new CastingConverter<long, ulong>());

        [Fact]
        public void Can_convert_DateTime_to_unsigned_ticks()
        {
            var converter = _dateTimeToUTicks.ConvertToProviderExpression.Compile();

            Assert.Equal((ulong)622514598150000000, converter(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Utc)));
            Assert.Equal((ulong)622514598150000000, converter(new DateTime(1973, 9, 3, 0, 10, 15)));
            Assert.Equal((ulong)0, converter(new DateTime()));
        }

        [Fact]
        public void Can_convert_unsigned_ticks_to_DateTime()
        {
            var converter = _dateTimeToUTicks.ConvertFromProviderExpression.Compile();

            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Unspecified), converter(622514598150000000));
            Assert.Equal(new DateTime(), converter(0));
        }

        private static readonly CompositeValueConverter<DateTime, long, ulong> _dateTimeToUBinary
            = (CompositeValueConverter<DateTime, long, ulong>)new DateTimeToBinaryConverter().ComposeWith(
                new CastingConverter<long, ulong>());

        [Fact]
        public void Can_convert_DateTime_to_unsigned_binary()
        {
            var converter = _dateTimeToUBinary.ConvertToProviderExpression.Compile();

            Assert.Equal((ulong)5234200616577387904, converter(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Utc)));
            Assert.Equal((ulong)622514598150000000, converter(new DateTime(1973, 9, 3, 0, 10, 15)));
            Assert.Equal((ulong)0, converter(new DateTime()));
        }

        [Fact]
        public void Can_convert_unsigned_binary_to_DateTime()
        {
            var converter = _dateTimeToUBinary.ConvertFromProviderExpression.Compile();

            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Utc), converter(5234200616577387904));
            Assert.Equal(DateTimeKind.Utc, converter(5234200616577387904).Kind);
            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Unspecified), converter(622514598150000000));
            Assert.Equal(DateTimeKind.Unspecified, converter(622514598150000000).Kind);
            Assert.Equal(new DateTime(), converter(0));
        }

        private static readonly DateTimeToStringConverter _dateTimeToString
            = new DateTimeToStringConverter();

        [Fact]
        public void Can_convert_DateTime_to_string()
        {
            var converter = _dateTimeToString.ConvertToProviderExpression.Compile();

            Assert.Equal("1973-09-03 00:10:15", converter(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Utc)));
            Assert.Equal("1973-09-03 00:10:15", converter(new DateTime(1973, 9, 3, 0, 10, 15)));
            Assert.Equal("0001-01-01 00:00:00", converter(new DateTime()));
        }

        [Fact]
        public void Can_convert_string_to_DateTime()
        {
            var converter = _dateTimeToString.ConvertFromProviderExpression.Compile();

            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15), converter("1973-09-03 00:10:15"));
            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15), converter("1973-09-03 00:10:15"));
            // Kind is not preserved
            Assert.NotEqual(DateTimeKind.Utc, converter("1973-09-03 00:10:15").Kind);
            Assert.Equal(new DateTime(), converter("0001-01-01 00:00:00"));
        }

        private static readonly CompositeValueConverter<DateTime, long, byte[]> _dateTimeToBytes
            = (CompositeValueConverter<DateTime, long, byte[]>)new DateTimeToBinaryConverter().ComposeWith(
                new NumberToBytesConverter<long>());

        [Fact]
        public void Can_convert_DateTime_to_bytes()
        {
            var converter = _dateTimeToBytes.ConvertToProviderExpression.Compile();

            Assert.Equal(
                new byte[] { 72, 163, 157, 186, 146, 57, 205, 128 },
                converter(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Utc)));

            Assert.Equal(
                new byte[] { 8, 163, 157, 186, 146, 57, 205, 128 },
                converter(new DateTime(1973, 9, 3, 0, 10, 15)));

            Assert.Equal(
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
                converter(new DateTime()));
        }

        [Fact]
        public void Can_convert_bytes_to_DateTime()
        {
            var converter = _dateTimeToBytes.ConvertFromProviderExpression.Compile();

            var utcKind = converter(new byte[] { 72, 163, 157, 186, 146, 57, 205, 128 });
            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Utc), utcKind);
            Assert.Equal(DateTimeKind.Utc, utcKind.Kind);

            var unspecifiedKind = converter(new byte[] { 8, 163, 157, 186, 146, 57, 205, 128 });
            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Unspecified), unspecifiedKind);
            Assert.Equal(DateTimeKind.Unspecified, unspecifiedKind.Kind);

            Assert.Equal(new DateTime(), converter(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }));
        }
    }
}
