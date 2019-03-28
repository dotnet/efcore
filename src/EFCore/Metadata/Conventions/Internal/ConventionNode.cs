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
        private abstract class ConventionNode
        {
            public abstract ConventionNode Accept(ConventionVisitor visitor);
        }

        private class ConventionScope : ConventionNode
        {
            private List<ConventionNode> _children;
#if DEBUG
            private bool _readonly;
#endif

            public ConventionScope(ConventionScope parent, List<ConventionNode> children = null)
            {
                Parent = parent;
                _children = children;
            }

            public ConventionScope Parent { [DebuggerStepThrough] get; }

            public IReadOnlyList<ConventionNode> Children
            {
                [DebuggerStepThrough] get => _children;
            }

            public int GetLeafCount()
            {
                if (Children == null)
                {
                    return 0;
                }

                var scopesToVisit = new Queue<ConventionScope>();
                scopesToVisit.Enqueue(this);
                var leafCount = 0;
                while (scopesToVisit.Count > 0)
                {
                    var scope = scopesToVisit.Dequeue();
                    if (scope.Children == null)
                    {
                        continue;
                    }

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

            private void Add(ConventionNode node)
            {
#if DEBUG
                Debug.Assert(!_readonly);
#endif

                if (_children == null)
                {
                    _children = new List<ConventionNode>();
                }

                _children.Add(node);
            }

            public void MakeReadonly()
            {
#if DEBUG
                _readonly = true;
#endif
            }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitConventionScope(this);

            public virtual IConventionEntityTypeBuilder OnEntityTypeAdded([NotNull] IConventionEntityTypeBuilder entityTypeBuilder)
            {
                Add(new OnEntityTypeAddedNode(entityTypeBuilder));
                return entityTypeBuilder;
            }

            public virtual string OnEntityTypeIgnored(
                [NotNull] IConventionModelBuilder modelBuilder, [NotNull] string name, [CanBeNull] Type type)
            {
                Add(new OnEntityTypeIgnoredNode(modelBuilder, name, type));
                return name;
            }

            public virtual IConventionEntityType OnEntityTypeRemoved(
                [NotNull] IConventionModelBuilder modelBuilder, [NotNull] IConventionEntityType entityType)
            {
                Add(new OnEntityTypeRemovedNode(modelBuilder, entityType));
                return entityType;
            }

            public virtual string OnEntityTypeMemberIgnored(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
                [NotNull] string name)
            {
                Add(new OnEntityTypeMemberIgnoredNode(entityTypeBuilder, name));
                return name;
            }

            public virtual IConventionEntityType OnEntityTypeBaseTypeChanged(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
                [CanBeNull] IConventionEntityType newBaseType,
                [CanBeNull] IConventionEntityType previousBaseType)
            {
                Add(new OnEntityTypeBaseTypeChangedNode(entityTypeBuilder, newBaseType, previousBaseType));
                return newBaseType;
            }

            public virtual IConventionAnnotation OnEntityTypeAnnotationChanged(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation)
            {
                Add(new OnEntityTypeAnnotationChangedNode(entityTypeBuilder, name, annotation, oldAnnotation));
                return annotation;
            }

            public virtual IConventionAnnotation OnModelAnnotationChanged(
                [NotNull] IConventionModelBuilder modelBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation)
            {
                Add(new OnModelAnnotationChangedNode(modelBuilder, name, annotation, oldAnnotation));
                return annotation;
            }

            public virtual IConventionRelationshipBuilder OnForeignKeyAdded([NotNull] IConventionRelationshipBuilder relationshipBuilder)
            {
                Add(new OnForeignKeyAddedNode(relationshipBuilder));
                return relationshipBuilder;
            }

            public virtual IConventionForeignKey OnForeignKeyRemoved(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder, [NotNull] IConventionForeignKey foreignKey)
            {
                Add(new OnForeignKeyRemovedNode(entityTypeBuilder, foreignKey));
                return foreignKey;
            }

            public virtual IConventionAnnotation OnForeignKeyAnnotationChanged(
                IConventionRelationshipBuilder relationshipBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                Add(new OnForeignKeyAnnotationChangedNode(relationshipBuilder, name, annotation, oldAnnotation));
                return annotation;
            }

            public virtual IConventionKeyBuilder OnKeyAdded([NotNull] IConventionKeyBuilder keyBuilder)
            {
                Add(new OnKeyAddedNode(keyBuilder));
                return keyBuilder;
            }

            public virtual IConventionKey OnKeyRemoved(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder, [NotNull] IConventionKey key)
            {
                Add(new OnKeyRemovedNode(entityTypeBuilder, key));
                return key;
            }

            public virtual IConventionAnnotation OnKeyAnnotationChanged(
                IConventionKeyBuilder keyBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                Add(new OnKeyAnnotationChangedNode(keyBuilder, name, annotation, oldAnnotation));
                return annotation;
            }

            public virtual IConventionKey OnEntityTypePrimaryKeyChanged(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
                [CanBeNull] IConventionKey newPrimaryKey,
                [CanBeNull] IConventionKey previousPrimaryKey)
            {
                Add(new OnEntityTypePrimaryKeyChangedNode(entityTypeBuilder, newPrimaryKey, previousPrimaryKey));
                return newPrimaryKey;
            }

            public virtual IConventionIndexBuilder OnIndexAdded([NotNull] IConventionIndexBuilder indexBuilder)
            {
                Add(new OnIndexAddedNode(indexBuilder));
                return indexBuilder;
            }

            public virtual IConventionIndex OnIndexRemoved(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder, [NotNull] IConventionIndex index)
            {
                Add(new OnIndexRemovedNode(entityTypeBuilder, index));
                return index;
            }

            public virtual IConventionIndexBuilder OnIndexUniquenessChanged([NotNull] IConventionIndexBuilder indexBuilder)
            {
                Add(new OnIndexUniquenessChangedNode(indexBuilder));
                return indexBuilder;
            }

            public virtual IConventionAnnotation OnIndexAnnotationChanged(
                [NotNull] IConventionIndexBuilder indexBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation)
            {
                Add(new OnIndexAnnotationChangedNode(indexBuilder, name, annotation, oldAnnotation));
                return annotation;
            }

            public virtual IConventionNavigation OnNavigationAdded(
                [NotNull] IConventionRelationshipBuilder relationshipBuilder, [NotNull] IConventionNavigation navigation)
            {
                Add(new OnNavigationAddedNode(relationshipBuilder, navigation));
                return navigation;
            }

            public virtual string OnNavigationRemoved(
                [NotNull] IConventionEntityTypeBuilder sourceEntityTypeBuilder,
                [NotNull] IConventionEntityTypeBuilder targetEntityTypeBuilder,
                [NotNull] string navigationName,
                [CanBeNull] MemberInfo memberInfo)
            {
                Add(new OnNavigationRemovedNode(sourceEntityTypeBuilder, targetEntityTypeBuilder, navigationName, memberInfo));
                return navigationName;
            }

            public virtual IConventionRelationshipBuilder OnForeignKeyPropertiesChanged(
                [NotNull] IConventionRelationshipBuilder relationshipBuilder,
                [NotNull] IReadOnlyList<IConventionProperty> oldDependentProperties,
                [NotNull] IConventionKey oldPrincipalKey)
            {
                Add(new OnForeignKeyPropertiesChangedNode(relationshipBuilder, oldDependentProperties, oldPrincipalKey));
                return relationshipBuilder;
            }

            public virtual IConventionRelationshipBuilder OnForeignKeyUniquenessChanged(
                [NotNull] IConventionRelationshipBuilder relationshipBuilder)
            {
                Add(new OnForeignKeyUniquenessChangedNode(relationshipBuilder));
                return relationshipBuilder;
            }

            public virtual IConventionRelationshipBuilder OnForeignKeyRequirednessChanged(
                [NotNull] IConventionRelationshipBuilder relationshipBuilder)
            {
                Add(new OnForeignKeyRequirednessChangedNode(relationshipBuilder));
                return relationshipBuilder;
            }

            public virtual IConventionRelationshipBuilder OnForeignKeyOwnershipChanged(
                [NotNull] IConventionRelationshipBuilder relationshipBuilder)
            {
                Add(new OnForeignKeyOwnershipChangedNode(relationshipBuilder));
                return relationshipBuilder;
            }

            public virtual IConventionRelationshipBuilder OnForeignKeyPrincipalEndChanged(
                [NotNull] IConventionRelationshipBuilder relationshipBuilder)
            {
                Add(new OnForeignKeyPrincipalEndChangedNode(relationshipBuilder));
                return relationshipBuilder;
            }

            public virtual IConventionPropertyBuilder OnPropertyAdded([NotNull] IConventionPropertyBuilder propertyBuilder)
            {
                Add(new OnPropertyAddedNode(propertyBuilder));
                return propertyBuilder;
            }

            public virtual IConventionPropertyBuilder OnPropertyNullableChanged([NotNull] IConventionPropertyBuilder propertyBuilder)
            {
                Add(new OnPropertyNullableChangedNode(propertyBuilder));
                return propertyBuilder;
            }

            public virtual FieldInfo OnPropertyFieldChanged(
                [NotNull] IConventionPropertyBuilder propertyBuilder, FieldInfo newFieldInfo, [CanBeNull] FieldInfo oldFieldInfo)
            {
                Add(new OnPropertyFieldChangedNode(propertyBuilder, newFieldInfo, oldFieldInfo));
                return newFieldInfo;
            }

            public virtual IConventionAnnotation OnPropertyAnnotationChanged(
                [NotNull] IConventionPropertyBuilder propertyBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation)
            {
                Add(new OnPropertyAnnotationChangedNode(propertyBuilder, name, annotation, oldAnnotation));
                return annotation;
            }
        }

        private class ImmediateConventionScope : ConventionScope
        {
            private readonly ConventionSet _conventionSet;
            private readonly ConventionDispatcher _dispatcher;
            private readonly ConventionContext<IConventionEntityTypeBuilder> _entityTypeBuilderConventionContext;
            private readonly ConventionContext<IConventionEntityType> _entityTypeConventionContext;
            private readonly ConventionContext<IConventionRelationshipBuilder> _relationshipBuilderConventionContext;
            private readonly ConventionContext<IConventionForeignKey> _foreignKeyConventionContext;
            private readonly ConventionContext<IConventionNavigation> _navigationConventionContext;
            private readonly ConventionContext<IConventionIndexBuilder> _indexBuilderConventionContext;
            private readonly ConventionContext<IConventionIndex> _indexConventionContext;
            private readonly ConventionContext<IConventionKeyBuilder> _keyBuilderConventionContext;
            private readonly ConventionContext<IConventionKey> _keyConventionContext;
            private readonly ConventionContext<IConventionPropertyBuilder> _propertyBuilderConventionContext;
            private readonly ConventionContext<IConventionModelBuilder> _modelBuilderConventionContext;
            private readonly ConventionContext<IConventionAnnotation> _annotationConventionContext;
            private readonly ConventionContext<string> _stringConventionContext;
            private readonly ConventionContext<FieldInfo> _fieldInfoConventionContext;

            public ImmediateConventionScope([NotNull] ConventionSet conventionSet, ConventionDispatcher dispatcher)
                : base(parent: null)
            {
                _conventionSet = conventionSet;
                _dispatcher = dispatcher;
                _entityTypeBuilderConventionContext = new ConventionContext<IConventionEntityTypeBuilder>(dispatcher);
                _entityTypeConventionContext = new ConventionContext<IConventionEntityType>(dispatcher);
                _relationshipBuilderConventionContext = new ConventionContext<IConventionRelationshipBuilder>(dispatcher);
                _foreignKeyConventionContext = new ConventionContext<IConventionForeignKey>(dispatcher);
                _navigationConventionContext = new ConventionContext<IConventionNavigation>(dispatcher);
                _indexBuilderConventionContext = new ConventionContext<IConventionIndexBuilder>(dispatcher);
                _indexConventionContext = new ConventionContext<IConventionIndex>(dispatcher);
                _keyBuilderConventionContext = new ConventionContext<IConventionKeyBuilder>(dispatcher);
                _keyConventionContext = new ConventionContext<IConventionKey>(dispatcher);
                _propertyBuilderConventionContext = new ConventionContext<IConventionPropertyBuilder>(dispatcher);
                _modelBuilderConventionContext = new ConventionContext<IConventionModelBuilder>(dispatcher);
                _annotationConventionContext = new ConventionContext<IConventionAnnotation>(dispatcher);
                _stringConventionContext = new ConventionContext<string>(dispatcher);
                _fieldInfoConventionContext = new ConventionContext<FieldInfo>(dispatcher);
                MakeReadonly();
            }

            public IConventionModelBuilder OnModelFinalized([NotNull] IConventionModelBuilder modelBuilder)
            {
                _modelBuilderConventionContext.ResetState(modelBuilder);
                foreach (var modelConvention in _conventionSet.ModelFinalizedConventions)
                {
                    // Execute each convention in a separate batch so model validation will get an up-to-date model
                    using (_dispatcher.DelayConventions())
                    {
                        modelConvention.ProcessModelFinalized(modelBuilder, _modelBuilderConventionContext);
                        if (_modelBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _modelBuilderConventionContext.Result;
                        }
                    }
                }

                return modelBuilder;
            }

            public IConventionModelBuilder OnModelInitialized([NotNull] IConventionModelBuilder modelBuilder)
            {
                using (_dispatcher.DelayConventions())
                {
                    _modelBuilderConventionContext.ResetState(modelBuilder);
                    foreach (var modelConvention in _conventionSet.ModelInitializedConventions)
                    {
                        modelConvention.ProcessModelInitialized(modelBuilder, _modelBuilderConventionContext);
                        if (_modelBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _modelBuilderConventionContext.Result;
                        }
                    }
                }

                return modelBuilder;
            }

            public override IConventionAnnotation OnModelAnnotationChanged(
                IConventionModelBuilder modelBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                using (_dispatcher.DelayConventions())
                {
                    _annotationConventionContext.ResetState(annotation);
                    foreach (var modelAnnotationSetConvention in _conventionSet.ModelAnnotationChangedConventions)
                    {
                        modelAnnotationSetConvention.ProcessModelAnnotationChanged(
                            modelBuilder, name, annotation, oldAnnotation, _annotationConventionContext);

                        if (_annotationConventionContext.ShouldStopProcessing())
                        {
                            return _annotationConventionContext.Result;
                        }
                    }
                }

                return annotation;
            }

            public override IConventionEntityTypeBuilder OnEntityTypeAdded(IConventionEntityTypeBuilder entityTypeBuilder)
            {
                using (_dispatcher.DelayConventions())
                {
                    _entityTypeBuilderConventionContext.ResetState(entityTypeBuilder);
                    foreach (var entityTypeConvention in _conventionSet.EntityTypeAddedConventions)
                    {
                        if (entityTypeBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        entityTypeConvention.ProcessEntityTypeAdded(entityTypeBuilder, _entityTypeBuilderConventionContext);
                        if (_entityTypeBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _entityTypeBuilderConventionContext.Result;
                        }
                    }
                }

                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return entityTypeBuilder;
            }

            public override string OnEntityTypeIgnored(IConventionModelBuilder modelBuilder, string name, Type type)
            {
                using (_dispatcher.DelayConventions())
                {
                    _stringConventionContext.ResetState(name);
                    foreach (var entityTypeIgnoredConvention in _conventionSet.EntityTypeIgnoredConventions)
                    {
                        if (!modelBuilder.Metadata.IsIgnored(name))
                        {
                            return null;
                        }

                        entityTypeIgnoredConvention.ProcessEntityTypeIgnored(modelBuilder, name, type, _stringConventionContext);
                        if (_stringConventionContext.ShouldStopProcessing())
                        {
                            return _stringConventionContext.Result;
                        }
                    }
                }

                if (!modelBuilder.Metadata.IsIgnored(name))
                {
                    return null;
                }

                return name;
            }

            public override IConventionEntityType OnEntityTypeRemoved(
                IConventionModelBuilder modelBuilder, IConventionEntityType entityType)
            {
                using (_dispatcher.DelayConventions())
                {
                    _entityTypeConventionContext.ResetState(entityType);
                    foreach (var entityTypeRemovedConvention in _conventionSet.EntityTypeRemovedConventions)
                    {
                        entityTypeRemovedConvention.ProcessEntityTypeRemoved(modelBuilder, entityType, _entityTypeConventionContext);
                        if (_entityTypeConventionContext.ShouldStopProcessing())
                        {
                            return _entityTypeConventionContext.Result;
                        }
                    }
                }

                return entityType;
            }

            public override string OnEntityTypeMemberIgnored(IConventionEntityTypeBuilder entityTypeBuilder, string name)
            {
                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _stringConventionContext.ResetState(name);
                    foreach (var entityType in entityTypeBuilder.Metadata.GetDerivedTypesInclusive())
                    {
                        foreach (var entityTypeMemberIgnoredConvention in _conventionSet.EntityTypeMemberIgnoredConventions)
                        {
                            if (!entityTypeBuilder.Metadata.IsIgnored(name))
                            {
                                return null;
                            }

                            entityTypeMemberIgnoredConvention.ProcessEntityTypeMemberIgnored(
                                entityType.Builder, name, _stringConventionContext);
                            if (_stringConventionContext.ShouldStopProcessing())
                            {
                                return _stringConventionContext.Result;
                            }
                        }
                    }
                }

                if (!entityTypeBuilder.Metadata.IsIgnored(name))
                {
                    return null;
                }

                return name;
            }

            public override IConventionEntityType OnEntityTypeBaseTypeChanged(
                IConventionEntityTypeBuilder entityTypeBuilder,
                IConventionEntityType newBaseType,
                IConventionEntityType previousBaseType)
            {
                using (_dispatcher.DelayConventions())
                {
                    _entityTypeConventionContext.ResetState(newBaseType);
                    foreach (var entityTypeConvention in _conventionSet.EntityTypeBaseTypeChangedConventions)
                    {
                        if (entityTypeBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        entityTypeConvention.ProcessEntityTypeBaseTypeChanged(
                            entityTypeBuilder, newBaseType, previousBaseType, _entityTypeConventionContext);
                        if (_entityTypeConventionContext.ShouldStopProcessing())
                        {
                            return _entityTypeConventionContext.Result;
                        }
                    }
                }

                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return newBaseType;
            }

            public override IConventionKey OnEntityTypePrimaryKeyChanged(
                IConventionEntityTypeBuilder entityTypeBuilder, IConventionKey newPrimaryKey, IConventionKey previousPrimaryKey)
            {
                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _keyConventionContext.ResetState(newPrimaryKey);
                    foreach (var keyConvention in _conventionSet.EntityTypePrimaryKeyChangedConventions)
                    {
                        // Some conventions rely on this running even if the new key has been removed
                        // This will be fixed by reference counting, see #214
                        //if (newPrimaryKey != null && newPrimaryKey.Builder == null)
                        //{
                            //return null;
                        //}

                        keyConvention.ProcessEntityTypePrimaryKeyChanged(
                            entityTypeBuilder, newPrimaryKey, previousPrimaryKey, _keyConventionContext);
                        if (_keyConventionContext.ShouldStopProcessing())
                        {
                            return _keyConventionContext.Result;
                        }
                    }
                }

                if (newPrimaryKey != null && newPrimaryKey.Builder == null)
                {
                    return null;
                }

                return newPrimaryKey;
            }

            public override IConventionAnnotation OnEntityTypeAnnotationChanged(
                IConventionEntityTypeBuilder entityTypeBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                using (_dispatcher.DelayConventions())
                {
                    _annotationConventionContext.ResetState(annotation);
                    foreach (var entityTypeAnnotationSetConvention in _conventionSet.EntityTypeAnnotationChangedConventions)
                    {
                        if (entityTypeBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        entityTypeAnnotationSetConvention.ProcessEntityTypeAnnotationChanged(entityTypeBuilder, name, annotation, oldAnnotation, _annotationConventionContext);
                        if (_annotationConventionContext.ShouldStopProcessing())
                        {
                            return _annotationConventionContext.Result;
                        }
                    }
                }

                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return annotation;
            }

            public override IConventionRelationshipBuilder OnForeignKeyAdded(IConventionRelationshipBuilder relationshipBuilder)
            {
                if (relationshipBuilder.Metadata.DeclaringEntityType.Builder == null
                    || relationshipBuilder.Metadata.PrincipalEntityType.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _relationshipBuilderConventionContext.ResetState(relationshipBuilder);
                    foreach (var relationshipConvention in _conventionSet.ForeignKeyAddedConventions)
                    {
                        if (relationshipBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        relationshipConvention.ProcessForeignKeyAdded(relationshipBuilder, _relationshipBuilderConventionContext);
                        if (_relationshipBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _relationshipBuilderConventionContext.Result;
                        }
                    }
                }

                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return relationshipBuilder;
            }

            public override IConventionForeignKey OnForeignKeyRemoved(
                IConventionEntityTypeBuilder entityTypeBuilder, IConventionForeignKey foreignKey)
            {
                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _foreignKeyConventionContext.ResetState(foreignKey);
                    foreach (var foreignKeyConvention in _conventionSet.ForeignKeyRemovedConventions)
                    {
                        foreignKeyConvention.ProcessForeignKeyRemoved(entityTypeBuilder, foreignKey, _foreignKeyConventionContext);
                        if (_foreignKeyConventionContext.ShouldStopProcessing())
                        {
                            return _foreignKeyConventionContext.Result;
                        }
                    }
                }

                return foreignKey;
            }

            public override IConventionRelationshipBuilder OnForeignKeyPropertiesChanged(
                IConventionRelationshipBuilder relationshipBuilder,
                IReadOnlyList<IConventionProperty> oldDependentProperties,
                IConventionKey oldPrincipalKey)
            {
                using (_dispatcher.DelayConventions())
                {
                    _relationshipBuilderConventionContext.ResetState(relationshipBuilder);
                    foreach (var propertiesChangedConvention in _conventionSet.ForeignKeyPropertiesChangedConventions)
                    {
                        // Some conventions rely on this running even if the relationship has been removed
                        // This will be fixed by reference counting, see #214
                        //if (relationshipBuilder.Metadata.Builder == null)
                        //{
                        //    return null;
                        //}

                        propertiesChangedConvention.ProcessForeignKeyPropertiesChanged(
                            relationshipBuilder, oldDependentProperties, oldPrincipalKey, _relationshipBuilderConventionContext);

                        if (_relationshipBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _relationshipBuilderConventionContext.Result;
                        }
                    }
                }

                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return relationshipBuilder;
            }

            public override IConventionRelationshipBuilder OnForeignKeyUniquenessChanged(IConventionRelationshipBuilder relationshipBuilder)
            {
                using (_dispatcher.DelayConventions())
                {
                    _relationshipBuilderConventionContext.ResetState(relationshipBuilder);
                    foreach (var uniquenessConvention in _conventionSet.ForeignKeyUniquenessChangedConventions)
                    {
                        if (relationshipBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        uniquenessConvention.ProcessForeignKeyUniquenessChanged(
                            relationshipBuilder, _relationshipBuilderConventionContext);

                        if (_relationshipBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _relationshipBuilderConventionContext.Result;
                        }
                    }
                }

                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return relationshipBuilder;
            }

            public override IConventionRelationshipBuilder OnForeignKeyRequirednessChanged(
                IConventionRelationshipBuilder relationshipBuilder)
            {
                using (_dispatcher.DelayConventions())
                {
                    _relationshipBuilderConventionContext.ResetState(relationshipBuilder);
                    foreach (var requirednessConvention in _conventionSet.ForeignKeyRequirednessChangedConventions)
                    {
                        if (relationshipBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        requirednessConvention.ProcessForeignKeyRequirednessChanged(
                            relationshipBuilder, _relationshipBuilderConventionContext);

                        if (_relationshipBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _relationshipBuilderConventionContext.Result;
                        }
                    }
                }

                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return relationshipBuilder;
            }

            public override IConventionRelationshipBuilder OnForeignKeyOwnershipChanged(
                IConventionRelationshipBuilder relationshipBuilder)
            {
                using (_dispatcher.DelayConventions())
                {
                    _relationshipBuilderConventionContext.ResetState(relationshipBuilder);
                    foreach (var ownershipConvention in _conventionSet.ForeignKeyOwnershipChangedConventions)
                    {
                        if (relationshipBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        ownershipConvention.ProcessForeignKeyOwnershipChanged(relationshipBuilder, _relationshipBuilderConventionContext);
                        if (_relationshipBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _relationshipBuilderConventionContext.Result;
                        }
                    }
                }

                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return relationshipBuilder;
            }

            public override IConventionRelationshipBuilder OnForeignKeyPrincipalEndChanged(
                IConventionRelationshipBuilder relationshipBuilder)
            {
                using (_dispatcher.DelayConventions())
                {
                    _relationshipBuilderConventionContext.ResetState(relationshipBuilder);
                    foreach (var relationshipConvention in _conventionSet.ForeignKeyPrincipalEndChangedConventions)
                    {
                        if (relationshipBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        relationshipConvention.ProcessForeignKeyPrincipalEndChanged(
                            relationshipBuilder, _relationshipBuilderConventionContext);
                        if (_relationshipBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _relationshipBuilderConventionContext.Result;
                        }
                    }
                }

                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return relationshipBuilder;
            }

            public override IConventionAnnotation OnForeignKeyAnnotationChanged(
                IConventionRelationshipBuilder relationshipBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _annotationConventionContext.ResetState(annotation);
                    foreach (var indexAnnotationSetConvention in _conventionSet.ForeignKeyAnnotationChangedConventions)
                    {
                        indexAnnotationSetConvention.ProcessForeignKeyAnnotationChanged(
                            relationshipBuilder, name, annotation, oldAnnotation, _annotationConventionContext);
                        if (_annotationConventionContext.ShouldStopProcessing())
                        {
                            return _annotationConventionContext.Result;
                        }
                    }
                }

                return annotation;
            }

            public override IConventionNavigation OnNavigationAdded(
                IConventionRelationshipBuilder relationshipBuilder, IConventionNavigation navigation)
            {
                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    if (relationshipBuilder.Metadata.GetNavigation(navigation.IsDependentToPrincipal()) != navigation)
                    {
                        return null;
                    }

                    _navigationConventionContext.ResetState(navigation);
                    foreach (var navigationConvention in _conventionSet.NavigationAddedConventions)
                    {
                        navigationConvention.ProcessNavigationAdded(relationshipBuilder, navigation, _navigationConventionContext);
                        if (_navigationConventionContext.ShouldStopProcessing())
                        {
                            return _navigationConventionContext.Result;
                        }
                    }
                }

                if (relationshipBuilder.Metadata.GetNavigation(navigation.IsDependentToPrincipal()) != navigation)
                {
                    return null;
                }

                return navigation;
            }

            public override string OnNavigationRemoved(
                IConventionEntityTypeBuilder sourceEntityTypeBuilder,
                IConventionEntityTypeBuilder targetEntityTypeBuilder,
                string navigationName,
                MemberInfo memberInfo)
            {
                if (sourceEntityTypeBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _stringConventionContext.ResetState(navigationName);
                    foreach (var navigationConvention in _conventionSet.NavigationRemovedConventions)
                    {
                        if (sourceEntityTypeBuilder.Metadata.FindNavigation(navigationName) != null)
                        {
                            return null;
                        }

                        navigationConvention.ProcessNavigationRemoved(
                            sourceEntityTypeBuilder, targetEntityTypeBuilder, navigationName, memberInfo, _stringConventionContext);

                        if (_stringConventionContext.ShouldStopProcessing())
                        {
                            return _stringConventionContext.Result;
                        }
                    }
                }

                if (sourceEntityTypeBuilder.Metadata.FindNavigation(navigationName) != null)
                {
                    return null;
                }

                return navigationName;
            }

            public override IConventionKeyBuilder OnKeyAdded(IConventionKeyBuilder keyBuilder)
            {
                if (keyBuilder.Metadata.DeclaringEntityType.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _keyBuilderConventionContext.ResetState(keyBuilder);
                    foreach (var keyConvention in _conventionSet.KeyAddedConventions)
                    {
                        if (keyBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        keyConvention.ProcessKeyAdded(keyBuilder, _keyBuilderConventionContext);
                        if (_keyBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _keyBuilderConventionContext.Result;
                        }
                    }
                }

                if (keyBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return keyBuilder;
            }

            public override IConventionKey OnKeyRemoved(IConventionEntityTypeBuilder entityTypeBuilder, IConventionKey key)
            {
                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _keyConventionContext.ResetState(key);
                    foreach (var keyConvention in _conventionSet.KeyRemovedConventions)
                    {
                        keyConvention.ProcessKeyRemoved(entityTypeBuilder, key, _keyConventionContext);
                        if (_keyConventionContext.ShouldStopProcessing())
                        {
                            return _keyConventionContext.Result;
                        }
                    }
                }

                return key;
            }

            public override IConventionAnnotation OnKeyAnnotationChanged(
                IConventionKeyBuilder keyBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                if (keyBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _annotationConventionContext.ResetState(annotation);
                    foreach (var keyAnnotationSetConvention in _conventionSet.KeyAnnotationChangedConventions)
                    {
                        keyAnnotationSetConvention.ProcessKeyAnnotationChanged(
                            keyBuilder, name, annotation, oldAnnotation, _annotationConventionContext);
                        if (_annotationConventionContext.ShouldStopProcessing())
                        {
                            return _annotationConventionContext.Result;
                        }
                    }
                }

                return annotation;
            }

            public override IConventionIndexBuilder OnIndexAdded(IConventionIndexBuilder indexBuilder)
            {
                if (indexBuilder.Metadata.DeclaringEntityType.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _indexBuilderConventionContext.ResetState(indexBuilder);
                    foreach (var indexConvention in _conventionSet.IndexAddedConventions)
                    {
                        if (indexBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        indexConvention.ProcessIndexAdded(indexBuilder, _indexBuilderConventionContext);
                        if (_indexBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _indexBuilderConventionContext.Result;
                        }
                    }
                }

                if (indexBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return indexBuilder;
            }

            public override IConventionIndex OnIndexRemoved(IConventionEntityTypeBuilder entityTypeBuilder, IConventionIndex index)
            {
                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _indexConventionContext.ResetState(index);
                    foreach (var indexConvention in _conventionSet.IndexRemovedConventions)
                    {
                        indexConvention.ProcessIndexRemoved(entityTypeBuilder, index, _indexConventionContext);
                        if (_indexConventionContext.ShouldStopProcessing())
                        {
                            return _indexConventionContext.Result;
                        }
                    }
                }

                return index;
            }

            public override IConventionIndexBuilder OnIndexUniquenessChanged(IConventionIndexBuilder indexBuilder)
            {
                using (_dispatcher.DelayConventions())
                {
                    _indexBuilderConventionContext.ResetState(indexBuilder);
                    foreach (var indexUniquenessConvention in _conventionSet.IndexUniquenessChangedConventions)
                    {
                        if (indexBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        indexUniquenessConvention.ProcessIndexUniquenessChanged(indexBuilder, _indexBuilderConventionContext);
                        if (_indexBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _indexBuilderConventionContext.Result;
                        }
                    }
                }

                if (indexBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return indexBuilder;
            }

            public override IConventionAnnotation OnIndexAnnotationChanged(
                IConventionIndexBuilder indexBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                if (indexBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _annotationConventionContext.ResetState(annotation);
                    foreach (var indexAnnotationSetConvention in _conventionSet.IndexAnnotationChangedConventions)
                    {
                        indexAnnotationSetConvention.ProcessIndexAnnotationChanged(
                            indexBuilder, name, annotation, oldAnnotation, _annotationConventionContext);
                        if (_annotationConventionContext.ShouldStopProcessing())
                        {
                            return _annotationConventionContext.Result;
                        }
                    }
                }

                return annotation;
            }

            public override IConventionPropertyBuilder OnPropertyAdded(IConventionPropertyBuilder propertyBuilder)
            {
                if (propertyBuilder.Metadata.DeclaringEntityType.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _propertyBuilderConventionContext.ResetState(propertyBuilder);
                    foreach (var propertyConvention in _conventionSet.PropertyAddedConventions)
                    {
                        if (propertyBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        propertyConvention.ProcessPropertyAdded(propertyBuilder, _propertyBuilderConventionContext);
                        if (_propertyBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _propertyBuilderConventionContext.Result;
                        }
                    }
                }

                if (propertyBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return propertyBuilder;
            }

            public override IConventionPropertyBuilder OnPropertyNullableChanged(IConventionPropertyBuilder propertyBuilder)
            {
                if (propertyBuilder.Metadata.DeclaringEntityType.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _propertyBuilderConventionContext.ResetState(propertyBuilder);
                    foreach (var propertyConvention in _conventionSet.PropertyNullabilityChangedConventions)
                    {
                        if (propertyBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        propertyConvention.ProcessPropertyNullabilityChanged(propertyBuilder, _propertyBuilderConventionContext);
                        if (_propertyBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _propertyBuilderConventionContext.Result;
                        }
                    }
                }

                if (propertyBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return propertyBuilder;
            }

            public override FieldInfo OnPropertyFieldChanged(
                IConventionPropertyBuilder propertyBuilder, FieldInfo newFieldInfo, FieldInfo oldFieldInfo)
            {
                if (propertyBuilder.Metadata.Builder == null
                    || propertyBuilder.Metadata.DeclaringEntityType.Builder == null)
                {
                    return null;
                }

                _fieldInfoConventionContext.ResetState(newFieldInfo);
                foreach (var propertyConvention in _conventionSet.PropertyFieldChangedConventions)
                {
                    propertyConvention.ProcessPropertyFieldChanged(
                        propertyBuilder, newFieldInfo, oldFieldInfo, _fieldInfoConventionContext);
                    if (_fieldInfoConventionContext.ShouldStopProcessing())
                    {
                        return _fieldInfoConventionContext.Result;
                    }
                }

                return newFieldInfo;
            }

            public override IConventionAnnotation OnPropertyAnnotationChanged(
                IConventionPropertyBuilder propertyBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                if (propertyBuilder.Metadata.Builder == null
                    || propertyBuilder.Metadata.DeclaringEntityType.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _annotationConventionContext.ResetState(annotation);
                    foreach (var propertyAnnotationSetConvention in _conventionSet.PropertyAnnotationChangedConventions)
                    {
                        propertyAnnotationSetConvention.ProcessPropertyAnnotationChanged(
                            propertyBuilder, name, annotation, oldAnnotation, _annotationConventionContext);

                        if (_annotationConventionContext.ShouldStopProcessing())
                        {
                            return _annotationConventionContext.Result;
                        }
                    }
                }

                return annotation;
            }
        }

        private class OnModelAnnotationChangedNode : ConventionNode
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

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnModelAnnotationChanged(this);
        }

        private class OnEntityTypeAddedNode : ConventionNode
        {
            public OnEntityTypeAddedNode(IConventionEntityTypeBuilder entityTypeBuilder)
            {
                EntityTypeBuilder = entityTypeBuilder;
            }

            public IConventionEntityTypeBuilder EntityTypeBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnEntityTypeAdded(this);
        }

        private class OnEntityTypeIgnoredNode : ConventionNode
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

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnEntityTypeIgnored(this);
        }

        private class OnEntityTypeRemovedNode : ConventionNode
        {
            public OnEntityTypeRemovedNode(IConventionModelBuilder modelBuilder, IConventionEntityType entityType)
            {
                ModelBuilder = modelBuilder;
                EntityType = entityType;
            }

            public IConventionModelBuilder ModelBuilder { get; }
            public IConventionEntityType EntityType { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnEntityTypeRemoved(this);
        }

        private class OnEntityTypeMemberIgnoredNode : ConventionNode
        {
            public OnEntityTypeMemberIgnoredNode(IConventionEntityTypeBuilder entityTypeBuilder, string name)
            {
                EntityTypeBuilder = entityTypeBuilder;
                Name = name;
            }

            public IConventionEntityTypeBuilder EntityTypeBuilder { get; }
            public string Name { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnEntityTypeMemberIgnored(this);
        }

        private class OnEntityTypeBaseTypeChangedNode : ConventionNode
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

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnBaseEntityTypeChanged(this);
        }

        private class OnEntityTypeAnnotationChangedNode : ConventionNode
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

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnEntityTypeAnnotationChanged(this);
        }

        private class OnForeignKeyAddedNode : ConventionNode
        {
            public OnForeignKeyAddedNode(IConventionRelationshipBuilder relationshipBuilder)
            {
                RelationshipBuilder = relationshipBuilder;
            }

            public IConventionRelationshipBuilder RelationshipBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnForeignKeyAdded(this);
        }

        private class OnForeignKeyRemovedNode : ConventionNode
        {
            public OnForeignKeyRemovedNode(IConventionEntityTypeBuilder entityTypeBuilder, IConventionForeignKey foreignKey)
            {
                EntityTypeBuilder = entityTypeBuilder;
                ForeignKey = foreignKey;
            }

            public IConventionEntityTypeBuilder EntityTypeBuilder { get; }
            public IConventionForeignKey ForeignKey { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnForeignKeyRemoved(this);
        }

        private class OnForeignKeyAnnotationChangedNode : ConventionNode
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

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnForeignKeyAnnotationChanged(this);
        }

        private class OnForeignKeyPropertiesChangedNode : ConventionNode
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

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnForeignKeyPropertiesChanged(this);
        }

        private class OnForeignKeyUniquenessChangedNode : ConventionNode
        {
            public OnForeignKeyUniquenessChangedNode(IConventionRelationshipBuilder relationshipBuilder)
            {
                RelationshipBuilder = relationshipBuilder;
            }

            public IConventionRelationshipBuilder RelationshipBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnForeignKeyUniquenessChanged(this);
        }

        private class OnForeignKeyRequirednessChangedNode : ConventionNode
        {
            public OnForeignKeyRequirednessChangedNode(IConventionRelationshipBuilder relationshipBuilder)
            {
                RelationshipBuilder = relationshipBuilder;
            }

            public IConventionRelationshipBuilder RelationshipBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnForeignKeyRequirednessChanged(this);
        }

        private class OnForeignKeyOwnershipChangedNode : ConventionNode
        {
            public OnForeignKeyOwnershipChangedNode(IConventionRelationshipBuilder relationshipBuilder)
            {
                RelationshipBuilder = relationshipBuilder;
            }

            public IConventionRelationshipBuilder RelationshipBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnForeignKeyOwnershipChanged(this);
        }

        private class OnForeignKeyPrincipalEndChangedNode : ConventionNode
        {
            public OnForeignKeyPrincipalEndChangedNode(IConventionRelationshipBuilder relationshipBuilder)
            {
                RelationshipBuilder = relationshipBuilder;
            }

            public IConventionRelationshipBuilder RelationshipBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnForeignKeyPrincipalEndChanged(this);
        }

        private class OnNavigationAddedNode : ConventionNode
        {
            public OnNavigationAddedNode(IConventionRelationshipBuilder relationshipBuilder, IConventionNavigation navigation)
            {
                RelationshipBuilder = relationshipBuilder;
                Navigation = navigation;
            }

            public IConventionRelationshipBuilder RelationshipBuilder { get; }
            public IConventionNavigation Navigation { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnNavigationAdded(this);
        }

        private class OnNavigationRemovedNode : ConventionNode
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

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnNavigationRemoved(this);
        }

        private class OnKeyAddedNode : ConventionNode
        {
            public OnKeyAddedNode(IConventionKeyBuilder keyBuilder)
            {
                KeyBuilder = keyBuilder;
            }

            public IConventionKeyBuilder KeyBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnKeyAdded(this);
        }

        private class OnKeyRemovedNode : ConventionNode
        {
            public OnKeyRemovedNode(IConventionEntityTypeBuilder entityTypeBuilder, IConventionKey key)
            {
                EntityTypeBuilder = entityTypeBuilder;
                Key = key;
            }

            public IConventionEntityTypeBuilder EntityTypeBuilder { get; }
            public IConventionKey Key { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnKeyRemoved(this);
        }
        
        private class OnKeyAnnotationChangedNode : ConventionNode
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

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnKeyAnnotationChanged(this);
        }

        private class OnEntityTypePrimaryKeyChangedNode : ConventionNode
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

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnPrimaryKeyChanged(this);
        }

        private class OnIndexAddedNode : ConventionNode
        {
            public OnIndexAddedNode(IConventionIndexBuilder indexBuilder)
            {
                IndexBuilder = indexBuilder;
            }

            public IConventionIndexBuilder IndexBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnIndexAdded(this);
        }

        private class OnIndexRemovedNode : ConventionNode
        {
            public OnIndexRemovedNode(IConventionEntityTypeBuilder entityTypeBuilder, IConventionIndex index)
            {
                EntityTypeBuilder = entityTypeBuilder;
                Index = index;
            }

            public IConventionEntityTypeBuilder EntityTypeBuilder { get; }
            public IConventionIndex Index { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnIndexRemoved(this);
        }

        private class OnIndexUniquenessChangedNode : ConventionNode
        {
            public OnIndexUniquenessChangedNode(IConventionIndexBuilder indexBuilder)
            {
                IndexBuilder = indexBuilder;
            }

            public IConventionIndexBuilder IndexBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnIndexUniquenessChanged(this);
        }

        private class OnIndexAnnotationChangedNode : ConventionNode
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

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnIndexAnnotationChanged(this);
        }

        private class OnPropertyAddedNode : ConventionNode
        {
            public OnPropertyAddedNode(IConventionPropertyBuilder propertyBuilder)
            {
                PropertyBuilder = propertyBuilder;
            }

            public IConventionPropertyBuilder PropertyBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnPropertyAdded(this);
        }

        private class OnPropertyNullableChangedNode : ConventionNode
        {
            public OnPropertyNullableChangedNode(IConventionPropertyBuilder propertyBuilder)
            {
                PropertyBuilder = propertyBuilder;
            }

            public IConventionPropertyBuilder PropertyBuilder { get; }

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnPropertyNullableChanged(this);
        }

        private class OnPropertyFieldChangedNode : ConventionNode
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

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnPropertyFieldChanged(this);
        }

        private class OnPropertyAnnotationChangedNode : ConventionNode
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

            public override ConventionNode Accept(ConventionVisitor visitor) => visitor.VisitOnPropertyAnnotationChanged(this);
        }
    }
}
