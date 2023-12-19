// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalSkipNavigationBuilder :
    InternalPropertyBaseBuilder<IConventionSkipNavigationBuilder, SkipNavigation>,
    IConventionSkipNavigationBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalSkipNavigationBuilder(SkipNavigation metadata, InternalModelBuilder modelBuilder)
        : base(metadata, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override InternalSkipNavigationBuilder This
        => this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalSkipNavigationBuilder? HasForeignKey(
        ForeignKey? foreignKey,
        ConfigurationSource configurationSource)
    {
        if (!CanSetForeignKey(foreignKey, configurationSource))
        {
            return null;
        }

        if (foreignKey != null)
        {
            foreignKey.UpdateConfigurationSource(configurationSource);

            if (Metadata.Inverse?.JoinEntityType != null
                && Metadata.Inverse.JoinEntityType
                != (Metadata.IsOnDependent ? foreignKey.PrincipalEntityType : foreignKey.DeclaringEntityType))
            {
                Metadata.Inverse.Builder.HasForeignKey(null, configurationSource);
            }
        }

        var oldForeignKey = Metadata.ForeignKey;

        Metadata.SetForeignKey(foreignKey, configurationSource);

        if (oldForeignKey?.IsInModel == true
            && oldForeignKey != foreignKey
            && oldForeignKey.ReferencingSkipNavigations?.Any() != true)
        {
            oldForeignKey.DeclaringEntityType.Builder.HasNoRelationship(oldForeignKey, ConfigurationSource.Explicit);
        }

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetForeignKey(ForeignKey? foreignKey, ConfigurationSource? configurationSource)
    {
        if (!configurationSource.Overrides(Metadata.GetForeignKeyConfigurationSource()))
        {
            return Equals(Metadata.ForeignKey, foreignKey);
        }

        if (foreignKey == null)
        {
            return true;
        }

        if (Metadata.DeclaringEntityType
            != (Metadata.IsOnDependent ? foreignKey.DeclaringEntityType : foreignKey.PrincipalEntityType))
        {
            return false;
        }

        if (Metadata.Inverse?.JoinEntityType == null)
        {
            return true;
        }

        return Metadata.Inverse.JoinEntityType
            == (Metadata.IsOnDependent ? foreignKey.PrincipalEntityType : foreignKey.DeclaringEntityType)
            || Metadata.Inverse.Builder.CanSetForeignKey(null, configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalSkipNavigationBuilder? HasInverse(
        SkipNavigation? inverse,
        ConfigurationSource configurationSource)
    {
        if (!CanSetInverse(inverse, configurationSource))
        {
            return null;
        }

        inverse?.UpdateConfigurationSource(configurationSource);

        using var _ = Metadata.DeclaringEntityType.Model.DelayConventions();

        if (Metadata.Inverse != null
            && Metadata.Inverse != inverse)
        {
            Metadata.Inverse.SetInverse(null, configurationSource);
        }

        Metadata.SetInverse(inverse, configurationSource);

        inverse?.SetInverse(Metadata, configurationSource);

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetInverse(
        SkipNavigation? inverse,
        ConfigurationSource? configurationSource)
    {
        if (!configurationSource.Overrides(Metadata.GetInverseConfigurationSource())
            || (inverse != null
                && !configurationSource.Overrides(inverse.GetInverseConfigurationSource())))
        {
            return Equals(Metadata.Inverse, inverse);
        }

        if (inverse == null)
        {
            return true;
        }

        return Metadata.TargetEntityType == inverse.DeclaringEntityType
            && Metadata.DeclaringEntityType == inverse.TargetEntityType
            && (Metadata.JoinEntityType == null
                || inverse.JoinEntityType == null
                || Metadata.JoinEntityType == inverse.JoinEntityType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalSkipNavigationBuilder? Attach(
        InternalEntityTypeBuilder? entityTypeBuilder = null,
        EntityType? targetEntityType = null,
        InternalSkipNavigationBuilder? inverseBuilder = null)
    {
        if (entityTypeBuilder is null)
        {
            if (Metadata.DeclaringEntityType.IsInModel)
            {
                entityTypeBuilder = Metadata.DeclaringEntityType.Builder;
            }
            else if (Metadata.DeclaringEntityType.Model.FindEntityType(Metadata.DeclaringEntityType.Name) is EntityType entityType)
            {
                entityTypeBuilder = entityType.Builder;
            }
            else
            {
                return null;
            }
        }

        targetEntityType ??= Metadata.TargetEntityType;
        if (!targetEntityType.IsInModel)
        {
            targetEntityType = Metadata.DeclaringEntityType.Model.FindEntityType(targetEntityType.Name);
            if (targetEntityType == null)
            {
                return null;
            }
        }

        var newSkipNavigationBuilder = entityTypeBuilder.HasSkipNavigation(
            Metadata.CreateMemberIdentity(),
            targetEntityType,
            Metadata.ClrType,
            Metadata.GetConfigurationSource(),
            Metadata.IsCollection,
            Metadata.IsOnDependent);
        if (newSkipNavigationBuilder == null)
        {
            return null;
        }

        newSkipNavigationBuilder.MergeAnnotationsFrom(Metadata);

        var foreignKeyConfigurationSource = Metadata.GetForeignKeyConfigurationSource();
        if (foreignKeyConfigurationSource.HasValue)
        {
            var foreignKey = Metadata.ForeignKey!;
            if (!foreignKey.IsInModel)
            {
                foreignKey = InternalForeignKeyBuilder.FindCurrentForeignKeyBuilder(
                    foreignKey.PrincipalEntityType,
                    foreignKey.DeclaringEntityType,
                    foreignKey.DependentToPrincipal?.CreateMemberIdentity(),
                    foreignKey.PrincipalToDependent?.CreateMemberIdentity(),
                    dependentProperties: foreignKey.Properties,
                    principalProperties: foreignKey.PrincipalKey.Properties)?.Metadata;
            }

            if (foreignKey != null)
            {
                newSkipNavigationBuilder.HasForeignKey(foreignKey, foreignKeyConfigurationSource.Value);
            }
        }

        var inverseConfigurationSource = Metadata.GetInverseConfigurationSource();
        if (inverseConfigurationSource.HasValue)
        {
            var inverse = Metadata.Inverse!;
            if (!inverse.IsInModel)
            {
                inverse = inverse.DeclaringEntityType.FindSkipNavigation(inverse.Name);
            }

            if (inverseBuilder != null)
            {
                inverse = inverseBuilder.Attach(targetEntityType.Builder, entityTypeBuilder.Metadata)?.Metadata
                    ?? inverse;
            }

            if (inverse != null)
            {
                newSkipNavigationBuilder.HasInverse(inverse, inverseConfigurationSource.Value);
            }
        }

        var propertyAccessModeConfigurationSource = Metadata.GetPropertyAccessModeConfigurationSource();
        if (propertyAccessModeConfigurationSource.HasValue)
        {
            newSkipNavigationBuilder.UsePropertyAccessMode(
                Metadata.GetPropertyAccessMode(), propertyAccessModeConfigurationSource.Value);
        }

        var oldFieldInfoConfigurationSource = Metadata.GetFieldInfoConfigurationSource();
        if (oldFieldInfoConfigurationSource.HasValue
            && newSkipNavigationBuilder.CanSetField(Metadata.FieldInfo, oldFieldInfoConfigurationSource))
        {
            newSkipNavigationBuilder.HasField(Metadata.FieldInfo, oldFieldInfoConfigurationSource.Value);
        }

        return newSkipNavigationBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetAutoInclude(bool? autoInclude, ConfigurationSource configurationSource)
    {
        IConventionSkipNavigation conventionNavigation = Metadata;

        return configurationSource.Overrides(conventionNavigation.GetIsEagerLoadedConfigurationSource())
            || conventionNavigation.IsEagerLoaded == autoInclude;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalSkipNavigationBuilder? AutoInclude(bool? autoInclude, ConfigurationSource configurationSource)
    {
        if (CanSetAutoInclude(autoInclude, configurationSource))
        {
            if (configurationSource == ConfigurationSource.Explicit)
            {
                ((IMutableSkipNavigation)Metadata).SetIsEagerLoaded(autoInclude);
            }
            else
            {
                ((IConventionSkipNavigation)Metadata).SetIsEagerLoaded(
                    autoInclude, configurationSource == ConfigurationSource.DataAnnotation);
            }

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
    public virtual bool CanSetLazyLoadingEnabled(bool? lazyLoadingEnabled, ConfigurationSource configurationSource)
    {
        IConventionSkipNavigation conventionNavigation = Metadata;

        return configurationSource.Overrides(conventionNavigation.GetLazyLoadingEnabledConfigurationSource())
            || conventionNavigation.LazyLoadingEnabled == lazyLoadingEnabled;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalSkipNavigationBuilder? EnableLazyLoading(bool? lazyLoadingEnabled, ConfigurationSource configurationSource)
    {
        if (CanSetLazyLoadingEnabled(lazyLoadingEnabled, configurationSource))
        {
            if (configurationSource == ConfigurationSource.Explicit)
            {
                ((IMutableSkipNavigation)Metadata).SetLazyLoadingEnabled(lazyLoadingEnabled);
            }
            else
            {
                ((IConventionSkipNavigation)Metadata).SetLazyLoadingEnabled(
                    lazyLoadingEnabled, configurationSource == ConfigurationSource.DataAnnotation);
            }

            return this;
        }

        return null;
    }

    IConventionPropertyBase IConventionPropertyBaseBuilder<IConventionSkipNavigationBuilder>.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }

    IConventionSkipNavigation IConventionSkipNavigationBuilder.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionSkipNavigationBuilder? IConventionPropertyBaseBuilder<IConventionSkipNavigationBuilder>.HasAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
        => (IConventionSkipNavigationBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionSkipNavigationBuilder? IConventionPropertyBaseBuilder<IConventionSkipNavigationBuilder>.HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
        => (IConventionSkipNavigationBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionSkipNavigationBuilder? IConventionPropertyBaseBuilder<IConventionSkipNavigationBuilder>.HasNoAnnotation(
        string name,
        bool fromDataAnnotation)
        => (IConventionSkipNavigationBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSkipNavigationBuilder? IConventionPropertyBaseBuilder<IConventionSkipNavigationBuilder>.HasField(
        string? fieldName,
        bool fromDataAnnotation)
        => HasField(
            fieldName,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSkipNavigationBuilder? IConventionPropertyBaseBuilder<IConventionSkipNavigationBuilder>.HasField(
        FieldInfo? fieldInfo,
        bool fromDataAnnotation)
        => HasField(
            fieldInfo,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionPropertyBaseBuilder<IConventionSkipNavigationBuilder>.CanSetField(string? fieldName, bool fromDataAnnotation)
        => CanSetField(
            fieldName,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionPropertyBaseBuilder<IConventionSkipNavigationBuilder>.CanSetField(FieldInfo? fieldInfo, bool fromDataAnnotation)
        => CanSetField(
            fieldInfo,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSkipNavigationBuilder? IConventionPropertyBaseBuilder<IConventionSkipNavigationBuilder>.UsePropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        bool fromDataAnnotation)
        => UsePropertyAccessMode(
            propertyAccessMode,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionPropertyBaseBuilder<IConventionSkipNavigationBuilder>.CanSetPropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        bool fromDataAnnotation)
        => CanSetPropertyAccessMode(
            propertyAccessMode,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSkipNavigationBuilder? IConventionSkipNavigationBuilder.HasForeignKey(
        IConventionForeignKey? foreignKey,
        bool fromDataAnnotation)
        => HasForeignKey(
            (ForeignKey?)foreignKey,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionSkipNavigationBuilder.CanSetForeignKey(
        IConventionForeignKey? foreignKey,
        bool fromDataAnnotation)
        => CanSetForeignKey(
            (ForeignKey?)foreignKey,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSkipNavigationBuilder? IConventionSkipNavigationBuilder.HasInverse(
        IConventionSkipNavigation? inverse,
        bool fromDataAnnotation)
        => HasInverse(
            (SkipNavigation?)inverse,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionSkipNavigationBuilder.CanSetInverse(
        IConventionSkipNavigation? inverse,
        bool fromDataAnnotation)
        => CanSetInverse(
            (SkipNavigation?)inverse,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionSkipNavigationBuilder.CanSetAutoInclude(bool? autoInclude, bool fromDataAnnotation)
        => CanSetAutoInclude(autoInclude, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSkipNavigationBuilder? IConventionSkipNavigationBuilder.AutoInclude(bool? autoInclude, bool fromDataAnnotation)
        => AutoInclude(autoInclude, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionSkipNavigationBuilder.CanSetLazyLoadingEnabled(bool? lazyLoadingEnabled, bool fromDataAnnotation)
        => CanSetLazyLoadingEnabled(
            lazyLoadingEnabled, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSkipNavigationBuilder? IConventionSkipNavigationBuilder.EnableLazyLoading(bool? lazyLoadingEnabled, bool fromDataAnnotation)
        => EnableLazyLoading(lazyLoadingEnabled, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
