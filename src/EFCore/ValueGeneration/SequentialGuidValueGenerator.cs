// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    /// <summary>
    ///     Generates sequential <see cref="Guid" /> values optimized for use in Microsoft SQL server clustered
    ///     keys or indexes, yielding better performance than random values. This is the default generator for
    ///     SQL Server <see cref="Guid" /> columns which are set to be generated on add.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See https://docs.microsoft.com/sql/t-sql/functions/newsequentialid-transact-sql.
    ///         Although this generator achieves the same goals as SQL Server's NEWSEQUENTIALID, the algorithm used
    ///         to generate the GUIDs is different.
    ///     </para>
    ///     <para>
    ///         The generated values are non-temporary, meaning they will be saved to the database.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information.
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
            var guidBytes = Guid.NewGuid().ToByteArray();
            var counterBytes = BitConverter.GetBytes(Interlocked.Increment(ref _counter));

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }

            guidBytes[08] = counterBytes[1];
            guidBytes[09] = counterBytes[0];
            guidBytes[10] = counterBytes[7];
            guidBytes[11] = counterBytes[6];
            guidBytes[12] = counterBytes[5];
            guidBytes[13] = counterBytes[4];
            guidBytes[14] = counterBytes[3];
            guidBytes[15] = counterBytes[2];

            return new Guid(guidBytes);
        }

        /// <summary>
        ///     Gets a value indicating whether the values generated are temporary or permanent. This implementation
        ///     always returns false, meaning the generated values will be saved to the database.
        /// </summary>
        public override bool GeneratesTemporaryValues
            => false;
    }
}
