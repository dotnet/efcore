// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalKeyOverrides :
    ConventionAnnotatable,
    IMutableRelationalKeyOverrides,
    IConventionRelationalKeyOverrides,
    IRelationalKeyOverrides
{
    private string? _name;
    private InternalRelationalKeyOverridesBuilder? _builder;

    private ConfigurationSource _configurationSource;
    private ConfigurationSource? _nameConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalKeyOverrides(
        IReadOnlyKey key,
        in StoreObjectIdentifier storeObject,
        ConfigurationSource configurationSource)
    {
        Key = key;
        StoreObject = storeObject;
        _configurationSource = configurationSource;
        _builder = new InternalRelationalKeyOverridesBuilder(
            this, ((IConventionModel)key.DeclaringEntityType.Model).Builder);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyKey Key { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoreObjectIdentifier StoreObject { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsReadOnly
        => ((Annotatable)Key).IsReadOnly;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalRelationalKeyOverridesBuilder Builder
    {
        [DebuggerStepThrough]
        get => _builder
            ?? throw new InvalidOperationException(
                CoreStrings.ObjectRemovedFromModel(
                    $"{Key.Properties.Format()} - {StoreObject.DisplayName()}"));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsInModel
        => _builder is not null
            && ((IConventionAnnotatable)Key).IsInModel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetRemovedFromModel()
        => _builder = null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void Attach(IConventionKey key, IConventionRelationalKeyOverrides detachedOverrides)
        => Attach(key, detachedOverrides, detachedOverrides.StoreObject);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void Attach(
        IConventionKey key,
        IConventionRelationalKeyOverrides detachedOverrides,
        StoreObjectIdentifier newStoreObject)
    {
        var newOverrides = GetOrCreate(
            (IMutableKey)key,
            newStoreObject,
            detachedOverrides.GetConfigurationSource());

        MergeInto(detachedOverrides, newOverrides);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static RelationalKeyOverrides MergeInto(
        IConventionRelationalKeyOverrides detachedOverrides,
        IConventionRelationalKeyOverrides existingOverrides)
    {
        var nameConfigurationSource = detachedOverrides.GetNameConfigurationSource();
        if (nameConfigurationSource != null)
        {
            existingOverrides = ((InternalRelationalKeyOverridesBuilder)existingOverrides.Builder)
                .HasName(detachedOverrides.Name, nameConfigurationSource.Value)
                !.Metadata;
        }

        return ((InternalRelationalKeyOverridesBuilder)existingOverrides.Builder)
            .MergeAnnotationsFrom((RelationalKeyOverrides)detachedOverrides)
            .Metadata;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource GetConfigurationSource()
        => _configurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
        => _configurationSource = configurationSource.Max(_configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? Name
    {
        get => _name;
        set => SetName(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? SetName(string? name, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _name = name;
        _nameConfigurationSource = configurationSource.Max(_nameConfigurationSource);

        return name;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsNameOverridden
        => _nameConfigurationSource != null;

    private bool RemoveNameOverride(ConfigurationSource configurationSource)
    {
        if (!IsNameOverridden)
        {
            return true;
        }

        if (!_nameConfigurationSource.Overrides(configurationSource))
        {
            return false;
        }

        EnsureMutable();

        _name = null;
        _nameConfigurationSource = null;

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetNameConfigurationSource()
        => _nameConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyRelationalKeyOverrides? Find(IReadOnlyKey key, in StoreObjectIdentifier storeObject)
        => ((IReadOnlyStoreObjectDictionary<IReadOnlyRelationalKeyOverrides>?)key[RelationalAnnotationNames.KeyOverrides])
            ?.Find(storeObject);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IReadOnlyRelationalKeyOverrides>? Get(IReadOnlyKey key)
        => ((IReadOnlyStoreObjectDictionary<IReadOnlyRelationalKeyOverrides>?)key[RelationalAnnotationNames.KeyOverrides])
            ?.GetValues();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static RelationalKeyOverrides GetOrCreate(
        IMutableKey key,
        in StoreObjectIdentifier storeObject,
        ConfigurationSource configurationSource)
    {
        var keyOverrides = (StoreObjectDictionary<RelationalKeyOverrides>?)key[RelationalAnnotationNames.KeyOverrides];
        if (keyOverrides == null)
        {
            keyOverrides = new StoreObjectDictionary<RelationalKeyOverrides>();
            key[RelationalAnnotationNames.KeyOverrides] = keyOverrides;
        }

        var overrides = keyOverrides.Find(storeObject);
        if (overrides == null)
        {
            overrides = new RelationalKeyOverrides(key, storeObject, configurationSource);
            keyOverrides.Add(storeObject, overrides);
        }
        else
        {
            overrides.UpdateConfigurationSource(configurationSource);
        }

        return overrides;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static RelationalKeyOverrides? Remove(
        IMutableKey key,
        in StoreObjectIdentifier storeObject)
    {
        var keyOverrides = (StoreObjectDictionary<RelationalKeyOverrides>?)key[RelationalAnnotationNames.KeyOverrides];
        if (keyOverrides == null)
        {
            return null;
        }

        var overrides = keyOverrides.Find(storeObject);
        if (overrides == null)
        {
            return null;
        }

        keyOverrides.Remove(storeObject);
        overrides.SetRemovedFromModel();

        return overrides;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IRelationalKeyOverrides)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IRelationalKeyOverrides)this).ToDebugString(),
            () => ((IRelationalKeyOverrides)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IKey IRelationalKeyOverrides.Key
    {
        [DebuggerStepThrough]
        get => (IKey)Key;
    }

    /// <inheritdoc />
    IMutableKey IMutableRelationalKeyOverrides.Key
    {
        [DebuggerStepThrough]
        get => (IMutableKey)Key;
    }

    /// <inheritdoc />
    IConventionKey IConventionRelationalKeyOverrides.Key
    {
        [DebuggerStepThrough]
        get => (IConventionKey)Key;
    }

    /// <inheritdoc />
    IConventionRelationalKeyOverridesBuilder IConventionRelationalKeyOverrides.Builder
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    string? IConventionRelationalKeyOverrides.SetName(string? name, bool fromDataAnnotation)
        => SetName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    void IMutableRelationalKeyOverrides.RemoveNameOverride()
        => RemoveNameOverride(ConfigurationSource.Explicit);

    bool IConventionRelationalKeyOverrides.RemoveNameOverride(bool fromDataAnnotation)
        => RemoveNameOverride(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
