// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a database sequence in the model.
    /// </summary>
    public interface IMutableSequence : IReadOnlySequence, IMutableAnnotatable
    {
        /// <summary>
        ///     Gets the <see cref="IMutableModel" /> in which this sequence is defined.
        /// </summary>
        new IMutableModel Model { get; }

        /// <summary>
        ///     Gets or sets the value at which the sequence will start.
        /// </summary>
        new long StartValue { get; set; }

        /// <summary>
        ///     Gets or sets the amount incremented to obtain each new value in the sequence.
        /// </summary>
        new int IncrementBy { get; set; }

        /// <summary>
        ///     Gets or sets the minimum value supported by the sequence, or <see langword="null" /> if none has been set.
        /// </summary>
        new long? MinValue { get; set; }

        /// <summary>
        ///     Gets or sets the maximum value supported by the sequence, or <see langword="null" /> if none has been set.
        /// </summary>
        new long? MaxValue { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="Type" /> of values returned by the sequence.
        /// </summary>
        new Type Type { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="Type" /> of values returned by the sequence.
        /// </summary>
        [Obsolete("Use Type")]
        new Type ClrType { get; set; }

        /// <summary>
        ///     Gets or sets the a value indicating whether the sequence will start again from the beginning when the max value
        ///     is reached.
        /// </summary>
        new bool IsCyclic { get; set; }
    }
}
