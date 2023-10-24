// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents the type of a complex property of a structural type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class RuntimeComplexType : RuntimeTypeBase, IRuntimeComplexType
{
    // Warning: Never access these fields directly as access needs to be thread-safe
    private InstantiationBinding? _constructorBinding;
    private InstantiationBinding? _serviceOnlyConstructorBinding;
    private RuntimePropertyBase[]? _snapshottableProperties;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeComplexType(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        RuntimeComplexProperty complexProperty,
        ChangeTrackingStrategy changeTrackingStrategy,
        PropertyInfo? indexerPropertyInfo,
        bool propertyBag,
        int propertyCount,
        int complexPropertyCount)
        : base(name, type, complexProperty.DeclaringType.Model, null, changeTrackingStrategy, indexerPropertyInfo, propertyBag,
            derivedTypesCount: 0,
            propertyCount: propertyCount,
            complexPropertyCount: complexPropertyCount)
    {
        ComplexProperty = complexProperty;
        ContainingEntityType = complexProperty.DeclaringType switch
        {
            RuntimeEntityType entityType => entityType,
            RuntimeComplexType declaringComplexType => declaringComplexType.ContainingEntityType,
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RuntimeComplexProperty ComplexProperty { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private RuntimeEntityType ContainingEntityType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<RuntimePropertyBase> GetMembers()
        => GetProperties()
            .Concat<RuntimePropertyBase>(GetComplexProperties());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<RuntimePropertyBase> GetDeclaredMembers()
        => GetDeclaredProperties()
            .Concat<RuntimePropertyBase>(GetDeclaredComplexProperties());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override RuntimePropertyBase? FindMember(string name)
        => FindProperty(name)
            ?? ((RuntimePropertyBase?)FindComplexProperty(name));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<RuntimePropertyBase> FindMembersInHierarchy(string name)
        => FindPropertiesInHierarchy(name)
            .Concat<RuntimePropertyBase>(FindComplexPropertiesInHierarchy(name));

    /// <summary>
    ///     Gets or sets the <see cref="InstantiationBinding" /> for the preferred constructor.
    /// </summary>
    public override InstantiationBinding? ConstructorBinding
    {
        get => !ClrType.IsAbstract
            ? NonCapturingLazyInitializer.EnsureInitialized(
                ref _constructorBinding, this, static complexType =>
                {
                    ((IModel)complexType.Model).GetModelDependencies().ConstructorBindingFactory.GetBindings(
                        complexType,
                        out complexType._constructorBinding,
                        out complexType._serviceOnlyConstructorBinding);
                })
            : _constructorBinding;

        [DebuggerStepThrough]
        set => _constructorBinding = value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual InstantiationBinding? ServiceOnlyConstructorBinding
    {
        [DebuggerStepThrough]
        get => _serviceOnlyConstructorBinding;

        [DebuggerStepThrough]
        set => _serviceOnlyConstructorBinding = value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<RuntimePropertyBase> GetSnapshottableMembers()
    {
        return NonCapturingLazyInitializer.EnsureInitialized(
            ref _snapshottableProperties, this,
            static type => Create(type).ToArray());

        static IEnumerable<RuntimePropertyBase> Create(RuntimeComplexType type)
        {
            foreach (var property in type.GetProperties())
            {
                yield return property;
            }

            foreach (var complexProperty in type.GetComplexProperties())
            {
                yield return complexProperty;

                foreach (var propertyBase in complexProperty.ComplexType.GetSnapshottableMembers())
                {
                    yield return propertyBase;
                }
            }
        }
    }

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
        => ((IReadOnlyComplexType)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlyComplexType)this).ToDebugString(),
            () => ((IReadOnlyComplexType)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    bool IReadOnlyTypeBase.HasSharedClrType
    {
        [DebuggerStepThrough]
        get => true;
    }

    /// <inheritdoc />
    IReadOnlyModel IReadOnlyTypeBase.Model
    {
        [DebuggerStepThrough]
        get => Model;
    }

    /// <inheritdoc />
    IModel ITypeBase.Model
    {
        [DebuggerStepThrough]
        get => Model;
    }

    /// <inheritdoc />
    IReadOnlyComplexProperty IReadOnlyComplexType.ComplexProperty
    {
        [DebuggerStepThrough]
        get => ComplexProperty;
    }

    /// <inheritdoc />
    IComplexProperty IComplexType.ComplexProperty
    {
        [DebuggerStepThrough]
        get => ComplexProperty;
    }

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyTypeBase.ContainingEntityType
    {
        [DebuggerStepThrough]
        get => ContainingEntityType;
    }

    /// <inheritdoc />
    IEntityType ITypeBase.ContainingEntityType
    {
        [DebuggerStepThrough]
        get => ContainingEntityType;
    }
}
