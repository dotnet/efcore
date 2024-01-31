// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="ISequence" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
/// </remarks>
public class SequenceBuilder : IInfrastructure<IConventionSequenceBuilder>
{
    /// <summary>
    ///     Creates a new builder for the given <see cref="ISequence" />.
    /// </summary>
    /// <param name="sequence">The <see cref="IMutableSequence" /> to configure.</param>
    public SequenceBuilder(IMutableSequence sequence)
    {
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <param name="increment">The amount to increment between values.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual SequenceBuilder IncrementsBy(int increment)
    {
        Builder.IncrementsBy(increment, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Sets the <see cref="ISequence" /> to start at the given value.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <param name="startValue">The starting value for the sequence.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual SequenceBuilder StartsAt(long startValue)
    {
        Builder.StartsAt(startValue, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Sets the maximum value for the <see cref="ISequence" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <param name="maximum">The maximum value for the sequence.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual SequenceBuilder HasMax(long maximum)
    {
        Builder.HasMax(maximum, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Sets the minimum value for the <see cref="ISequence" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <param name="minimum">The minimum value for the sequence.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual SequenceBuilder HasMin(long minimum)
    {
        Builder.HasMin(minimum, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Sets whether or not the sequence will start again from the beginning once
    ///     the maximum value is reached.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <param name="cyclic">If <see langword="true" />, then the sequence will restart when the maximum is reached.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual SequenceBuilder IsCyclic(bool cyclic = true)
    {
        Builder.IsCyclic(cyclic, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Sets that sequence does not use preallocated values.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual SequenceBuilder UseNoCache()
    {
        Builder.UseNoCache(ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Sets the amount of preallocated values for the <see cref="ISequence" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <param name="cacheSize">The amount of preallocated values.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual SequenceBuilder UseCache(int? cacheSize = default)
    {
        Builder.UseCache(cacheSize, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Adds or updates an annotation on the sequence. If an annotation with the key specified in <paramref name="annotation" />
    ///     already exists, its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual SequenceBuilder HasAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

        return this;
    }

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectEqualsIsObjectEquals
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
