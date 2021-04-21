// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class StringToDateTimeOffsetConverterTest
    {
        private static readonly StringToDateTimeOffsetConverter _stringToDateTimeOffset = new();

        [ConditionalFact]
        public void Can_convert_string_to_DateTimeOffset()
        {
            var converter = _stringToDateTimeOffset.ConvertToProviderExpression.Compile();

            Assert.Equal(
                new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0)),
                converter("1973-09-03 00:10:15+07:30"));

            Assert.Equal(
                new DateTimeOffset(), converter("0001-01-01 00:00:00+00:00"));

            Assert.Throws<FormatException>(() => converter("Not a DateTime"));
            Assert.Throws<ArgumentNullException>(() => converter(null));
        }

        [ConditionalFact]
        public void Can_convert_DateTimeOffset_to_string()
        {
            var converter = _stringToDateTimeOffset.ConvertFromProviderExpression.Compile();

            Assert.Equal(
                "1973-09-03 00:10:15+07:30",
                converter(new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0))));

            Assert.Equal(
                "0001-01-01 00:00:00+00:00",
                converter(new DateTimeOffset()));
        }

        [ConditionalFact]
        public void Can_convert_string_to_DateTimeOffset_object()
        {
            var converter = _stringToDateTimeOffset.ConvertToProvider;

            Assert.Equal(
                new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0)),
                converter("1973-09-03 00:10:15+07:30"));

            Assert.Equal(
                new DateTimeOffset(), converter("0001-01-01 00:00:00+00:00"));

            Assert.Throws<FormatException>(() => converter("Not a DateTime"));
            Assert.Null(converter(null));
        }

        [ConditionalFact]
        public void Can_convert_DateTimeOffset_to_string_object()
        {
            var converter = _stringToDateTimeOffset.ConvertFromProvider;

            Assert.Equal(
                "1973-09-03 00:10:15+07:30",
                converter(new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0))));

            Assert.Equal(
                "0001-01-01 00:00:00+00:00",
                converter(new DateTimeOffset()));
        }
    }
}
