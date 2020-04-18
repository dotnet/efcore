// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class IPAddressToBytesConverterTest
    {
        private static readonly IPAddressToBytesConverter _ipAddressToBytes
            = new IPAddressToBytesConverter();

        [ConditionalFact]
        public void Can_convert_ipaddress_to_bytes()
        {
            var converter = _ipAddressToBytes.ConvertToProviderExpression.Compile();

            Assert.Equal(
                new byte[] { 255, 255, 255, 0 },
                converter(IPAddress.Parse("255.255.255.0")));

            Assert.Equal(
                new byte[] { 255, 255, 255, 255 },
                converter(IPAddress.None));
        }

        [ConditionalFact]
        public void Can_convert_ipaddress_to_bytes_object()
        {
            var converter = _ipAddressToBytes.ConvertToProvider;

            Assert.Equal(
                new byte[] { 255, 255, 255, 0 },
                converter(IPAddress.Parse("255.255.255.0")));

            Assert.Equal(
                new byte[] { 255, 255, 255, 255 },
                converter(IPAddress.None));

            Assert.Null(converter(null));
        }

        [ConditionalFact]
        public void Can_convert_bytes_to_ipaddress()
        {
            var converter = _ipAddressToBytes.ConvertFromProviderExpression.Compile();

            Assert.Equal(
                IPAddress.Parse("255.255.255.0"),
                converter(new byte[] { 255, 255, 255, 0 }));

            Assert.Equal(
                IPAddress.None,
                converter(new byte[] { 255, 255, 255, 255 }));

            Assert.Throws<ArgumentException>(
                () => converter(new byte[] { 255, 255, 255, 255, 255 }));

            Assert.Throws<ArgumentException>(
                () => converter(new byte[] { 255, 255, 255, 255, 0 }));

            Assert.Throws<ArgumentException>(
                () => converter(new byte[] { 1, 1, 1, 1, 1 }));

            Assert.Throws<ArgumentException>(
                () => converter(new byte[] { 0, 0, 0, 0, 0 }));

            Assert.Equal(IPAddress.None, converter(null));
        }

        [ConditionalFact]
        public void Can_convert_bytes_to_ipaddress_object()
        {
            var converter = _ipAddressToBytes.ConvertFromProvider;

            Assert.Equal(
                IPAddress.Parse("255.255.255.0"),
                converter(new byte[] { 255, 255, 255, 0 }));

            Assert.Equal(
                IPAddress.None,
                converter(new byte[] { 255, 255, 255, 255 }));

            Assert.Null(converter(null));
        }
    }
}
