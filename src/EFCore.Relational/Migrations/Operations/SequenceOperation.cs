// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A <see cref="MigrationOperation" /> for operations on sequences.
///     See also <see cref="CreateSequenceOperation" /> and <see cref="AlterSequenceOperation" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
public abstract class SequenceOperation : MigrationOperation
{
    /// <summary>
    ///     The amount to increment by when generating the next value in the sequence, defaulting to 1.
    /// </summary>
    public virtual int IncrementBy { get; set; } = 1;

    /// <summary>
    ///     The maximum value of the sequence, or <see langword="null" /> if not specified.
    /// </summary>
    public virtual long? MaxValue { get; set; }

    /// <summary>
    ///     The minimum value of the sequence, or <see langword="null" /> if not specified.
    /// </summary>
    public virtual long? MinValue { get; set; }

    /// <summary>
    ///     Indicates whether or not the sequence will re-start when the maximum value is reached.
    /// </summary>
    public virtual bool IsCyclic { get; set; }

    /// <summary>
    ///     Indicates whether the sequence use preallocated values.
    /// </summary>
    public virtual bool IsCached { get; set; } = true;

    /// <summary>
    ///     The amount of preallocated values of the sequence, or <see langword="null" /> if not specified.
    /// </summary>
    public virtual int? CacheSize { get; set; }
}
