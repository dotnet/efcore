// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalSequenceBuilder : AnnotatableBuilder<Sequence, IConventionModelBuilder>, IConventionSequenceBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalSequenceBuilder(Sequence sequence, IConventionModelBuilder modelBuilder)
        : base(sequence, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionSequenceBuilder? HasType(Type? type, ConfigurationSource configurationSource)
    {
        if (configurationSource.Overrides(Metadata.GetTypeConfigurationSource())
            || Metadata.Type == type)
        {
            Metadata.SetType(type, configurationSource);
            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetType(Type? type, ConfigurationSource configurationSource)
        => (type == null || Sequence.SupportedTypes.Contains(type))
            && (configurationSource.Overrides(Metadata.GetTypeConfigurationSource())
                || Metadata.Type == type);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionSequenceBuilder? IncrementsBy(
        int? increment,
        ConfigurationSource configurationSource)
    {
        if (CanSetIncrementsBy(increment, configurationSource))
        {
            Metadata.SetIncrementBy(increment, configurationSource);
            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetIncrementsBy(int? increment, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetIncrementByConfigurationSource())
            || Metadata.IncrementBy == increment;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionSequenceBuilder? StartsAt(long? startValue, ConfigurationSource configurationSource)
    {
        if (CanSetStartsAt(startValue, configurationSource))
        {
            Metadata.SetStartValue(startValue, configurationSource);
            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetStartsAt(long? startValue, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetStartValueConfigurationSource())
            || Metadata.StartValue == startValue;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionSequenceBuilder? HasMax(long? maximum, ConfigurationSource configurationSource)
    {
        if (CanSetMax(maximum, configurationSource))
        {
            Metadata.SetMaxValue(maximum, configurationSource);
            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetMax(long? maximum, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetMaxValueConfigurationSource())
            || Metadata.MaxValue == maximum;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionSequenceBuilder? HasMin(long? minimum, ConfigurationSource configurationSource)
    {
        if (CanSetMin(minimum, configurationSource))
        {
            Metadata.SetMinValue(minimum, configurationSource);
            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetMin(long? minimum, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetMinValueConfigurationSource())
            || Metadata.MinValue == minimum;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionSequenceBuilder? IsCyclic(bool? cyclic, ConfigurationSource configurationSource)
    {
        if (CanSetIsCyclic(cyclic, configurationSource))
        {
            Metadata.SetIsCyclic(cyclic, configurationSource);
            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetIsCyclic(bool? cyclic, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetIsCyclicConfigurationSource())
            || Metadata.IsCyclic == cyclic;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionSequenceBuilder? UseNoCache(ConfigurationSource configurationSource)
    {
        if (CanSetNoCache(configurationSource))
        {
            Metadata.SetIsCached(false, configurationSource);
            Metadata.SetCacheSize(null, configurationSource);
            return this;
        }
        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetNoCache(ConfigurationSource configurationSource)
                => configurationSource.Overrides(Metadata.GetIsCachedConfigurationSource())
            || Metadata.IsCached == false;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionSequenceBuilder? UseCache(int? cacheSize, ConfigurationSource configurationSource)
    {
        if (CanSetCacheSize(cacheSize, configurationSource))
        {
            Metadata.SetIsCached(true, configurationSource);
            Metadata.SetCacheSize(cacheSize, configurationSource);
            return this;
        }
        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetCacheSize(long? cacheSize, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetCacheSizeConfigurationSource())
            || Metadata.CacheSize == cacheSize;

    /// <inheritdoc />
    IConventionSequence IConventionSequenceBuilder.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSequenceBuilder? IConventionSequenceBuilder.HasAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionSequenceBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSequenceBuilder? IConventionSequenceBuilder.HasNonNullAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionSequenceBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSequenceBuilder? IConventionSequenceBuilder.HasNoAnnotation(string name, bool fromDataAnnotation)
        => (IConventionSequenceBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSequenceBuilder? IConventionSequenceBuilder.HasType(Type? type, bool fromDataAnnotation)
        => HasType(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionSequenceBuilder.CanSetType(Type? type, bool fromDataAnnotation)
        => CanSetType(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSequenceBuilder? IConventionSequenceBuilder.IncrementsBy(int? increment, bool fromDataAnnotation)
        => IncrementsBy(increment, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionSequenceBuilder.CanSetIncrementsBy(int? increment, bool fromDataAnnotation)
        => CanSetIncrementsBy(increment, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSequenceBuilder? IConventionSequenceBuilder.StartsAt(long? startValue, bool fromDataAnnotation)
        => StartsAt(startValue, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionSequenceBuilder.CanSetStartsAt(long? startValue, bool fromDataAnnotation)
        => CanSetStartsAt(startValue, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSequenceBuilder? IConventionSequenceBuilder.HasMax(long? maximum, bool fromDataAnnotation)
        => HasMax(maximum, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionSequenceBuilder.CanSetMax(long? maximum, bool fromDataAnnotation)
        => CanSetMax(maximum, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSequenceBuilder? IConventionSequenceBuilder.HasMin(long? minimum, bool fromDataAnnotation)
        => HasMin(minimum, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionSequenceBuilder.CanSetMin(long? minimum, bool fromDataAnnotation)
        => CanSetMin(minimum, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSequenceBuilder? IConventionSequenceBuilder.IsCyclic(bool? cyclic, bool fromDataAnnotation)
        => IsCyclic(cyclic, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionSequenceBuilder.CanSetIsCyclic(bool? cyclic, bool fromDataAnnotation)
        => CanSetIsCyclic(cyclic, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSequenceBuilder? IConventionSequenceBuilder.UseNoCache(bool fromDataAnnotation)
        => UseNoCache(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionSequenceBuilder.CanSetNoCache(bool fromDataAnnotation)
        => CanSetNoCache(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSequenceBuilder? IConventionSequenceBuilder.UseCache(int? cacheSize, bool fromDataAnnotation)
        => UseCache(cacheSize, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionSequenceBuilder.CanSetCache(int? cacheSize, bool fromDataAnnotation)
        => CanSetCacheSize(cacheSize, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
