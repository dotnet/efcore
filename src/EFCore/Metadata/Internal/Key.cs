// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class Key : ConventionAnnotatable, IMutableKey, IConventionKey, IRuntimeKey
{
    private InternalKeyBuilder? _builder;
    private ConfigurationSource _configurationSource;

    // Warning: Never access these fields directly as access needs to be thread-safe
    private Func<bool, IIdentityMap>? _identityMapFactory;

    private object? _principalKeyValueFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Key(IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
    {
        Properties = properties;
        _configurationSource = configurationSource;

        _builder = new InternalKeyBuilder(this, DeclaringEntityType.Model.Builder);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<Property> Properties { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual EntityType DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => (EntityType)Properties[0].DeclaringType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalKeyBuilder Builder
    {
        [DebuggerStepThrough]
        get => _builder ?? throw new InvalidOperationException(CoreStrings.ObjectRemovedFromModel(
            Property.Format(Properties.Select(p => p.Name))));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsInModel
        => _builder is not null
            && DeclaringEntityType.IsInModel;

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
    public override bool IsReadOnly
        => DeclaringEntityType.Model.IsReadOnly;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public virtual ConfigurationSource GetConfigurationSource()
        => _configurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
    {
        _configurationSource = configurationSource.Max(_configurationSource);
        foreach (var property in Properties)
        {
            property.UpdateConfigurationSource(configurationSource);
        }
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
        => Builder.ModelBuilder.Metadata.ConventionDispatcher.OnKeyAnnotationChanged(Builder, name, annotation, oldAnnotation);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ForeignKey> GetReferencingForeignKeys()
        => ReferencingForeignKeys ?? Enumerable.Empty<ForeignKey>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<bool, IIdentityMap> IdentityMapFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _identityMapFactory, this, static key =>
            {
                key.EnsureReadOnly();
                return new IdentityMapFactoryFactory().Create(key);
            });

    private static readonly MethodInfo _createPrincipalKeyValueFactoryMethod = typeof(Key).GetTypeInfo()
        .GetDeclaredMethod(nameof(CreatePrincipalKeyValueFactory))!;

    [UsedImplicitly]
    private IPrincipalKeyValueFactory<TKey> CreatePrincipalKeyValueFactory<TKey>()
        where TKey : notnull
    {
        EnsureReadOnly();
        return new KeyValueFactoryFactory().Create<TKey>(this);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ISet<ForeignKey>? ReferencingForeignKeys { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IReadOnlyKey)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlyKey)this).ToDebugString(),
            () => ((IReadOnlyKey)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IReadOnlyList<IReadOnlyProperty> IReadOnlyKey.Properties
    {
        [DebuggerStepThrough]
        get => Properties;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IReadOnlyEntityType IReadOnlyKey.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IReadOnlyList<IMutableProperty> IMutableKey.Properties
    {
        [DebuggerStepThrough]
        get => Properties;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IReadOnlyList<IConventionProperty> IConventionKey.Properties
    {
        [DebuggerStepThrough]
        get => Properties;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IReadOnlyList<IProperty> IKey.Properties
    {
        [DebuggerStepThrough]
        get => Properties;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IMutableEntityType IMutableKey.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionEntityType IConventionKey.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IEntityType IKey.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionKeyBuilder IConventionKey.Builder
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
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyKey.GetReferencingForeignKeys()
        => ReferencingForeignKeys ?? Enumerable.Empty<IReadOnlyForeignKey>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    Func<bool, IIdentityMap> IRuntimeKey.GetIdentityMapFactory()
        => IdentityMapFactory;

    IPrincipalKeyValueFactory<TKey> IKey.GetPrincipalKeyValueFactory<TKey>()
        => (IPrincipalKeyValueFactory<TKey>)NonCapturingLazyInitializer.EnsureInitialized(
            ref _principalKeyValueFactory, this, static key => key.CreatePrincipalKeyValueFactory<TKey>());

    IPrincipalKeyValueFactory IKey.GetPrincipalKeyValueFactory()
        => (IPrincipalKeyValueFactory)NonCapturingLazyInitializer.EnsureInitialized(
            ref _principalKeyValueFactory, (IKey)this, static key => _createPrincipalKeyValueFactoryMethod
                .MakeGenericMethod(key.GetKeyType())
                .Invoke(key, [])!);
}
