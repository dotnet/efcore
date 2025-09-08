// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public class BytesToDateTimeConverter : ValueConverter<byte[], DateTime>
    {
        private static readonly NumberToBytesConverter<long> _longToBytes
            = new NumberToBytesConverter<long>();

        public BytesToDateTimeConverter()
            : base(
                v => FromBytes(v),
                v => ToBytes(v))
        {
        }

        public static byte[] ToBytes(DateTime v)
            => (byte[])_longToBytes.ConvertToProvider(v.ToBinary());

        public static DateTime FromBytes(byte[] v)
            => DateTime.FromBinary((long)_longToBytes.ConvertFromProvider(v));
    }
}
