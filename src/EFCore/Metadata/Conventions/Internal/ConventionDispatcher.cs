// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public partial class ConventionDispatcher
{
    private ConventionScope _scope;
    private readonly ImmediateConventionScope _immediateConventionScope;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ConventionDispatcher(ConventionSet conventionSet)
    {
        _immediateConventionScope = new ImmediateConventionScope(conventionSet, this);
        _scope = _immediateConventionScope;
        Tracker = new MetadataTracker();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MetadataTracker Tracker { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionModelBuilder OnModelInitialized(IConventionModelBuilder modelBuilder)
        => _immediateConventionScope.OnModelInitialized(modelBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionModelBuilder OnModelFinalizing(IConventionModelBuilder modelBuilder)
        => _immediateConventionScope.OnModelFinalizing(modelBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionAnnotation? OnModelAnnotationChanged(
        IConventionModelBuilder modelBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
    {
        if (CoreAnnotationNames.AllNames.Contains(name))
        {
            return annotation;
        }

        return _scope.OnModelAnnotationChanged(
            modelBuilder,
            name,
            annotation,
            oldAnnotation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? OnTypeIgnored(
        IConventionModelBuilder modelBuilder,
        string name,
        Type? type)
        => _scope.OnTypeIgnored(modelBuilder, name, type);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionEntityTypeBuilder? OnEntityTypeAdded(IConventionEntityTypeBuilder entityTypeBuilder)
        => _scope.OnEntityTypeAdded(entityTypeBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionEntityType? OnEntityTypeRemoved(
        IConventionModelBuilder modelBuilder,
        IConventionEntityType type)
        => _scope.OnEntityTypeRemoved(modelBuilder, type);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? OnEntityTypeMemberIgnored(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name)
        => _scope.OnEntityTypeMemberIgnored(entityTypeBuilder, name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? OnDiscriminatorPropertySet(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string? name)
        => _scope.OnDiscriminatorPropertySet(entityTypeBuilder, name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionEntityType? OnEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? previousBaseType)
        => _scope.OnEntityTypeBaseTypeChanged(entityTypeBuilder, newBaseType, previousBaseType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionAnnotation? OnEntityTypeAnnotationChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
    {
        if (CoreAnnotationNames.AllNames.Contains(name))
        {
            return annotation;
        }

        return _scope.OnEntityTypeAnnotationChanged(
            entityTypeBuilder,
            name,
            annotation,
            oldAnnotation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? OnComplexTypeMemberIgnored(
        IConventionComplexTypeBuilder propertyBuilder,
        string name)
        => _scope.OnComplexTypeMemberIgnored(propertyBuilder, name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionComplexPropertyBuilder? OnComplexPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder)
        => _scope.OnComplexPropertyAdded(propertyBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionAnnotation? OnComplexTypeAnnotationChanged(
        IConventionComplexTypeBuilder complexTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
    {
        if (CoreAnnotationNames.AllNames.Contains(name))
        {
            return annotation;
        }

        return _scope.OnComplexTypeAnnotationChanged(
            complexTypeBuilder,
            name,
            annotation,
            oldAnnotation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionComplexProperty? OnComplexPropertyRemoved(
        IConventionTypeBaseBuilder typeBaseBuilder,
        IConventionComplexProperty property)
        => _scope.OnComplexPropertyRemoved(typeBaseBuilder, property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual FieldInfo? OnComplexPropertyFieldChanged(
        IConventionComplexPropertyBuilder propertyBuilder,
        FieldInfo? newFieldInfo,
        FieldInfo? oldFieldInfo)
        => _scope.OnComplexPropertyFieldChanged(propertyBuilder, newFieldInfo, oldFieldInfo);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? OnComplexPropertyNullabilityChanged(
        IConventionComplexPropertyBuilder propertyBuilder)
        => _scope.OnComplexPropertyNullabilityChanged(propertyBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionAnnotation? OnComplexPropertyAnnotationChanged(
        IConventionComplexPropertyBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
    {
        if (CoreAnnotationNames.AllNames.Contains(name))
        {
            return annotation;
        }

        return _scope.OnComplexPropertyAnnotationChanged(
            propertyBuilder,
            name,
            annotation,
            oldAnnotation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionForeignKeyBuilder? OnForeignKeyAdded(IConventionForeignKeyBuilder relationshipBuilder)
        => _scope.OnForeignKeyAdded(relationshipBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionForeignKey? OnForeignKeyRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionForeignKey foreignKey)
        => _scope.OnForeignKeyRemoved(entityTypeBuilder, foreignKey);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<IConventionProperty>? OnForeignKeyPropertiesChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IReadOnlyList<IConventionProperty> oldDependentProperties,
        IConventionKey oldPrincipalKey)
        => _scope.OnForeignKeyPropertiesChanged(
            relationshipBuilder,
            oldDependentProperties,
            oldPrincipalKey);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? OnForeignKeyUniquenessChanged(
        IConventionForeignKeyBuilder relationshipBuilder)
        => _scope.OnForeignKeyUniquenessChanged(relationshipBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? OnForeignKeyRequirednessChanged(
        IConventionForeignKeyBuilder relationshipBuilder)
        => _scope.OnForeignKeyRequirednessChanged(relationshipBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? OnForeignKeyDependentRequirednessChanged(
        IConventionForeignKeyBuilder relationshipBuilder)
        => _scope.OnForeignKeyDependentRequirednessChanged(relationshipBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? OnForeignKeyOwnershipChanged(
        IConventionForeignKeyBuilder relationshipBuilder)
        => _scope.OnForeignKeyOwnershipChanged(relationshipBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionForeignKeyBuilder? OnForeignKeyPrincipalEndChanged(
        IConventionForeignKeyBuilder relationshipBuilder)
        => _scope.OnForeignKeyPrincipalEndChanged(relationshipBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionAnnotation? OnForeignKeyAnnotationChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
    {
        if (CoreAnnotationNames.AllNames.Contains(name))
        {
            return annotation;
        }

        return _scope.OnForeignKeyAnnotationChanged(
            relationshipBuilder,
            name,
            annotation,
            oldAnnotation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionNavigation? OnForeignKeyNullNavigationSet(
        IConventionForeignKeyBuilder relationshipBuilder,
        bool pointsToPrincipal)
        => _scope.OnForeignKeyNullNavigationSet(relationshipBuilder, pointsToPrincipal);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionNavigationBuilder? OnNavigationAdded(IConventionNavigationBuilder navigationBuilder)
        => _scope.OnNavigationAdded(navigationBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? OnNavigationRemoved(
        IConventionEntityTypeBuilder sourceEntityTypeBuilder,
        IConventionEntityTypeBuilder targetEntityTypeBuilder,
        string navigationName,
        MemberInfo? memberInfo)
        => _scope.OnNavigationRemoved(
            sourceEntityTypeBuilder,
            targetEntityTypeBuilder,
            navigationName,
            memberInfo);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionAnnotation? OnNavigationAnnotationChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionNavigation navigation,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
    {
        if (CoreAnnotationNames.AllNames.Contains(name))
        {
            return annotation;
        }

        return _scope.OnNavigationAnnotationChanged(
            relationshipBuilder,
            navigation,
            name,
            annotation,
            oldAnnotation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionSkipNavigationBuilder? OnSkipNavigationAdded(
        IConventionSkipNavigationBuilder navigationBuilder)
        => _scope.OnSkipNavigationAdded(navigationBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionForeignKey? OnSkipNavigationForeignKeyChanged(
        IConventionSkipNavigationBuilder navigationBuilder,
        IConventionForeignKey? foreignKey,
        IConventionForeignKey? oldForeignKey)
        => _scope.OnSkipNavigationForeignKeyChanged(navigationBuilder, foreignKey, oldForeignKey);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionSkipNavigation? OnSkipNavigationInverseChanged(
        IConventionSkipNavigationBuilder navigationBuilder,
        IConventionSkipNavigation? inverse,
        IConventionSkipNavigation? oldInverse)
        => _scope.OnSkipNavigationInverseChanged(navigationBuilder, inverse, oldInverse);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionSkipNavigation? OnSkipNavigationRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionSkipNavigation navigation)
        => _scope.OnSkipNavigationRemoved(entityTypeBuilder, navigation);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionAnnotation? OnSkipNavigationAnnotationChanged(
        IConventionSkipNavigationBuilder navigationBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
    {
        if (CoreAnnotationNames.AllNames.Contains(name))
        {
            return annotation;
        }

        return _scope.OnSkipNavigationAnnotationChanged(
            navigationBuilder,
            name,
            annotation,
            oldAnnotation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionTriggerBuilder? OnTriggerAdded(
        IConventionTriggerBuilder triggerBuilder)
        => _scope.OnTriggerAdded(triggerBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionTrigger? OnTriggerRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionTrigger trigger)
        => _scope.OnTriggerRemoved(entityTypeBuilder, trigger);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionKeyBuilder? OnKeyAdded(IConventionKeyBuilder keyBuilder)
        => _scope.OnKeyAdded(keyBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionKey? OnKeyRemoved(IConventionEntityTypeBuilder entityTypeBuilder, IConventionKey key)
        => _scope.OnKeyRemoved(entityTypeBuilder, key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionAnnotation? OnKeyAnnotationChanged(
        IConventionKeyBuilder keyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
    {
        if (CoreAnnotationNames.AllNames.Contains(name))
        {
            return annotation;
        }

        return _scope.OnKeyAnnotationChanged(
            keyBuilder,
            name,
            annotation,
            oldAnnotation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionKey? OnPrimaryKeyChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionKey? newPrimaryKey,
        IConventionKey? previousPrimaryKey)
        => _scope.OnEntityTypePrimaryKeyChanged(entityTypeBuilder, newPrimaryKey, previousPrimaryKey);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionIndexBuilder? OnIndexAdded(IConventionIndexBuilder indexBuilder)
        => _scope.OnIndexAdded(indexBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionIndex? OnIndexRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionIndex index)
        => _scope.OnIndexRemoved(entityTypeBuilder, index);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? OnIndexUniquenessChanged(IConventionIndexBuilder indexBuilder)
        => _scope.OnIndexUniquenessChanged(indexBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<bool>? OnIndexSortOrderChanged(IConventionIndexBuilder indexBuilder)
        => _scope.OnIndexSortOrderChanged(indexBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionAnnotation? OnIndexAnnotationChanged(
        IConventionIndexBuilder indexBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
    {
        if (CoreAnnotationNames.AllNames.Contains(name))
        {
            return annotation;
        }

        return _scope.OnIndexAnnotationChanged(
            indexBuilder,
            name,
            annotation,
            oldAnnotation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionPropertyBuilder? OnPropertyAdded(IConventionPropertyBuilder propertyBuilder)
        => _scope.OnPropertyAdded(propertyBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionProperty? OnPropertyRemoved(
        IConventionTypeBaseBuilder typeBaseBuilder,
        IConventionProperty property)
        => _scope.OnPropertyRemoved(typeBaseBuilder, property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? OnPropertyNullabilityChanged(IConventionPropertyBuilder propertyBuilder)
        => _scope.OnPropertyNullabilityChanged(propertyBuilder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? OnElementTypeNullabilityChanged(IConventionElementTypeBuilder builder)
        => _scope.OnElementTypeNullabilityChanged(builder);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual FieldInfo? OnPropertyFieldChanged(
        IConventionPropertyBuilder propertyBuilder,
        FieldInfo? newFieldInfo,
        FieldInfo? oldFieldInfo)
        => _scope.OnPropertyFieldChanged(propertyBuilder, newFieldInfo, oldFieldInfo);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IElementType? OnPropertyElementTypeChanged(
        IConventionPropertyBuilder propertyBuilder,
        IElementType? newElementType,
        IElementType? oldElementType)
        => _scope.OnPropertyElementTypeChanged(propertyBuilder, newElementType, oldElementType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionAnnotation? OnPropertyAnnotationChanged(
        IConventionPropertyBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
    {
        if (CoreAnnotationNames.AllNames.Contains(name))
        {
            return annotation;
        }

        return _scope.OnPropertyAnnotationChanged(
            propertyBuilder,
            name,
            annotation,
            oldAnnotation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionAnnotation? OnElementTypeAnnotationChanged(
        IConventionElementTypeBuilder builder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
        => CoreAnnotationNames.AllNames.Contains(name)
            ? annotation
            : _scope.OnElementTypeAnnotationChanged(
                builder,
                name,
                annotation,
                oldAnnotation);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionBatch DelayConventions()
        => new ConventionBatch(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual T Track<T>(Func<T> func, [DisallowNull] ref IConventionForeignKey? foreignKey)
    {
        var batch = DelayConventions();
        using var foreignKeyReference = Tracker.Track(foreignKey);
        var result = func();
        batch.Dispose();
        foreignKey = foreignKeyReference.Object is null || !foreignKeyReference.Object.IsInModel
            ? null
            : foreignKeyReference.Object;
        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [Conditional("DEBUG")]
    public virtual void AssertNoScope()
        => Check.DebugAssert(_scope == _immediateConventionScope, "Expected no active convention scopes");

    private sealed class ConventionBatch : IConventionBatch
    {
        private readonly ConventionDispatcher _dispatcher;
        private int? _runCount;

        public ConventionBatch(ConventionDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            if (_dispatcher._scope == _dispatcher._immediateConventionScope)
            {
                _runCount = 0;
                dispatcher._scope = new DelayedConventionScope(_dispatcher._scope);
            }
        }

        private void Run()
        {
            if (_runCount == null)
            {
                return;
            }

            while (true)
            {
                if (_runCount++ == short.MaxValue)
                {
                    throw new InvalidOperationException(CoreStrings.ConventionsInfiniteLoop);
                }

                var currentScope = _dispatcher._scope;
                if (currentScope == _dispatcher._immediateConventionScope)
                {
                    return;
                }

                _dispatcher._scope = currentScope.Parent!;

                if (currentScope.Children == null)
                {
                    return;
                }

                if (currentScope.Parent != _dispatcher._immediateConventionScope
                    || currentScope.GetLeafCount() == 0)
                {
                    return;
                }

                // Capture all nested convention invocations to unwind the stack
                _dispatcher._scope = new DelayedConventionScope(_dispatcher._immediateConventionScope);
                currentScope.Run(_dispatcher);
            }
        }

        public IConventionForeignKey? Run(IConventionForeignKey foreignKey)
        {
            if (_runCount == null)
            {
                return foreignKey;
            }

            using var foreignKeyReference = _dispatcher.Tracker.Track(foreignKey);
            Run();
            return foreignKeyReference.Object is null || !foreignKeyReference.Object.IsInModel
                ? null
                : foreignKeyReference.Object;
        }

        public void Dispose()
        {
            if (_runCount == 0)
            {
                Run();
            }
        }

        /// <inheritdoc />
        IMetadataReference<IConventionForeignKey> IConventionBatch.Track(IConventionForeignKey foreignKey)
            => _dispatcher.Tracker.Track(foreignKey);
    }
}
