// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a relationship where a foreign key composed of properties on the dependent entity type
///     references a corresponding primary or alternate key on the principal entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class RuntimeForeignKey : RuntimeAnnotatableBase, IRuntimeForeignKey
{
    private readonly DeleteBehavior _deleteBehavior;
    private readonly bool _isUnique;
    private readonly bool _isRequired;
    private readonly bool _isRequiredDependent;
    private readonly bool _isOwnership;

    private IDependentKeyValueFactory? _dependentKeyValueFactory;
    private Func<IDependentsMap>? _dependentsMapFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeForeignKey(
        IReadOnlyList<RuntimeProperty> dependentProperties,
        RuntimeKey principalKey,
        RuntimeEntityType dependentEntityType,
        RuntimeEntityType principalEntityType,
        DeleteBehavior deleteBehavior,
        bool unique,
        bool required,
        bool requiredDependent,
        bool ownership)
    {
        Properties = dependentProperties;
        PrincipalKey = principalKey;
        DeclaringEntityType = dependentEntityType;
        PrincipalEntityType = principalEntityType;
        _isRequired = required;
        _isRequiredDependent = requiredDependent;
        _deleteBehavior = deleteBehavior;
        _isUnique = unique;
        _isOwnership = ownership;
    }

    /// <summary>
    ///     Gets the foreign key properties in the dependent entity.
    /// </summary>
    public virtual IReadOnlyList<RuntimeProperty> Properties { get; }

    /// <summary>
    ///     Gets the primary or alternate key that the relationship targets.
    /// </summary>
    public virtual RuntimeKey PrincipalKey { get; }

    /// <summary>
    ///     Gets the dependent entity type. This may be different from the type that <see cref="Properties" />
    ///     are defined on when the relationship is defined a derived type in an inheritance hierarchy (since the properties
    ///     may be defined on a base type).
    /// </summary>
    public virtual RuntimeEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     Gets the principal entity type that this relationship targets. This may be different from the type that
    ///     <see cref="PrincipalKey" /> is defined on when the relationship targets a derived type in an inheritance
    ///     hierarchy (since the key is defined on the base type of the hierarchy).
    /// </summary>
    public virtual RuntimeEntityType PrincipalEntityType { get; }

    [DisallowNull]
    private RuntimeNavigation? DependentToPrincipal { get; set; }

    [DisallowNull]
    private RuntimeNavigation? PrincipalToDependent { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void AddNavigation(
        RuntimeNavigation navigation,
        bool onDependent)
    {
        if (onDependent)
        {
            DependentToPrincipal = navigation;
        }
        else
        {
            PrincipalToDependent = navigation;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual ISet<RuntimeSkipNavigation>? ReferencingSkipNavigations { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlyForeignKey)this).ToDebugString(),
            () => ((IReadOnlyForeignKey)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
        => ((IReadOnlyForeignKey)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <inheritdoc />
    IReadOnlyList<IReadOnlyProperty> IReadOnlyForeignKey.Properties
    {
        [DebuggerStepThrough]
        get => Properties;
    }

    /// <inheritdoc />
    IReadOnlyList<IProperty> IForeignKey.Properties
    {
        [DebuggerStepThrough]
        get => Properties;
    }

    /// <inheritdoc />
    IReadOnlyKey IReadOnlyForeignKey.PrincipalKey
    {
        [DebuggerStepThrough]
        get => PrincipalKey;
    }

    /// <inheritdoc />
    IKey IForeignKey.PrincipalKey
    {
        [DebuggerStepThrough]
        get => PrincipalKey;
    }

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyForeignKey.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <inheritdoc />
    IEntityType IForeignKey.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyForeignKey.PrincipalEntityType
    {
        [DebuggerStepThrough]
        get => PrincipalEntityType;
    }

    /// <inheritdoc />
    IEntityType IForeignKey.PrincipalEntityType
    {
        [DebuggerStepThrough]
        get => PrincipalEntityType;
    }

    /// <inheritdoc />
    IReadOnlyNavigation? IReadOnlyForeignKey.DependentToPrincipal
    {
        [DebuggerStepThrough]
        get => DependentToPrincipal;
    }

    /// <inheritdoc />
    INavigation? IForeignKey.DependentToPrincipal
    {
        [DebuggerStepThrough]
        get => DependentToPrincipal;
    }

    /// <inheritdoc />
    IReadOnlyNavigation? IReadOnlyForeignKey.PrincipalToDependent
    {
        [DebuggerStepThrough]
        get => PrincipalToDependent;
    }

    /// <inheritdoc />
    INavigation? IForeignKey.PrincipalToDependent
    {
        [DebuggerStepThrough]
        get => PrincipalToDependent;
    }

    /// <inheritdoc />
    bool IReadOnlyForeignKey.IsUnique
    {
        [DebuggerStepThrough]
        get => _isUnique;
    }

    /// <inheritdoc />
    bool IReadOnlyForeignKey.IsRequired
    {
        [DebuggerStepThrough]
        get => _isRequired;
    }

    /// <inheritdoc />
    bool IReadOnlyForeignKey.IsRequiredDependent
    {
        [DebuggerStepThrough]
        get => _isRequiredDependent;
    }

    /// <inheritdoc />
    DeleteBehavior IReadOnlyForeignKey.DeleteBehavior
    {
        [DebuggerStepThrough]
        get => _deleteBehavior;
    }

    /// <inheritdoc />
    bool IReadOnlyForeignKey.IsOwnership
    {
        [DebuggerStepThrough]
        get => _isOwnership;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlySkipNavigation> IReadOnlyForeignKey.GetReferencingSkipNavigations()
        => ReferencingSkipNavigations ?? Enumerable.Empty<RuntimeSkipNavigation>();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IDependentKeyValueFactory<TKey> IForeignKey.GetDependentKeyValueFactory<TKey>()
        => (IDependentKeyValueFactory<TKey>)_dependentKeyValueFactory!;

    /// <inheritdoc />
    [DebuggerStepThrough]
    IDependentKeyValueFactory IForeignKey.GetDependentKeyValueFactory()
        => _dependentKeyValueFactory!;

    // Note: This is set and used only by IdentityMapFactoryFactory, which ensures thread-safety
    /// <inheritdoc />
    IDependentKeyValueFactory IRuntimeForeignKey.DependentKeyValueFactory
    {
        [DebuggerStepThrough]
        get => _dependentKeyValueFactory!;

        [DebuggerStepThrough]
        set => _dependentKeyValueFactory = value;
    }

    // Note: This is set and used only by IdentityMapFactoryFactory, which ensures thread-safety
    /// <inheritdoc />
    Func<IDependentsMap> IRuntimeForeignKey.DependentsMapFactory
    {
        [DebuggerStepThrough]
        get => _dependentsMapFactory!;

        [DebuggerStepThrough]
        set => _dependentsMapFactory = value;
    }
}
