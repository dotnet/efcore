// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class TimeSpanConvertersTest
    {
        private static readonly TimeSpanToStringConverter _timeSpanToString
            = new TimeSpanToStringConverter();

        [Fact]
        public void Can_convert_TimeSpan_to_string()
        {
            var converter = _timeSpanToString.ConvertToProviderExpression.Compile();

            Assert.Equal(
                "10.07:30:18.3330000",
                converter(new TimeSpan(10, 7, 30, 15, 3333)));

            Assert.Equal("00:00:00", converter(new TimeSpan()));
        }

        [Fact]
        public void Can_convert_string_to_TimeSpan()
        {
            var converter = _timeSpanToString.ConvertFromProviderExpression.Compile();

            Assert.Equal(
                new TimeSpan(10, 7, 30, 15, 3333),
                converter("10.07:30:18.3330000"));

            Assert.Equal(new TimeSpan(), converter("00:00:00"));
        }

        private static readonly TimeSpanToTicksConverter _timeSpanToTicks
            = new TimeSpanToTicksConverter();

        [Fact]
        public void Can_convert_TimeSpan_to_ticks()
        {
            var converter = _timeSpanToTicks.ConvertToProviderExpression.Compile();

            Assert.Equal(8910183330000, converter(new TimeSpan(10, 7, 30, 15, 3333)));
            Assert.Equal(0, converter(new TimeSpan()));
        }

        [Fact]
        public void Can_convert_ticks_to_TimeSpan()
        {
            var converter = _timeSpanToTicks.ConvertFromProviderExpression.Compile();

            Assert.Equal(new TimeSpan(10, 7, 30, 15, 3333), converter(8910183330000));
            Assert.Equal(new TimeSpan(), converter(0));
        }

        private static readonly CompositeValueConverter<TimeSpan, long, uint> _timeSpanToIntTicks
            = (CompositeValueConverter<TimeSpan, long, uint>)new TimeSpanToTicksConverter().ComposeWith(
                new CastingConverter<long, uint>());

        [Fact]
        public void Can_convert_TimeSpan_to_int_ticks()
        {
            var converter = _timeSpanToIntTicks.ConvertToProviderExpression.Compile();

            Assert.Equal((uint)183330000, converter(new TimeSpan(0, 0, 0, 15, 3333)));
            Assert.Equal((uint)0, converter(new TimeSpan()));
        }

        [Fact]
        public void Can_convert_int_ticks_to_TimeSpan()
        {
            var converter = _timeSpanToIntTicks.ConvertFromProviderExpression.Compile();

            Assert.Equal(new TimeSpan(0, 0, 0, 15, 3333), converter(183330000));
            Assert.Equal(new TimeSpan(), converter(0));
        }
    }
}
