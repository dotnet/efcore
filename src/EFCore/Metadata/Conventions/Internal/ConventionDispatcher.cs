// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public partial class ConventionDispatcher
    {
        private ConventionScope _scope;
        private readonly ImmediateConventionScope _immediateConventionScope;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ConventionDispatcher([NotNull] ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            _immediateConventionScope = new ImmediateConventionScope(conventionSet);
            _scope = _immediateConventionScope;
            Tracker = new MetadataTracker();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MetadataTracker Tracker { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder OnEntityTypeAdded([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
            => _scope.OnEntityTypeAdded(Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool OnEntityTypeIgnored([NotNull] InternalModelBuilder modelBuilder, [NotNull] string name, [CanBeNull] Type type)
            => _scope.OnEntityTypeIgnored(Check.NotNull(modelBuilder, nameof(modelBuilder)), Check.NotNull(name, nameof(name)), type);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool OnEntityTypeRemoved([NotNull] InternalModelBuilder modelBuilder, [NotNull] EntityType type)
            => _scope.OnEntityTypeRemoved(Check.NotNull(modelBuilder, nameof(modelBuilder)), Check.NotNull(type, nameof(type)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder OnEntityTypeMemberIgnored(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [NotNull] string ignoredMemberName)
            => _scope.OnEntityTypeMemberIgnored(
                Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder)),
                Check.NotEmpty(ignoredMemberName, nameof(ignoredMemberName)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder OnBaseEntityTypeChanged(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] EntityType previousBaseType)
            => _scope.OnBaseEntityTypeChanged(Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder)), previousBaseType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Annotation OnEntityTypeAnnotationChanged(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [NotNull] string name,
            [CanBeNull] Annotation annotation,
            [CanBeNull] Annotation oldAnnotation)
            => _scope.OnEntityTypeAnnotationChanged(
                Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder)),
                Check.NotNull(name, nameof(name)),
                annotation,
                oldAnnotation);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Annotation OnModelAnnotationChanged(
            [NotNull] InternalModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] Annotation annotation,
            [CanBeNull] Annotation oldAnnotation)
            => _scope.OnModelAnnotationChanged(
                Check.NotNull(modelBuilder, nameof(modelBuilder)),
                Check.NotNull(name, nameof(name)),
                annotation,
                oldAnnotation);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder OnForeignKeyAdded([NotNull] InternalRelationshipBuilder relationshipBuilder)
            => _scope.OnForeignKeyAdded(Check.NotNull(relationshipBuilder, nameof(relationshipBuilder)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void OnForeignKeyRemoved([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [NotNull] ForeignKey foreignKey)
            => _scope.OnForeignKeyRemoved(
                Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder)),
                Check.NotNull(foreignKey, nameof(foreignKey)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalKeyBuilder OnKeyAdded([NotNull] InternalKeyBuilder keyBuilder)
            => _scope.OnKeyAdded(Check.NotNull(keyBuilder, nameof(keyBuilder)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void OnKeyRemoved([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [NotNull] Key key)
            => _scope.OnKeyRemoved(Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder)), Check.NotNull(key, nameof(key)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void OnPrimaryKeyChanged(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder, [CanBeNull] Key previousPrimaryKey)
            => _scope.OnPrimaryKeyChanged(Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder)), previousPrimaryKey);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalIndexBuilder OnIndexAdded([NotNull] InternalIndexBuilder indexBuilder)
            => _scope.OnIndexAdded(Check.NotNull(indexBuilder, nameof(indexBuilder)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void OnIndexRemoved([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [NotNull] Index index)
            => _scope.OnIndexRemoved(Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder)), Check.NotNull(index, nameof(index)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool OnIndexUniquenessChanged([NotNull] InternalIndexBuilder indexBuilder)
            => _scope.OnIndexUniquenessChanged(Check.NotNull(indexBuilder, nameof(indexBuilder)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Annotation OnIndexAnnotationChanged(
            [NotNull] InternalIndexBuilder indexBuilder,
            [NotNull] string name,
            [CanBeNull] Annotation annotation,
            [CanBeNull] Annotation oldAnnotation)
            => _scope.OnIndexAnnotationChanged(
                Check.NotNull(indexBuilder, nameof(indexBuilder)),
                Check.NotNull(name, nameof(name)),
                annotation,
                oldAnnotation);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder OnNavigationAdded(
            [NotNull] InternalRelationshipBuilder relationshipBuilder, [NotNull] Navigation navigation)
            => _scope.OnNavigationAdded(
                Check.NotNull(relationshipBuilder, nameof(relationshipBuilder)),
                Check.NotNull(navigation, nameof(navigation)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void OnNavigationRemoved(
            [NotNull] InternalEntityTypeBuilder sourceEntityTypeBuilder,
            [NotNull] InternalEntityTypeBuilder targetEntityTypeBuilder,
            [NotNull] string navigationName,
            [CanBeNull] MemberInfo memberInfo)
            => _scope.OnNavigationRemoved(
                Check.NotNull(sourceEntityTypeBuilder, nameof(sourceEntityTypeBuilder)),
                Check.NotNull(targetEntityTypeBuilder, nameof(targetEntityTypeBuilder)),
                Check.NotNull(navigationName, nameof(navigationName)),
                memberInfo);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder OnForeignKeyUniquenessChanged([NotNull] InternalRelationshipBuilder relationshipBuilder)
            => _scope.OnForeignKeyUniquenessChanged(Check.NotNull(relationshipBuilder, nameof(relationshipBuilder)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder OnForeignKeyRequirednessChanged([NotNull] InternalRelationshipBuilder relationshipBuilder)
            => _scope.OnForeignKeyRequirednessChanged(Check.NotNull(relationshipBuilder, nameof(relationshipBuilder)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder OnForeignKeyOwnershipChanged([NotNull] InternalRelationshipBuilder relationshipBuilder)
            => _scope.OnForeignKeyOwnershipChanged(Check.NotNull(relationshipBuilder, nameof(relationshipBuilder)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder OnPrincipalEndChanged([NotNull] InternalRelationshipBuilder relationshipBuilder)
            => _scope.OnPrincipalEndChanged(Check.NotNull(relationshipBuilder, nameof(relationshipBuilder)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalPropertyBuilder OnPropertyAdded([NotNull] InternalPropertyBuilder propertyBuilder)
            => _scope.OnPropertyAdded(Check.NotNull(propertyBuilder, nameof(propertyBuilder)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool OnPropertyNullableChanged([NotNull] InternalPropertyBuilder propertyBuilder)
            => _scope.OnPropertyNullableChanged(Check.NotNull(propertyBuilder, nameof(propertyBuilder)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool OnPropertyFieldChanged(
            [NotNull] InternalPropertyBuilder propertyBuilder, [CanBeNull] FieldInfo oldFieldInfo)
            => _scope.OnPropertyFieldChanged(Check.NotNull(propertyBuilder, nameof(propertyBuilder)), oldFieldInfo);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Annotation OnPropertyAnnotationChanged(
            [NotNull] InternalPropertyBuilder propertyBuilder,
            [NotNull] string name,
            [CanBeNull] Annotation annotation,
            [CanBeNull] Annotation oldAnnotation)
            => _scope.OnPropertyAnnotationChanged(
                Check.NotNull(propertyBuilder, nameof(propertyBuilder)),
                Check.NotNull(name, nameof(name)),
                annotation,
                oldAnnotation);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder OnModelBuilt([NotNull] InternalModelBuilder modelBuilder)
            => _immediateConventionScope.OnModelBuilt(Check.NotNull(modelBuilder, nameof(modelBuilder)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder OnModelInitialized([NotNull] InternalModelBuilder modelBuilder)
            => _immediateConventionScope.OnModelInitialized(Check.NotNull(modelBuilder, nameof(modelBuilder)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IConventionBatch StartBatch() => new ConventionBatch(this);

        private class ConventionBatch : IConventionBatch
        {
            private readonly ConventionDispatcher _dispatcher;
            private int _runCount;

            public ConventionBatch(ConventionDispatcher dispatcher)
            {
                _dispatcher = dispatcher;
                var currentScope = _dispatcher._scope;

                if (currentScope.Children?[currentScope.Children.Count - 1] is ConventionScope lastScope
                    && lastScope.Children == null)
                {
                    dispatcher._scope = lastScope;
                    return;
                }

                dispatcher._scope = new ConventionScope(currentScope);

                if (currentScope != _dispatcher._immediateConventionScope)
                {
                    currentScope.Add(dispatcher._scope);
                }
            }

            private void Run()
            {
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
        }
    }
}
