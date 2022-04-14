// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     A unique identifier for the context instance and pool lease, if any.
/// </summary>
/// <remarks>
///     <para>
///         This identifier is primarily intended as a correlation ID for logging and debugging such
///         that it is easy to identify that multiple events are using the same or different context instances.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-dbcontext">DbContext lifetime, configuration, and initialization</see>
///         for more information and examples.
///     </para>
/// </remarks>
public readonly struct DbContextId
{
    /// <summary>
    ///     Compares this ID to another ID to see if they represent the same leased context.
    /// </summary>
    /// <param name="other">The other ID.</param>
    /// <returns><see langword="true" /> if they represent the same leased context; <see langword="false" /> otherwise.</returns>
    public bool Equals(DbContextId other)
        => InstanceId == other.InstanceId
            && Lease == other.Lease;

    /// <summary>
    ///     Compares this ID to another ID to see if they represent the same leased context.
    /// </summary>
    /// <param name="obj">The other ID.</param>
    /// <returns><see langword="true" /> if they represent the same leased context; <see langword="false" /> otherwise.</returns>
    public override bool Equals(object? obj)
        => obj is DbContextId other && Equals(other);

    /// <summary>
    ///     A hash code for this ID.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
        => HashCode.Combine(InstanceId, Lease);

    /// <summary>
    ///     Compares one ID to another ID to see if they represent the same leased context.
    /// </summary>
    /// <param name="left">The first ID.</param>
    /// <param name="right">The second ID.</param>
    /// <returns><see langword="true" /> if they represent the same leased context; <see langword="false" /> otherwise.</returns>
    public static bool operator ==(DbContextId left, DbContextId right)
        => left.Equals(right);

    /// <summary>
    ///     Compares one ID to another ID to see if they represent different leased contexts.
    /// </summary>
    /// <param name="left">The first ID.</param>
    /// <param name="right">The second ID.</param>
    /// <returns><see langword="true" /> if they represent different leased contexts; <see langword="false" /> otherwise.</returns>
    public static bool operator !=(DbContextId left, DbContextId right)
        => !left.Equals(right);

    /// <summary>
    ///     Creates a new <see cref="DbContextId" /> with the given <see cref="InstanceId" /> and lease number.
    /// </summary>
    /// <param name="id">A unique identifier for the <see cref="DbContext" /> being used.</param>
    /// <param name="lease">A number indicating whether this is the first, second, third, etc. lease of this instance.</param>
    public DbContextId(Guid id, int lease)
    {
        InstanceId = id;
        Lease = lease;
    }

    /// <summary>
    ///     A unique identifier for the <see cref="DbContext" /> being used.
    /// </summary>
    /// <remarks>
    ///     When context pooling is being used, then this ID must be combined with
    ///     the <see cref="Lease" /> in order to get a unique ID for the effective instance being used.
    /// </remarks>
    public Guid InstanceId { get; }

    /// <summary>
    ///     A number that is incremented each time this particular <see cref="DbContext" /> instance is leased
    ///     from the context pool.
    /// </summary>
    /// <remarks>
    ///     Will be zero if context pooling is not being used.
    /// </remarks>
    public int Lease { get; }

    /// <summary>
    ///     Returns the instance ID and lease number.
    /// </summary>
    /// <returns>The instance ID and lease number.</returns>
    public override string ToString()
        => InstanceId + ":" + Lease.ToString(CultureInfo.InvariantCulture);
}
