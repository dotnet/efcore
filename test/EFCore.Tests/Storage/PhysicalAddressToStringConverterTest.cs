// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class PhysicalAddressToStringConverterTest
    {
        private static readonly PhysicalAddressToStringConverter _physicalAddressToString
            = new PhysicalAddressToStringConverter();

        [ConditionalTheory]
        [MemberData(nameof(Data))]
        public void Can_convert_physical_address_to_String(string physicalAddress)
        {
            var converter = _physicalAddressToString.ConvertToProviderExpression.Compile();

            var alphaNumerics = new Regex("[^a-zA-Z0-9]");

            Assert.Equal(
                alphaNumerics.Replace(physicalAddress, ""),
                converter(PhysicalAddress.Parse(physicalAddress)));

            Assert.Null(converter(null));
        }

        [ConditionalTheory]
        [MemberData(nameof(Data))]
        public void Can_convert_String_to_physical_address(string physicalAddress)
        {
            var converter = _physicalAddressToString.ConvertFromProviderExpression.Compile();

            Assert.Equal(
                PhysicalAddress.Parse(physicalAddress),
                converter(physicalAddress));

            Assert.Null(converter(null));
        }

        [ConditionalTheory]
        [InlineData(new byte[] { 74, 74, 45, 72, 72, 45, 89, 89, 45, 68, 54, 45, 57, 50, 45, 55, 51 })]
        [InlineData(new byte[] { 72, 72, 45, 77, 77, 45, 53, 53, 45, 68, 54, 45, 57, 50, 45, 55, 51 })]
        [InlineData(new byte[] { 86, 86, 45, 56, 48, 45, 66, 55, 45, 51, 56, 45, 52, 65, 45, 54, 56 })]
        [InlineData(new byte[] { 75, 75, 75, 45, 53, 57, 45, 68, 48, 45, 57, 57, 45, 69, 49, 45, 56, 53 })]
        public void Can_convert_bytes_to_physical_address(byte[] bytesPhysicalAddressInvalid)
        {
            var converter = new PhysicalAddressToStringConverter().ConvertFromProviderExpression.Compile();
            var physicalAddress = Encoding.UTF8.GetString(bytesPhysicalAddressInvalid);

            var exception = Assert.Throws<FormatException>(
                () =>
                {
                    converter(physicalAddress);
                });

            Assert.Null(converter(null));
            Assert.Equal($"An invalid physical address was specified: '{physicalAddress}'.", exception.Message);
        }

        public static IEnumerable<object[]> Data
            => new List<object[]>
            {
                new object[] { "1D-4E-55-D6-92-73-D6" },
                new object[] { "24-80-B7-38-4A-68-D6" },
                new object[] { "04-59-D0-99-E1-85" },
#if NET5_0
                new object[] { "1D:4E:55:D6:92:73" },
                new object[] { "24:80:B7:38:4A:68" },
                new object[] { "04:59:D0:99:E1:85" },
#endif
                new object[] { "1D4E55D69273" },
                new object[] { "2480B7384A68" },
                new object[] { "0459D099E185" },
            };
    }
}
