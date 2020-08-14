// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a database sequence in the <see cref="IConventionModel" /> in a form that
    ///     can be mutated while building the model.
    /// </summary>
    public interface IConventionSequence : ISequence, IConventionAnnotatable
    {
        /// <summary>
        ///     Gets the <see cref="IConventionModel" /> in which this sequence is defined.
        /// </summary>
        new IConventionModel Model { get; }

        /// <summary>
        ///     Gets the builder that can be used to configure this sequence.
        /// </summary>
        new IConventionSequenceBuilder Builder { get; }

        /// <summary>
        ///     Gets the configuration source for this <see cref="IConventionSequence" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IConventionSequence" />. </returns>
        ConfigurationSource GetConfigurationSource();

        /// <summary>
        ///     Sets the value at which the sequence will start.
        /// </summary>
        /// <param name="startValue"> The value at which the sequence will start. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        long? SetStartValue(long? startValue, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="ISequence.StartValue" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="ISequence.StartValue" />. </returns>
        ConfigurationSource? GetStartValueConfigurationSource();

        /// <summary>
        ///     Sets the amount incremented to obtain each new value in the sequence.
        /// </summary>
        /// <param name="incrementBy"> The amount incremented to obtain each new value in the sequence. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        int? SetIncrementBy(int? incrementBy, bool fromDataAnnotation = false);

        /// <summary>
        ///     Gets the configuration source for <see cref="ISequence.IncrementBy" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="ISequence.IncrementBy" />. </returns>
        ConfigurationSource? GetIncrementByConfigurationSource();

        /// <summary>
        ///     Sets the minimum value supported by the sequence.
        /// </summary>
        /// <param name="minValue"> The minimum value supported by the sequence. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        long? SetMinValue(long? minValue, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="ISequence.MinValue" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="ISequence.MinValue" />. </returns>
        ConfigurationSource? GetMinValueConfigurationSource();

        /// <summary>
        ///     Sets the maximum value supported by the sequence.
        /// </summary>
        /// <param name="maxValue"> The maximum value supported by the sequence. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        long? SetMaxValue(long? maxValue, bool fromDataAnnotation = false);

        /// <summary>
        ///     Gets the configuration source for <see cref="ISequence.MaxValue" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="ISequence.MaxValue" />. </returns>
        ConfigurationSource? GetMaxValueConfigurationSource();

        /// <summary>
        ///     Sets the <see cref="Type" /> of values returned by the sequence.
        /// </summary>
        /// <param name="type"> The <see cref="Type" /> of values returned by the sequence. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        Type SetType([CanBeNull] Type type, bool fromDataAnnotation = false);

        /// <summary>
        ///     Gets the configuration source for <see cref="ISequence.ClrType" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="ISequence.ClrType" />. </returns>
        ConfigurationSource? GetTypeConfigurationSource();

        /// <summary>
        ///     Sets the <see cref="Type" /> of values returned by the sequence.
        /// </summary>
        /// <param name="type"> The <see cref="Type" /> of values returned by the sequence. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        [Obsolete("Use SetType")]
        Type SetClrType([CanBeNull] Type type, bool fromDataAnnotation = false);

        /// <summary>
        ///     Gets the configuration source for <see cref="ISequence.ClrType" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="ISequence.ClrType" />. </returns>
        [Obsolete("Use GetTypeConfigurationSource")]
        ConfigurationSource? GetClrTypeConfigurationSource();

        /// <summary>
        ///     Sets whether the sequence will start again from the beginning when the max value is reached.
        /// </summary>
        /// <param name="cyclic">
        ///     If <see langword="true" />, then the sequence will start again from the beginning when the max value
        ///     is reached.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        bool? SetIsCyclic(bool? cyclic, bool fromDataAnnotation = false);

        /// <summary>
        ///     Gets the configuration source for <see cref="ISequence.IsCyclic" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="ISequence.IsCyclic" />. </returns>
        ConfigurationSource? GetIsCyclicConfigurationSource();
    }
}
