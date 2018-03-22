// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class BytesToStringConverterTest
    {
        private static readonly BytesToStringConverter _bytesToStringConverter
            = new BytesToStringConverter();

        [Fact]
        public void Can_convert_strings_to_bytes()
        {
            var converter = _bytesToStringConverter.ConvertToProviderExpression.Compile();

            Assert.Equal("U3DEsW7MiGFsIFRhcA==", converter(new byte[] { 83, 112, 196, 177, 110, 204, 136, 97, 108, 32, 84, 97, 112 }));
            Assert.Equal("", converter(Array.Empty<byte>()));
            Assert.Null(converter(null));
        }

        [Fact]
        public void Can_convert_bytes_to_strings()
        {
            var converter = _bytesToStringConverter.ConvertFromProviderExpression.Compile();

            Assert.Equal(new byte[] { 83, 112, 196, 177, 110, 204, 136, 97, 108, 32, 84, 97, 112 }, converter("U3DEsW7MiGFsIFRhcA=="));
            Assert.Equal(Array.Empty<byte>(), converter(""));
            Assert.Null(converter(null));
        }

        [Fact]
        public void Can_convert_strings_to_long_non_char_bytes()
        {
            var converter = _bytesToStringConverter.ConvertToProviderExpression.Compile();

            Assert.Equal(CreateLongBytesString(), converter(CreateLongBytes()));
        }

        [Fact]
        public void Can_convert_long_non_char_bytes_to_strings()
        {
            var converter = _bytesToStringConverter.ConvertFromProviderExpression.Compile();

            Assert.Equal(CreateLongBytes(), converter(CreateLongBytesString()));
        }

        private static byte[] CreateLongBytes()
        {
            var longBinary = new byte[1000];
            for (var i = 0; i < longBinary.Length; i++)
            {
                longBinary[i] = (byte)i;
            }

            return longBinary;
        }

        private static string CreateLongBytesString()
        {
            var longBinary = new byte[1000];
            for (var i = 0; i < longBinary.Length; i++)
            {
                longBinary[i] = (byte)i;
            }

            return Convert.ToBase64String(longBinary);
        }
    }
}
