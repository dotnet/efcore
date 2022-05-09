// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class EntityTypeMappingFragment :
    ConventionAnnotatable,
    IEntityTypeMappingFragment,
    IMutableEntityTypeMappingFragment,
    IConventionEntityTypeMappingFragment
{
    private bool? _isTableExcludedFromMigrations;

    private ConfigurationSource _configurationSource;
    private ConfigurationSource? _isTableExcludedFromMigrationsConfigurationSource;
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityTypeMappingFragment(
        IReadOnlyEntityType entityType,
        in StoreObjectIdentifier storeObject,
        ConfigurationSource configurationSource)
    {
        EntityType = entityType;
        StoreObject = storeObject;
        _configurationSource = configurationSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyEntityType EntityType { get; }

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
        => ((Annotatable)EntityType).IsReadOnly;

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

    /// <inheritdoc />
    public virtual bool? IsTableExcludedFromMigrations
    {
        get => _isTableExcludedFromMigrations;
        set => SetIsTableExcludedFromMigrations(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? SetIsTableExcludedFromMigrations(bool? excluded, ConfigurationSource configurationSource)
    {
        if (!configurationSource.Overrides(_isTableExcludedFromMigrationsConfigurationSource))
        {
            return null;
        }
        
        _isTableExcludedFromMigrations = excluded;
        _isTableExcludedFromMigrationsConfigurationSource =
            excluded == null
            ? null
            : configurationSource.Max(_isTableExcludedFromMigrationsConfigurationSource);
        return excluded;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetIsTableExcludedFromMigrationsConfigurationSource()
        => _isTableExcludedFromMigrationsConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyEntityTypeMappingFragment? Find(
        IReadOnlyEntityType entityType,
        in StoreObjectIdentifier storeObject)
        => ((IReadOnlyStoreObjectDictionary<IReadOnlyEntityTypeMappingFragment>?)entityType[RelationalAnnotationNames.MappingFragments])
            ?.Find(storeObject);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IReadOnlyEntityTypeMappingFragment>? Get(IReadOnlyEntityType entityType)
        => ((IReadOnlyStoreObjectDictionary<IReadOnlyEntityTypeMappingFragment>?)entityType[RelationalAnnotationNames.MappingFragments])
            ?.GetValues();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static EntityTypeMappingFragment GetOrCreate(
        IMutableEntityType entityType,
        in StoreObjectIdentifier storeObject,
        ConfigurationSource configurationSource)
    {
        var fragments = (StoreObjectDictionary<EntityTypeMappingFragment>?)
            entityType[RelationalAnnotationNames.MappingFragments];
        if (fragments == null)
        {
            fragments = new();
            entityType[RelationalAnnotationNames.MappingFragments] = fragments;
        }

        var fragment = fragments.Find(storeObject);
        if (fragment == null)
        {
            fragment = new (entityType, storeObject, configurationSource);
            fragments.Add(storeObject, fragment);
        }
        else
        {
            fragment.UpdateConfigurationSource(configurationSource);
        }

        return fragment;
    }
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static EntityTypeMappingFragment? Remove(
        IMutableEntityType entityType,
        in StoreObjectIdentifier storeObject,
        ConfigurationSource configurationSource)
    {
        var fragments = (StoreObjectDictionary<EntityTypeMappingFragment>?)
            entityType[RelationalAnnotationNames.MappingFragments];
        if (fragments == null)
        {
            return null;
        }

        var fragment = fragments.Find(storeObject);
        if (fragment == null)
        {
            return null;
        }

        if (configurationSource.Overrides(fragment.GetConfigurationSource()))
        {
            fragments.Remove(storeObject);

            return fragment;
        }

        return null;
    }

    /// <inheritdoc />
    IEntityType IEntityTypeMappingFragment.EntityType
    {
        [DebuggerStepThrough]
        get => (IEntityType)EntityType;
    }
    
    /// <inheritdoc />
    IMutableEntityType IMutableEntityTypeMappingFragment.EntityType
    {
        [DebuggerStepThrough]
        get => (IMutableEntityType)EntityType;
    }

    /// <inheritdoc />
    IConventionEntityType IConventionEntityTypeMappingFragment.EntityType
    {
        [DebuggerStepThrough]
        get => (IConventionEntityType)EntityType;
    }

    bool? IConventionEntityTypeMappingFragment.SetIsTableExcludedFromMigrations(bool? excluded, bool fromDataAnnotation)
        => SetIsTableExcludedFromMigrations(excluded, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    bool? IReadOnlyEntityTypeMappingFragment.IsTableExcludedFromMigrations => IsTableExcludedFromMigrations;
}
