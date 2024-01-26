// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class Navigation : PropertyBase, IMutableNavigation, IConventionNavigation, INavigation
{
    private InternalNavigationBuilder? _builder;

    // Warning: Never access these fields directly as access needs to be thread-safe
    private IClrCollectionAccessor? _collectionAccessor;
    private bool _collectionAccessorInitialized;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Navigation(
        string name,
        PropertyInfo? propertyInfo,
        FieldInfo? fieldInfo,
        ForeignKey foreignKey)
        : base(name, propertyInfo, fieldInfo, ConfigurationSource.Convention)
    {
        ForeignKey = foreignKey;

        _builder = new InternalNavigationBuilder(this, foreignKey.DeclaringEntityType.Model.Builder);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)]
    public override Type ClrType
        => this.GetIdentifyingMemberInfo()?.GetMemberType()
            ?? (((IReadOnlyNavigation)this).IsCollection
                ? typeof(IEnumerable<>).MakeGenericType(TargetEntityType.ClrType)
                : TargetEntityType.ClrType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object? Sentinel
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ForeignKey ForeignKey { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalNavigationBuilder Builder
    {
        [DebuggerStepThrough]
        get => _builder ?? throw new InvalidOperationException(CoreStrings.ObjectRemovedFromModel(Name));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsInModel
        => _builder is not null
            && ForeignKey.IsInModel;

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
    public virtual EntityType DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => (EntityType)((IReadOnlyNavigation)this).DeclaringEntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override TypeBase DeclaringType
    {
        [DebuggerStepThrough]
        get => (EntityType)((IReadOnlyNavigation)this).DeclaringEntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual EntityType TargetEntityType
    {
        [DebuggerStepThrough]
        get => (EntityType)((IReadOnlyNavigationBase)this).TargetEntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsOnDependent
    {
        [DebuggerStepThrough]
        get => ForeignKey.DependentToPrincipal == this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ConfigurationSource GetConfigurationSource()
        => (ConfigurationSource)(IsOnDependent
            ? ForeignKey.GetDependentToPrincipalConfigurationSource()
            : ForeignKey.GetPrincipalToDependentConfigurationSource())!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void UpdateConfigurationSource(ConfigurationSource configurationSource)
    {
        if (IsOnDependent)
        {
            ForeignKey.UpdateDependentToPrincipalConfigurationSource(configurationSource);
        }
        else
        {
            ForeignKey.UpdatePrincipalToDependentConfigurationSource(configurationSource);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override PropertyAccessMode GetPropertyAccessMode()
        => (PropertyAccessMode)(this[CoreAnnotationNames.PropertyAccessMode]
            ?? DeclaringEntityType.GetNavigationAccessMode());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsCompatible(
        string navigationName,
        MemberInfo navigationProperty,
        EntityType sourceType,
        EntityType targetType,
        bool? shouldBeCollection,
        bool shouldThrow)
    {
        if (!navigationProperty.DeclaringType!.IsAssignableFrom(sourceType.ClrType))
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    CoreStrings.NoClrNavigation(navigationName, sourceType.DisplayName()));
            }

            return false;
        }

        var targetClrType = targetType.ClrType;
        var navigationTargetClrType = navigationProperty.GetMemberType().TryGetSequenceType();
        shouldBeCollection ??= navigationTargetClrType != null && navigationProperty.GetMemberType() != targetClrType;
        if (shouldBeCollection.Value
            && navigationTargetClrType?.IsAssignableFrom(targetClrType) != true)
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    CoreStrings.NavigationCollectionWrongClrType(
                        navigationName,
                        sourceType.DisplayName(),
                        navigationProperty.GetMemberType().ShortDisplayName(),
                        targetClrType.ShortDisplayName()));
            }

            return false;
        }

        if (!shouldBeCollection.Value
            && !navigationProperty.GetMemberType().IsAssignableFrom(targetClrType))
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    CoreStrings.NavigationSingleWrongClrType(
                        navigationName,
                        sourceType.DisplayName(),
                        navigationProperty.GetMemberType().ShortDisplayName(),
                        targetClrType.ShortDisplayName()));
            }

            return false;
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Navigation? Inverse
    {
        [DebuggerStepThrough]
        get => (Navigation?)((IReadOnlyNavigationBase)this).Inverse;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Navigation? SetInverse(string? inverseName, ConfigurationSource configurationSource)
        => IsOnDependent
            ? ForeignKey.SetPrincipalToDependent(inverseName, configurationSource)
            : ForeignKey.SetDependentToPrincipal(inverseName, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Navigation? SetInverse(MemberInfo? inverse, ConfigurationSource configurationSource)
        => IsOnDependent
            ? ForeignKey.SetPrincipalToDependent(inverse, configurationSource)
            : ForeignKey.SetDependentToPrincipal(inverse, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetInverseConfigurationSource()
        => IsOnDependent
            ? ForeignKey.GetPrincipalToDependentConfigurationSource()
            : ForeignKey.GetDependentToPrincipalConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IClrCollectionAccessor? CollectionAccessor
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _collectionAccessor,
            ref _collectionAccessorInitialized,
            this,
            static navigation => ClrCollectionAccessorFactory.Instance.Create(navigation));

    /// <summary>
    ///     Runs the conventions when an annotation was set or removed.
    /// </summary>
    /// <param name="name">The key of the set annotation.</param>
    /// <param name="annotation">The annotation set.</param>
    /// <param name="oldAnnotation">The old annotation.</param>
    /// <returns>The annotation that was set.</returns>
    protected override IConventionAnnotation? OnAnnotationSet(
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
        => DeclaringType.Model.ConventionDispatcher.OnNavigationAnnotationChanged(
            ForeignKey.Builder, this, name, annotation, oldAnnotation);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IReadOnlyNavigation)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlyNavigation)this).ToDebugString(),
            () => ((IReadOnlyNavigation)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IReadOnlyForeignKey IReadOnlyNavigation.ForeignKey
    {
        [DebuggerStepThrough]
        get => ForeignKey;
    }

    [DebuggerStepThrough]
    IMutableNavigation? IMutableNavigation.SetInverse(string? inverseName)
        => SetInverse(inverseName, ConfigurationSource.Explicit);

    [DebuggerStepThrough]
    IMutableNavigation? IMutableNavigation.SetInverse(MemberInfo? inverse)
        => SetInverse(inverse, ConfigurationSource.Explicit);

    [DebuggerStepThrough]
    IConventionNavigation? IConventionNavigation.SetInverse(string? inverseName, bool fromDataAnnotation)
        => SetInverse(inverseName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    [DebuggerStepThrough]
    IConventionNavigation? IConventionNavigation.SetInverse(MemberInfo? inverse, bool fromDataAnnotation)
        => SetInverse(inverse, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    IConventionNavigationBuilder IConventionNavigation.Builder
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionAnnotatableBuilder IConventionAnnotatable.Builder
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IClrCollectionAccessor? INavigationBase.GetCollectionAccessor()
        => CollectionAccessor;
}
