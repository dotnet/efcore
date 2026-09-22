// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class IPAddressToBytesConverterTest
{
    private static readonly IPAddressToBytesConverter _ipAddressToBytes = new();

    [ConditionalTheory]
    [InlineData("255.255.255.255")]
    [InlineData("255.255.255.0")]
    [InlineData("255.255.0.0")]
    [InlineData("255.0.0.0")]
    [InlineData("0.0.0.0")]
    public void Can_convert_ipaddress_ipv4_to_bytes(string ipv4)
    {
        var converter = _ipAddressToBytes.ConvertToProviderExpression.Compile();
        var ip = IPAddress.Parse(ipv4);
        var bytes = ip.GetAddressBytes();

        Assert.Equal(bytes, converter(ip));
    }

    [ConditionalTheory]
    [InlineData("255.255.255.255")]
    [InlineData("255.255.255.0")]
    [InlineData("255.255.0.0")]
    [InlineData("255.0.0.0")]
    [InlineData("0.0.0.0")]
    public void Can_convert_ipaddress_ipv4_to_bytes_object(string ipv4)
    {
        var converter = _ipAddressToBytes.ConvertToProvider;
        var ip = IPAddress.Parse(ipv4);
        var bytes = ip.GetAddressBytes();

        Assert.Equal(bytes, converter(ip));

        Assert.Null(converter(null));
    }

    [ConditionalTheory]
    [InlineData(new byte[] { 255, 255, 255, 255, 255 })]
    [InlineData(new byte[] { 255, 255, 255, 0, 0 })]
    [InlineData(new byte[] { 192, 168, 2, 1, 0 })]
    [InlineData(new byte[] { 0, 0, 0, 0, 0 })]
    public void Can_convert_bytes_to_ipaddress_ipv4(byte[] bytesIPV4Invalid)
    {
        var converter = _ipAddressToBytes.ConvertFromProviderExpression.Compile();

        Assert.Throws<ArgumentException>(
            () => converter(bytesIPV4Invalid));
    }

    [ConditionalTheory]
    [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("27ff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("2001:db8::f:ffff")]
    [InlineData("2001:db8::1")]
    [InlineData("2001:db8::")]
    public void Can_convert_bytes_to_ipaddress_ipv6(string ipv6)
    {
        var converter = _ipAddressToBytes.ConvertFromProviderExpression.Compile();

        var ip = IPAddress.Parse(ipv6);
        var bytes = ip.GetAddressBytes();

        Assert.Equal(ip, converter(bytes));
    }

    [ConditionalTheory]
    [InlineData("255.255.255.255")]
    [InlineData("255.255.255.0")]
    [InlineData("255.255.0.0")]
    [InlineData("255.0.0.0")]
    [InlineData("0.0.0.0")]
    public void Can_convert_bytes_to_ipaddress_ipv4_object(string ipv4)
    {
        var converter = _ipAddressToBytes.ConvertFromProvider;

        var ip = IPAddress.Parse(ipv4);
        var bytes = ip.GetAddressBytes();

        Assert.Equal(ip, converter(bytes));

        Assert.Null(converter(null));
    }

    [ConditionalTheory]
    [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("27ff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("2001:db8::f:ffff")]
    [InlineData("2001:db8::1")]
    [InlineData("2001:db8::")]
    public void Can_convert_bytes_to_ipaddress_ipv6_object(string ipv6)
    {
        var converter = _ipAddressToBytes.ConvertFromProvider;

        var ip = IPAddress.Parse(ipv6);
        var bytes = ip.GetAddressBytes();

        Assert.Equal(ip, converter(bytes));

        Assert.Null(converter(null));
    }
}
