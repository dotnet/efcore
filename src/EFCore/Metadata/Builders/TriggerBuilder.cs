// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides an API point for provider-specific extensions for configuring a <see cref="ITrigger" />.
/// </summary>
public class TriggerBuilder : IInfrastructure<IConventionTriggerBuilder>
{
    /// <summary>
    ///     Creates a new builder for the given <see cref="ITrigger" />.
    /// </summary>
    /// <param name="trigger">The <see cref="IMutableTrigger" /> to configure.</param>
    public TriggerBuilder(IMutableTrigger trigger)
    {
        InternalBuilder = ((Trigger)trigger).Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalTriggerBuilder InternalBuilder { [DebuggerStepThrough] get; }

    /// <summary>
    ///     Gets the builder that can be used to configure this trigger.
    /// </summary>
    protected virtual IConventionTriggerBuilder Builder
        => InternalBuilder;

    /// <inheritdoc />
    IConventionTriggerBuilder IInfrastructure<IConventionTriggerBuilder>.Instance
    {
        [DebuggerStepThrough]
        get => InternalBuilder;
    }

    /// <summary>
    ///     The trigger being configured.
    /// </summary>
    public virtual IMutableTrigger Metadata
        => InternalBuilder.Metadata;

    /// <summary>
    ///     Adds or updates an annotation on the trigger. If an annotation with the key specified in <paramref name="annotation" />
    ///     already exists, its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual TriggerBuilder HasAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        InternalBuilder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

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
