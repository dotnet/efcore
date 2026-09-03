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
public class ComplexProperty : PropertyBase, IMutableComplexProperty, IConventionComplexProperty, IComplexProperty
{
    private InternalComplexPropertyBuilder? _builder;
    private bool? _isNullable;

    private ConfigurationSource? _isNullableConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ComplexProperty(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        PropertyInfo? propertyInfo,
        FieldInfo? fieldInfo,
        TypeBase declaringType,
        string? targetTypeName,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type targetType,
        bool collection,
        ConfigurationSource configurationSource)
        : base(name, propertyInfo, fieldInfo, configurationSource)
    {
        ClrType = type;
        DeclaringType = declaringType;
        IsCollection = collection;
        ComplexType = new ComplexType(
            targetTypeName ?? declaringType.GetOwnedName(targetType.ShortDisplayName(), name),
            targetType, this, configurationSource);
        _builder = new InternalComplexPropertyBuilder(this, declaringType.Model.Builder);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalComplexPropertyBuilder Builder
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
    public virtual ComplexType ComplexType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsCollection { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsInModel
        => _builder is not null
            && DeclaringType.IsInModel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetRemovedFromModel()
    {
        _builder = null;
        ComplexType.SetRemovedFromModel();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsNullable
    {
        get => _isNullable ?? DefaultIsNullable;
        set => SetIsNullable(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? SetIsNullable(bool? nullable, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        var isChanging = (nullable ?? DefaultIsNullable) != IsNullable;
        if (nullable == null)
        {
            _isNullable = null;
            _isNullableConfigurationSource = null;
            if (isChanging)
            {
                OnPropertyNullableChanged();
            }

            return nullable;
        }

        if (nullable.Value)
        {
            if (!ClrType.IsNullableType())
            {
                throw new InvalidOperationException(
                    CoreStrings.CannotBeNullable(Name, DeclaringType.DisplayName(), ClrType.ShortDisplayName()));
            }
        }

        _isNullableConfigurationSource = configurationSource.Max(_isNullableConfigurationSource);

        _isNullable = nullable;

        return isChanging
            ? OnPropertyNullableChanged()
            : nullable;
    }

    private bool DefaultIsNullable
        => ClrType.IsNullableType();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetIsNullableConfigurationSource()
        => _isNullableConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual bool? OnPropertyNullableChanged()
        => DeclaringType.Model.ConventionDispatcher.OnComplexPropertyNullabilityChanged(Builder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override FieldInfo? OnFieldInfoSet(FieldInfo? newFieldInfo, FieldInfo? oldFieldInfo)
        => DeclaringType.Model.ConventionDispatcher.OnComplexPropertyFieldChanged(Builder, newFieldInfo, oldFieldInfo);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override TypeBase DeclaringType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)]
    public override Type ClrType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsCompatible(
        string propertyName,
        MemberInfo memberInfo,
        TypeBase sourceType,
        Type targetType,
        bool shouldBeCollection,
        bool shouldThrow)
    {
        var memberClrType = memberInfo.GetMemberType().TryGetSequenceType();
        if (shouldBeCollection
            && memberClrType?.IsAssignableFrom(targetType) != true)
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    CoreStrings.ComplexCollectionWrongClrType(
                        propertyName,
                        sourceType.DisplayName(),
                        memberInfo.GetMemberType().ShortDisplayName(),
                        targetType.ShortDisplayName()));
            }

            return false;
        }

        if (!shouldBeCollection
            && !memberInfo.GetMemberType().IsAssignableFrom(targetType))
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    CoreStrings.ComplexPropertyWrongClrType(
                        propertyName,
                        sourceType.DisplayName(),
                        memberInfo.GetMemberType().ShortDisplayName(),
                        targetType.ShortDisplayName()));
            }

            return false;
        }

        return true;
    }

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
        => DeclaringType.Model.ConventionDispatcher.OnComplexPropertyAnnotationChanged(Builder, name, annotation, oldAnnotation);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlyComplexProperty)this).ToDebugString(),
            () => ((IReadOnlyComplexProperty)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IReadOnlyComplexProperty)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionComplexPropertyBuilder IConventionComplexProperty.Builder
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
    IReadOnlyComplexType IReadOnlyComplexProperty.ComplexType
    {
        [DebuggerStepThrough]
        get => ComplexType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IMutableComplexType IMutableComplexProperty.ComplexType
    {
        [DebuggerStepThrough]
        get => ComplexType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionComplexType IConventionComplexProperty.ComplexType
    {
        [DebuggerStepThrough]
        get => ComplexType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IComplexType IComplexProperty.ComplexType
    {
        [DebuggerStepThrough]
        get => ComplexType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool? IConventionComplexProperty.SetIsNullable(bool? nullable, bool fromDataAnnotation)
        => SetIsNullable(
            nullable, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
