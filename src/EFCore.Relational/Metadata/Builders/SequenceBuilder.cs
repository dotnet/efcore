// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API for configuring a <see cref="ISequence" />.
    /// </summary>
    public class SequenceBuilder : IConventionSequenceBuilder
    {
        private readonly Sequence _sequence;

        /// <summary>
        ///     Creates a new builder for the given <see cref="ISequence" />.
        /// </summary>
        /// <param name="sequence"> The <see cref="IMutableSequence" /> to configure. </param>
        public SequenceBuilder([NotNull] IMutableSequence sequence)
        {
            Check.NotNull(sequence, nameof(sequence));

            _sequence = (Sequence)sequence;
        }

        /// <summary>
        ///     The sequence.
        /// </summary>
        public virtual IMutableSequence Metadata => _sequence;

        /// <inheritdoc />
        IConventionSequenceBuilder IConventionSequenceBuilder.HasType(Type type, bool fromDataAnnotation)
        {
            if (Overrides(fromDataAnnotation, _sequence.GetClrTypeConfigurationSource())
                || _sequence.ClrType == type)
            {
                ((IConventionSequence)_sequence).SetClrType(type, fromDataAnnotation);
                return this;
            }

            return null;
        }

        /// <inheritdoc />
        bool IConventionSequenceBuilder.CanSetType(Type type, bool fromDataAnnotation)
            => (type == null || Sequence.SupportedTypes.Contains(type))
                && (Overrides(fromDataAnnotation, _sequence.GetClrTypeConfigurationSource())
                    || _sequence.ClrType == type);

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

        /// <inheritdoc />
        IConventionSequenceBuilder IConventionSequenceBuilder.IncrementsBy(int? increment, bool fromDataAnnotation)
        {
            if (((IConventionSequenceBuilder)this).CanSetIncrementsBy(increment, fromDataAnnotation))
            {
                ((IConventionSequence)_sequence).SetIncrementBy(increment, fromDataAnnotation);
                return this;
            }

            return null;
        }

        /// <inheritdoc />
        bool IConventionSequenceBuilder.CanSetIncrementsBy(int? increment, bool fromDataAnnotation)
            => Overrides(fromDataAnnotation, _sequence.GetIncrementByConfigurationSource())
                || _sequence.IncrementBy == increment;

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

        /// <inheritdoc />
        IConventionSequenceBuilder IConventionSequenceBuilder.StartsAt(long? startValue, bool fromDataAnnotation)
        {
            if (((IConventionSequenceBuilder)this).CanSetStartsAt(startValue, fromDataAnnotation))
            {
                ((IConventionSequence)_sequence).SetStartValue(startValue, fromDataAnnotation);
                return this;
            }

            return null;
        }

        /// <inheritdoc />
        bool IConventionSequenceBuilder.CanSetStartsAt(long? startValue, bool fromDataAnnotation)
            => Overrides(fromDataAnnotation, _sequence.GetStartValueConfigurationSource())
                || _sequence.StartValue == startValue;

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

        /// <inheritdoc />
        IConventionSequenceBuilder IConventionSequenceBuilder.HasMax(long? maximum, bool fromDataAnnotation)
        {
            if (((IConventionSequenceBuilder)this).CanSetMax(maximum, fromDataAnnotation))
            {
                ((IConventionSequence)_sequence).SetMaxValue(maximum, fromDataAnnotation);
                return this;
            }

            return null;
        }

        /// <inheritdoc />
        bool IConventionSequenceBuilder.CanSetMax(long? maximum, bool fromDataAnnotation)
            => Overrides(fromDataAnnotation, _sequence.GetMaxValueConfigurationSource())
                || _sequence.MaxValue == maximum;

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

        /// <inheritdoc />
        IConventionSequenceBuilder IConventionSequenceBuilder.HasMin(long? minimum, bool fromDataAnnotation)
        {
            if (((IConventionSequenceBuilder)this).CanSetMin(minimum, fromDataAnnotation))
            {
                ((IConventionSequence)_sequence).SetMinValue(minimum, fromDataAnnotation);
                return this;
            }

            return null;
        }

        /// <inheritdoc />
        bool IConventionSequenceBuilder.CanSetMin(long? minimum, bool fromDataAnnotation)
            => Overrides(fromDataAnnotation, _sequence.GetMinValueConfigurationSource())
                || _sequence.MinValue == minimum;

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

        /// <inheritdoc />
        IConventionSequenceBuilder IConventionSequenceBuilder.IsCyclic(bool? cyclic, bool fromDataAnnotation)
        {
            if (((IConventionSequenceBuilder)this).CanSetCyclic(cyclic, fromDataAnnotation))
            {
                ((IConventionSequence)_sequence).SetIsCyclic(cyclic, fromDataAnnotation);
                return this;
            }

            return null;
        }

        /// <inheritdoc />
        bool IConventionSequenceBuilder.CanSetCyclic(bool? cyclic, bool fromDataAnnotation)
            => Overrides(fromDataAnnotation, _sequence.GetIsCyclicConfigurationSource())
                || _sequence.IsCyclic == cyclic;

        private bool Overrides(bool fromDataAnnotation, ConfigurationSource? configurationSource)
            => (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                .Overrides(configurationSource);

        IConventionSequence IConventionSequenceBuilder.Metadata => (IConventionSequence)Metadata;

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
        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
