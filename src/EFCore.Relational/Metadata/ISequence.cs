// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a database sequence in the <see cref="IModel" />.
    /// </summary>
    public interface ISequence
    {
        /// <summary>
        ///     The name of the sequence in the database.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The database schema that contains the sequence.
        /// </summary>
        string Schema { get; }

        /// <summary>
        ///     The <see cref="IModel" /> in which this sequence is defined.
        /// </summary>
        IModel Model { get; }

        /// <summary>
        ///     The value at which the sequence will start.
        /// </summary>
        long StartValue { get; }

        /// <summary>
        ///     The amount incremented to obtain each new value in the sequence.
        /// </summary>
        int IncrementBy { get; }

        /// <summary>
        ///     The minimum value supported by the sequence, or <c>null</c> if none has been set.
        /// </summary>
        long? MinValue { get; }

        /// <summary>
        ///     The maximum value supported by the sequence, or <c>null</c> if none has been set.
        /// </summary>
        long? MaxValue { get; }

        /// <summary>
        ///     The <see cref="Type" /> of values returned by the sequence.
        /// </summary>
        Type ClrType { get; }

        /// <summary>
        ///     If <c>true</c>, then the sequence will start again from the beginning when the max value
        ///     is reached.
        /// </summary>
        bool IsCyclic { get; }
    }
}
