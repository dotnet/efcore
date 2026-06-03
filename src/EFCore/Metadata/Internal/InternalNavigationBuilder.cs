// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalNavigationBuilder :
    InternalPropertyBaseBuilder<IConventionNavigationBuilder, Navigation>,
    IConventionNavigationBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalNavigationBuilder(Navigation metadata, InternalModelBuilder modelBuilder)
        : base(metadata, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override IConventionNavigationBuilder This
        => this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual InternalNavigationBuilder? HasField(string? fieldName, ConfigurationSource configurationSource)
        => (InternalNavigationBuilder?)base.HasField(fieldName, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual InternalNavigationBuilder? HasField(FieldInfo? fieldInfo, ConfigurationSource configurationSource)
        => (InternalNavigationBuilder?)base.HasField(fieldInfo, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual InternalNavigationBuilder? UsePropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        ConfigurationSource configurationSource)
        => (InternalNavigationBuilder?)base.UsePropertyAccessMode(propertyAccessMode, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetAutoInclude(bool? autoInclude, ConfigurationSource configurationSource)
    {
        IConventionNavigation conventionNavigation = Metadata;

        return configurationSource.Overrides(conventionNavigation.GetIsEagerLoadedConfigurationSource())
            || conventionNavigation.IsEagerLoaded == autoInclude;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalNavigationBuilder? AutoInclude(bool? autoInclude, ConfigurationSource configurationSource)
    {
        if (CanSetAutoInclude(autoInclude, configurationSource))
        {
            if (configurationSource == ConfigurationSource.Explicit)
            {
                ((IMutableNavigation)Metadata).SetIsEagerLoaded(autoInclude);
            }
            else
            {
                ((IConventionNavigation)Metadata).SetIsEagerLoaded(
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
        IConventionNavigation conventionNavigation = Metadata;

        return configurationSource.Overrides(conventionNavigation.GetLazyLoadingEnabledConfigurationSource())
            || conventionNavigation.LazyLoadingEnabled == lazyLoadingEnabled;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalNavigationBuilder? EnableLazyLoading(bool? lazyLoadingEnabled, ConfigurationSource configurationSource)
    {
        if (CanSetLazyLoadingEnabled(lazyLoadingEnabled, configurationSource))
        {
            if (configurationSource == ConfigurationSource.Explicit)
            {
                ((IMutableNavigation)Metadata).SetLazyLoadingEnabled(lazyLoadingEnabled);
            }
            else
            {
                ((IConventionNavigation)Metadata).SetLazyLoadingEnabled(
                    lazyLoadingEnabled, configurationSource == ConfigurationSource.DataAnnotation);
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
    public virtual bool CanSetIsRequired(bool? required, ConfigurationSource configurationSource)
    {
        var foreignKey = Metadata.ForeignKey;
        return foreignKey.IsUnique
            ? foreignKey.GetPrincipalEndConfigurationSource() != null
            && (Metadata.IsOnDependent
                ? foreignKey.Builder.CanSetIsRequired(required, configurationSource)
                : foreignKey.Builder.CanSetIsRequiredDependent(required, configurationSource))
            : Metadata.IsOnDependent
            && foreignKey.Builder.CanSetIsRequired(required, configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalNavigationBuilder? IsRequired(bool? required, ConfigurationSource configurationSource)
    {
        if (configurationSource == ConfigurationSource.Explicit
            || CanSetIsRequired(required, configurationSource))
        {
            var foreignKey = Metadata.ForeignKey;
            if (foreignKey.IsUnique)
            {
                if (foreignKey.GetPrincipalEndConfigurationSource() == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.AmbiguousEndRequiredDependentNavigation(
                            Metadata.DeclaringEntityType.DisplayName(),
                            Metadata.Name,
                            foreignKey.Properties.Format()));
                }

                return Metadata.IsOnDependent
                    ? foreignKey.Builder.IsRequired(required, configurationSource)!
                        .Metadata.DependentToPrincipal!.Builder
                    : foreignKey.Builder.IsRequiredDependent(required, configurationSource)!
                        .Metadata.PrincipalToDependent!.Builder;
            }

            if (Metadata.IsOnDependent)
            {
                return foreignKey.Builder.IsRequired(required, configurationSource)!
                    .Metadata.DependentToPrincipal!.Builder;
            }

            throw new InvalidOperationException(
                CoreStrings.NonUniqueRequiredDependentNavigation(
                    foreignKey.PrincipalEntityType.DisplayName(), Metadata.Name));
        }

        return null;
    }

    IConventionPropertyBase IConventionPropertyBaseBuilder<IConventionNavigationBuilder>.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }

    IConventionNavigation IConventionNavigationBuilder.Metadata
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
    IConventionNavigationBuilder? IConventionPropertyBaseBuilder<IConventionNavigationBuilder>.HasAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
        => (IConventionNavigationBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionNavigationBuilder? IConventionPropertyBaseBuilder<IConventionNavigationBuilder>.HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
        => (IConventionNavigationBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionNavigationBuilder? IConventionPropertyBaseBuilder<IConventionNavigationBuilder>.HasNoAnnotation(
        string name,
        bool fromDataAnnotation)
        => (IConventionNavigationBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionPropertyBaseBuilder<IConventionNavigationBuilder>.CanSetPropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        bool fromDataAnnotation)
        => CanSetPropertyAccessMode(
            propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionNavigationBuilder? IConventionPropertyBaseBuilder<IConventionNavigationBuilder>.UsePropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        bool fromDataAnnotation)
        => UsePropertyAccessMode(
            propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionNavigationBuilder? IConventionPropertyBaseBuilder<IConventionNavigationBuilder>.HasField(
        string? fieldName,
        bool fromDataAnnotation)
        => HasField(
            fieldName,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionNavigationBuilder? IConventionPropertyBaseBuilder<IConventionNavigationBuilder>.HasField(
        FieldInfo? fieldInfo,
        bool fromDataAnnotation)
        => HasField(
            fieldInfo,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionPropertyBaseBuilder<IConventionNavigationBuilder>.CanSetField(string? fieldName, bool fromDataAnnotation)
        => CanSetField(
            fieldName,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionPropertyBaseBuilder<IConventionNavigationBuilder>.CanSetField(FieldInfo? fieldInfo, bool fromDataAnnotation)
        => CanSetField(
            fieldInfo,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionNavigationBuilder.CanSetAutoInclude(bool? autoInclude, bool fromDataAnnotation)
        => CanSetAutoInclude(autoInclude, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionNavigationBuilder? IConventionNavigationBuilder.AutoInclude(bool? autoInclude, bool fromDataAnnotation)
        => AutoInclude(autoInclude, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionNavigationBuilder.CanSetLazyLoadingEnabled(bool? lazyLoadingEnabled, bool fromDataAnnotation)
        => CanSetLazyLoadingEnabled(
            lazyLoadingEnabled, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionNavigationBuilder? IConventionNavigationBuilder.EnableLazyLoading(bool? lazyLoadingEnabled, bool fromDataAnnotation)
        => EnableLazyLoading(lazyLoadingEnabled, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionNavigationBuilder.CanSetIsRequired(bool? required, bool fromDataAnnotation)
        => CanSetIsRequired(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionNavigationBuilder? IConventionNavigationBuilder.IsRequired(bool? required, bool fromDataAnnotation)
        => IsRequired(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
