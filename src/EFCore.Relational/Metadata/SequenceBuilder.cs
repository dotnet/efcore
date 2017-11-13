// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     A fluent API builder for <see cref="ISequence" /> objects.
    /// </summary>
    public class SequenceBuilder
    {
        private readonly IMutableSequence _sequence;

        /// <summary>
        ///     Creates a new builder for the given <see cref="ISequence" />.
        /// </summary>
        /// <param name="sequence"> The <see cref="IMutableSequence" /> to configure. </param>
        public SequenceBuilder([NotNull] IMutableSequence sequence)
        {
            Check.NotNull(sequence, nameof(sequence));

            _sequence = sequence;
        }

        /// <summary>
        ///     The <see cref="ISequence" />.
        /// </summary>
        public virtual IMutableSequence Metadata => _sequence;

        /// <summary>
        ///     Sets the <see cref="ISequence" /> to increment by the given amount when generating each next value.
        /// </summary>
        /// <param name="increment"> The amount to increment between values. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual SequenceBuilder IncrementsBy(int increment)
        {
            _sequence.IncrementBy = increment;

            return this;
        }

        /// <summary>
        ///     Sets the <see cref="ISequence" /> to start at the given value.
        /// </summary>
        /// <param name="startValue"> The starting value for the sequence. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual SequenceBuilder StartsAt(long startValue)
        {
            _sequence.StartValue = startValue;

            return this;
        }

        /// <summary>
        ///     Sets the maximum value for the <see cref="ISequence" />.
        /// </summary>
        /// <param name="maximum"> The maximum value for the sequence. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual SequenceBuilder HasMax(long maximum)
        {
            _sequence.MaxValue = maximum;

            return this;
        }

        /// <summary>
        ///     Sets the minimum value for the <see cref="ISequence" />.
        /// </summary>
        /// <param name="minimum"> The minimum value for the sequence. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual SequenceBuilder HasMin(long minimum)
        {
            _sequence.MinValue = minimum;

            return this;
        }

        /// <summary>
        ///     Sets whether or not the sequence will start again from the beginning once
        ///     the maximum value is reached.
        /// </summary>
        /// <param name="cyclic"> If <c>true</c>, then the sequence with restart when the maximum is reached. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual SequenceBuilder IsCyclic(bool cyclic = true)
        {
            _sequence.IsCyclic = cyclic;

            return this;
        }

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
