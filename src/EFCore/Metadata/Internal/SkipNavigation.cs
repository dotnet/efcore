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
public class SkipNavigation : PropertyBase, IMutableSkipNavigation, IConventionSkipNavigation, IRuntimeSkipNavigation
{
    private ConfigurationSource? _foreignKeyConfigurationSource;
    private ConfigurationSource? _inverseConfigurationSource;
    private InternalSkipNavigationBuilder? _builder;
    private readonly Type _type;

    // Warning: Never access these fields directly as access needs to be thread-safe
    private IClrCollectionAccessor? _collectionAccessor;
    private bool _collectionAccessorInitialized;
    private ICollectionLoader? _manyToManyLoader;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SkipNavigation(
        string name,
        Type? navigationType,
        PropertyInfo? propertyInfo,
        FieldInfo? fieldInfo,
        EntityType declaringEntityType,
        EntityType targetEntityType,
        bool collection,
        bool onDependent,
        ConfigurationSource configurationSource)
        : base(name, propertyInfo, fieldInfo, configurationSource)
    {
        DeclaringEntityType = declaringEntityType;
        TargetEntityType = targetEntityType;
        IsCollection = collection;
        IsOnDependent = onDependent;
        _type = navigationType
            ?? this.GetIdentifyingMemberInfo()?.GetMemberType()
            ?? (IsCollection
                ? typeof(IEnumerable<>).MakeGenericType(TargetEntityType.ClrType)
                : TargetEntityType.ClrType);
        _builder = new InternalSkipNavigationBuilder(this, targetEntityType.Model.Builder);
    }

    private void ProcessForeignKey(ForeignKey foreignKey)
    {
        ForeignKey = foreignKey;

        if (foreignKey.ReferencingSkipNavigations == null)
        {
            foreignKey.ReferencingSkipNavigations = new SortedSet<SkipNavigation>(SkipNavigationComparer.Instance) { this };
        }
        else
        {
            foreignKey.ReferencingSkipNavigations.Add(this);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Type ClrType
        => _type;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalSkipNavigationBuilder Builder
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
    public virtual EntityType DeclaringEntityType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual EntityType TargetEntityType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override TypeBase DeclaringType
        => DeclaringEntityType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual EntityType? JoinEntityType
        => IsOnDependent ? ForeignKey?.PrincipalEntityType : ForeignKey?.DeclaringEntityType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ForeignKey? ForeignKey { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SkipNavigation? Inverse { get; private set; }

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
    public virtual bool IsOnDependent { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ForeignKey? SetForeignKey(ForeignKey? foreignKey, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        var oldForeignKey = ForeignKey;
        var isChanging = foreignKey != ForeignKey;

        if (oldForeignKey != null)
        {
            oldForeignKey.ReferencingSkipNavigations!.Remove(this);
        }

        if (foreignKey == null)
        {
            ForeignKey = null;
            _foreignKeyConfigurationSource = null;

            return isChanging
                ? (ForeignKey?)DeclaringEntityType.Model.ConventionDispatcher
                    .OnSkipNavigationForeignKeyChanged(Builder, foreignKey, oldForeignKey)
                : foreignKey;
        }

        var expectedEntityType = IsOnDependent ? foreignKey.DeclaringEntityType : foreignKey.PrincipalEntityType;
        if (expectedEntityType != DeclaringEntityType)
        {
            var message = IsOnDependent
                ? CoreStrings.SkipNavigationForeignKeyWrongDependentType(
                    foreignKey.Properties.Format(), DeclaringEntityType.DisplayName(), Name, expectedEntityType.DisplayName())
                : CoreStrings.SkipNavigationForeignKeyWrongPrincipalType(
                    foreignKey.Properties.Format(), DeclaringEntityType.DisplayName(), Name, expectedEntityType.DisplayName());
            throw new InvalidOperationException(message);
        }

        ProcessForeignKey(foreignKey);
        UpdateForeignKeyConfigurationSource(configurationSource);

        if (Inverse?.JoinEntityType != null
            && Inverse.JoinEntityType != JoinEntityType)
        {
            throw new InvalidOperationException(
                CoreStrings.SkipInverseMismatchedForeignKey(
                    foreignKey.Properties.Format(),
                    Name, JoinEntityType!.DisplayName(),
                    Inverse.Name, Inverse.JoinEntityType.DisplayName()));
        }

        return isChanging
            ? (ForeignKey?)DeclaringEntityType.Model.ConventionDispatcher
                .OnSkipNavigationForeignKeyChanged(Builder, foreignKey, oldForeignKey!)
            : foreignKey;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetForeignKeyConfigurationSource()
        => _foreignKeyConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void UpdateForeignKeyConfigurationSource(ConfigurationSource configurationSource)
        => _foreignKeyConfigurationSource = _foreignKeyConfigurationSource.Max(configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SkipNavigation? SetInverse(SkipNavigation? inverse, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        var oldInverse = Inverse;
        var isChanging = inverse != Inverse;
        if (inverse == null)
        {
            Inverse = null;
            _inverseConfigurationSource = null;

            return isChanging
                ? (SkipNavigation?)DeclaringEntityType.Model.ConventionDispatcher
                    .OnSkipNavigationInverseChanged(Builder, inverse!, oldInverse!)
                : inverse;
        }

        if (inverse.DeclaringEntityType != TargetEntityType)
        {
            throw new InvalidOperationException(
                CoreStrings.SkipNavigationWrongInverse(
                    inverse.Name, inverse.DeclaringEntityType.DisplayName(), Name, TargetEntityType.DisplayName()));
        }

        if (inverse.JoinEntityType != null
            && JoinEntityType != null
            && inverse.JoinEntityType != JoinEntityType)
        {
            throw new InvalidOperationException(
                CoreStrings.SkipInverseMismatchedJoinType(
                    inverse.Name, inverse.JoinEntityType.DisplayName(), Name, JoinEntityType.DisplayName()));
        }

        Inverse = inverse;
        UpdateInverseConfigurationSource(configurationSource);

        return isChanging
            ? (SkipNavigation?)DeclaringEntityType.Model.ConventionDispatcher
                .OnSkipNavigationInverseChanged(Builder, inverse, oldInverse!)
            : inverse;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetInverseConfigurationSource()
        => _inverseConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void UpdateInverseConfigurationSource(ConfigurationSource configurationSource)
        => _inverseConfigurationSource = _inverseConfigurationSource.Max(configurationSource);

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
    ///     Gets the sentinel value that indicates that this property is not set.
    /// </summary>
    public virtual object? Sentinel
        => null;

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
        => DeclaringType.Model.ConventionDispatcher.OnSkipNavigationAnnotationChanged(
            Builder, name, annotation, oldAnnotation);

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
            static navigation =>
            {
                navigation.EnsureReadOnly();
                return ClrCollectionAccessorFactory.Instance.Create(navigation);
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ICollectionLoader ManyToManyLoader
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _manyToManyLoader, this, static navigation =>
            {
                navigation.EnsureReadOnly();
                return ManyToManyLoaderFactory.Instance.Create(navigation);
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public override string ToString()
        => ((IReadOnlySkipNavigation)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlySkipNavigation)this).ToDebugString(),
            () => ((IReadOnlySkipNavigation)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IConventionSkipNavigationBuilder IConventionSkipNavigation.Builder
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <inheritdoc />
    IConventionAnnotatableBuilder IConventionAnnotatable.Builder
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyNavigationBase.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyNavigationBase.TargetEntityType
    {
        [DebuggerStepThrough]
        get => TargetEntityType;
    }

    /// <inheritdoc />
    IReadOnlyForeignKey IReadOnlySkipNavigation.ForeignKey
    {
        // ModelValidator makes sure ForeignKey isn't null, so we expose it as non-nullable.
        [DebuggerStepThrough]
        get => ForeignKey!;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    void IMutableSkipNavigation.SetForeignKey(IMutableForeignKey? foreignKey)
        => SetForeignKey((ForeignKey?)foreignKey, ConfigurationSource.Explicit);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionForeignKey? IConventionSkipNavigation.SetForeignKey(IConventionForeignKey? foreignKey, bool fromDataAnnotation)
        => SetForeignKey(
            (ForeignKey?)foreignKey, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    IReadOnlySkipNavigation IReadOnlySkipNavigation.Inverse
    {
        // ModelValidator makes sure ForeignKey isn't null, so we expose it as non-nullable.
        [DebuggerStepThrough]
        get => Inverse!;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IMutableSkipNavigation? IMutableSkipNavigation.SetInverse(IMutableSkipNavigation? inverse)
        => SetInverse((SkipNavigation?)inverse, ConfigurationSource.Explicit);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSkipNavigation? IConventionSkipNavigation.SetInverse(
        IConventionSkipNavigation? inverse,
        bool fromDataAnnotation)
        => SetInverse(
            (SkipNavigation?)inverse, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IClrCollectionAccessor? INavigationBase.GetCollectionAccessor()
        => CollectionAccessor;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    ICollectionLoader IRuntimeSkipNavigation.GetManyToManyLoader()
        => ManyToManyLoader;
}
