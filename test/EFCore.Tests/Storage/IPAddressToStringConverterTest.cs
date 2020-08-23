// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class IPAddressToStringConverterTest
    {
        private static readonly IPAddressToStringConverter _ipAddressToString
            = new IPAddressToStringConverter();

        [ConditionalTheory]
        [InlineData("255.255.255.255")]
        [InlineData("255.255.255.0")]
        [InlineData("255.255.0.0")]
        [InlineData("255.0.0.0")]
        [InlineData("0.0.0.0")]
        public void Can_convert_ipaddress_ipv4_to_String(string ipv4)
        {
            var converter = _ipAddressToString.ConvertToProviderExpression.Compile();

            Assert.Equal(ipv4, converter(IPAddress.Parse(ipv4)));

            Assert.Null(converter(null));
        }

        [ConditionalTheory]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("27ff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("2001:db8::f:ffff")]
        [InlineData("2001:db8::1")]
        [InlineData("2001:db8::")]
        public void Can_convert_ipaddress_ipv6_to_String(string ipv6)
        {
            var converter = _ipAddressToString.ConvertToProviderExpression.Compile();

            Assert.Equal(ipv6, converter(IPAddress.Parse(ipv6)));

            Assert.Null(converter(null));
        }

        [ConditionalTheory]
        [InlineData("255.255.255.255")]
        [InlineData("255.255.255.0")]
        [InlineData("255.255.0.0")]
        [InlineData("255.0.0.0")]
        [InlineData("0.0.0.0")]
        public void Can_convert_String_to_ipaddress_ipv4(string ipv4)
        {
            var converter = _ipAddressToString.ConvertFromProviderExpression.Compile();

            Assert.Equal(
                IPAddress.Parse(ipv4),
                converter(ipv4));

            Assert.Null(converter(null));
        }

        [ConditionalTheory]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("27ff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("2001:db8::f:ffff")]
        [InlineData("2001:db8::1")]
        [InlineData("2001:db8::")]
        public void Can_convert_String_to_ipaddress_ipv6(string ipv6)
        {
            var converter = _ipAddressToString.ConvertFromProviderExpression.Compile();

            Assert.Equal(
                IPAddress.Parse(ipv6),
                converter(ipv6));

            Assert.Null(converter(null));
        }
    }
}
