// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API for configuring a <see cref="ISequence" />.
    /// </summary>
    public class SequenceBuilder : IInfrastructure<IConventionSequenceBuilder>
    {
        /// <summary>
        ///     Creates a new builder for the given <see cref="ISequence" />.
        /// </summary>
        /// <param name="sequence"> The <see cref="IMutableSequence" /> to configure. </param>
        public SequenceBuilder([NotNull] IMutableSequence sequence)
        {
            Check.NotNull(sequence, nameof(sequence));

            Builder = ((Sequence)sequence).Builder;
        }

        private InternalSequenceBuilder Builder { [DebuggerStepThrough] get; }

        /// <inheritdoc />
        IConventionSequenceBuilder IInfrastructure<IConventionSequenceBuilder>.Instance
        {
            [DebuggerStepThrough]
            get => Builder;
        }

        /// <summary>
        ///     The sequence.
        /// </summary>
        public virtual IMutableSequence Metadata
            => Builder.Metadata;

        /// <summary>
        ///     Sets the <see cref="ISequence" /> to increment by the given amount when generating each next value.
        /// </summary>
        /// <param name="increment"> The amount to increment between values. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual SequenceBuilder IncrementsBy(int increment)
        {
            Builder.IncrementsBy(increment, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Sets the <see cref="ISequence" /> to start at the given value.
        /// </summary>
        /// <param name="startValue"> The starting value for the sequence. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual SequenceBuilder StartsAt(long startValue)
        {
            Builder.StartsAt(startValue, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Sets the maximum value for the <see cref="ISequence" />.
        /// </summary>
        /// <param name="maximum"> The maximum value for the sequence. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual SequenceBuilder HasMax(long maximum)
        {
            Builder.HasMax(maximum, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Sets the minimum value for the <see cref="ISequence" />.
        /// </summary>
        /// <param name="minimum"> The minimum value for the sequence. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual SequenceBuilder HasMin(long minimum)
        {
            Builder.HasMin(minimum, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Sets whether or not the sequence will start again from the beginning once
        ///     the maximum value is reached.
        /// </summary>
        /// <param name="cyclic"> If <see langword="true" />, then the sequence will restart when the maximum is reached. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual SequenceBuilder IsCyclic(bool cyclic = true)
        {
            Builder.IsCyclic(cyclic, ConfigurationSource.Explicit);

            return this;
        }

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
            => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        public override bool Equals(object obj)
            => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode()
            => base.GetHashCode();

        #endregion
    }
}
