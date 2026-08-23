// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalForeignKeyOverrides :
    ConventionAnnotatable,
    IMutableRelationalForeignKeyOverrides,
    IConventionRelationalForeignKeyOverrides,
    IRelationalForeignKeyOverrides
{
    private string? _name;
    private InternalRelationalForeignKeyOverridesBuilder? _builder;

    private ConfigurationSource _configurationSource;
    private ConfigurationSource? _nameConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalForeignKeyOverrides(
        IReadOnlyForeignKey foreignKey,
        in StoreObjectPair storeObjects,
        ConfigurationSource configurationSource)
    {
        ForeignKey = foreignKey;
        StoreObjects = storeObjects;
        _configurationSource = configurationSource;
        _builder = new InternalRelationalForeignKeyOverridesBuilder(
            this, ((IConventionModel)foreignKey.DeclaringEntityType.Model).Builder);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyForeignKey ForeignKey { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoreObjectPair StoreObjects { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsReadOnly
        => ((Annotatable)ForeignKey).IsReadOnly;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalRelationalForeignKeyOverridesBuilder Builder
    {
        [DebuggerStepThrough]
        get => _builder
            ?? throw new InvalidOperationException(
                CoreStrings.ObjectRemovedFromModel(
                    $"{ForeignKey.Properties.Format()} - {StoreObjects}"));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsInModel
        => _builder is not null
            && ((IConventionAnnotatable)ForeignKey).IsInModel;

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
    public static void Attach(IConventionForeignKey foreignKey, IConventionRelationalForeignKeyOverrides detachedOverrides)
        => Attach(foreignKey, detachedOverrides, detachedOverrides.StoreObjects);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void Attach(
        IConventionForeignKey foreignKey,
        IConventionRelationalForeignKeyOverrides detachedOverrides,
        StoreObjectPair newStoreObjects)
    {
        var newOverrides = GetOrCreate(
            (IMutableForeignKey)foreignKey,
            newStoreObjects,
            detachedOverrides.GetConfigurationSource());

        MergeInto(detachedOverrides, newOverrides);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static RelationalForeignKeyOverrides MergeInto(
        IConventionRelationalForeignKeyOverrides detachedOverrides,
        IConventionRelationalForeignKeyOverrides existingOverrides)
    {
        var nameConfigurationSource = detachedOverrides.GetNameConfigurationSource();
        if (nameConfigurationSource != null)
        {
            // The null-forgiving operator is safe here: MergeInto is only ever reached through
            // Attach, which calls HasName on the override it just created via GetOrCreate, so
            // CanSetName has nothing to refuse and HasName cannot return null.
            existingOverrides = ((InternalRelationalForeignKeyOverridesBuilder)existingOverrides.Builder)
                .HasName(detachedOverrides.Name, nameConfigurationSource.Value)
                !.Metadata;
        }

        return ((InternalRelationalForeignKeyOverridesBuilder)existingOverrides.Builder)
            .MergeAnnotationsFrom((RelationalForeignKeyOverrides)detachedOverrides)
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
    public static IReadOnlyRelationalForeignKeyOverrides? Find(IReadOnlyForeignKey foreignKey, in StoreObjectPair storeObjects)
        => ((IReadOnlyStoreObjectPairDictionary<IReadOnlyRelationalForeignKeyOverrides>?)
                foreignKey[RelationalAnnotationNames.ForeignKeyOverrides])
            ?.Find(storeObjects);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IReadOnlyRelationalForeignKeyOverrides>? Get(IReadOnlyForeignKey foreignKey)
        => ((IReadOnlyStoreObjectPairDictionary<IReadOnlyRelationalForeignKeyOverrides>?)
                foreignKey[RelationalAnnotationNames.ForeignKeyOverrides])
            ?.GetValues();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static RelationalForeignKeyOverrides GetOrCreate(
        IMutableForeignKey foreignKey,
        in StoreObjectPair storeObjects,
        ConfigurationSource configurationSource)
    {
        var foreignKeyOverrides = (StoreObjectPairDictionary<RelationalForeignKeyOverrides>?)
            foreignKey[RelationalAnnotationNames.ForeignKeyOverrides];
        if (foreignKeyOverrides == null)
        {
            foreignKeyOverrides = new StoreObjectPairDictionary<RelationalForeignKeyOverrides>();
            foreignKey[RelationalAnnotationNames.ForeignKeyOverrides] = foreignKeyOverrides;
        }

        var overrides = foreignKeyOverrides.Find(storeObjects);
        if (overrides == null)
        {
            overrides = new RelationalForeignKeyOverrides(foreignKey, storeObjects, configurationSource);
            foreignKeyOverrides.Add(storeObjects, overrides);
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
    public static RelationalForeignKeyOverrides? Remove(
        IMutableForeignKey foreignKey,
        in StoreObjectPair storeObjects)
    {
        var foreignKeyOverrides = (StoreObjectPairDictionary<RelationalForeignKeyOverrides>?)
            foreignKey[RelationalAnnotationNames.ForeignKeyOverrides];
        if (foreignKeyOverrides == null)
        {
            return null;
        }

        var overrides = foreignKeyOverrides.Find(storeObjects);
        if (overrides == null)
        {
            return null;
        }

        foreignKeyOverrides.Remove(storeObjects);
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
        => ((IRelationalForeignKeyOverrides)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IRelationalForeignKeyOverrides)this).ToDebugString(),
            () => ((IRelationalForeignKeyOverrides)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IForeignKey IRelationalForeignKeyOverrides.ForeignKey
    {
        [DebuggerStepThrough]
        get => (IForeignKey)ForeignKey;
    }

    /// <inheritdoc />
    IMutableForeignKey IMutableRelationalForeignKeyOverrides.ForeignKey
    {
        [DebuggerStepThrough]
        get => (IMutableForeignKey)ForeignKey;
    }

    /// <inheritdoc />
    IConventionForeignKey IConventionRelationalForeignKeyOverrides.ForeignKey
    {
        [DebuggerStepThrough]
        get => (IConventionForeignKey)ForeignKey;
    }

    /// <inheritdoc />
    IConventionRelationalForeignKeyOverridesBuilder IConventionRelationalForeignKeyOverrides.Builder
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    string? IConventionRelationalForeignKeyOverrides.SetName(string? name, bool fromDataAnnotation)
        => SetName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    void IMutableRelationalForeignKeyOverrides.RemoveNameOverride()
        => RemoveNameOverride(ConfigurationSource.Explicit);

    bool IConventionRelationalForeignKeyOverrides.RemoveNameOverride(bool fromDataAnnotation)
        => RemoveNameOverride(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
