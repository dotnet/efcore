// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

/// <summary>
///     Generates sequential <see cref="Guid" /> values optimized for use in Microsoft SQL server clustered
///     keys or indexes, yielding better performance than random values. This is the default generator for
///     SQL Server <see cref="Guid" /> columns which are set to be generated on add.
/// </summary>
/// <remarks>
///     <para>
///         Although this generator achieves the same goals as SQL Server's NEWSEQUENTIALID, the algorithm used
///         to generate the GUIDs is different. See
///         <see href="https://docs.microsoft.com/sql/t-sql/functions/newsequentialid-transact-sql">NEWSEQUENTIALID</see>
///         for more information on the advantages of sequential GUIDs.
///     </para>
///     <para>
///         The generated values are non-temporary, meaning they will be saved to the database.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information and examples.
///     </para>
/// </remarks>
public class SequentialGuidValueGenerator : ValueGenerator<Guid>
{
    private long _counter = DateTime.UtcNow.Ticks;

    /// <summary>
    ///     Gets a value to be assigned to a property.
    /// </summary>
    /// <param name="entry">The change tracking entry of the entity for which the value is being generated.</param>
    /// <returns>The value to be assigned to a property.</returns>
    public override Guid Next(EntityEntry entry)
    {
        var guid = Guid.NewGuid();

        var counter = BitConverter.IsLittleEndian
            ? Interlocked.Increment(ref _counter)
            : BinaryPrimitives.ReverseEndianness(Interlocked.Increment(ref _counter));

        var counterBytes = MemoryMarshal.AsBytes(
            new ReadOnlySpan<long>(in counter));

        // Guid uses a sequential layout where the first 8 bytes (_a, _b, _c)
        // are subject to byte-swapping on big-endian systems when reading from
        // or writing to a byte array (e.g., via MemoryMarshal or Guid constructors).
        // The remaining 8 bytes (_d through _k) are interpreted as-is,
        // regardless of endianness.
        //
        // Since we only modify the last 8 bytes of the Guid (bytes 8–15),
        // byte order does not affect the result.
        //
        // This allows us to safely use MemoryMarshal.AsBytes to directly access
        // and modify the Guid's underlying bytes without any extra conversions,
        // which also slightly improves performance on big-endian architectures.
        var guidBytes = MemoryMarshal.AsBytes(
            new Span<Guid>(ref guid));

        guidBytes[08] = counterBytes[1];
        guidBytes[09] = counterBytes[0];
        guidBytes[10] = counterBytes[7];
        guidBytes[11] = counterBytes[6];
        guidBytes[12] = counterBytes[5];
        guidBytes[13] = counterBytes[4];
        guidBytes[14] = counterBytes[3];
        guidBytes[15] = counterBytes[2];

        return guid;
    }

    /// <summary>
    ///     Gets a value indicating whether the values generated are temporary or permanent. This implementation
    ///     always returns false, meaning the generated values will be saved to the database.
    /// </summary>
    public override bool GeneratesTemporaryValues
        => false;
}
