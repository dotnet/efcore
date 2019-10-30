// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     <para>
    ///         A unique identifier for the context instance and pool lease, if any.
    ///     </para>
    ///     <para>
    ///         This identifier is primarily intended as a correlation ID for logging and debugging such
    ///         that it is easy to identify that multiple events are using the same or different context instances.
    ///     </para>
    /// </summary>
    public readonly struct DbContextId
    {
        /// <summary>
        ///     Compares this ID to another ID to see if they represent the same leased context.
        /// </summary>
        /// <param name="other"> The other ID. </param>
        /// <returns> True if they represent the same leased context; false otherwise. </returns>
        public bool Equals(DbContextId other)
            => InstanceId == other.InstanceId
                && Lease == other.Lease;

        /// <summary>
        ///     Compares this ID to another ID to see if they represent the same leased context.
        /// </summary>
        /// <param name="obj"> The other ID. </param>
        /// <returns> True if they represent the same leased context; false otherwise. </returns>
        public override bool Equals(object obj)
            => obj is DbContextId other && Equals(other);

        /// <summary>
        ///     A hash code for this ID.
        /// </summary>
        /// <returns> The hash code. </returns>
        public override int GetHashCode()
            => HashCode.Combine(InstanceId, Lease);

        /// <summary>
        ///     Compares one ID to another ID to see if they represent the same leased context.
        /// </summary>
        /// <param name="left"> The first ID. </param>
        /// <param name="right"> The second ID. </param>
        /// <returns> True if they represent the same leased context; false otherwise. </returns>
        public static bool operator ==(DbContextId left, DbContextId right) => left.Equals(right);

        /// <summary>
        ///     Compares one ID to another ID to see if they represent different leased contexts.
        /// </summary>
        /// <param name="left"> The first ID. </param>
        /// <param name="right"> The second ID. </param>
        /// <returns> True if they represent different leased contexts; false otherwise. </returns>
        public static bool operator !=(DbContextId left, DbContextId right) => !left.Equals(right);

        /// <summary>
        ///     Creates a new <see cref="DbContextId" /> with the given <see cref="InstanceId" /> and lease number.
        /// </summary>
        /// <param name="id"> A unique identifier for the <see cref="DbContext" /> being used. </param>
        /// <param name="lease"> A number indicating whether this is the first, second, third, etc. lease of this instance. </param>
        public DbContextId(Guid id, int lease)
        {
            InstanceId = id;
            Lease = lease;
        }

        /// <summary>
        ///     <para>
        ///         A unique identifier for the <see cref="DbContext" /> being used.
        ///     </para>
        ///     <para>
        ///         When context pooling is being used, then this ID must be combined with
        ///         the <see cref="Lease" /> in order to get a unique ID for the effective instance being used.
        ///     </para>
        /// </summary>
        public Guid InstanceId { get; }

        /// <summary>
        ///     <para>
        ///         A number that is incremented each time this particular <see cref="DbContext" /> instance is leased
        ///         from the context pool.
        ///     </para>
        ///     <para>
        ///         Will be zero if context pooling is not being used.
        ///     </para>
        /// </summary>
        public int Lease { get; }

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>The fully qualified type name.</returns>
        public override string ToString()
        {
            return InstanceId + ":" + Lease.ToString(CultureInfo.InvariantCulture);
        }
    }
}
