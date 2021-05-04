// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class StringToBoolConverterTest
    {
        private static readonly StringToBoolConverter _stringToBool = new();

        [ConditionalFact]
        public void Can_convert_strings_to_bools()
        {
            var converter = _stringToBool.ConvertToProviderExpression.Compile();

            Assert.False(converter("False"));
            Assert.True(converter("True"));
            Assert.False(converter("false"));
            Assert.True(converter("true"));
            Assert.False(converter(null));
        }

        [ConditionalFact]
        public void Can_convert_bools_to_strings()
        {
            var converter = _stringToBool.ConvertFromProviderExpression.Compile();

            Assert.Equal("True", converter(true));
            Assert.Equal("False", converter(false));
        }

        [ConditionalFact]
        public void Can_convert_strings_to_bools_object()
        {
            var converter = _stringToBool.ConvertToProvider;

            Assert.False((bool)converter("False"));
            Assert.True((bool)converter("True"));
            Assert.False((bool)converter("false"));
            Assert.True((bool)converter("true"));
            Assert.Null(converter(null));
        }

        [ConditionalFact]
        public void Can_convert_bools_to_strings_object()
        {
            var converter = _stringToBool.ConvertFromProvider;

            Assert.Equal("True", converter(true));
            Assert.Equal("False", converter(false));
            Assert.Null(converter(null));
        }
    }
}
