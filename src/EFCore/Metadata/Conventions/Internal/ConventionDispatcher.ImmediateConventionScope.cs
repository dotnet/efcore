// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public partial class ConventionDispatcher
    {
        private sealed class ImmediateConventionScope : ConventionScope
        {
            private readonly ConventionSet _conventionSet;
            private readonly ConventionDispatcher _dispatcher;
            private readonly ConventionContext<IConventionEntityTypeBuilder> _entityTypeBuilderConventionContext;
            private readonly ConventionContext<IConventionEntityType> _entityTypeConventionContext;
            private readonly ConventionContext<IConventionForeignKeyBuilder> _relationshipBuilderConventionContext;
            private readonly ConventionContext<IConventionForeignKey> _foreignKeyConventionContext;
            private readonly ConventionContext<IConventionSkipNavigationBuilder> _skipNavigationBuilderConventionContext;
            private readonly ConventionContext<IConventionSkipNavigation> _skipNavigationConventionContext;
            private readonly ConventionContext<IConventionNavigationBuilder> _navigationConventionBuilderContext;
            private readonly ConventionContext<IConventionNavigation> _navigationConventionContext;
            private readonly ConventionContext<IConventionIndexBuilder> _indexBuilderConventionContext;
            private readonly ConventionContext<IConventionIndex> _indexConventionContext;
            private readonly ConventionContext<IConventionKeyBuilder> _keyBuilderConventionContext;
            private readonly ConventionContext<IConventionKey> _keyConventionContext;
            private readonly ConventionContext<IConventionPropertyBuilder> _propertyBuilderConventionContext;
            private readonly ConventionContext<IConventionProperty> _propertyConventionContext;
            private readonly ConventionContext<IConventionModelBuilder> _modelBuilderConventionContext;
            private readonly ConventionContext<IConventionAnnotation> _annotationConventionContext;
            private readonly ConventionContext<IReadOnlyList<IConventionProperty>> _propertyListConventionContext;
            private readonly ConventionContext<string> _stringConventionContext;
            private readonly ConventionContext<FieldInfo> _fieldInfoConventionContext;
            private readonly ConventionContext<bool?> _boolConventionContext;

            public ImmediateConventionScope([NotNull] ConventionSet conventionSet, ConventionDispatcher dispatcher)
            {
                _conventionSet = conventionSet;
                _dispatcher = dispatcher;
                _entityTypeBuilderConventionContext = new ConventionContext<IConventionEntityTypeBuilder>(dispatcher);
                _entityTypeConventionContext = new ConventionContext<IConventionEntityType>(dispatcher);
                _relationshipBuilderConventionContext = new ConventionContext<IConventionForeignKeyBuilder>(dispatcher);
                _foreignKeyConventionContext = new ConventionContext<IConventionForeignKey>(dispatcher);
                _skipNavigationBuilderConventionContext = new ConventionContext<IConventionSkipNavigationBuilder>(dispatcher);
                _skipNavigationConventionContext = new ConventionContext<IConventionSkipNavigation>(dispatcher);
                _navigationConventionBuilderContext = new ConventionContext<IConventionNavigationBuilder>(dispatcher);
                _navigationConventionContext = new ConventionContext<IConventionNavigation>(dispatcher);
                _indexBuilderConventionContext = new ConventionContext<IConventionIndexBuilder>(dispatcher);
                _indexConventionContext = new ConventionContext<IConventionIndex>(dispatcher);
                _keyBuilderConventionContext = new ConventionContext<IConventionKeyBuilder>(dispatcher);
                _keyConventionContext = new ConventionContext<IConventionKey>(dispatcher);
                _propertyBuilderConventionContext = new ConventionContext<IConventionPropertyBuilder>(dispatcher);
                _propertyConventionContext = new ConventionContext<IConventionProperty>(dispatcher);
                _modelBuilderConventionContext = new ConventionContext<IConventionModelBuilder>(dispatcher);
                _annotationConventionContext = new ConventionContext<IConventionAnnotation>(dispatcher);
                _propertyListConventionContext = new ConventionContext<IReadOnlyList<IConventionProperty>>(dispatcher);
                _stringConventionContext = new ConventionContext<string>(dispatcher);
                _fieldInfoConventionContext = new ConventionContext<FieldInfo>(dispatcher);
                _boolConventionContext = new ConventionContext<bool?>(dispatcher);
            }

            public override void Run(ConventionDispatcher dispatcher)
                => throw new NotImplementedException(CoreStrings.ImmediateConventionScopeCannotBeRunAgain);

            public IConventionModelBuilder OnModelFinalizing([NotNull] IConventionModelBuilder modelBuilder)
            {
                _modelBuilderConventionContext.ResetState(modelBuilder);
                foreach (var modelConvention in _conventionSet.ModelFinalizingConventions)
                {
                    // Execute each convention in a separate batch so model validation will get an up-to-date model
                    using (_dispatcher.DelayConventions())
                    {
                        modelConvention.ProcessModelFinalizing(modelBuilder, _modelBuilderConventionContext);
                        if (_modelBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _modelBuilderConventionContext.Result;
                        }
                    }
                }

                return modelBuilder;
            }

            public IModel OnModelFinalized([NotNull] IModel model)
            {
                foreach (var modelConvention in _conventionSet.ModelFinalizedConventions)
                {
                    model = modelConvention.ProcessModelFinalized(model);
                }

                return model;
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
                    foreach (var modelConvention in _conventionSet.ModelAnnotationChangedConventions)
                    {
                        modelConvention.ProcessModelAnnotationChanged(
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
                    foreach (var entityTypeConvention in _conventionSet.EntityTypeIgnoredConventions)
                    {
                        if (!modelBuilder.Metadata.IsIgnored(name))
                        {
                            return null;
                        }

                        entityTypeConvention.ProcessEntityTypeIgnored(modelBuilder, name, type, _stringConventionContext);
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
                IConventionModelBuilder modelBuilder,
                IConventionEntityType entityType)
            {
                using (_dispatcher.DelayConventions())
                {
                    _entityTypeConventionContext.ResetState(entityType);
                    foreach (var entityTypeConvention in _conventionSet.EntityTypeRemovedConventions)
                    {
                        entityTypeConvention.ProcessEntityTypeRemoved(modelBuilder, entityType, _entityTypeConventionContext);
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
                        foreach (var entityTypeConvention in _conventionSet.EntityTypeMemberIgnoredConventions)
                        {
                            if (!entityTypeBuilder.Metadata.IsIgnored(name))
                            {
                                return null;
                            }

                            entityTypeConvention.ProcessEntityTypeMemberIgnored(
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
                IConventionEntityTypeBuilder entityTypeBuilder,
                IConventionKey newPrimaryKey,
                IConventionKey previousPrimaryKey)
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
                        // This will be fixed by reference counting, see #15898
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

                if (newPrimaryKey != null
                    && newPrimaryKey.Builder == null)
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
                    foreach (var entityTypeConvention in _conventionSet.EntityTypeAnnotationChangedConventions)
                    {
                        if (entityTypeBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        entityTypeConvention.ProcessEntityTypeAnnotationChanged(
                            entityTypeBuilder, name, annotation, oldAnnotation, _annotationConventionContext);
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

            public override IConventionForeignKeyBuilder OnForeignKeyAdded(IConventionForeignKeyBuilder relationshipBuilder)
            {
                if (relationshipBuilder.Metadata.DeclaringEntityType.Builder == null
                    || relationshipBuilder.Metadata.PrincipalEntityType.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _relationshipBuilderConventionContext.ResetState(relationshipBuilder);
                    foreach (var foreignKeyConvention in _conventionSet.ForeignKeyAddedConventions)
                    {
                        if (relationshipBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        foreignKeyConvention.ProcessForeignKeyAdded(relationshipBuilder, _relationshipBuilderConventionContext);
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
                IConventionEntityTypeBuilder entityTypeBuilder,
                IConventionForeignKey foreignKey)
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

            public override IReadOnlyList<IConventionProperty> OnForeignKeyPropertiesChanged(
                IConventionForeignKeyBuilder relationshipBuilder,
                IReadOnlyList<IConventionProperty> oldDependentProperties,
                IConventionKey oldPrincipalKey)
            {
                using (_dispatcher.DelayConventions())
                {
                    _propertyListConventionContext.ResetState(relationshipBuilder.Metadata.Properties);
                    foreach (var foreignKeyConvention in _conventionSet.ForeignKeyPropertiesChangedConventions)
                    {
                        // Some conventions rely on this running even if the relationship has been removed
                        // This will be fixed by reference counting, see #15898
                        //if (relationshipBuilder.Metadata.Builder == null)
                        //{
                        //    return null;
                        //}

                        foreignKeyConvention.ProcessForeignKeyPropertiesChanged(
                            relationshipBuilder, oldDependentProperties, oldPrincipalKey, _propertyListConventionContext);

                        if (_propertyListConventionContext.ShouldStopProcessing())
                        {
                            if (_propertyListConventionContext.Result != null)
                            {
                                // Preserve the old configuration to let the conventions finish processing them
                                _dispatcher.OnForeignKeyPropertiesChanged(relationshipBuilder, oldDependentProperties, oldPrincipalKey);
                            }

                            return _propertyListConventionContext.Result;
                        }
                    }
                }

                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return relationshipBuilder.Metadata.Properties;
            }

            public override bool? OnForeignKeyUniquenessChanged(IConventionForeignKeyBuilder relationshipBuilder)
            {
                using (_dispatcher.DelayConventions())
                {
                    _boolConventionContext.ResetState(relationshipBuilder.Metadata.IsUnique);
                    foreach (var foreignKeyConvention in _conventionSet.ForeignKeyUniquenessChangedConventions)
                    {
                        if (relationshipBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        foreignKeyConvention.ProcessForeignKeyUniquenessChanged(relationshipBuilder, _boolConventionContext);

                        if (_boolConventionContext.ShouldStopProcessing())
                        {
                            return _boolConventionContext.Result;
                        }
                    }
                }

                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return _boolConventionContext.Result;
            }

            public override bool? OnForeignKeyRequirednessChanged(
                IConventionForeignKeyBuilder relationshipBuilder)
            {
                using (_dispatcher.DelayConventions())
                {
                    _boolConventionContext.ResetState(relationshipBuilder.Metadata.IsRequired);
                    foreach (var foreignKeyConvention in _conventionSet.ForeignKeyRequirednessChangedConventions)
                    {
                        if (relationshipBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        foreignKeyConvention.ProcessForeignKeyRequirednessChanged(relationshipBuilder, _boolConventionContext);

                        if (_boolConventionContext.ShouldStopProcessing())
                        {
                            return _boolConventionContext.Result;
                        }
                    }
                }

                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return _boolConventionContext.Result;
            }

            public override bool? OnForeignKeyDependentRequirednessChanged(
                [NotNull] IConventionForeignKeyBuilder relationshipBuilder)
            {
                using (_dispatcher.DelayConventions())
                {
                    _boolConventionContext.ResetState(relationshipBuilder.Metadata.IsRequiredDependent);
                    foreach (var foreignKeyConvention in _conventionSet.ForeignKeyDependentRequirednessChangedConventions)
                    {
                        if (relationshipBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        foreignKeyConvention.ProcessForeignKeyDependentRequirednessChanged(relationshipBuilder, _boolConventionContext);

                        if (_boolConventionContext.ShouldStopProcessing())
                        {
                            return _boolConventionContext.Result;
                        }
                    }
                }

                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return _boolConventionContext.Result;
            }

            public override bool? OnForeignKeyOwnershipChanged(
                IConventionForeignKeyBuilder relationshipBuilder)
            {
                using (_dispatcher.DelayConventions())
                {
                    _boolConventionContext.ResetState(relationshipBuilder.Metadata.IsOwnership);
                    foreach (var foreignKeyConvention in _conventionSet.ForeignKeyOwnershipChangedConventions)
                    {
                        if (relationshipBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        foreignKeyConvention.ProcessForeignKeyOwnershipChanged(relationshipBuilder, _boolConventionContext);
                        if (_boolConventionContext.ShouldStopProcessing())
                        {
                            return _boolConventionContext.Result;
                        }
                    }
                }

                if (relationshipBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return _boolConventionContext.Result;
            }

            public override IConventionForeignKeyBuilder OnForeignKeyPrincipalEndChanged(
                IConventionForeignKeyBuilder relationshipBuilder)
            {
                using (_dispatcher.DelayConventions())
                {
                    _relationshipBuilderConventionContext.ResetState(relationshipBuilder);
                    foreach (var foreignKeyConvention in _conventionSet.ForeignKeyPrincipalEndChangedConventions)
                    {
                        if (relationshipBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        foreignKeyConvention.ProcessForeignKeyPrincipalEndChanged(
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
                IConventionForeignKeyBuilder relationshipBuilder,
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
                    foreach (var foreignKeyConvention in _conventionSet.ForeignKeyAnnotationChangedConventions)
                    {
                        foreignKeyConvention.ProcessForeignKeyAnnotationChanged(
                            relationshipBuilder, name, annotation, oldAnnotation, _annotationConventionContext);
                        if (_annotationConventionContext.ShouldStopProcessing())
                        {
                            return _annotationConventionContext.Result;
                        }
                    }
                }

                return annotation;
            }

            public override IConventionNavigationBuilder OnNavigationAdded(IConventionNavigationBuilder navigationBuilder)
            {
                if (navigationBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _navigationConventionBuilderContext.ResetState(navigationBuilder);
                    foreach (var navigationConvention in _conventionSet.NavigationAddedConventions)
                    {
                        navigationConvention.ProcessNavigationAdded(navigationBuilder, _navigationConventionBuilderContext);
                        if (_navigationConventionBuilderContext.ShouldStopProcessing())
                        {
                            return _navigationConventionBuilderContext.Result;
                        }
                    }
                }

                if (navigationBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return navigationBuilder;
            }

            public override IConventionAnnotation OnNavigationAnnotationChanged(
                IConventionForeignKeyBuilder relationshipBuilder,
                IConventionNavigation navigation,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                if (relationshipBuilder.Metadata.Builder == null
                    || relationshipBuilder.Metadata.GetNavigation(navigation.IsOnDependent) != navigation)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _annotationConventionContext.ResetState(annotation);
                    foreach (var navigationConvention in _conventionSet.NavigationAnnotationChangedConventions)
                    {
                        navigationConvention.ProcessNavigationAnnotationChanged(
                            relationshipBuilder, navigation, name, annotation, oldAnnotation, _annotationConventionContext);
                        if (_annotationConventionContext.ShouldStopProcessing())
                        {
                            return _annotationConventionContext.Result;
                        }
                    }
                }

                return annotation;
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

            public override IConventionSkipNavigationBuilder OnSkipNavigationAdded(
                IConventionSkipNavigationBuilder navigationBuilder)
            {
                if (navigationBuilder.Metadata.DeclaringEntityType.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _skipNavigationBuilderConventionContext.ResetState(navigationBuilder);
                    foreach (var skipNavigationConvention in _conventionSet.SkipNavigationAddedConventions)
                    {
                        if (navigationBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        skipNavigationConvention.ProcessSkipNavigationAdded(navigationBuilder, _skipNavigationBuilderConventionContext);
                        if (_skipNavigationBuilderConventionContext.ShouldStopProcessing())
                        {
                            return _skipNavigationBuilderConventionContext.Result;
                        }
                    }
                }

                if (navigationBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return navigationBuilder;
            }

            public override IConventionAnnotation OnSkipNavigationAnnotationChanged(
                IConventionSkipNavigationBuilder navigationBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation)
            {
                if (navigationBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _annotationConventionContext.ResetState(annotation);
                    foreach (var skipNavigationConvention in _conventionSet.SkipNavigationAnnotationChangedConventions)
                    {
                        if (navigationBuilder.Metadata.Builder != null
                            && navigationBuilder.Metadata.FindAnnotation(name) != annotation)
                        {
                            Check.DebugAssert(false, "annotation removed");
                            return null;
                        }

                        skipNavigationConvention.ProcessSkipNavigationAnnotationChanged(
                            navigationBuilder, name, annotation, oldAnnotation, _annotationConventionContext);
                        if (_annotationConventionContext.ShouldStopProcessing())
                        {
                            return _annotationConventionContext.Result;
                        }
                    }
                }

                return annotation;
            }

            public override IConventionForeignKey OnSkipNavigationForeignKeyChanged(
                IConventionSkipNavigationBuilder navigationBuilder,
                IConventionForeignKey foreignKey,
                IConventionForeignKey oldForeignKey)
            {
                if (navigationBuilder.Metadata.DeclaringEntityType.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _foreignKeyConventionContext.ResetState(foreignKey);
                    foreach (var skipNavigationConvention in _conventionSet.SkipNavigationForeignKeyChangedConventions)
                    {
                        skipNavigationConvention.ProcessSkipNavigationForeignKeyChanged(
                            navigationBuilder, foreignKey, oldForeignKey, _foreignKeyConventionContext);
                        if (_foreignKeyConventionContext.ShouldStopProcessing())
                        {
                            if (_foreignKeyConventionContext.Result != null)
                            {
                                // Preserve the old configuration to let the conventions finish processing them
                                _dispatcher.OnSkipNavigationForeignKeyChanged(navigationBuilder, foreignKey, oldForeignKey);
                            }

                            return _foreignKeyConventionContext.Result;
                        }
                    }
                }

                if (navigationBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return foreignKey;
            }

            public override IConventionSkipNavigation OnSkipNavigationInverseChanged(
                IConventionSkipNavigationBuilder navigationBuilder,
                IConventionSkipNavigation inverse,
                IConventionSkipNavigation oldInverse)
            {
                if (navigationBuilder.Metadata.DeclaringEntityType.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _skipNavigationConventionContext.ResetState(inverse);
                    foreach (var skipNavigationConvention in _conventionSet.SkipNavigationInverseChangedConventions)
                    {
                        skipNavigationConvention.ProcessSkipNavigationInverseChanged(
                            navigationBuilder, inverse, oldInverse, _skipNavigationConventionContext);
                        if (_skipNavigationConventionContext.ShouldStopProcessing())
                        {
                            return _skipNavigationConventionContext.Result;
                        }
                    }
                }

                if (navigationBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return inverse;
            }

            public override IConventionSkipNavigation OnSkipNavigationRemoved(
                IConventionEntityTypeBuilder entityTypeBuilder,
                IConventionSkipNavigation navigation)
            {
                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _skipNavigationConventionContext.ResetState(navigation);
                    foreach (var skipNavigationConvention in _conventionSet.SkipNavigationRemovedConventions)
                    {
                        skipNavigationConvention.ProcessSkipNavigationRemoved(
                            entityTypeBuilder, navigation, _skipNavigationConventionContext);
                        if (_skipNavigationConventionContext.ShouldStopProcessing())
                        {
                            return _skipNavigationConventionContext.Result;
                        }
                    }
                }

                return navigation;
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
                    foreach (var keyConvention in _conventionSet.KeyAnnotationChangedConventions)
                    {
                        keyConvention.ProcessKeyAnnotationChanged(
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

            public override bool? OnIndexUniquenessChanged(IConventionIndexBuilder indexBuilder)
            {
                using (_dispatcher.DelayConventions())
                {
                    _boolConventionContext.ResetState(indexBuilder.Metadata.IsUnique);
                    foreach (var indexConvention in _conventionSet.IndexUniquenessChangedConventions)
                    {
                        if (indexBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        indexConvention.ProcessIndexUniquenessChanged(indexBuilder, _boolConventionContext);
                        if (_boolConventionContext.ShouldStopProcessing())
                        {
                            return _boolConventionContext.Result;
                        }
                    }
                }

                if (indexBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return _boolConventionContext.Result;
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
                    foreach (var indexConvention in _conventionSet.IndexAnnotationChangedConventions)
                    {
                        indexConvention.ProcessIndexAnnotationChanged(
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

            public override bool? OnPropertyNullabilityChanged(IConventionPropertyBuilder propertyBuilder)
            {
                if (propertyBuilder.Metadata.DeclaringEntityType.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _boolConventionContext.ResetState(propertyBuilder.Metadata.IsNullable);
                    foreach (var propertyConvention in _conventionSet.PropertyNullabilityChangedConventions)
                    {
                        if (propertyBuilder.Metadata.Builder == null)
                        {
                            return null;
                        }

                        propertyConvention.ProcessPropertyNullabilityChanged(propertyBuilder, _boolConventionContext);
                        if (_boolConventionContext.ShouldStopProcessing())
                        {
                            return _boolConventionContext.Result;
                        }
                    }
                }

                if (propertyBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                return _boolConventionContext.Result;
            }

            public override FieldInfo OnPropertyFieldChanged(
                IConventionPropertyBuilder propertyBuilder,
                FieldInfo newFieldInfo,
                FieldInfo oldFieldInfo)
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

                return _fieldInfoConventionContext.Result;
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
                    foreach (var propertyConvention in _conventionSet.PropertyAnnotationChangedConventions)
                    {
                        propertyConvention.ProcessPropertyAnnotationChanged(
                            propertyBuilder, name, annotation, oldAnnotation, _annotationConventionContext);

                        if (_annotationConventionContext.ShouldStopProcessing())
                        {
                            return _annotationConventionContext.Result;
                        }
                    }
                }

                return annotation;
            }

            public override IConventionProperty OnPropertyRemoved(
                IConventionEntityTypeBuilder entityTypeBuilder,
                IConventionProperty property)
            {
                if (entityTypeBuilder.Metadata.Builder == null)
                {
                    return null;
                }

                using (_dispatcher.DelayConventions())
                {
                    _propertyConventionContext.ResetState(property);
                    foreach (var propertyConvention in _conventionSet.PropertyRemovedConventions)
                    {
                        propertyConvention.ProcessPropertyRemoved(entityTypeBuilder, property, _propertyConventionContext);
                        if (_propertyConventionContext.ShouldStopProcessing())
                        {
                            return _propertyConventionContext.Result;
                        }
                    }
                }

                return property;
            }
        }
    }
}
