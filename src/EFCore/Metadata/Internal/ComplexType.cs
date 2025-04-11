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
public class ComplexType : TypeBase, IMutableComplexType, IConventionComplexType, IRuntimeComplexType
{
    private InternalComplexTypeBuilder? _builder;

    private ConfigurationSource? _baseTypeConfigurationSource;

    // Warning: Never access these fields directly as access needs to be thread-safe
    private PropertyCounts? _counts;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ComplexType(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        ComplexProperty property,
        ConfigurationSource configurationSource)
        : base(name, type, property.DeclaringType.Model, configurationSource)
    {
        if (!type.IsValidComplexType())
        {
            throw new ArgumentException(CoreStrings.InvalidComplexType(type));
        }

        if (EntityType.DynamicProxyGenAssemblyName.Equals(
                type.Assembly.GetName().Name, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                CoreStrings.AddingProxyTypeAsEntityType(type.FullName));
        }

        ComplexProperty = property;
        _builder = new InternalComplexTypeBuilder(this, property.DeclaringType.Model.Builder);

        Model.AddComplexType(this);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual InternalComplexTypeBuilder Builder
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
    protected override InternalTypeBaseBuilder BaseBuilder
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
    public virtual ComplexProperty ComplexProperty { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual EntityType ContainingEntityType
        => ComplexProperty.DeclaringType switch
        {
            EntityType entityType => entityType,
            ComplexType declaringComplexType => declaringComplexType.ContainingEntityType,
            _ => throw new NotImplementedException()
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TypeBase ContainingType
        => ComplexProperty.DeclaringType switch
        {
            ComplexType declaringComplexType when !declaringComplexType.ComplexProperty.IsCollection => declaringComplexType.ContainingType,
            _ => ComplexProperty.DeclaringType
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsInModel
        => _builder is not null
            && ComplexProperty.IsInModel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetRemovedFromModel()
    {
        foreach (var complexProperty in GetDeclaredComplexProperties())
        {
            Check.DebugAssert(complexProperty.IsInModel, $"IsInModel is false for {complexProperty}");

            complexProperty.SetRemovedFromModel();
        }

        _builder = null;
        Model.RemoveComplexType(this);
        BaseType?.DirectlyDerivedTypes.Remove(this);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual ComplexType? BaseType
        => (ComplexType?)base.BaseType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override TypeBase? SetBaseType(TypeBase? newBaseType, ConfigurationSource configurationSource)
        => SetBaseType((ComplexType?)newBaseType, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ComplexType? SetBaseType(ComplexType? newBaseType, ConfigurationSource configurationSource)
    {
        EnsureMutable();
        Check.DebugAssert(IsInModel, "The entity type has been removed from the model");

        if (BaseType == newBaseType)
        {
            UpdateBaseTypeConfigurationSource(configurationSource);
            newBaseType?.UpdateConfigurationSource(configurationSource);
            return newBaseType;
        }

        if (BaseType == null)
        {
            throw new NotImplementedException();
        }

        var originalBaseType = BaseType;

        BaseType?.DirectlyDerivedTypes.Remove(this);
        base.BaseType = null;

        if (newBaseType != null)
        {
            if (!newBaseType.ClrType.IsAssignableFrom(ClrType))
            {
                throw new InvalidOperationException(
                    CoreStrings.NotAssignableClrBaseType(
                        DisplayName(), newBaseType.DisplayName(), ClrType.ShortDisplayName(),
                        newBaseType.ClrType.ShortDisplayName()));
            }

            if (newBaseType.InheritsFrom(this))
            {
                throw new InvalidOperationException(CoreStrings.CircularInheritance(DisplayName(), newBaseType.DisplayName()));
            }

            var conflictingMember = newBaseType.GetMembers()
                .Select(p => p.Name)
                .SelectMany(FindMembersInHierarchy)
                .FirstOrDefault();

            if (conflictingMember != null)
            {
                var baseProperty = newBaseType.FindMembersInHierarchy(conflictingMember.Name).Single();
                throw new InvalidOperationException(
                    CoreStrings.DuplicatePropertiesOnBase(
                        DisplayName(),
                        newBaseType.DisplayName(),
                        conflictingMember.DeclaringType.DisplayName(),
                        conflictingMember.Name,
                        baseProperty.DeclaringType.DisplayName(),
                        baseProperty.Name));
            }

            base.BaseType = newBaseType;
            newBaseType.DirectlyDerivedTypes.Add(this);
        }

        UpdateBaseTypeConfigurationSource(configurationSource);
        newBaseType?.UpdateConfigurationSource(configurationSource);

        return newBaseType;
        //return (ComplexType?)Model.ConventionDispatcher.OnComplexTypeBaseTypeChanged(Builder, newBaseType, originalBaseType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public override ConfigurationSource? GetBaseTypeConfigurationSource()
        => _baseTypeConfigurationSource;

    [DebuggerStepThrough]
    private void UpdateBaseTypeConfigurationSource(ConfigurationSource configurationSource)
        => _baseTypeConfigurationSource = configurationSource.Max(_baseTypeConfigurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public virtual IEnumerable<ComplexType> GetDirectlyDerivedTypes()
        => base.DirectlyDerivedTypes.Cast<ComplexType>();

    private bool InheritsFrom(ComplexType type)
    {
        var currentType = this;

        do
        {
            if (type == currentType)
            {
                return true;
            }
        }
        while ((currentType = currentType.BaseType) != null);

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsAssignableFrom(ComplexType derivedType)
    {
        Check.NotNull(derivedType, nameof(derivedType));

        if (derivedType == this)
        {
            return true;
        }

        if (!GetDirectlyDerivedTypes().Any())
        {
            return false;
        }

        var baseType = derivedType.BaseType;
        while (baseType != null)
        {
            if (baseType == this)
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public new virtual ComplexType GetRootType()
        => BaseType?.GetRootType() ?? this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private string DisplayName()
        => ((IReadOnlyComplexType)this).DisplayName();

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
        => Model.ConventionDispatcher.OnComplexTypeAnnotationChanged(Builder, name, annotation, oldAnnotation);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<PropertyBase> GetMembers()
        => GetProperties()
            .Concat<PropertyBase>(GetComplexProperties());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<PropertyBase> GetDeclaredMembers()
        => GetDeclaredProperties()
            .Concat<PropertyBase>(GetDeclaredComplexProperties());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override PropertyBase? FindMember(string name)
        => FindProperty(name)
            ?? ((PropertyBase?)FindComplexProperty(name));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<PropertyBase> FindMembersInHierarchy(string name)
        => FindPropertiesInHierarchy(name)
            .Concat<PropertyBase>(FindComplexPropertiesInHierarchy(name));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override PropertyCounts CalculateCounts()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _counts, this,
            // This will calculate the counts for all contained complex collections
            static complexType => complexType.ContainingEntityType.CalculateCounts());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetCounts(PropertyCounts value) => _counts = value!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string? OnTypeMemberIgnored(string name)
        => Model.ConventionDispatcher.OnComplexTypeMemberIgnored(Builder, name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlyComplexType)this).ToDebugString(),
            () => ((IReadOnlyComplexType)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IReadOnlyComplexType)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    #region Explicit interface implementations

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IReadOnlyComplexType? IReadOnlyComplexType.BaseType
    {
        [DebuggerStepThrough]
        get => BaseType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IMutableComplexType? IMutableComplexType.BaseType
    {
        get => BaseType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionComplexType? IConventionComplexType.BaseType
    {
        [DebuggerStepThrough]
        get => BaseType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IComplexType? IComplexType.BaseType
    {
        [DebuggerStepThrough]
        get => BaseType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionComplexTypeBuilder IConventionComplexType.Builder
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
    IReadOnlyModel IReadOnlyTypeBase.Model
    {
        [DebuggerStepThrough]
        get => Model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IMutableModel IMutableTypeBase.Model
    {
        [DebuggerStepThrough]
        get => Model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IModel ITypeBase.Model
    {
        [DebuggerStepThrough]
        get => Model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IComplexProperty IComplexType.ComplexProperty
    {
        [DebuggerStepThrough]
        get => ComplexProperty;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IReadOnlyComplexProperty IReadOnlyComplexType.ComplexProperty
    {
        [DebuggerStepThrough]
        get => ComplexProperty;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionComplexProperty IConventionComplexType.ComplexProperty
    {
        [DebuggerStepThrough]
        get => ComplexProperty;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IMutableComplexProperty IMutableComplexType.ComplexProperty
    {
        [DebuggerStepThrough]
        get => ComplexProperty;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IReadOnlyEntityType IReadOnlyTypeBase.ContainingEntityType
    {
        [DebuggerStepThrough]
        get => ContainingEntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IMutableEntityType IMutableTypeBase.ContainingEntityType
    {
        [DebuggerStepThrough]
        get => ContainingEntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionEntityType IConventionTypeBase.ContainingEntityType
    {
        [DebuggerStepThrough]
        get => ContainingEntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IEntityType ITypeBase.ContainingEntityType
    {
        [DebuggerStepThrough]
        get => ContainingEntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IReadOnlyTypeBase IReadOnlyTypeBase.ContainingType
    {
        [DebuggerStepThrough]
        get => ContainingType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IMutableTypeBase IMutableTypeBase.ContainingType
    {
        [DebuggerStepThrough]
        get => ContainingType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionTypeBase IConventionTypeBase.ContainingType
    {
        [DebuggerStepThrough]
        get => ContainingType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    ITypeBase ITypeBase.ContainingType
    {
        [DebuggerStepThrough]
        get => ContainingType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyComplexType> IReadOnlyComplexType.GetDerivedTypes()
        => GetDerivedTypes<ComplexType>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyComplexType> IReadOnlyComplexType.GetDirectlyDerivedTypes()
        => GetDirectlyDerivedTypes();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IComplexType> IComplexType.GetDirectlyDerivedTypes()
        => GetDirectlyDerivedTypes();

    #endregion
}
