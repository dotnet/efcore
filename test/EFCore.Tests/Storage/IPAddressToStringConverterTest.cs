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

        [ConditionalFact]
        public void Can_convert_ipaddress_to_String()
        {
            var converter = _ipAddressToString.ConvertToProviderExpression.Compile();

            Assert.Equal(
                "255.255.255.0",
                converter(IPAddress.Parse("255.255.255.0")));

            Assert.Equal(
                "255.255.255.255",
                converter(IPAddress.None));
        }

        [ConditionalFact]
        public void Can_convert_String_to_ipaddress()
        {
            var converter = _ipAddressToString.ConvertFromProviderExpression.Compile();

            Assert.Equal(
                IPAddress.Parse("255.255.255.0"),
                converter("255.255.255.0"));

            Assert.Equal(
                IPAddress.None,
                converter("255.255.255.255"));

            Assert.Equal(IPAddress.None, converter(null));
        }
    }
}
