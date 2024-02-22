// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="IConventionSequence" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionSequenceBuilder : IConventionAnnotatableBuilder
{
    /// <summary>
    ///     The sequence being configured.
    /// </summary>
    new IConventionSequence Metadata { get; }

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionSequenceBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionSequenceBuilder? HasAnnotation(string name, object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    ///     Removes the annotation if <see langword="null" /> value is specified.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation. <see langword="null" /> to remove the annotations.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionSequenceBuilder" /> to continue configuration if the annotation was set or removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionSequenceBuilder? HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the annotation with the given name from this object.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionSequenceBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionSequenceBuilder? HasNoAnnotation(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the type of values returned by the sequence.
    /// </summary>
    /// <param name="type">The type of values returned by the sequence.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
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
    /// </returns>
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
    /// </returns>
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
    /// </returns>
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
    /// </returns>
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
    /// </returns>
    IConventionSequenceBuilder? IsCyclic(bool? cyclic, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given cyclicity can be set for the sequence.
    /// </summary>
    /// <param name="cyclic">If <see langword="true" />, then the sequence with restart when the maximum is reached.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given cyclicity can be set for the sequence.</returns>
    bool CanSetIsCyclic(bool? cyclic, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets that sequence does not use preallocated values.
    /// </summary>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionSequenceBuilder? UseNoCache(bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating nocache can be set for the sequence.
    /// </summary>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if nocache can be set for the sequence.</returns>
    bool CanSetNoCache(bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets amount of preallocated values.
    /// </summary>
    /// <param name="cacheSize">The amount of preallocated values.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionSequenceBuilder? UseCache(int? cacheSize, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given cache size can be set for the sequence.
    /// </summary>
    /// <param name="cacheSize">The cache size for the sequence.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given cache size can be set for the sequence.</returns>
    bool CanSetCache(int? cacheSize, bool fromDataAnnotation = false);
}
