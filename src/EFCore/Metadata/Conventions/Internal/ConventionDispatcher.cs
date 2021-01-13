// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
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
        public ConventionDispatcher([NotNull] ConventionSet conventionSet)
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
        public virtual IConventionModelBuilder OnModelInitialized([NotNull] IConventionModelBuilder modelBuilder)
            => _immediateConventionScope.OnModelInitialized(modelBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionModelBuilder OnModelFinalizing([NotNull] IConventionModelBuilder modelBuilder)
            => _immediateConventionScope.OnModelFinalizing(modelBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IModel OnModelFinalized([NotNull] IModel model)
            => _immediateConventionScope.OnModelFinalized(model);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionAnnotation OnModelAnnotationChanged(
            [NotNull] IConventionModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] IConventionAnnotation annotation,
            [CanBeNull] IConventionAnnotation oldAnnotation)
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
        public virtual IConventionEntityTypeBuilder OnEntityTypeAdded([NotNull] IConventionEntityTypeBuilder entityTypeBuilder)
            => _scope.OnEntityTypeAdded(entityTypeBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string OnEntityTypeIgnored(
            [NotNull] IConventionModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] Type type)
            => _scope.OnEntityTypeIgnored(modelBuilder, name, type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionEntityType OnEntityTypeRemoved(
            [NotNull] IConventionModelBuilder modelBuilder,
            [NotNull] IConventionEntityType type)
            => _scope.OnEntityTypeRemoved(modelBuilder, type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string OnEntityTypeMemberIgnored(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] string name)
            => _scope.OnEntityTypeMemberIgnored(entityTypeBuilder, name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionEntityType OnEntityTypeBaseTypeChanged(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] IConventionEntityType newBaseType,
            [CanBeNull] IConventionEntityType previousBaseType)
            => _scope.OnEntityTypeBaseTypeChanged(entityTypeBuilder, newBaseType, previousBaseType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionAnnotation OnEntityTypeAnnotationChanged(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] string name,
            [CanBeNull] IConventionAnnotation annotation,
            [CanBeNull] IConventionAnnotation oldAnnotation)
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
        public virtual IConventionForeignKeyBuilder OnForeignKeyAdded([NotNull] IConventionForeignKeyBuilder relationshipBuilder)
            => _scope.OnForeignKeyAdded(relationshipBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionForeignKey OnForeignKeyRemoved(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] IConventionForeignKey foreignKey)
            => _scope.OnForeignKeyRemoved(entityTypeBuilder, foreignKey);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<IConventionProperty> OnForeignKeyPropertiesChanged(
            [NotNull] IConventionForeignKeyBuilder relationshipBuilder,
            [NotNull] IReadOnlyList<IConventionProperty> oldDependentProperties,
            [NotNull] IConventionKey oldPrincipalKey)
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
            [NotNull] IConventionForeignKeyBuilder relationshipBuilder)
            => _scope.OnForeignKeyUniquenessChanged(relationshipBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool? OnForeignKeyRequirednessChanged(
            [NotNull] IConventionForeignKeyBuilder relationshipBuilder)
            => _scope.OnForeignKeyRequirednessChanged(relationshipBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool? OnForeignKeyDependentRequirednessChanged(
            [NotNull] IConventionForeignKeyBuilder relationshipBuilder)
            => _scope.OnForeignKeyDependentRequirednessChanged(relationshipBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool? OnForeignKeyOwnershipChanged(
            [NotNull] IConventionForeignKeyBuilder relationshipBuilder)
            => _scope.OnForeignKeyOwnershipChanged(relationshipBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionForeignKeyBuilder OnForeignKeyPrincipalEndChanged(
            [NotNull] IConventionForeignKeyBuilder relationshipBuilder)
            => _scope.OnForeignKeyPrincipalEndChanged(relationshipBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionAnnotation OnForeignKeyAnnotationChanged(
            [NotNull] IConventionForeignKeyBuilder relationshipBuilder,
            [NotNull] string name,
            [CanBeNull] IConventionAnnotation annotation,
            [CanBeNull] IConventionAnnotation oldAnnotation)
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
        public virtual IConventionNavigationBuilder OnNavigationAdded([NotNull] IConventionNavigationBuilder navigationBuilder)
            => _scope.OnNavigationAdded(navigationBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string OnNavigationRemoved(
            [NotNull] IConventionEntityTypeBuilder sourceEntityTypeBuilder,
            [NotNull] IConventionEntityTypeBuilder targetEntityTypeBuilder,
            [NotNull] string navigationName,
            [CanBeNull] MemberInfo memberInfo)
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
        public virtual IConventionAnnotation OnNavigationAnnotationChanged(
            [NotNull] IConventionForeignKeyBuilder relationshipBuilder,
            [NotNull] IConventionNavigation navigation,
            [NotNull] string name,
            [CanBeNull] IConventionAnnotation annotation,
            [CanBeNull] IConventionAnnotation oldAnnotation)
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
        public virtual IConventionSkipNavigationBuilder OnSkipNavigationAdded(
            [NotNull] IConventionSkipNavigationBuilder navigationBuilder)
            => _scope.OnSkipNavigationAdded(navigationBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionForeignKey OnSkipNavigationForeignKeyChanged(
            [NotNull] IConventionSkipNavigationBuilder navigationBuilder,
            [NotNull] IConventionForeignKey foreignKey,
            [NotNull] IConventionForeignKey oldForeignKey)
            => _scope.OnSkipNavigationForeignKeyChanged(navigationBuilder, foreignKey, oldForeignKey);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionSkipNavigation OnSkipNavigationInverseChanged(
            [NotNull] IConventionSkipNavigationBuilder navigationBuilder,
            [CanBeNull] IConventionSkipNavigation inverse,
            [CanBeNull] IConventionSkipNavigation oldInverse)
            => _scope.OnSkipNavigationInverseChanged(navigationBuilder, inverse, oldInverse);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionSkipNavigation OnSkipNavigationRemoved(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] IConventionSkipNavigation navigation)
            => _scope.OnSkipNavigationRemoved(entityTypeBuilder, navigation);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionAnnotation OnSkipNavigationAnnotationChanged(
            [NotNull] IConventionSkipNavigationBuilder navigationBuilder,
            [NotNull] string name,
            [CanBeNull] IConventionAnnotation annotation,
            [CanBeNull] IConventionAnnotation oldAnnotation)
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
        public virtual IConventionKeyBuilder OnKeyAdded([NotNull] IConventionKeyBuilder keyBuilder)
            => _scope.OnKeyAdded(keyBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionKey OnKeyRemoved([NotNull] IConventionEntityTypeBuilder entityTypeBuilder, [NotNull] IConventionKey key)
            => _scope.OnKeyRemoved(entityTypeBuilder, key);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionAnnotation OnKeyAnnotationChanged(
            [NotNull] IConventionKeyBuilder keyBuilder,
            [NotNull] string name,
            [CanBeNull] IConventionAnnotation annotation,
            [CanBeNull] IConventionAnnotation oldAnnotation)
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
        public virtual IConventionKey OnPrimaryKeyChanged(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] IConventionKey newPrimaryKey,
            [CanBeNull] IConventionKey previousPrimaryKey)
            => _scope.OnEntityTypePrimaryKeyChanged(entityTypeBuilder, newPrimaryKey, previousPrimaryKey);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionIndexBuilder OnIndexAdded([NotNull] IConventionIndexBuilder indexBuilder)
            => _scope.OnIndexAdded(indexBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionIndex OnIndexRemoved(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] IConventionIndex index)
            => _scope.OnIndexRemoved(entityTypeBuilder, index);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool? OnIndexUniquenessChanged([NotNull] IConventionIndexBuilder indexBuilder)
            => _scope.OnIndexUniquenessChanged(indexBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionAnnotation OnIndexAnnotationChanged(
            [NotNull] IConventionIndexBuilder indexBuilder,
            [NotNull] string name,
            [CanBeNull] IConventionAnnotation annotation,
            [CanBeNull] IConventionAnnotation oldAnnotation)
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
        public virtual IConventionPropertyBuilder OnPropertyAdded([NotNull] IConventionPropertyBuilder propertyBuilder)
            => _scope.OnPropertyAdded(propertyBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionProperty OnPropertyRemoved(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] IConventionProperty property)
            => _scope.OnPropertyRemoved(entityTypeBuilder, property);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool? OnPropertyNullableChanged([NotNull] IConventionPropertyBuilder propertyBuilder)
            => _scope.OnPropertyNullabilityChanged(propertyBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual FieldInfo OnPropertyFieldChanged(
            [NotNull] IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] FieldInfo newFieldInfo,
            [CanBeNull] FieldInfo oldFieldInfo)
            => _scope.OnPropertyFieldChanged(propertyBuilder, newFieldInfo, oldFieldInfo);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionAnnotation OnPropertyAnnotationChanged(
            [NotNull] IConventionPropertyBuilder propertyBuilder,
            [NotNull] string name,
            [CanBeNull] IConventionAnnotation annotation,
            [CanBeNull] IConventionAnnotation oldAnnotation)
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
        public virtual IConventionBatch DelayConventions()
            => new ConventionBatch(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual T Run<T>([NotNull] Func<T> func, [CanBeNull] ref IConventionForeignKey foreignKey)
        {
            var batch = DelayConventions();
            using var foreignKeyReference = Tracker.Track(foreignKey);
            var result = func();
            batch.Dispose();
            foreignKey = foreignKeyReference.Object?.Builder == null ? null : foreignKeyReference.Object;
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
        {
            Check.DebugAssert(_scope == _immediateConventionScope, "Expected no active convention scopes");
        }

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

                    _dispatcher._scope = currentScope.Parent;

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

            public IConventionForeignKey Run(IConventionForeignKey foreignKey)
            {
                if (_runCount == null)
                {
                    return foreignKey;
                }

                using var foreignKeyReference = _dispatcher.Tracker.Track(foreignKey);
                Run();
                return foreignKeyReference.Object?.Builder == null ? null : foreignKeyReference.Object;
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
}
