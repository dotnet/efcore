// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class IPAddressToStringConverterTest
{
    private static readonly IPAddressToStringConverter _ipAddressToString
        = new();

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
    }
}
