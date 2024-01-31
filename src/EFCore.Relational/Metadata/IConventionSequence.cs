// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a database sequence in the model in a form that
///     can be mutated while building the model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
/// </remarks>
public interface IConventionSequence : IReadOnlySequence, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the <see cref="IConventionModel" /> in which this sequence is defined.
    /// </summary>
    new IConventionModel Model { get; }

    /// <summary>
    ///     Gets the builder that can be used to configure this sequence.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the sequence has been removed from the model.</exception>
    new IConventionSequenceBuilder Builder { get; }

    /// <summary>
    ///     Gets the configuration source for this <see cref="IConventionSequence" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IConventionSequence" />.</returns>
    ConfigurationSource GetConfigurationSource();

    /// <summary>
    ///     Sets the value at which the sequence will start.
    /// </summary>
    /// <param name="startValue">The value at which the sequence will start.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    long? SetStartValue(long? startValue, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlySequence.StartValue" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlySequence.StartValue" />.</returns>
    ConfigurationSource? GetStartValueConfigurationSource();

    /// <summary>
    ///     Sets the amount incremented to obtain each new value in the sequence.
    /// </summary>
    /// <param name="incrementBy">The amount incremented to obtain each new value in the sequence.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    int? SetIncrementBy(int? incrementBy, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for <see cref="IReadOnlySequence.IncrementBy" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlySequence.IncrementBy" />.</returns>
    ConfigurationSource? GetIncrementByConfigurationSource();

    /// <summary>
    ///     Sets the minimum value supported by the sequence.
    /// </summary>
    /// <param name="minValue">The minimum value supported by the sequence.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    long? SetMinValue(long? minValue, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlySequence.MinValue" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlySequence.MinValue" />.</returns>
    ConfigurationSource? GetMinValueConfigurationSource();

    /// <summary>
    ///     Sets the maximum value supported by the sequence.
    /// </summary>
    /// <param name="maxValue">The maximum value supported by the sequence.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    long? SetMaxValue(long? maxValue, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for <see cref="IReadOnlySequence.MaxValue" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlySequence.MaxValue" />.</returns>
    ConfigurationSource? GetMaxValueConfigurationSource();

    /// <summary>
    ///     Sets the <see cref="Type" /> of values returned by the sequence.
    /// </summary>
    /// <param name="type">The <see cref="Type" /> of values returned by the sequence.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    Type? SetType(Type? type, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for <see cref="IReadOnlySequence.Type" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlySequence.Type" />.</returns>
    ConfigurationSource? GetTypeConfigurationSource();

    /// <summary>
    ///     Sets whether the sequence will start again from the beginning when the max value is reached.
    /// </summary>
    /// <param name="cyclic">
    ///     If <see langword="true" />, then the sequence will start again from the beginning when the max value
    ///     is reached.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    bool? SetIsCyclic(bool? cyclic, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for <see cref="IReadOnlySequence.IsCyclic" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlySequence.IsCyclic" />.</returns>
    ConfigurationSource? GetIsCyclicConfigurationSource();

    /// <summary>
    ///     Sets whether the sequence use preallocated values.
    /// </summary>
    /// <param name="cached">
    ///     If <see langword="true" />, then the sequence use preallocated values.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    bool? SetIsCached(bool? cached, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for <see cref="IReadOnlySequence.IsCached" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlySequence.IsCached" />.</returns>
    ConfigurationSource? GetIsCachedConfigurationSource();

    /// <summary>
    ///     Sets the amount of preallocated values.
    /// </summary>
    /// <param name="cacheSize">The amount of preallocated values.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    int? SetCacheSize(int? cacheSize, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for <see cref="IReadOnlySequence.CacheSize" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlySequence.CacheSize" />.</returns>
    ConfigurationSource? GetCacheSizeConfigurationSource();
}
