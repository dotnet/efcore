// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a property on an entity type that represents an
///     injected service from the <see cref="DbContext" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class RuntimeServiceProperty : RuntimePropertyBase, IServiceProperty
{
    private ServiceParameterBinding? _parameterBinding;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeServiceProperty(
        string name,
        PropertyInfo? propertyInfo,
        FieldInfo? fieldInfo,
        Type serviceType,
        RuntimeEntityType declaringEntityType,
        PropertyAccessMode propertyAccessMode)
        : base(name, propertyInfo, fieldInfo, propertyAccessMode)
    {
        Check.NotNull(declaringEntityType, nameof(declaringEntityType));

        DeclaringEntityType = declaringEntityType;
        ClrType = serviceType;
    }

    /// <summary>
    ///     Gets the type that this property-like object belongs to.
    /// </summary>
    public virtual RuntimeEntityType DeclaringEntityType { get; }

    /// <inheritdoc />
    public override RuntimeTypeBase DeclaringType
        => DeclaringEntityType;

    /// <summary>
    ///     Gets the type of value that this property-like object holds.
    /// </summary>
    [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)]
    protected override Type ClrType { get; }

    /// <summary>
    ///     The <see cref="ServiceParameterBinding" /> for this property.
    /// </summary>
    public virtual ServiceParameterBinding ParameterBinding
    {
        get => NonCapturingLazyInitializer.EnsureInitialized(
            ref _parameterBinding, (IServiceProperty)this, static property =>
            {
                var entityType = property.DeclaringEntityType;
                var factory = entityType.Model.GetModelDependencies().ParameterBindingFactories
                    .FindFactory(property.ClrType, property.Name)!;
                return (ServiceParameterBinding)factory.Bind(entityType, property.ClrType, property.Name);
            });

        [DebuggerStepThrough]
        set => _parameterBinding = value;
    }

    /// <inheritdoc />
    public override object? Sentinel
        => null;

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
        => ((IServiceProperty)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IServiceProperty)this).ToDebugString(),
            () => ((IServiceProperty)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyServiceProperty.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <inheritdoc />
    IEntityType IServiceProperty.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }
}
