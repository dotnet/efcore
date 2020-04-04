// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    /// <summary>
    ///     Generates sequential <see cref="Guid" /> values using the same algorithm as NEWSEQUENTIALID()
    ///     in Microsoft SQL Server. This is useful when entities are being saved to a database where sequential
    ///     GUIDs will provide a performance benefit. The generated values are non-temporary, meaning they will
    ///     be saved to the database.
    /// </summary>
    public class SequentialGuidValueGenerator : ValueGenerator<Guid>
    {
        private long _counterA = DateTime.UtcNow.Ticks;
        private long _counterB = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 8);
        /// <summary>
        ///     Gets a value to be assigned to a property.
        /// </summary>
        /// <param name="entry"> The change tracking entry of the entity for which the value is being generated. </param>
        /// <returns> The value to be assigned to a property. </returns>
        public override Guid Next(EntityEntry entry)
        {
            Span<long> guidValue = stackalloc long[] { Interlocked.Increment(ref _counterA), _counterA switch { 0 => Interlocked.Increment(ref _counterB), _ => _counterB } };
            if (BitConverter.IsLittleEndian)
            {
                Span<byte> bytes = MemoryMarshal.Cast<long, byte>(guidValue);
                // Change the first byte containing the first int and the two consecutive shorts to litle endian byte order.
                bytes.Slice(0, 4).Reverse();
                bytes.Slice(4, 2).Reverse();
                bytes.Slice(6, 2).Reverse();
            }
            return MemoryMarshal.Cast<long, Guid>(guidValue)[0];
            //return new Guid(guidBytes);
        }

        /// <summary>
        ///     Gets a value indicating whether the values generated are temporary or permanent. This implementation
        ///     always returns false, meaning the generated values will be saved to the database.
        /// </summary>
        public override bool GeneratesTemporaryValues => false;
    }
}
