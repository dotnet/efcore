// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class DateTimeOffsetConvertersTest
    {
        private static readonly DateTimeOffsetToStringConverter _dateTimeOffsetToString
            = new DateTimeOffsetToStringConverter();

        [ConditionalFact]
        public void Can_convert_DateTimeOffset_to_string()
        {
            var converter = _dateTimeOffsetToString.ConvertToProviderExpression.Compile();

            Assert.Equal(
                "1973-09-03 00:10:15+07:30",
                converter(new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0))));

            Assert.Equal(
                "0001-01-01 00:00:00+00:00",
                converter(new DateTimeOffset()));
        }

        [ConditionalFact]
        public void Can_convert_string_to_DateTimeOffset()
        {
            var converter = _dateTimeOffsetToString.ConvertFromProviderExpression.Compile();

            Assert.Equal(
                new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0)),
                converter("1973-09-03 00:10:15+07:30"));

            Assert.Equal(
                new DateTimeOffset(), converter("0001-01-01 00:00:00+00:00"));
        }

        private static readonly DateTimeOffsetToBytesConverter _dateTimeOffsetToBytes
            = new DateTimeOffsetToBytesConverter();

        [ConditionalFact]
        public void Can_convert_DateTimeOffset_to_bytes()
        {
            var converter = _dateTimeOffsetToBytes.ConvertToProviderExpression.Compile();

            Assert.Equal(
                new byte[] { 8, 163, 157, 186, 146, 57, 205, 128, 1, 194 },
                converter(new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0))));

            Assert.Equal(
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                converter(new DateTimeOffset()));
        }

        [ConditionalFact]
        public void Can_convert_bytes_to_DateTimeOffset()
        {
            var converter = _dateTimeOffsetToBytes.ConvertFromProviderExpression.Compile();

            Assert.Equal(
                new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0)),
                converter(new byte[] { 8, 163, 157, 186, 146, 57, 205, 128, 1, 194 }));

            Assert.Equal(
                new DateTimeOffset(),
                converter(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
        }

        private static readonly DateTimeOffsetToBinaryConverter _dateTimeOffsetToBinary
            = new DateTimeOffsetToBinaryConverter();

        [ConditionalFact]
        public void Can_convert_DateTimeOffset_to_binary()
        {
            var converter = _dateTimeOffsetToBinary.ConvertToProviderExpression.Compile();

            Assert.Equal(
                1274909897011200450,
                converter(new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0))));

            Assert.Equal(
                1274909897018021048,
                converter(new DateTimeOffset(new DateTime(1973, 9, 3, 0, 10, 15, 333), new TimeSpan(-14, 0, 0))));

            Assert.Equal(
                1274909897018020680,
                converter(new DateTimeOffset(new DateTime(1973, 9, 3, 0, 10, 15, 333), new TimeSpan(14, 0, 0))));

            Assert.Equal(0, converter(new DateTimeOffset()));
        }

        [ConditionalFact]
        public void Can_convert_binary_to_DateTimeOffset()
        {
            var converter = _dateTimeOffsetToBinary.ConvertFromProviderExpression.Compile();

            Assert.Equal(
                new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0)),
                converter(1274909897011200450));

            Assert.Equal(
                new DateTimeOffset(new DateTime(1973, 9, 3, 0, 10, 15, 333), new TimeSpan(-14, 0, 0)),
                converter(1274909897018021048));

            Assert.Equal(
                new DateTimeOffset(new DateTime(1973, 9, 3, 0, 10, 15, 333), new TimeSpan(14, 0, 0)),
                converter(1274909897018020680));

            Assert.Equal(new DateTimeOffset(), converter(0));
        }
    }
}
