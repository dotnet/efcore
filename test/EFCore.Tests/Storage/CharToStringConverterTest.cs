// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class CharToStringConverterTest
    {
        private static readonly CharToStringConverter _charToString
            = new CharToStringConverter();

        [ConditionalFact]
        public void Can_convert_chars_to_strings()
        {
            var converter = _charToString.ConvertToProviderExpression.Compile();

            Assert.Equal("A", converter('A'));
            Assert.Equal("!", converter('!'));
        }

        [ConditionalFact]
        public void Can_convert_chars_to_strings_object()
        {
            var converter = _charToString.ConvertToProvider;

            Assert.Equal("A", converter('A'));
            Assert.Equal("!", converter('!'));
            Assert.Equal("A", converter((char?)'A'));
            Assert.Equal("!", converter((char?)'!'));
            Assert.Null(converter(null));
        }

        [ConditionalFact]
        public void Can_convert_strings_to_chars()
        {
            var converter = _charToString.ConvertFromProviderExpression.Compile();

            Assert.Equal('A', converter("A"));
            Assert.Equal('z', converter("z"));
            Assert.Equal('F', converter("Funkadelic"));
            Assert.Equal('\0', converter(null));
        }

        [ConditionalFact]
        public void Can_convert_strings_to_chars_object()
        {
            var converter = _charToString.ConvertFromProvider;

            Assert.Equal('A', converter("A"));
            Assert.Equal('z', converter("z"));
            Assert.Equal('F', converter("Funkadelic"));
            Assert.Null(converter(null));
        }
    }
}
