// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public partial class ConventionDispatcher
    {
        private abstract class ConventionNode
        {
            public abstract ConventionNode Accept(ConventionVisitor visitor);
        }

        private class ConventionScope : ConventionNode
        {
            private readonly List<ConventionNode> _children;
            private bool _readonly;

            public ConventionScope(ConventionScope parent, List<ConventionNode> children)
            {
                Parent = parent;
                _children = children ?? new List<ConventionNode>();
            }

            public ConventionScope Parent { [DebuggerStepThrough] get; }

            public IReadOnlyList<ConventionNode> Children
            {
                [DebuggerStepThrough] get { return _children; }
            }

            public int GetLeafCount()
            {
                var scopesToVisit = new Queue<ConventionScope>();
                scopesToVisit.Enqueue(this);
                var leafCount = 0;
                while (scopesToVisit.Count > 0)
                {
                    var scope = scopesToVisit.Dequeue();
                    foreach (var conventionNode in scope.Children)
                    {
                        if (conventionNode is ConventionScope nextScope)
                        {
                            scopesToVisit.Enqueue(nextScope);
                        }
                        else
                        {
                            leafCount++;
                        }
                    }
                }

                return leafCount;
            }

            public void Add(ConventionNode node)
            {
                if (_readonly)
                {
                    throw new InvalidOperationException();
                }
                _children.Add(node);
            }

            public void MakeReadonly() => _readonly = true;

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitConventionScope(this);

            public virtual InternalEntityTypeBuilder OnEntityTypeAdded([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
            {
                Add(new OnEntityTypeAddedNode(entityTypeBuilder));
                return entityTypeBuilder;
            }

            public virtual bool OnEntityTypeIgnored([NotNull] InternalModelBuilder modelBuilder, [NotNull] string name, [CanBeNull] Type type)
            {
                Add(new OnEntityTypeIgnoredNode(modelBuilder, name, type));
                return true;
            }

            public virtual InternalEntityTypeBuilder OnEntityTypeMemberIgnored(
                [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
                [NotNull] string ignoredMemberName)
            {
                Add(new OnEntityTypeMemberIgnoredNode(entityTypeBuilder, ignoredMemberName));
                return entityTypeBuilder;
            }

            public virtual InternalEntityTypeBuilder OnBaseEntityTypeSet(
                [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
                [CanBeNull] EntityType previousBaseType)
            {
                Add(new OnBaseEntityTypeSetNode(entityTypeBuilder, previousBaseType));
                return entityTypeBuilder;
            }

            public virtual Annotation OnEntityTypeAnnotationSet(
                [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
                [NotNull] string name,
                [CanBeNull] Annotation annotation,
                [CanBeNull] Annotation oldAnnotation)
            {
                Add(new OnEntityTypeAnnotationSetNode(entityTypeBuilder, name, annotation, oldAnnotation));
                return annotation;
            }

            public virtual InternalRelationshipBuilder OnForeignKeyAdded([NotNull] InternalRelationshipBuilder relationshipBuilder)
            {
                Add(new OnForeignKeyAddedNode(relationshipBuilder));
                return relationshipBuilder;
            }

            public virtual void OnForeignKeyRemoved([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [NotNull] ForeignKey foreignKey)
                => Add(new OnForeignKeyRemovedNode(entityTypeBuilder, foreignKey));

            public virtual InternalKeyBuilder OnKeyAdded([NotNull] InternalKeyBuilder keyBuilder)
            {
                Add(new OnKeyAddedNode(keyBuilder));
                return keyBuilder;
            }

            public virtual void OnKeyRemoved([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [NotNull] Key key)
                => Add(new OnKeyRemovedNode(entityTypeBuilder, key));

            public virtual void OnPrimaryKeySet(
                [NotNull] InternalEntityTypeBuilder entityTypeBuilder, [CanBeNull] Key previousPrimaryKey)
                => Add(new OnPrimaryKeySetNode(entityTypeBuilder, previousPrimaryKey));

            public virtual InternalIndexBuilder OnIndexAdded([NotNull] InternalIndexBuilder indexBuilder)
            {
                Add(new OnIndexAddedNode(indexBuilder));
                return indexBuilder;
            }

            public virtual void OnIndexRemoved([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [NotNull] Index index)
                => Add(new OnIndexRemovedNode(entityTypeBuilder, index));

            public virtual bool OnIndexUniquenessChanged([NotNull] InternalIndexBuilder indexBuilder)
            {
                Add(new OnIndexUniquenessChangedNode(indexBuilder));
                return true;
            }

            public virtual Annotation OnIndexAnnotationSet(
                [NotNull] InternalIndexBuilder indexBuilder,
                [NotNull] string name,
                [CanBeNull] Annotation annotation,
                [CanBeNull] Annotation oldAnnotation)
            {
                Add(new OnIndexAnnotationSetNode(indexBuilder, name, annotation, oldAnnotation));
                return annotation;
            }

            public virtual InternalRelationshipBuilder OnNavigationAdded(
                [NotNull] InternalRelationshipBuilder relationshipBuilder, [NotNull] Navigation navigation)
            {
                Add(new OnNavigationAddedNode(relationshipBuilder, navigation));
                return relationshipBuilder;
            }

            public virtual void OnNavigationRemoved(
                [NotNull] InternalEntityTypeBuilder sourceEntityTypeBuilder,
                [NotNull] InternalEntityTypeBuilder targetEntityTypeBuilder,
                [NotNull] string navigationName,
                [CanBeNull] PropertyInfo propertyInfo)
                => Add(new OnNavigationRemovedNode(sourceEntityTypeBuilder, targetEntityTypeBuilder, navigationName, propertyInfo));

            public virtual InternalRelationshipBuilder OnForeignKeyUniquenessChanged([NotNull] InternalRelationshipBuilder relationshipBuilder)
            {
                Add(new OnForeignKeyUniquenessChangedNode(relationshipBuilder));
                return relationshipBuilder;
            }

            public virtual InternalRelationshipBuilder OnForeignKeyOwnershipChanged([NotNull] InternalRelationshipBuilder relationshipBuilder)
            {
                Add(new OnForeignKeyOwnershipChangedNode(relationshipBuilder));
                return relationshipBuilder;
            }

            public virtual InternalRelationshipBuilder OnPrincipalEndSet([NotNull] InternalRelationshipBuilder relationshipBuilder)
            {
                Add(new OnPrincipalEndSetNode(relationshipBuilder));
                return relationshipBuilder;
            }

            public virtual InternalPropertyBuilder OnPropertyAdded([NotNull] InternalPropertyBuilder propertyBuilder)
            {
                Add(new OnPropertyAddedNode(propertyBuilder));
                return propertyBuilder;
            }

            public virtual bool OnPropertyNullableChanged([NotNull] InternalPropertyBuilder propertyBuilder)
            {
                Add(new OnPropertyNullableChangedNode(propertyBuilder));
                return true;
            }

            public virtual bool OnPropertyFieldChanged(
                [NotNull] InternalPropertyBuilder propertyBuilder, [CanBeNull] FieldInfo oldFieldInfo)
            {
                Add(new OnPropertyFieldChangedNode(propertyBuilder, oldFieldInfo));
                return true;
            }

            public virtual Annotation OnPropertyAnnotationSet(
                [NotNull] InternalPropertyBuilder propertyBuilder,
                [NotNull] string name,
                [CanBeNull] Annotation annotation,
                [CanBeNull] Annotation oldAnnotation)
            {
                Add(new OnPropertyAnnotationSetNode(propertyBuilder, name, annotation, oldAnnotation));
                return annotation;
            }
        }

        private class ImmediateConventionScope : ConventionScope
        {
            private readonly ConventionSet _conventionSet;

            public ImmediateConventionScope([NotNull] ConventionSet conventionSet)
                : base(parent: null, children: null)
            {
                _conventionSet = conventionSet;
                MakeReadonly();
            }

            public override InternalEntityTypeBuilder OnEntityTypeAdded(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                foreach (var entityTypeConvention in _conventionSet.EntityTypeAddedConventions)
                {
                    entityTypeBuilder = entityTypeConvention.Apply(entityTypeBuilder);
                    if (entityTypeBuilder?.Metadata.Builder == null)
                    {
                        return null;
                    }
                }

                return entityTypeBuilder;
            }

            public override bool OnEntityTypeIgnored(InternalModelBuilder modelBuilder, string name, Type type)
            {
                foreach (var entityTypeIgnoredConvention in _conventionSet.EntityTypeIgnoredConventions)
                {
                    if (!entityTypeIgnoredConvention.Apply(modelBuilder, name, type))
                    {
                        return false;
                    }
                }

                return true;
            }

            public override InternalEntityTypeBuilder OnEntityTypeMemberIgnored(
                InternalEntityTypeBuilder entityTypeBuilder, string ignoredMemberName)
            {
                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                foreach (var entityType in entityTypeBuilder.Metadata.GetDerivedTypesInclusive())
                {
                    foreach (var entityTypeMemberIgnoredConvention in _conventionSet.EntityTypeMemberIgnoredConventions)
                    {
                        if (!entityTypeMemberIgnoredConvention.Apply(entityType.Builder, ignoredMemberName))
                        {
                            return null;
                        }
                    }
                }

                return entityTypeBuilder;
            }

            public override InternalEntityTypeBuilder OnBaseEntityTypeSet(
                InternalEntityTypeBuilder entityTypeBuilder, EntityType previousBaseType)
            {
                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                foreach (var entityTypeConvention in _conventionSet.BaseEntityTypeSetConventions)
                {
                    if (!entityTypeConvention.Apply(entityTypeBuilder, previousBaseType))
                    {
                        return null;
                    }
                }

                return entityTypeBuilder;
            }

            public override Annotation OnEntityTypeAnnotationSet(
                InternalEntityTypeBuilder entityTypeBuilder,
                string name,
                Annotation annotation,
                Annotation oldAnnotation)
            {
                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                foreach (var entityTypeAnnotationSetConvention in _conventionSet.EntityTypeAnnotationSetConventions)
                {
                    var newAnnotation = entityTypeAnnotationSetConvention.Apply(entityTypeBuilder, name, annotation, oldAnnotation);
                    if (newAnnotation != annotation)
                    {
                        return newAnnotation;
                    }
                }

                return annotation;
            }

            public override InternalRelationshipBuilder OnForeignKeyAdded(InternalRelationshipBuilder relationshipBuilder)
            {
                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                foreach (var relationshipConvention in _conventionSet.ForeignKeyAddedConventions)
                {
                    relationshipBuilder = relationshipConvention.Apply(relationshipBuilder);
                    if (relationshipBuilder?.Metadata.Builder == null)
                    {
                        return null;
                    }
                }

                return relationshipBuilder;
            }

            public override void OnForeignKeyRemoved(InternalEntityTypeBuilder entityTypeBuilder, ForeignKey foreignKey)
            {
                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return;
                }

                foreach (var foreignKeyConvention in _conventionSet.ForeignKeyRemovedConventions)
                {
                    foreignKeyConvention.Apply(entityTypeBuilder, foreignKey);
                }
            }

            public override InternalKeyBuilder OnKeyAdded(InternalKeyBuilder keyBuilder)
            {
                if (keyBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                foreach (var keyConvention in _conventionSet.KeyAddedConventions)
                {
                    keyBuilder = keyConvention.Apply(keyBuilder);
                    if (keyBuilder?.Metadata.Builder == null)
                    {
                        return null;
                    }
                }

                return keyBuilder;
            }

            public override void OnKeyRemoved(InternalEntityTypeBuilder entityTypeBuilder, Key key)
            {
                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return;
                }

                foreach (var keyConvention in _conventionSet.KeyRemovedConventions)
                {
                    keyConvention.Apply(entityTypeBuilder, key);
                }
            }

            public override void OnPrimaryKeySet(InternalEntityTypeBuilder entityTypeBuilder, Key previousPrimaryKey)
            {
                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return;
                }

                foreach (var keyConvention in _conventionSet.PrimaryKeySetConventions)
                {
                    if (!keyConvention.Apply(entityTypeBuilder, previousPrimaryKey))
                    {
                        return;
                    }
                }
            }

            public override InternalIndexBuilder OnIndexAdded(InternalIndexBuilder indexBuilder)
            {
                if (indexBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                foreach (var indexConvention in _conventionSet.IndexAddedConventions)
                {
                    indexBuilder = indexConvention.Apply(indexBuilder);
                    if (indexBuilder?.Metadata.Builder == null)
                    {
                        return null;
                    }
                }

                return indexBuilder;
            }

            public override void OnIndexRemoved(InternalEntityTypeBuilder entityTypeBuilder, Index index)
            {
                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return;
                }

                foreach (var indexConvention in _conventionSet.IndexRemovedConventions)
                {
                    indexConvention.Apply(entityTypeBuilder, index);
                }
            }

            public override bool OnIndexUniquenessChanged(InternalIndexBuilder indexBuilder)
            {
                if (indexBuilder.Metadata.Builder == null)
                {
                    return false;
                }

                foreach (var indexUniquenessConvention in _conventionSet.IndexUniquenessConventions)
                {
                    if (!indexUniquenessConvention.Apply(indexBuilder))
                    {
                        return false;
                    }
                }

                return true;
            }

            public override Annotation OnIndexAnnotationSet(
                InternalIndexBuilder indexBuilder,
                string name,
                Annotation annotation,
                Annotation oldAnnotation)
            {
                if (indexBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                foreach (var indexAnnotationSetConvention in _conventionSet.IndexAnnotationSetConventions)
                {
                    var newAnnotation = indexAnnotationSetConvention.Apply(indexBuilder, name, annotation, oldAnnotation);
                    if (newAnnotation != annotation)
                    {
                        return newAnnotation;
                    }
                }

                return annotation;
            }

            public override InternalRelationshipBuilder OnNavigationAdded(
                InternalRelationshipBuilder relationshipBuilder, Navigation navigation)
            {
                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                foreach (var navigationConvention in _conventionSet.NavigationAddedConventions)
                {
                    relationshipBuilder = navigationConvention.Apply(relationshipBuilder, navigation);
                    if (relationshipBuilder?.Metadata.Builder == null)
                    {
                        return null;
                    }
                }

                return relationshipBuilder;
            }

            public override void OnNavigationRemoved(
                InternalEntityTypeBuilder sourceEntityTypeBuilder,
                InternalEntityTypeBuilder targetEntityTypeBuilder,
                string navigationName,
                PropertyInfo propertyInfo)
            {
                if (sourceEntityTypeBuilder.Metadata.Builder == null)
                {
                    return;
                }

                foreach (var navigationConvention in _conventionSet.NavigationRemovedConventions)
                {
                    if (!navigationConvention.Apply(sourceEntityTypeBuilder, targetEntityTypeBuilder, navigationName, propertyInfo))
                    {
                        break;
                    }
                }
            }

            public override InternalRelationshipBuilder OnForeignKeyUniquenessChanged(InternalRelationshipBuilder relationshipBuilder)
            {
                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                foreach (var uniquenessConvention in _conventionSet.ForeignKeyUniquenessConventions)
                {
                    relationshipBuilder = uniquenessConvention.Apply(relationshipBuilder);
                    if (relationshipBuilder?.Metadata.Builder == null)
                    {
                        return null;
                    }
                }

                return relationshipBuilder;
            }

            public override InternalRelationshipBuilder OnForeignKeyOwnershipChanged(InternalRelationshipBuilder relationshipBuilder)
            {
                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                foreach (var ownershipConvention in _conventionSet.ForeignKeyOwnershipConventions)
                {
                    relationshipBuilder = ownershipConvention.Apply(relationshipBuilder);
                    if (relationshipBuilder?.Metadata.Builder == null)
                    {
                        return null;
                    }
                }

                return relationshipBuilder;
            }

            public override InternalRelationshipBuilder OnPrincipalEndSet(InternalRelationshipBuilder relationshipBuilder)
            {
                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                foreach (var relationshipConvention in _conventionSet.PrincipalEndSetConventions)
                {
                    relationshipBuilder = relationshipConvention.Apply(relationshipBuilder);
                    if (relationshipBuilder == null)
                    {
                        break;
                    }
                }

                return relationshipBuilder;
            }

            public override InternalPropertyBuilder OnPropertyAdded(InternalPropertyBuilder propertyBuilder)
            {
                if (propertyBuilder.Metadata.Builder == null
                    || propertyBuilder.Metadata.DeclaringEntityType.Builder == null)
                {
                    return null;
                }

                foreach (var propertyConvention in _conventionSet.PropertyAddedConventions)
                {
                    propertyBuilder = propertyConvention.Apply(propertyBuilder);
                    if (propertyBuilder?.Metadata.Builder == null
                        || propertyBuilder.Metadata.DeclaringEntityType.Builder == null)
                    {
                        return null;
                    }
                }

                return propertyBuilder;
            }

            public override bool OnPropertyNullableChanged(InternalPropertyBuilder propertyBuilder)
            {
                if (propertyBuilder.Metadata.Builder == null
                    || propertyBuilder.Metadata.DeclaringEntityType.Builder == null)
                {
                    return false;
                }

                foreach (var propertyConvention in _conventionSet.PropertyNullableChangedConventions)
                {
                    if (!propertyConvention.Apply(propertyBuilder))
                    {
                        return false;
                    }
                }

                return true;
            }

            public override bool OnPropertyFieldChanged(InternalPropertyBuilder propertyBuilder, FieldInfo oldFieldInfo)
            {
                if (propertyBuilder.Metadata.Builder == null
                    || propertyBuilder.Metadata.DeclaringEntityType.Builder == null)
                {
                    return false;
                }

                foreach (var propertyConvention in _conventionSet.PropertyFieldChangedConventions)
                {
                    if (!propertyConvention.Apply(propertyBuilder, oldFieldInfo))
                    {
                        return false;
                    }
                }

                return true;
            }

            public override Annotation OnPropertyAnnotationSet(
                InternalPropertyBuilder propertyBuilder,
                string name,
                Annotation annotation,
                Annotation oldAnnotation)
            {
                if (propertyBuilder.Metadata.Builder == null
                    || propertyBuilder.Metadata.DeclaringEntityType.Builder == null)
                {
                    return null;
                }

                foreach (var propertyAnnotationSetConvention in _conventionSet.PropertyAnnotationSetConventions)
                {
                    var newAnnotation = propertyAnnotationSetConvention.Apply(propertyBuilder, name, annotation, oldAnnotation);
                    if (newAnnotation != annotation)
                    {
                        return newAnnotation;
                    }
                }

                return annotation;
            }

            public virtual InternalModelBuilder OnModelBuilt([NotNull] InternalModelBuilder modelBuilder)
            {
                foreach (var modelConvention in _conventionSet.ModelBuiltConventions)
                {
                    modelBuilder = modelConvention.Apply(modelBuilder);
                    if (modelBuilder == null)
                    {
                        break;
                    }
                }

                return modelBuilder;
            }

            public virtual InternalModelBuilder OnModelInitialized([NotNull] InternalModelBuilder modelBuilder)
            {
                foreach (var modelConvention in _conventionSet.ModelInitializedConventions)
                {
                    modelBuilder = modelConvention.Apply(modelBuilder);
                    if (modelBuilder == null)
                    {
                        break;
                    }
                }

                return modelBuilder;
            }
        }

        private class OnEntityTypeAddedNode : ConventionNode
        {
            public OnEntityTypeAddedNode(InternalEntityTypeBuilder entityTypeBuilder)
            {
                EntityTypeBuilder = entityTypeBuilder;
            }

            public InternalEntityTypeBuilder EntityTypeBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnEntityTypeAdded(this);
        }

        private class OnEntityTypeIgnoredNode : ConventionNode
        {
            public OnEntityTypeIgnoredNode(InternalModelBuilder modelBuilder, string name, Type type)
            {
                ModelBuilder = modelBuilder;
                Name = name;
                Type = type;
            }

            public InternalModelBuilder ModelBuilder { get; }
            public string Name { get; }
            public Type Type { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnEntityTypeIgnored(this);
        }

        private class OnEntityTypeMemberIgnoredNode : ConventionNode
        {
            public OnEntityTypeMemberIgnoredNode(InternalEntityTypeBuilder entityTypeBuilder, string ignoredMemberName)
            {
                EntityTypeBuilder = entityTypeBuilder;
                IgnoredMemberName = ignoredMemberName;
            }

            public InternalEntityTypeBuilder EntityTypeBuilder { get; }
            public string IgnoredMemberName { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnEntityTypeMemberIgnored(this);
        }

        private class OnBaseEntityTypeSetNode : ConventionNode
        {
            public OnBaseEntityTypeSetNode(InternalEntityTypeBuilder entityTypeBuilder, EntityType previousBaseType)
            {
                EntityTypeBuilder = entityTypeBuilder;
                PreviousBaseType = previousBaseType;
            }

            public InternalEntityTypeBuilder EntityTypeBuilder { get; }
            public EntityType PreviousBaseType { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnBaseEntityTypeSet(this);
        }

        private class OnEntityTypeAnnotationSetNode : ConventionNode
        {
            public OnEntityTypeAnnotationSetNode(
                InternalEntityTypeBuilder entityTypeBuilder,
                string name,
                Annotation annotation,
                Annotation oldAnnotation)
            {
                EntityTypeBuilder = entityTypeBuilder;
                Name = name;
                Annotation = annotation;
                OldAnnotation = oldAnnotation;
            }

            public InternalEntityTypeBuilder EntityTypeBuilder { get; }
            public string Name { get; }
            public Annotation Annotation { get; }
            public Annotation OldAnnotation { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnEntityTypeAnnotationSet(this);
        }

        private class OnForeignKeyAddedNode : ConventionNode
        {
            public OnForeignKeyAddedNode(InternalRelationshipBuilder relationshipBuilder)
            {
                RelationshipBuilder = relationshipBuilder;
            }

            public InternalRelationshipBuilder RelationshipBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnForeignKeyAdded(this);
        }

        private class OnForeignKeyRemovedNode : ConventionNode
        {
            public OnForeignKeyRemovedNode(InternalEntityTypeBuilder entityTypeBuilder, ForeignKey foreignKey)
            {
                EntityTypeBuilder = entityTypeBuilder;
                ForeignKey = foreignKey;
            }

            public InternalEntityTypeBuilder EntityTypeBuilder { get; }
            public ForeignKey ForeignKey { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnForeignKeyRemoved(this);
        }

        private class OnKeyAddedNode : ConventionNode
        {
            public OnKeyAddedNode(InternalKeyBuilder keyBuilder)
            {
                KeyBuilder = keyBuilder;
            }

            public InternalKeyBuilder KeyBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnKeyAdded(this);
        }

        private class OnKeyRemovedNode : ConventionNode
        {
            public OnKeyRemovedNode(InternalEntityTypeBuilder entityTypeBuilder, Key key)
            {
                EntityTypeBuilder = entityTypeBuilder;
                Key = key;
            }

            public InternalEntityTypeBuilder EntityTypeBuilder { get; }
            public Key Key { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnKeyRemoved(this);
        }

        private class OnPrimaryKeySetNode : ConventionNode
        {
            public OnPrimaryKeySetNode(InternalEntityTypeBuilder entityTypeBuilder, Key previousPrimaryKey)
            {
                EntityTypeBuilder = entityTypeBuilder;
                PreviousPrimaryKey = previousPrimaryKey;
            }

            public InternalEntityTypeBuilder EntityTypeBuilder { get; }
            public Key PreviousPrimaryKey { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnPrimaryKeySet(this);
        }

        private class OnIndexAddedNode : ConventionNode
        {
            public OnIndexAddedNode(InternalIndexBuilder indexBuilder)
            {
                IndexBuilder = indexBuilder;
            }

            public InternalIndexBuilder IndexBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnIndexAdded(this);
        }

        private class OnIndexRemovedNode : ConventionNode
        {
            public OnIndexRemovedNode(InternalEntityTypeBuilder entityTypeBuilder, Index index)
            {
                EntityTypeBuilder = entityTypeBuilder;
                Index = index;
            }

            public InternalEntityTypeBuilder EntityTypeBuilder { get; }
            public Index Index { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnIndexRemoved(this);
        }

        private class OnIndexUniquenessChangedNode : ConventionNode
        {
            public OnIndexUniquenessChangedNode(InternalIndexBuilder indexBuilder)
            {
                IndexBuilder = indexBuilder;
            }

            public InternalIndexBuilder IndexBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnIndexUniquenessChanged(this);
        }

        private class OnIndexAnnotationSetNode : ConventionNode
        {
            public OnIndexAnnotationSetNode(InternalIndexBuilder indexBuilder, string name, Annotation annotation, Annotation oldAnnotation)
            {
                IndexBuilder = indexBuilder;
                Name = name;
                Annotation = annotation;
                OldAnnotation = oldAnnotation;
            }

            public InternalIndexBuilder IndexBuilder { get; }
            public string Name { get; }
            public Annotation Annotation { get; }
            public Annotation OldAnnotation { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnIndexAnnotationSet(this);
        }

        private class OnNavigationAddedNode : ConventionNode
        {
            public OnNavigationAddedNode(InternalRelationshipBuilder relationshipBuilder, Navigation navigation)
            {
                RelationshipBuilder = relationshipBuilder;
                Navigation = navigation;
            }

            public InternalRelationshipBuilder RelationshipBuilder { get; }
            public Navigation Navigation { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnNavigationAdded(this);
        }

        private class OnNavigationRemovedNode : ConventionNode
        {
            public OnNavigationRemovedNode(
                InternalEntityTypeBuilder sourceEntityTypeBuilder,
                InternalEntityTypeBuilder targetEntityTypeBuilder,
                string navigationName,
                PropertyInfo propertyInfo)
            {
                SourceEntityTypeBuilder = sourceEntityTypeBuilder;
                TargetEntityTypeBuilder = targetEntityTypeBuilder;
                NavigationName = navigationName;
                PropertyInfo = propertyInfo;
            }

            public InternalEntityTypeBuilder SourceEntityTypeBuilder { get; }
            public InternalEntityTypeBuilder TargetEntityTypeBuilder { get; }
            public string NavigationName { get; }
            public PropertyInfo PropertyInfo { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnNavigationRemoved(this);
        }

        private class OnForeignKeyUniquenessChangedNode : ConventionNode
        {
            public OnForeignKeyUniquenessChangedNode(InternalRelationshipBuilder relationshipBuilder)
            {
                RelationshipBuilder = relationshipBuilder;
            }

            public InternalRelationshipBuilder RelationshipBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnForeignKeyUniquenessChanged(this);
        }

        private class OnForeignKeyOwnershipChangedNode : ConventionNode
        {
            public OnForeignKeyOwnershipChangedNode(InternalRelationshipBuilder relationshipBuilder)
            {
                RelationshipBuilder = relationshipBuilder;
            }

            public InternalRelationshipBuilder RelationshipBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnForeignKeyOwnershipChanged(this);
        }

        private class OnPrincipalEndSetNode : ConventionNode
        {
            public OnPrincipalEndSetNode(InternalRelationshipBuilder relationshipBuilder)
            {
                RelationshipBuilder = relationshipBuilder;
            }

            public InternalRelationshipBuilder RelationshipBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnPrincipalEndSet(this);
        }

        private class OnPropertyAddedNode : ConventionNode
        {
            public OnPropertyAddedNode(InternalPropertyBuilder propertyBuilder)
            {
                PropertyBuilder = propertyBuilder;
            }

            public InternalPropertyBuilder PropertyBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnPropertyAdded(this);
        }

        private class OnPropertyNullableChangedNode : ConventionNode
        {
            public OnPropertyNullableChangedNode(InternalPropertyBuilder propertyBuilder)
            {
                PropertyBuilder = propertyBuilder;
            }

            public InternalPropertyBuilder PropertyBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnPropertyNullableChanged(this);
        }

        private class OnPropertyFieldChangedNode : ConventionNode
        {
            public OnPropertyFieldChangedNode(InternalPropertyBuilder propertyBuilder, FieldInfo oldFieldInfo)
            {
                PropertyBuilder = propertyBuilder;
                OldFieldInfo = oldFieldInfo;
            }

            public InternalPropertyBuilder PropertyBuilder { get; }
            public FieldInfo OldFieldInfo { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnPropertyFieldChanged(this);
        }

        private class OnPropertyAnnotationSetNode : ConventionNode
        {
            public OnPropertyAnnotationSetNode(
                InternalPropertyBuilder propertyBuilder,
                string name,
                Annotation annotation,
                Annotation oldAnnotation)
            {
                PropertyBuilder = propertyBuilder;
                Name = name;
                Annotation = annotation;
                OldAnnotation = oldAnnotation;
            }

            public InternalPropertyBuilder PropertyBuilder { get; }
            public string Name { get; }
            public Annotation Annotation { get; }
            public Annotation OldAnnotation { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnPropertyAnnotationSet(this);
        }
    }
}
