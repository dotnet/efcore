// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class StringToDateTimeConverterTest
    {
        private static readonly StringToDateTimeConverter _stringToDateTime
            = new StringToDateTimeConverter();

        [Fact]
        public void Can_convert_string_to_DateTime()
        {
            var converter = _stringToDateTime.ConvertToProviderExpression.Compile();

            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15), converter("1973-09-03 00:10:15"));
            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15), converter("1973-09-03 00:10:15"));
            // Kind is not preserved
            Assert.NotEqual(DateTimeKind.Utc, converter("1973-09-03 00:10:15").Kind);
            Assert.Equal(new DateTime(), converter("0001-01-01 00:00:00"));
        }

        [Fact]
        public void Can_convert_DateTime_to_string()
        {
            var converter = _stringToDateTime.ConvertFromProviderExpression.Compile();

            Assert.Equal("1973-09-03 00:10:15", converter(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Utc)));
            Assert.Equal("1973-09-03 00:10:15", converter(new DateTime(1973, 9, 3, 0, 10, 15)));
            Assert.Equal("0001-01-01 00:00:00", converter(new DateTime()));
        }
    }
}
