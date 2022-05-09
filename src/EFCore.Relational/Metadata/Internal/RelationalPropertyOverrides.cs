// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalPropertyOverrides :
    ConventionAnnotatable,
    IMutableRelationalPropertyOverrides,
    IConventionRelationalPropertyOverrides,
    IRelationalPropertyOverrides
{
    private string? _columnName;

    private ConfigurationSource _configurationSource;
    private ConfigurationSource? _columnNameConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalPropertyOverrides(
        IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject,
        ConfigurationSource configurationSource)
    {
        Property = property;
        StoreObject = storeObject;
        _configurationSource = configurationSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyProperty Property { get; }

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
        => ((Annotatable)Property).IsReadOnly;

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
    public virtual string? ColumnName
    {
        get => _columnName;
        set => SetColumnName(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? SetColumnName(string? columnName, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _columnName = columnName;
        _columnNameConfigurationSource = configurationSource.Max(_columnNameConfigurationSource);

        return columnName;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ColumnNameOverridden
        => _columnNameConfigurationSource != null;

    private bool RemoveColumnNameOverride(ConfigurationSource configurationSource)
    {
        if (ColumnNameOverridden)
        {
            if (!_columnNameConfigurationSource.Overrides(configurationSource))
            {
                return false;
            }

            EnsureMutable();

            _columnName = null;
            _columnNameConfigurationSource = null;

            return true;
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetColumnNameConfigurationSource()
        => _columnNameConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyRelationalPropertyOverrides? Find(IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        => ((IReadOnlyStoreObjectDictionary<IReadOnlyRelationalPropertyOverrides>?)property[RelationalAnnotationNames.RelationalOverrides])
                ?.Find(storeObject);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IReadOnlyRelationalPropertyOverrides>? Get(IReadOnlyProperty property)
        => ((IReadOnlyStoreObjectDictionary<IReadOnlyRelationalPropertyOverrides>?)property[RelationalAnnotationNames.RelationalOverrides])
                ?.GetValues();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static RelationalPropertyOverrides GetOrCreate(
        IMutableProperty property,
        in StoreObjectIdentifier storeObject,
        ConfigurationSource configurationSource)
    {
        var tableOverrides = (StoreObjectDictionary<RelationalPropertyOverrides>?)
            property[RelationalAnnotationNames.RelationalOverrides];
        if (tableOverrides == null)
        {
            tableOverrides = new ();
            property[RelationalAnnotationNames.RelationalOverrides] = tableOverrides;
        }

        var overrides = tableOverrides.Find(storeObject);
        if (overrides == null)
        {
            overrides = new (property, storeObject, configurationSource);
            tableOverrides.Add(storeObject, overrides);
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
    public static RelationalPropertyOverrides? Remove(
        IMutableProperty property,
        in StoreObjectIdentifier storeObject,
        ConfigurationSource configurationSource)
    {
        var tableOverrides = (StoreObjectDictionary<RelationalPropertyOverrides>?)
            property[RelationalAnnotationNames.RelationalOverrides];
        if (tableOverrides == null)
        {
            return null;
        }

        var overrides = tableOverrides.Find(storeObject);
        if (overrides == null)
        {
            return null;
        }

        if (configurationSource.Overrides(overrides.GetConfigurationSource()))
        {
            tableOverrides.Remove(storeObject);

            return overrides;
        }

        return null;
    }

    /// <inheritdoc />
    IProperty IRelationalPropertyOverrides.Property
    {
        [DebuggerStepThrough]
        get => (IProperty)Property;
    }

    /// <inheritdoc />
    IMutableProperty IMutableRelationalPropertyOverrides.Property
    {
        [DebuggerStepThrough]
        get => (IMutableProperty)Property;
    }

    /// <inheritdoc />
    IConventionProperty IConventionRelationalPropertyOverrides.Property
    {
        [DebuggerStepThrough]
        get => (IConventionProperty)Property;
    }

    string? IConventionRelationalPropertyOverrides.SetColumnName(string? name, bool fromDataAnnotation)
        => SetColumnName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    void IMutableRelationalPropertyOverrides.RemoveColumnNameOverride()
        => RemoveColumnNameOverride(ConfigurationSource.Explicit);

    bool IConventionRelationalPropertyOverrides.RemoveColumnNameOverride(bool fromDataAnnotation)
        => RemoveColumnNameOverride(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
