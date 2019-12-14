// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public partial class ConventionDispatcher
    {
        private sealed class DelayedConventionScope : ConventionScope
        {
            private List<ConventionNode> _children;

            public DelayedConventionScope(ConventionScope parent, List<ConventionNode> children = null)
            {
                Parent = parent;
                _children = children;
            }

            public override ConventionScope Parent { [DebuggerStepThrough] get; }

            public override IReadOnlyList<ConventionNode> Children
            {
                [DebuggerStepThrough]
                get => _children;
            }


            private void Add(ConventionNode node)
            {
                if (_children == null)
                {
                    _children = new List<ConventionNode>();
                }

                _children.Add(node);
            }

            public override void Run(ConventionDispatcher dispatcher)
            {
                if (_children == null)
                {
                    return;
                }

                foreach (var conventionNode in _children)
                {
                    conventionNode.Run(dispatcher);
                }
            }

            public override IConventionEntityTypeBuilder OnEntityTypeAdded(IConventionEntityTypeBuilder entityTypeBuilder)
            {
                Add(new OnEntityTypeAddedNode(entityTypeBuilder));
                return entityTypeBuilder;
            }

            public override string OnEntityTypeIgnored(IConventionModelBuilder modelBuilder, string name, Type type)
            {
                Add(new OnEntityTypeIgnoredNode(modelBuilder, name, type));
                return name;
            }

            public override IConventionEntityType OnEntityTypeRemoved(
                IConventionModelBuilder modelBuilder, IConventionEntityType entityType)
            {
                Add(new OnEntityTypeRemovedNode(modelBuilder, entityType));
                return entityType;
            }

            public override string OnEntityTypeMemberIgnored(IConventionEntityTypeBuilder entityTypeBuilder, string name)
            {
                Add(new OnEntityTypeMemberIgnoredNode(entityTypeBuilder, name));
                return name;
            }

            public override IConventionEntityType OnEntityTypeBaseTypeChanged(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
                [CanBeNull] IConventionEntityType newBaseType,
                [CanBeNull] IConventionEntityType previousBaseType)
            {
                Add(new OnEntityTypeBaseTypeChangedNode(entityTypeBuilder, newBaseType, previousBaseType));
                return newBaseType;
            }

            public override IConventionAnnotation OnEntityTypeAnnotationChanged(
                IConventionEntityTypeBuilder entityTypeBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                Add(new OnEntityTypeAnnotationChangedNode(entityTypeBuilder, name, annotation, oldAnnotation));
                return annotation;
            }

            public override IConventionAnnotation OnModelAnnotationChanged(
                IConventionModelBuilder modelBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                Add(new OnModelAnnotationChangedNode(modelBuilder, name, annotation, oldAnnotation));
                return annotation;
            }

            public override IConventionRelationshipBuilder OnForeignKeyAdded(IConventionRelationshipBuilder relationshipBuilder)
            {
                Add(new OnForeignKeyAddedNode(relationshipBuilder));
                return relationshipBuilder;
            }

            public override IConventionForeignKey OnForeignKeyRemoved(
                IConventionEntityTypeBuilder entityTypeBuilder, IConventionForeignKey foreignKey)
            {
                Add(new OnForeignKeyRemovedNode(entityTypeBuilder, foreignKey));
                return foreignKey;
            }

            public override IConventionAnnotation OnForeignKeyAnnotationChanged(
                IConventionRelationshipBuilder relationshipBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                Add(new OnForeignKeyAnnotationChangedNode(relationshipBuilder, name, annotation, oldAnnotation));
                return annotation;
            }

            public override IConventionKeyBuilder OnKeyAdded(IConventionKeyBuilder keyBuilder)
            {
                Add(new OnKeyAddedNode(keyBuilder));
                return keyBuilder;
            }

            public override IConventionKey OnKeyRemoved(
                IConventionEntityTypeBuilder entityTypeBuilder, IConventionKey key)
            {
                Add(new OnKeyRemovedNode(entityTypeBuilder, key));
                return key;
            }

            public override IConventionAnnotation OnKeyAnnotationChanged(
                IConventionKeyBuilder keyBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                Add(new OnKeyAnnotationChangedNode(keyBuilder, name, annotation, oldAnnotation));
                return annotation;
            }

            public override IConventionKey OnEntityTypePrimaryKeyChanged(
                IConventionEntityTypeBuilder entityTypeBuilder,
                IConventionKey newPrimaryKey,
                IConventionKey previousPrimaryKey)
            {
                Add(new OnEntityTypePrimaryKeyChangedNode(entityTypeBuilder, newPrimaryKey, previousPrimaryKey));
                return newPrimaryKey;
            }

            public override IConventionIndexBuilder OnIndexAdded(IConventionIndexBuilder indexBuilder)
            {
                Add(new OnIndexAddedNode(indexBuilder));
                return indexBuilder;
            }

            public override IConventionIndex OnIndexRemoved(
                IConventionEntityTypeBuilder entityTypeBuilder, IConventionIndex index)
            {
                Add(new OnIndexRemovedNode(entityTypeBuilder, index));
                return index;
            }

            public override IConventionIndexBuilder OnIndexUniquenessChanged(IConventionIndexBuilder indexBuilder)
            {
                Add(new OnIndexUniquenessChangedNode(indexBuilder));
                return indexBuilder;
            }

            public override IConventionAnnotation OnIndexAnnotationChanged(
                IConventionIndexBuilder indexBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                Add(new OnIndexAnnotationChangedNode(indexBuilder, name, annotation, oldAnnotation));
                return annotation;
            }

            public override IConventionNavigation OnNavigationAdded(
                IConventionRelationshipBuilder relationshipBuilder, IConventionNavigation navigation)
            {
                Add(new OnNavigationAddedNode(relationshipBuilder, navigation));
                return navigation;
            }

            public override string OnNavigationRemoved(
                IConventionEntityTypeBuilder sourceEntityTypeBuilder,
                IConventionEntityTypeBuilder targetEntityTypeBuilder,
                string navigationName,
                MemberInfo memberInfo)
            {
                Add(new OnNavigationRemovedNode(sourceEntityTypeBuilder, targetEntityTypeBuilder, navigationName, memberInfo));
                return navigationName;
            }

            public override IConventionRelationshipBuilder OnForeignKeyPropertiesChanged(
                IConventionRelationshipBuilder relationshipBuilder,
                IReadOnlyList<IConventionProperty> oldDependentProperties,
                IConventionKey oldPrincipalKey)
            {
                Add(new OnForeignKeyPropertiesChangedNode(relationshipBuilder, oldDependentProperties, oldPrincipalKey));
                return relationshipBuilder;
            }

            public override IConventionRelationshipBuilder OnForeignKeyUniquenessChanged(
                IConventionRelationshipBuilder relationshipBuilder)
            {
                Add(new OnForeignKeyUniquenessChangedNode(relationshipBuilder));
                return relationshipBuilder;
            }

            public override IConventionRelationshipBuilder OnForeignKeyRequirednessChanged(
                IConventionRelationshipBuilder relationshipBuilder)
            {
                Add(new OnForeignKeyRequirednessChangedNode(relationshipBuilder));
                return relationshipBuilder;
            }

            public override IConventionRelationshipBuilder OnForeignKeyOwnershipChanged(
                IConventionRelationshipBuilder relationshipBuilder)
            {
                Add(new OnForeignKeyOwnershipChangedNode(relationshipBuilder));
                return relationshipBuilder;
            }

            public override IConventionRelationshipBuilder OnForeignKeyPrincipalEndChanged(
                IConventionRelationshipBuilder relationshipBuilder)
            {
                Add(new OnForeignKeyPrincipalEndChangedNode(relationshipBuilder));
                return relationshipBuilder;
            }

            public override IConventionPropertyBuilder OnPropertyAdded(IConventionPropertyBuilder propertyBuilder)
            {
                Add(new OnPropertyAddedNode(propertyBuilder));
                return propertyBuilder;
            }

            public override IConventionPropertyBuilder OnPropertyNullableChanged(IConventionPropertyBuilder propertyBuilder)
            {
                Add(new OnPropertyNullableChangedNode(propertyBuilder));
                return propertyBuilder;
            }

            public override FieldInfo OnPropertyFieldChanged(
                IConventionPropertyBuilder propertyBuilder, FieldInfo newFieldInfo, FieldInfo oldFieldInfo)
            {
                Add(new OnPropertyFieldChangedNode(propertyBuilder, newFieldInfo, oldFieldInfo));
                return newFieldInfo;
            }

            public override IConventionAnnotation OnPropertyAnnotationChanged(
                IConventionPropertyBuilder propertyBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                Add(new OnPropertyAnnotationChangedNode(propertyBuilder, name, annotation, oldAnnotation));
                return annotation;
            }
        }

        private sealed class OnModelAnnotationChangedNode : ConventionNode
        {
            public OnModelAnnotationChangedNode(
                IConventionModelBuilder modelBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                ModelBuilder = modelBuilder;
                Name = name;
                Annotation = annotation;
                OldAnnotation = oldAnnotation;
            }

            public IConventionModelBuilder ModelBuilder { get; }
            public string Name { get; }
            public IConventionAnnotation Annotation { get; }
            public IConventionAnnotation OldAnnotation { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnModelAnnotationChanged(
                    ModelBuilder, Name, Annotation, OldAnnotation);
        }

        private sealed class OnEntityTypeAddedNode : ConventionNode
        {
            public OnEntityTypeAddedNode(IConventionEntityTypeBuilder entityTypeBuilder)
            {
                EntityTypeBuilder = entityTypeBuilder;
            }

            public IConventionEntityTypeBuilder EntityTypeBuilder { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnEntityTypeAdded(EntityTypeBuilder);
        }

        private sealed class OnEntityTypeIgnoredNode : ConventionNode
        {
            public OnEntityTypeIgnoredNode(IConventionModelBuilder modelBuilder, string name, Type type)
            {
                ModelBuilder = modelBuilder;
                Name = name;
                Type = type;
            }

            public IConventionModelBuilder ModelBuilder { get; }
            public string Name { get; }
            public Type Type { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnEntityTypeIgnored(ModelBuilder, Name, Type);
        }

        private sealed class OnEntityTypeRemovedNode : ConventionNode
        {
            public OnEntityTypeRemovedNode(IConventionModelBuilder modelBuilder, IConventionEntityType entityType)
            {
                ModelBuilder = modelBuilder;
                EntityType = entityType;
            }

            public IConventionModelBuilder ModelBuilder { get; }
            public IConventionEntityType EntityType { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnEntityTypeRemoved(ModelBuilder, EntityType);
        }

        private sealed class OnEntityTypeMemberIgnoredNode : ConventionNode
        {
            public OnEntityTypeMemberIgnoredNode(IConventionEntityTypeBuilder entityTypeBuilder, string name)
            {
                EntityTypeBuilder = entityTypeBuilder;
                Name = name;
            }

            public IConventionEntityTypeBuilder EntityTypeBuilder { get; }
            public string Name { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnEntityTypeMemberIgnored(EntityTypeBuilder, Name);
        }

        private sealed class OnEntityTypeBaseTypeChangedNode : ConventionNode
        {
            public OnEntityTypeBaseTypeChangedNode(
                IConventionEntityTypeBuilder entityTypeBuilder,
                IConventionEntityType newBaseType,
                IConventionEntityType previousBaseType)
            {
                EntityTypeBuilder = entityTypeBuilder;
                NewBaseType = newBaseType;
                PreviousBaseType = previousBaseType;
            }

            public IConventionEntityTypeBuilder EntityTypeBuilder { get; }
            public IConventionEntityType NewBaseType { get; }
            public IConventionEntityType PreviousBaseType { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnEntityTypeBaseTypeChanged(
                    EntityTypeBuilder, NewBaseType, PreviousBaseType);
        }

        private sealed class OnEntityTypeAnnotationChangedNode : ConventionNode
        {
            public OnEntityTypeAnnotationChangedNode(
                IConventionEntityTypeBuilder entityTypeBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                EntityTypeBuilder = entityTypeBuilder;
                Name = name;
                Annotation = annotation;
                OldAnnotation = oldAnnotation;
            }

            public IConventionEntityTypeBuilder EntityTypeBuilder { get; }
            public string Name { get; }
            public IConventionAnnotation Annotation { get; }
            public IConventionAnnotation OldAnnotation { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnEntityTypeAnnotationChanged(
                    EntityTypeBuilder, Name, Annotation, OldAnnotation);
        }

        private sealed class OnForeignKeyAddedNode : ConventionNode
        {
            public OnForeignKeyAddedNode(IConventionRelationshipBuilder relationshipBuilder)
            {
                RelationshipBuilder = relationshipBuilder;
            }

            public IConventionRelationshipBuilder RelationshipBuilder { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnForeignKeyAdded(RelationshipBuilder);
        }

        private sealed class OnForeignKeyRemovedNode : ConventionNode
        {
            public OnForeignKeyRemovedNode(IConventionEntityTypeBuilder entityTypeBuilder, IConventionForeignKey foreignKey)
            {
                EntityTypeBuilder = entityTypeBuilder;
                ForeignKey = foreignKey;
            }

            public IConventionEntityTypeBuilder EntityTypeBuilder { get; }
            public IConventionForeignKey ForeignKey { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnForeignKeyRemoved(EntityTypeBuilder, ForeignKey);
        }

        private sealed class OnForeignKeyAnnotationChangedNode : ConventionNode
        {
            public OnForeignKeyAnnotationChangedNode(
                IConventionRelationshipBuilder relationshipBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                RelationshipBuilder = relationshipBuilder;
                Name = name;
                Annotation = annotation;
                OldAnnotation = oldAnnotation;
            }

            public IConventionRelationshipBuilder RelationshipBuilder { get; }
            public string Name { get; }
            public IConventionAnnotation Annotation { get; }
            public IConventionAnnotation OldAnnotation { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnForeignKeyAnnotationChanged(
                    RelationshipBuilder, Name, Annotation, OldAnnotation);
        }

        private sealed class OnForeignKeyPropertiesChangedNode : ConventionNode
        {
            public OnForeignKeyPropertiesChangedNode(
                IConventionRelationshipBuilder relationshipBuilder,
                IReadOnlyList<IConventionProperty> oldDependentProperties,
                IConventionKey oldPrincipalKey)
            {
                RelationshipBuilder = relationshipBuilder;
                OldDependentProperties = oldDependentProperties;
                OldPrincipalKey = oldPrincipalKey;
            }

            public IConventionRelationshipBuilder RelationshipBuilder { get; }
            public IReadOnlyList<IConventionProperty> OldDependentProperties { get; }
            public IConventionKey OldPrincipalKey { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnForeignKeyPropertiesChanged(
                    RelationshipBuilder, OldDependentProperties, OldPrincipalKey);
        }

        private sealed class OnForeignKeyUniquenessChangedNode : ConventionNode
        {
            public OnForeignKeyUniquenessChangedNode(IConventionRelationshipBuilder relationshipBuilder)
            {
                RelationshipBuilder = relationshipBuilder;
            }

            public IConventionRelationshipBuilder RelationshipBuilder { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnForeignKeyUniquenessChanged(RelationshipBuilder);
        }

        private sealed class OnForeignKeyRequirednessChangedNode : ConventionNode
        {
            public OnForeignKeyRequirednessChangedNode(IConventionRelationshipBuilder relationshipBuilder)
            {
                RelationshipBuilder = relationshipBuilder;
            }

            public IConventionRelationshipBuilder RelationshipBuilder { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnForeignKeyRequirednessChanged(RelationshipBuilder);
        }

        private sealed class OnForeignKeyOwnershipChangedNode : ConventionNode
        {
            public OnForeignKeyOwnershipChangedNode(IConventionRelationshipBuilder relationshipBuilder)
            {
                RelationshipBuilder = relationshipBuilder;
            }

            public IConventionRelationshipBuilder RelationshipBuilder { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnForeignKeyOwnershipChanged(RelationshipBuilder);
        }

        private sealed class OnForeignKeyPrincipalEndChangedNode : ConventionNode
        {
            public OnForeignKeyPrincipalEndChangedNode(IConventionRelationshipBuilder relationshipBuilder)
            {
                RelationshipBuilder = relationshipBuilder;
            }

            public IConventionRelationshipBuilder RelationshipBuilder { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnForeignKeyPrincipalEndChanged(RelationshipBuilder);
        }

        private sealed class OnNavigationAddedNode : ConventionNode
        {
            public OnNavigationAddedNode(IConventionRelationshipBuilder relationshipBuilder, IConventionNavigation navigation)
            {
                RelationshipBuilder = relationshipBuilder;
                Navigation = navigation;
            }

            public IConventionRelationshipBuilder RelationshipBuilder { get; }
            public IConventionNavigation Navigation { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnNavigationAdded(RelationshipBuilder, Navigation);
        }

        private sealed class OnNavigationRemovedNode : ConventionNode
        {
            public OnNavigationRemovedNode(
                IConventionEntityTypeBuilder sourceEntityTypeBuilder,
                IConventionEntityTypeBuilder targetEntityTypeBuilder,
                string navigationName,
                MemberInfo memberInfo)
            {
                SourceEntityTypeBuilder = sourceEntityTypeBuilder;
                TargetEntityTypeBuilder = targetEntityTypeBuilder;
                NavigationName = navigationName;
                MemberInfo = memberInfo;
            }

            public IConventionEntityTypeBuilder SourceEntityTypeBuilder { get; }
            public IConventionEntityTypeBuilder TargetEntityTypeBuilder { get; }
            public string NavigationName { get; }
            public MemberInfo MemberInfo { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnNavigationRemoved(
                    SourceEntityTypeBuilder, TargetEntityTypeBuilder, NavigationName, MemberInfo);
        }

        private sealed class OnKeyAddedNode : ConventionNode
        {
            public OnKeyAddedNode(IConventionKeyBuilder keyBuilder)
            {
                KeyBuilder = keyBuilder;
            }

            public IConventionKeyBuilder KeyBuilder { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnKeyAdded(KeyBuilder);
        }

        private sealed class OnKeyRemovedNode : ConventionNode
        {
            public OnKeyRemovedNode(IConventionEntityTypeBuilder entityTypeBuilder, IConventionKey key)
            {
                EntityTypeBuilder = entityTypeBuilder;
                Key = key;
            }

            public IConventionEntityTypeBuilder EntityTypeBuilder { get; }
            public IConventionKey Key { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnKeyRemoved(EntityTypeBuilder, Key);
        }

        private sealed class OnKeyAnnotationChangedNode : ConventionNode
        {
            public OnKeyAnnotationChangedNode(
                IConventionKeyBuilder keyBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                KeyBuilder = keyBuilder;
                Name = name;
                Annotation = annotation;
                OldAnnotation = oldAnnotation;
            }

            public IConventionKeyBuilder KeyBuilder { get; }
            public string Name { get; }
            public IConventionAnnotation Annotation { get; }
            public IConventionAnnotation OldAnnotation { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnKeyAnnotationChanged(
                    KeyBuilder, Name, Annotation, OldAnnotation);
        }

        private sealed class OnEntityTypePrimaryKeyChangedNode : ConventionNode
        {
            public OnEntityTypePrimaryKeyChangedNode(
                IConventionEntityTypeBuilder entityTypeBuilder, IConventionKey newPrimaryKey, IConventionKey previousPrimaryKey)
            {
                EntityTypeBuilder = entityTypeBuilder;
                NewPrimaryKey = newPrimaryKey;
                PreviousPrimaryKey = previousPrimaryKey;
            }

            public IConventionEntityTypeBuilder EntityTypeBuilder { get; }
            public IConventionKey NewPrimaryKey { get; }
            public IConventionKey PreviousPrimaryKey { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnEntityTypePrimaryKeyChanged(
                    EntityTypeBuilder, NewPrimaryKey, PreviousPrimaryKey);
        }

        private sealed class OnIndexAddedNode : ConventionNode
        {
            public OnIndexAddedNode(IConventionIndexBuilder indexBuilder)
            {
                IndexBuilder = indexBuilder;
            }

            public IConventionIndexBuilder IndexBuilder { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnIndexAdded(IndexBuilder);
        }

        private sealed class OnIndexRemovedNode : ConventionNode
        {
            public OnIndexRemovedNode(IConventionEntityTypeBuilder entityTypeBuilder, IConventionIndex index)
            {
                EntityTypeBuilder = entityTypeBuilder;
                Index = index;
            }

            public IConventionEntityTypeBuilder EntityTypeBuilder { get; }
            public IConventionIndex Index { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnIndexRemoved(EntityTypeBuilder, Index);
        }

        private sealed class OnIndexUniquenessChangedNode : ConventionNode
        {
            public OnIndexUniquenessChangedNode(IConventionIndexBuilder indexBuilder)
            {
                IndexBuilder = indexBuilder;
            }

            public IConventionIndexBuilder IndexBuilder { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnIndexUniquenessChanged(IndexBuilder);
        }

        private sealed class OnIndexAnnotationChangedNode : ConventionNode
        {
            public OnIndexAnnotationChangedNode(
                IConventionIndexBuilder indexBuilder, string name, IConventionAnnotation annotation, IConventionAnnotation oldAnnotation)
            {
                IndexBuilder = indexBuilder;
                Name = name;
                Annotation = annotation;
                OldAnnotation = oldAnnotation;
            }

            public IConventionIndexBuilder IndexBuilder { get; }
            public string Name { get; }
            public IConventionAnnotation Annotation { get; }
            public IConventionAnnotation OldAnnotation { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnIndexAnnotationChanged(
                    IndexBuilder, Name, Annotation, OldAnnotation);
        }

        private sealed class OnPropertyAddedNode : ConventionNode
        {
            public OnPropertyAddedNode(IConventionPropertyBuilder propertyBuilder)
            {
                PropertyBuilder = propertyBuilder;
            }

            public IConventionPropertyBuilder PropertyBuilder { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnPropertyAdded(PropertyBuilder);
        }

        private sealed class OnPropertyNullableChangedNode : ConventionNode
        {
            public OnPropertyNullableChangedNode(IConventionPropertyBuilder propertyBuilder)
            {
                PropertyBuilder = propertyBuilder;
            }

            public IConventionPropertyBuilder PropertyBuilder { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnPropertyNullableChanged(PropertyBuilder);
        }

        private sealed class OnPropertyFieldChangedNode : ConventionNode
        {
            public OnPropertyFieldChangedNode(IConventionPropertyBuilder propertyBuilder, FieldInfo newFieldInfo, FieldInfo oldFieldInfo)
            {
                PropertyBuilder = propertyBuilder;
                NewFieldInfo = newFieldInfo;
                OldFieldInfo = oldFieldInfo;
            }

            public IConventionPropertyBuilder PropertyBuilder { get; }
            public FieldInfo NewFieldInfo { get; }
            public FieldInfo OldFieldInfo { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnPropertyFieldChanged(PropertyBuilder, NewFieldInfo, OldFieldInfo);
        }

        private sealed class OnPropertyAnnotationChangedNode : ConventionNode
        {
            public OnPropertyAnnotationChangedNode(
                IConventionPropertyBuilder propertyBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                PropertyBuilder = propertyBuilder;
                Name = name;
                Annotation = annotation;
                OldAnnotation = oldAnnotation;
            }

            public IConventionPropertyBuilder PropertyBuilder { get; }
            public string Name { get; }
            public IConventionAnnotation Annotation { get; }
            public IConventionAnnotation OldAnnotation { get; }

            public override void Run(ConventionDispatcher dispatcher)
                => dispatcher._immediateConventionScope.OnPropertyAnnotationChanged(
                    PropertyBuilder, Name, Annotation, OldAnnotation);
        }
    }
}
