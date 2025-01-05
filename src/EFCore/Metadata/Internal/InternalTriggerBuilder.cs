// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalTriggerBuilder : AnnotatableBuilder<Trigger, IConventionModelBuilder>, IConventionTriggerBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalTriggerBuilder(Trigger trigger, IConventionModelBuilder modelBuilder)
        : base(trigger, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalTriggerBuilder? Attach(InternalEntityTypeBuilder entityTypeBuilder)
    {
        var detachedTrigger = Metadata;
        var newTriggerBuilder = entityTypeBuilder.HasTrigger(
            detachedTrigger.ModelName,
            detachedTrigger.GetConfigurationSource());

        newTriggerBuilder?.MergeAnnotationsFrom(detachedTrigger);

        return newTriggerBuilder;
    }

    IConventionTrigger IConventionTriggerBuilder.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionTriggerBuilder? IConventionTriggerBuilder.HasAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionTriggerBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionTriggerBuilder? IConventionTriggerBuilder.HasNonNullAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionTriggerBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionTriggerBuilder? IConventionTriggerBuilder.HasNoAnnotation(string name, bool fromDataAnnotation)
        => (IConventionTriggerBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
