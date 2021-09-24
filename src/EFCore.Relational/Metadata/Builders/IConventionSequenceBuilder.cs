// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API for configuring a <see cref="IConventionSequence" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information.
    /// </remarks>
    public interface IConventionSequenceBuilder : IConventionAnnotatableBuilder
    {
        /// <summary>
        ///     The sequence being configured.
        /// </summary>
        new IConventionSequence Metadata { get; }

        /// <summary>
        ///     Sets the type of values returned by the sequence.
        /// </summary>
        /// <param name="type">The type of values returned by the sequence.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        ///</returns>
        IConventionSequenceBuilder? HasType(Type? type, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given type can be set for the sequence.
        /// </summary>
        /// <param name="type">The type of values returned by the sequence.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><see langword="true" /> if the given type can be set for the sequence.</returns>
        bool CanSetType(Type? type, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the sequence to increment by the given amount when generating each next value.
        /// </summary>
        /// <param name="increment">The amount to increment between values.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        ///</returns>
        IConventionSequenceBuilder? IncrementsBy(int? increment, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given increment can be set for the sequence.
        /// </summary>
        /// <param name="increment">The amount to increment between values.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><see langword="true" /> if the given increment can be set for the sequence.</returns>
        bool CanSetIncrementsBy(int? increment, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the sequence to start at the given value.
        /// </summary>
        /// <param name="startValue">The starting value for the sequence.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        ///</returns>
        IConventionSequenceBuilder? StartsAt(long? startValue, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given starting value can be set for the sequence.
        /// </summary>
        /// <param name="startValue">The starting value for the sequence.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><see langword="true" /> if the given starting value can be set for the sequence.</returns>
        bool CanSetStartsAt(long? startValue, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the maximum value for the sequence.
        /// </summary>
        /// <param name="maximum">The maximum value for the sequence.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        ///</returns>
        IConventionSequenceBuilder? HasMax(long? maximum, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given maximum value can be set for the sequence.
        /// </summary>
        /// <param name="maximum">The maximum value for the sequence.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><see langword="true" /> if the given maximum value can be set for the sequence.</returns>
        bool CanSetMax(long? maximum, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the minimum value for the sequence.
        /// </summary>
        /// <param name="minimum">The minimum value for the sequence.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        ///</returns>
        IConventionSequenceBuilder? HasMin(long? minimum, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given minimum value can be set for the sequence.
        /// </summary>
        /// <param name="minimum">The minimum value for the sequence.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><see langword="true" /> if the given minimum value can be set for the sequence.</returns>
        bool CanSetMin(long? minimum, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets whether or not the sequence will start again from the beginning once
        ///     the maximum value is reached.
        /// </summary>
        /// <param name="cyclic">If <see langword="true" />, then the sequence with restart when the maximum is reached.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        ///</returns>
        IConventionSequenceBuilder? IsCyclic(bool? cyclic, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given cyclicity can be set for the sequence.
        /// </summary>
        /// <param name="cyclic">If <see langword="true" />, then the sequence with restart when the maximum is reached.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><see langword="true" /> if the given cyclicity can be set for the sequence.</returns>
        bool CanSetIsCyclic(bool? cyclic, bool fromDataAnnotation = false);
    }
}
