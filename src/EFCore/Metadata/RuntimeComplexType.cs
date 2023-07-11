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
    private InstantiationBinding? _constructorBinding;
    private InstantiationBinding? _serviceOnlyConstructorBinding;

    // Warning: Never access these fields directly as access needs to be thread-safe
    private PropertyCounts? _counts;

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
        bool propertyBag)
        : base(name, type, complexProperty.DeclaringType.Model, null, changeTrackingStrategy, indexerPropertyInfo, propertyBag)
    {
        ComplexProperty = complexProperty;
        FundamentalEntityType = complexProperty.DeclaringType switch
        {
            RuntimeEntityType entityType => entityType,
            RuntimeComplexType declaringComplexType => declaringComplexType.FundamentalEntityType,
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
    private RuntimeEntityType FundamentalEntityType { get; }

    /// <summary>
    ///     Gets or sets the <see cref="InstantiationBinding" /> for the preferred constructor.
    /// </summary>
    public virtual InstantiationBinding? ConstructorBinding
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
    protected override PropertyCounts Counts
        => NonCapturingLazyInitializer.EnsureInitialized(ref _counts, this, static complexType => complexType.CalculateCounts());

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
        => ((IReadOnlyEntityType)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

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
    IReadOnlyEntityType IReadOnlyTypeBase.FundamentalEntityType
    {
        [DebuggerStepThrough]
        get => FundamentalEntityType;
    }

    /// <inheritdoc />
    IEntityType ITypeBase.FundamentalEntityType
    {
        [DebuggerStepThrough]
        get => FundamentalEntityType;
    }
}
