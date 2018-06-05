// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class StringToBoolConverterTest
    {
        private static readonly StringToBoolConverter _stringToBool
            = new StringToBoolConverter();

        [Fact]
        public void Can_convert_strings_to_bools()
        {
            var converter = _stringToBool.ConvertToProviderExpression.Compile();

            Assert.False(converter("False"));
            Assert.True(converter("True"));
            Assert.False(converter("false"));
            Assert.True(converter("true"));
            Assert.False(converter(null));
        }

        [Fact]
        public void Can_convert_bools_to_strings()
        {
            var converter = _stringToBool.ConvertFromProviderExpression.Compile();

            Assert.Equal("True", converter(true));
            Assert.Equal("False", converter(false));
        }
    }
}
