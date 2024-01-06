// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a primary or alternate key on an entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class RuntimeKey : RuntimeAnnotatableBase, IRuntimeKey
{
    // Warning: Never access these fields directly as access needs to be thread-safe
    private Func<bool, IIdentityMap>? _identityMapFactory;
    private object? _principalKeyValueFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeKey(IReadOnlyList<RuntimeProperty> properties)
    {
        Properties = properties;
    }

    /// <summary>
    ///     Gets the properties that make up the key.
    /// </summary>
    public virtual IReadOnlyList<RuntimeProperty> Properties { get; }

    /// <summary>
    ///     Gets the entity type the key is defined on. This may be different from the type that <see cref="IKey.Properties" />
    ///     are defined on when the key is defined a derived type in an inheritance hierarchy (since the properties
    ///     may be defined on a base type).
    /// </summary>
    public virtual RuntimeEntityType DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => (RuntimeEntityType)Properties[0].DeclaringType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual ISet<RuntimeForeignKey>? ReferencingForeignKeys { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetPrincipalKeyValueFactory<TKey>(IPrincipalKeyValueFactory<TKey> factory)
        => _principalKeyValueFactory = factory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetIdentityMapFactory(Func<bool, IIdentityMap> factory)
        => _identityMapFactory = factory;

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
        => ((IReadOnlyKey)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlyKey)this).ToDebugString(),
            () => ((IReadOnlyKey)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IReadOnlyList<IReadOnlyProperty> IReadOnlyKey.Properties
    {
        [DebuggerStepThrough]
        get => Properties;
    }

    /// <inheritdoc />
    IReadOnlyList<IProperty> IKey.Properties
    {
        [DebuggerStepThrough]
        get => Properties;
    }

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyKey.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <inheritdoc />
    IEntityType IKey.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyKey.GetReferencingForeignKeys()
        => ReferencingForeignKeys ?? Enumerable.Empty<IReadOnlyForeignKey>();

    /// <inheritdoc />
    IPrincipalKeyValueFactory<TKey> IKey.GetPrincipalKeyValueFactory<TKey>()
        => (IPrincipalKeyValueFactory<TKey>)NonCapturingLazyInitializer.EnsureInitialized(
            ref _principalKeyValueFactory, this, static key => key.CreatePrincipalKeyValueFactory<TKey>());

    /// <inheritdoc />
    IPrincipalKeyValueFactory IKey.GetPrincipalKeyValueFactory()
        => (IPrincipalKeyValueFactory)NonCapturingLazyInitializer.EnsureInitialized(
            ref _principalKeyValueFactory, (IKey)this, static key => _createPrincipalKeyValueFactoryMethod
                .MakeGenericMethod(key.GetKeyType())
                .Invoke(key, [])!);

    private static readonly MethodInfo _createPrincipalKeyValueFactoryMethod = typeof(Key).GetTypeInfo()
        .GetDeclaredMethod(nameof(CreatePrincipalKeyValueFactory))!;

    private IPrincipalKeyValueFactory<TKey> CreatePrincipalKeyValueFactory<TKey>()
        where TKey : notnull => new KeyValueFactoryFactory().Create<TKey>(this);

    /// <inheritdoc />
    Func<bool, IIdentityMap> IRuntimeKey.GetIdentityMapFactory()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _identityMapFactory, this, static key => new IdentityMapFactoryFactory().Create(key));
}
