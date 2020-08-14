// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a database sequence in the <see cref="IModel" />.
    /// </summary>
    public interface ISequence : IAnnotatable
    {
        /// <summary>
        ///     Gets the name of the sequence in the database.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the database schema that contains the sequence.
        /// </summary>
        string Schema { get; }

        /// <summary>
        ///     Gets the <see cref="IModel" /> in which this sequence is defined.
        /// </summary>
        IModel Model { get; }

        /// <summary>
        ///     Gets the value at which the sequence will start.
        /// </summary>
        long StartValue { get; }

        /// <summary>
        ///     Gets the amount incremented to obtain each new value in the sequence.
        /// </summary>
        int IncrementBy { get; }

        /// <summary>
        ///     Gets the minimum value supported by the sequence, or <see langword="null" /> if none has been set.
        /// </summary>
        long? MinValue { get; }

        /// <summary>
        ///     Gets the maximum value supported by the sequence, or <see langword="null" /> if none has been set.
        /// </summary>
        long? MaxValue { get; }

        /// <summary>
        ///     Gets the <see cref="Type" /> of values returned by the sequence.
        /// </summary>
        Type Type { get; }

        /// <summary>
        ///     Gets the <see cref="Type" /> of values returned by the sequence.
        /// </summary>
        [Obsolete("Use Type")]
        Type ClrType { get; }

        /// <summary>
        ///     Gets the value indicating whether the sequence will start again from the beginning when the max value
        ///     is reached.
        /// </summary>
        bool IsCyclic { get; }
    }
}
