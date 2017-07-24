// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a database sequence in the <see cref="IMutableModel" /> in a form that
    ///     can be mutated while building the model.
    /// </summary>
    public interface IMutableSequence : ISequence
    {
        /// <summary>
        ///     The value at which the sequence will start.
        /// </summary>
        new long StartValue { get; set; }

        /// <summary>
        ///     The amount incremented to obtain each new value in the sequence.
        /// </summary>
        new int IncrementBy { get; set; }

        /// <summary>
        ///     The minimum value supported by the sequence, or <c>null</c> if none has been set.
        /// </summary>
        new long? MinValue { get; set; }

        /// <summary>
        ///     The maximum value supported by the sequence, or <c>null</c> if none has been set.
        /// </summary>
        new long? MaxValue { get; set; }

        /// <summary>
        ///     The <see cref="Type" /> of values returned by the sequence.
        /// </summary>
        new Type ClrType { get; [param: NotNull] set; }

        /// <summary>
        ///     If <c>true</c>, then the sequence will start again from the beginning when the max value
        ///     is reached.
        /// </summary>
        new bool IsCyclic { get; set; }
    }
}
