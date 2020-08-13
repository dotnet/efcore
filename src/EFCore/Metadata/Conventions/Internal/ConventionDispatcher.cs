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
        public virtual IConventionModelBuilder OnModelFinalized([NotNull] IConventionModelBuilder modelBuilder)
            => _immediateConventionScope.OnModelFinalized(modelBuilder);

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
            [NotNull] IConventionModelBuilder modelBuilder, [NotNull] string name, [CanBeNull] Type type)
            => _scope.OnEntityTypeIgnored(modelBuilder, name, type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionEntityType OnEntityTypeRemoved(
            [NotNull] IConventionModelBuilder modelBuilder, [NotNull] IConventionEntityType type)
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
        public virtual IConventionRelationshipBuilder OnForeignKeyAdded([NotNull] IConventionRelationshipBuilder relationshipBuilder)
            => _scope.OnForeignKeyAdded(relationshipBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionForeignKey OnForeignKeyRemoved(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder, [NotNull] IConventionForeignKey foreignKey)
            => _scope.OnForeignKeyRemoved(entityTypeBuilder, foreignKey);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void OnForeignKeyPropertiesChanged(
            [NotNull] InternalRelationshipBuilder relationshipBuilder,
            [NotNull] IReadOnlyList<Property> oldDependentProperties,
            [NotNull] Key oldPrincipalKey)
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
        public virtual IConventionRelationshipBuilder OnForeignKeyUniquenessChanged(
            [NotNull] IConventionRelationshipBuilder relationshipBuilder)
            => _scope.OnForeignKeyUniquenessChanged(relationshipBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionRelationshipBuilder OnForeignKeyRequirednessChanged(
            [NotNull] IConventionRelationshipBuilder relationshipBuilder)
            => _scope.OnForeignKeyRequirednessChanged(relationshipBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionRelationshipBuilder OnForeignKeyOwnershipChanged(
            [NotNull] IConventionRelationshipBuilder relationshipBuilder)
            => _scope.OnForeignKeyOwnershipChanged(relationshipBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionRelationshipBuilder OnForeignKeyPrincipalEndChanged(
            [NotNull] IConventionRelationshipBuilder relationshipBuilder)
            => _scope.OnForeignKeyPrincipalEndChanged(relationshipBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionAnnotation OnForeignKeyAnnotationChanged(
            [NotNull] IConventionRelationshipBuilder relationshipBuilder,
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
        public virtual IConventionNavigation OnNavigationAdded(
            [NotNull] IConventionRelationshipBuilder relationshipBuilder, [NotNull] IConventionNavigation navigation)
            => _scope.OnNavigationAdded(relationshipBuilder, navigation);

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
        public virtual void OnIndexRemoved([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [NotNull] Index index)
            => _scope.OnIndexRemoved(entityTypeBuilder, index);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionIndexBuilder OnIndexUniquenessChanged([NotNull] IConventionIndexBuilder indexBuilder)
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
        public virtual IConventionPropertyBuilder OnPropertyNullableChanged([NotNull] IConventionPropertyBuilder propertyBuilder)
            => _scope.OnPropertyNullableChanged(propertyBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual FieldInfo OnPropertyFieldChanged(
            [NotNull] InternalPropertyBuilder propertyBuilder, [CanBeNull] FieldInfo newFieldInfo, [CanBeNull] FieldInfo oldFieldInfo)
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
        public virtual IConventionBatch DelayConventions() => new ConventionBatch(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual T Run<T>(Func<T> func, ref ForeignKey foreignKey)
        {
            var batch = DelayConventions();
            using (var foreignKeyReference = Tracker.Track(foreignKey))
            {
                var result = func();
                batch.Dispose();
                foreignKey = foreignKeyReference.Object?.Builder == null ? null : foreignKeyReference.Object;
                return result;
            }
        }

        private class ConventionBatch : IConventionBatch
        {
            private readonly ConventionDispatcher _dispatcher;
            private int? _runCount;

            public ConventionBatch(ConventionDispatcher dispatcher)
            {
                _dispatcher = dispatcher;
                if (_dispatcher._scope == _dispatcher._immediateConventionScope)
                {
                    _runCount = 0;
                    dispatcher._scope = new ConventionScope(_dispatcher._scope);
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

                    currentScope.MakeReadonly();

                    if (currentScope.Parent != _dispatcher._immediateConventionScope
                        || currentScope.GetLeafCount() == 0)
                    {
                        return;
                    }

                    // Capture all nested convention invocations to unwind the stack
                    _dispatcher._scope = new ConventionScope(_dispatcher._immediateConventionScope);
                    new RunVisitor(_dispatcher).VisitConventionScope(currentScope);
                }
            }

            public ForeignKey Run(ForeignKey foreignKey)
            {
                if (_runCount == null)
                {
                    return foreignKey;
                }

                using (var foreignKeyReference = _dispatcher.Tracker.Track(foreignKey))
                {
                    Run();
                    return foreignKeyReference.Object?.Builder == null ? null : foreignKeyReference.Object;
                }
            }

            public void Dispose()
            {
                if (_runCount == 0)
                {
                    Run();
                }
            }

            /// <inheritdoc />
            IConventionForeignKey IConventionBatch.Run(IConventionForeignKey foreignKey)
                => Run((ForeignKey)foreignKey);

            /// <inheritdoc />
            IMetadataReference<IConventionForeignKey> IConventionBatch.Track(IConventionForeignKey foreignKey)
                => _dispatcher.Tracker.Track((ForeignKey)foreignKey);
        }
    }
}
