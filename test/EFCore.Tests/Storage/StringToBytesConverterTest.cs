// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class StringToBytesConverterTest
    {
        private static readonly StringToBytesConverter _stringToUtf8Converter
            = new StringToBytesConverter(Encoding.UTF8);

        [ConditionalFact]
        public void Can_convert_strings_to_UTF8()
        {
            var converter = _stringToUtf8Converter.ConvertToProviderExpression.Compile();

            Assert.Equal(new byte[] { 83, 112, 196, 177, 110, 204, 136, 97, 108, 32, 84, 97, 112 }, converter("Spın̈al Tap"));
            Assert.Equal(Array.Empty<byte>(), converter(""));
            Assert.Null(converter(null));
        }

        [ConditionalFact]
        public void Can_convert_UTF8_to_strings()
        {
            var converter = _stringToUtf8Converter.ConvertFromProviderExpression.Compile();

            Assert.Equal("Spın̈al Tap", converter(new byte[] { 83, 112, 196, 177, 110, 204, 136, 97, 108, 32, 84, 97, 112 }));
            Assert.Equal("", converter(Array.Empty<byte>()));
            Assert.Null(converter(null));
        }
    }
}
