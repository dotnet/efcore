// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class PhysicalAddressToBytesConverterTest
    {
        private static readonly PhysicalAddressToBytesConverter _physicalAddressToBytes
            = new PhysicalAddressToBytesConverter();

        [ConditionalTheory]
        [MemberData(nameof(Data))]
        public void Can_convert_physical_address_to_bytes(string address)
        {
            var converter = _physicalAddressToBytes.ConvertToProviderExpression.Compile();
            var physicalAddress = PhysicalAddress.Parse(address);
            var bytes = physicalAddress.GetAddressBytes();

            Assert.Equal(bytes, converter(physicalAddress));
            Assert.Null(converter(null));
        }

        [ConditionalTheory]
        [MemberData(nameof(Data))]
        public void Can_convert_physical_address_to_bytes_object(string address)
        {
            var converter = _physicalAddressToBytes.ConvertToProvider;
            var physicalAddress = PhysicalAddress.Parse(address);
            var bytes = physicalAddress.GetAddressBytes();

            Assert.Equal(bytes, converter(physicalAddress));
            Assert.Null(converter(null));
        }

        [ConditionalTheory]
        [MemberData(nameof(Data))]
        public void Can_convert_bytes_to_physical_address_object(string address)
        {
            var converter = _physicalAddressToBytes.ConvertFromProvider;

            var physicalAddress = PhysicalAddress.Parse(address);
            var bytes = physicalAddress.GetAddressBytes();

            Assert.Equal(physicalAddress, converter(bytes));
            Assert.Null(converter(null));
        }

        public static IEnumerable<object[]> Data
            => new List<object[]>
            {
                new object[] { "1D-4E-55-D6-92-73-D6" },
                new object[] { "24-80-B7-38-4A-68-D6" },
                new object[] { "1B-AB-DF-C7-3D-6B" },
#if NET5_0
                new object[] { "1D:4E:55:D6:92:73" },
                new object[] { "24:80:B7:38:4A:68" },
                new object[] { "1B:AB:DF:C7:3D:6B" },
#endif
                new object[] { "1D4E55D69273" },
                new object[] { "1BABDFC73D6B" },
                new object[] { "0459D099E185" },
            };
    }
}
