// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class GuidToBytesConverterTest
    {
        private static readonly GuidToBytesConverter _guidToBytes
            = new GuidToBytesConverter();

        [Fact]
        public void Can_convert_GUIDs_to_bytes()
        {
            var converter = _guidToBytes.ConvertToProviderExpression.Compile();

            Assert.Equal(
                new byte[] { 180, 39, 238, 150, 139, 134, 73, 64, 186, 103, 203, 184, 60, 229, 180, 98 },
                converter(new Guid("96EE27B4-868B-4049-BA67-CBB83CE5B462")));

            Assert.Equal(
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                converter(Guid.Empty));
        }

        [Fact]
        public void Can_convert_GUIDs_to_bytes_object()
        {
            var converter = _guidToBytes.ConvertToProvider;

            Assert.Equal(
                new byte[] { 180, 39, 238, 150, 139, 134, 73, 64, 186, 103, 203, 184, 60, 229, 180, 98 },
                converter(new Guid("96EE27B4-868B-4049-BA67-CBB83CE5B462")));

            Assert.Equal(
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                converter(Guid.Empty));

            Assert.Equal(
                new byte[] { 180, 39, 238, 150, 139, 134, 73, 64, 186, 103, 203, 184, 60, 229, 180, 98 },
                converter((Guid?)new Guid("96EE27B4-868B-4049-BA67-CBB83CE5B462")));

            Assert.Equal(
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                converter((Guid?)Guid.Empty));

            Assert.Null(converter(null));
        }

        [Fact]
        public void Can_convert_bytes_to_GUIDs()
        {
            var converter = _guidToBytes.ConvertFromProviderExpression.Compile();

            Assert.Equal(
                new Guid("96EE27B4-868B-4049-BA67-CBB83CE5B462"),
                converter(new byte[] { 180, 39, 238, 150, 139, 134, 73, 64, 186, 103, 203, 184, 60, 229, 180, 98 }));

            Assert.Equal(
                Guid.Empty,
                converter(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));

            Assert.Throws<ArgumentException>(
                () => converter(new byte[] { 180, 39, 238, 150, 139, 134, 73, 64, 186, 103, 203, 184, 60, 229, 180, 98, 0 }));

            Assert.Throws<ArgumentException>(
                () => converter(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));

            Assert.Throws<ArgumentException>(
                () => converter(new byte[] { 180, 39, 238, 150, 139, 134, 73, 64, 186, 103, 203, 184, 60, 229, 180 }));

            Assert.Throws<ArgumentException>(
                () => converter(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));

            Assert.Equal(Guid.Empty, converter(null));
        }

        [Fact]
        public void Can_convert_bytes_to_GUIDs_object()
        {
            var converter = _guidToBytes.ConvertFromProvider;

            Assert.Equal(
                new Guid("96EE27B4-868B-4049-BA67-CBB83CE5B462"),
                converter(new byte[] { 180, 39, 238, 150, 139, 134, 73, 64, 186, 103, 203, 184, 60, 229, 180, 98 }));

            Assert.Equal(
                Guid.Empty,
                converter(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));

            Assert.Null(converter(null));
        }
    }
}
