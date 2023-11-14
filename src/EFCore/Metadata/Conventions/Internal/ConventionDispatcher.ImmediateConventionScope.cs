// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

public partial class ConventionDispatcher
{
    private sealed class ImmediateConventionScope : ConventionScope
    {
        private readonly ConventionSet _conventionSet;
        private readonly ConventionDispatcher _dispatcher;
        private readonly ConventionContext<IConventionEntityTypeBuilder> _entityTypeBuilderConventionContext;
        private readonly ConventionContext<IConventionEntityType> _entityTypeConventionContext;
        private readonly ConventionContext<IConventionComplexPropertyBuilder> _complexPropertyBuilderConventionContext;
        private readonly ConventionContext<IConventionComplexProperty> _complexPropertyConventionContext;
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
        private readonly ConventionContext<IConventionTriggerBuilder> _triggerBuilderConventionContext;
        private readonly ConventionContext<IConventionTrigger> _triggerConventionContext;
        private readonly ConventionContext<IConventionAnnotation> _annotationConventionContext;
        private readonly ConventionContext<IReadOnlyList<IConventionProperty>> _propertyListConventionContext;
        private readonly ConventionContext<string> _stringConventionContext;
        private readonly ConventionContext<string?> _nullableStringConventionContext;
        private readonly ConventionContext<FieldInfo> _fieldInfoConventionContext;
        private readonly ConventionContext<IElementType> _elementTypeConventionContext;
        private readonly ConventionContext<bool?> _boolConventionContext;
        private readonly ConventionContext<IReadOnlyList<bool>?> _boolListConventionContext;

        public ImmediateConventionScope(ConventionSet conventionSet, ConventionDispatcher dispatcher)
        {
            _conventionSet = conventionSet;
            _dispatcher = dispatcher;
            _entityTypeBuilderConventionContext = new ConventionContext<IConventionEntityTypeBuilder>(dispatcher);
            _entityTypeConventionContext = new ConventionContext<IConventionEntityType>(dispatcher);
            _complexPropertyBuilderConventionContext = new ConventionContext<IConventionComplexPropertyBuilder>(dispatcher);
            _complexPropertyConventionContext = new ConventionContext<IConventionComplexProperty>(dispatcher);
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
            _triggerBuilderConventionContext = new ConventionContext<IConventionTriggerBuilder>(dispatcher);
            _triggerConventionContext = new ConventionContext<IConventionTrigger>(dispatcher);
            _annotationConventionContext = new ConventionContext<IConventionAnnotation>(dispatcher);
            _propertyListConventionContext = new ConventionContext<IReadOnlyList<IConventionProperty>>(dispatcher);
            _stringConventionContext = new ConventionContext<string>(dispatcher);
            _nullableStringConventionContext = new ConventionContext<string?>(dispatcher);
            _fieldInfoConventionContext = new ConventionContext<FieldInfo>(dispatcher);
            _elementTypeConventionContext = new ConventionContext<IElementType>(dispatcher);
            _boolConventionContext = new ConventionContext<bool?>(dispatcher);
            _boolListConventionContext = new ConventionContext<IReadOnlyList<bool>?>(dispatcher);
        }

        public override void Run(ConventionDispatcher dispatcher)
            => Check.DebugFail("Immediate convention scope cannot be run again.");

        public IConventionModelBuilder OnModelFinalizing(IConventionModelBuilder modelBuilder)
        {
            _modelBuilderConventionContext.ResetState(modelBuilder);
            foreach (var modelConvention in _conventionSet.ModelFinalizingConventions)
            {
                // Execute each convention in a separate batch so each will get an up-to-date model as they are meant to be only run once
                using (_dispatcher.DelayConventions())
                {
                    modelConvention.ProcessModelFinalizing(modelBuilder, _modelBuilderConventionContext);
                    if (_modelBuilderConventionContext.ShouldStopProcessing())
                    {
                        return _modelBuilderConventionContext.Result ?? modelBuilder;
                    }
                }
            }

            return modelBuilder;
        }

        public IConventionModelBuilder OnModelInitialized(IConventionModelBuilder modelBuilder)
        {
            using (_dispatcher.DelayConventions())
            {
                _modelBuilderConventionContext.ResetState(modelBuilder);
                foreach (var modelConvention in _conventionSet.ModelInitializedConventions)
                {
                    modelConvention.ProcessModelInitialized(modelBuilder, _modelBuilderConventionContext);
                    if (_modelBuilderConventionContext.ShouldStopProcessing())
                    {
                        return _modelBuilderConventionContext.Result!;
                    }
                }
            }

            return modelBuilder;
        }

        public override IConventionAnnotation? OnModelAnnotationChanged(
            IConventionModelBuilder modelBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            using (_dispatcher.DelayConventions())
            {
                _annotationConventionContext.ResetState(annotation);
#if DEBUG
                var initialValue = modelBuilder.Metadata[name];
#endif
                foreach (var modelConvention in _conventionSet.ModelAnnotationChangedConventions)
                {
                    modelConvention.ProcessModelAnnotationChanged(
                        modelBuilder, name, annotation, oldAnnotation, _annotationConventionContext);

                    if (_annotationConventionContext.ShouldStopProcessing())
                    {
                        return _annotationConventionContext.Result;
                    }

#if DEBUG
                    Check.DebugAssert(
                        initialValue == modelBuilder.Metadata[name],
                        $"Convention {modelConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return annotation;
        }

        public override string? OnTypeIgnored(IConventionModelBuilder modelBuilder, string name, Type? type)
        {
            using (_dispatcher.DelayConventions())
            {
                _stringConventionContext.ResetState(name);
#if DEBUG
                var initialValue = modelBuilder.Metadata.IsIgnored(name);
#endif
                foreach (var entityTypeConvention in _conventionSet.TypeIgnoredConventions)
                {
                    entityTypeConvention.ProcessTypeIgnored(modelBuilder, name, type, _stringConventionContext);
                    if (_stringConventionContext.ShouldStopProcessing())
                    {
                        return _stringConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == modelBuilder.Metadata.IsIgnored(name),
                        $"Convention {entityTypeConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !modelBuilder.Metadata.IsIgnored(name) ? null : name;
        }

        public override IConventionEntityTypeBuilder? OnEntityTypeAdded(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            using (_dispatcher.DelayConventions())
            {
                _entityTypeBuilderConventionContext.ResetState(entityTypeBuilder);
#if DEBUG
                var initialValue = entityTypeBuilder.Metadata.IsInModel;
#endif
                foreach (var entityTypeConvention in _conventionSet.EntityTypeAddedConventions)
                {
                    if (!entityTypeBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    entityTypeConvention.ProcessEntityTypeAdded(entityTypeBuilder, _entityTypeBuilderConventionContext);
                    if (_entityTypeBuilderConventionContext.ShouldStopProcessing())
                    {
                        return _entityTypeBuilderConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == entityTypeBuilder.Metadata.IsInModel,
                        $"Convention {entityTypeConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !entityTypeBuilder.Metadata.IsInModel ? null : entityTypeBuilder;
        }

        public override IConventionEntityType? OnEntityTypeRemoved(
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

        public override string? OnEntityTypeMemberIgnored(IConventionEntityTypeBuilder entityTypeBuilder, string name)
        {
            if (!entityTypeBuilder.Metadata.IsInModel)
            {
                return null;
            }

#if DEBUG
            var initialValue = entityTypeBuilder.Metadata.IsIgnored(name);
#endif
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
#if DEBUG
                        Check.DebugAssert(
                            initialValue == entityTypeBuilder.Metadata.IsIgnored(name),
                            $"Convention {entityTypeConvention.GetType().Name} changed value without terminating");
#endif
                    }
                }
            }

            return !entityTypeBuilder.Metadata.IsIgnored(name) ? null : name;
        }

        public override string? OnDiscriminatorPropertySet(IConventionEntityTypeBuilder entityTypeBuilder, string? name)
        {
            if (!entityTypeBuilder.Metadata.IsInModel)
            {
                return null;
            }

#if DEBUG
            var initialValue = entityTypeBuilder.Metadata.GetDiscriminatorPropertyName();
#endif
            using (_dispatcher.DelayConventions())
            {
                _nullableStringConventionContext.ResetState(name);

                foreach (var entityTypeConvention in _conventionSet.DiscriminatorPropertySetConventions)
                {
                    entityTypeConvention.ProcessDiscriminatorPropertySet(
                        entityTypeBuilder, name, _nullableStringConventionContext);
                    if (_nullableStringConventionContext.ShouldStopProcessing())
                    {
                        return _nullableStringConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == entityTypeBuilder.Metadata.GetDiscriminatorPropertyName(),
                        $"Convention {entityTypeConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return entityTypeBuilder.Metadata.GetDiscriminatorPropertyName();
        }

        public override IConventionEntityType? OnEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType? newBaseType,
            IConventionEntityType? previousBaseType)
        {
#if DEBUG
            var initialValue = entityTypeBuilder.Metadata.BaseType;
#endif
            using (_dispatcher.DelayConventions())
            {
                _entityTypeConventionContext.ResetState(newBaseType);
                foreach (var entityTypeConvention in _conventionSet.EntityTypeBaseTypeChangedConventions)
                {
                    if (!entityTypeBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    entityTypeConvention.ProcessEntityTypeBaseTypeChanged(
                        entityTypeBuilder, newBaseType, previousBaseType, _entityTypeConventionContext);
                    if (_entityTypeConventionContext.ShouldStopProcessing())
                    {
                        return _entityTypeConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == entityTypeBuilder.Metadata.BaseType,
                        $"Convention {entityTypeConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !entityTypeBuilder.Metadata.IsInModel ? null : newBaseType;
        }

        public override IConventionKey? OnEntityTypePrimaryKeyChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionKey? newPrimaryKey,
            IConventionKey? previousPrimaryKey)
        {
            if (!entityTypeBuilder.Metadata.IsInModel)
            {
                return null;
            }

#if DEBUG
            var initialValue = entityTypeBuilder.Metadata.FindPrimaryKey();
#endif
            using (_dispatcher.DelayConventions())
            {
                _keyConventionContext.ResetState(newPrimaryKey);
                foreach (var keyConvention in _conventionSet.EntityTypePrimaryKeyChangedConventions)
                {
                    // Some conventions rely on this running even if the new key has been removed
                    keyConvention.ProcessEntityTypePrimaryKeyChanged(
                        entityTypeBuilder, newPrimaryKey, previousPrimaryKey, _keyConventionContext);
                    if (_keyConventionContext.ShouldStopProcessing())
                    {
                        return _keyConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == entityTypeBuilder.Metadata.FindPrimaryKey(),
                        $"Convention {keyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return newPrimaryKey is { IsInModel: false } ? null : newPrimaryKey;
        }

        public override IConventionAnnotation? OnEntityTypeAnnotationChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            if (!entityTypeBuilder.Metadata.IsInModel)
            {
                return null;
            }
#if DEBUG
            var initialValue = entityTypeBuilder.Metadata[name];
#endif
            using (_dispatcher.DelayConventions())
            {
                _annotationConventionContext.ResetState(annotation);
                foreach (var entityTypeConvention in _conventionSet.EntityTypeAnnotationChangedConventions)
                {
                    entityTypeConvention.ProcessEntityTypeAnnotationChanged(
                        entityTypeBuilder, name, annotation, oldAnnotation, _annotationConventionContext);
                    if (_annotationConventionContext.ShouldStopProcessing())
                    {
                        return _annotationConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        entityTypeBuilder.Metadata.IsInModel
                        && initialValue == entityTypeBuilder.Metadata[name],
                        $"Convention {entityTypeConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !entityTypeBuilder.Metadata.IsInModel ? null : annotation;
        }

        public override string? OnComplexTypeMemberIgnored(
            IConventionComplexTypeBuilder propertyBuilder,
            string name)
        {
            if (!propertyBuilder.Metadata.IsInModel)
            {
                return null;
            }

#if DEBUG
            var initialValue = propertyBuilder.Metadata.IsIgnored(name);
#endif
            using (_dispatcher.DelayConventions())
            {
                _stringConventionContext.ResetState(name);
                foreach (var entityTypeConvention in _conventionSet.ComplexTypeMemberIgnoredConventions)
                {
                    if (!propertyBuilder.Metadata.IsIgnored(name))
                    {
                        return null;
                    }

                    entityTypeConvention.ProcessComplexTypeMemberIgnored(propertyBuilder, name, _stringConventionContext);
                    if (_stringConventionContext.ShouldStopProcessing())
                    {
                        return _stringConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == propertyBuilder.Metadata.IsIgnored(name),
                        $"Convention {entityTypeConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !propertyBuilder.Metadata.IsIgnored(name) ? null : name;
        }

        public override IConventionAnnotation? OnComplexTypeAnnotationChanged(
            IConventionComplexTypeBuilder complexTypeBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            if (!complexTypeBuilder.Metadata.IsInModel)
            {
                return null;
            }
#if DEBUG
            var initialValue = complexTypeBuilder.Metadata[name];
#endif
            using (_dispatcher.DelayConventions())
            {
                _annotationConventionContext.ResetState(annotation);
                foreach (var complexTypeConvention in _conventionSet.ComplexTypeAnnotationChangedConventions)
                {
                    complexTypeConvention.ProcessComplexTypeAnnotationChanged(
                        complexTypeBuilder, name, annotation, oldAnnotation, _annotationConventionContext);
                    if (_annotationConventionContext.ShouldStopProcessing())
                    {
                        return _annotationConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        complexTypeBuilder.Metadata.IsInModel
                        && initialValue == complexTypeBuilder.Metadata[name],
                        $"Convention {complexTypeConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !complexTypeBuilder.Metadata.IsInModel ? null : annotation;
        }

        public override IConventionComplexPropertyBuilder? OnComplexPropertyAdded(
            IConventionComplexPropertyBuilder propertyBuilder)
        {
            using (_dispatcher.DelayConventions())
            {
                _complexPropertyBuilderConventionContext.ResetState(propertyBuilder);
#if DEBUG
                var initialValue = propertyBuilder.Metadata.IsInModel;
#endif
                foreach (var complexPropertyConvention in _conventionSet.ComplexPropertyAddedConventions)
                {
                    if (!propertyBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    complexPropertyConvention.ProcessComplexPropertyAdded(propertyBuilder, _complexPropertyBuilderConventionContext);
                    if (_complexPropertyBuilderConventionContext.ShouldStopProcessing())
                    {
                        return _complexPropertyBuilderConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == propertyBuilder.Metadata.IsInModel,
                        $"Convention {complexPropertyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !propertyBuilder.Metadata.IsInModel ? null : propertyBuilder;
        }

        public override IConventionComplexProperty? OnComplexPropertyRemoved(
            IConventionTypeBaseBuilder typeBaseBuilder,
            IConventionComplexProperty property)
        {
            using (_dispatcher.DelayConventions())
            {
                _complexPropertyConventionContext.ResetState(property);
                foreach (var complexPropertyConvention in _conventionSet.ComplexPropertyRemovedConventions)
                {
                    complexPropertyConvention.ProcessComplexPropertyRemoved(typeBaseBuilder, property, _complexPropertyConventionContext);
                    if (_complexPropertyConventionContext.ShouldStopProcessing())
                    {
                        return _complexPropertyConventionContext.Result;
                    }
                }
            }

            return property;
        }

        public override bool? OnComplexPropertyNullabilityChanged(
            IConventionComplexPropertyBuilder propertyBuilder)
        {
            if (!propertyBuilder.Metadata.DeclaringType.IsInModel)
            {
                return null;
            }
#if DEBUG
            var initialValue = propertyBuilder.Metadata.IsNullable;
#endif
            using (_dispatcher.DelayConventions())
            {
                _boolConventionContext.ResetState(propertyBuilder.Metadata.IsNullable);
                foreach (var propertyConvention in _conventionSet.ComplexPropertyNullabilityChangedConventions)
                {
                    if (!propertyBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    propertyConvention.ProcessComplexPropertyNullabilityChanged(propertyBuilder, _boolConventionContext);
                    if (_boolConventionContext.ShouldStopProcessing())
                    {
                        return _boolConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == propertyBuilder.Metadata.IsNullable,
                        $"Convention {propertyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !propertyBuilder.Metadata.IsInModel ? null : _boolConventionContext.Result;
        }

        public override FieldInfo? OnComplexPropertyFieldChanged(
            IConventionComplexPropertyBuilder propertyBuilder,
            FieldInfo? newFieldInfo,
            FieldInfo? oldFieldInfo)
        {
            if (!propertyBuilder.Metadata.IsInModel
                || !propertyBuilder.Metadata.DeclaringType.IsInModel)
            {
                return null;
            }
#if DEBUG
            var initialValue = propertyBuilder.Metadata.FieldInfo;
#endif
            _fieldInfoConventionContext.ResetState(newFieldInfo);
            foreach (var propertyConvention in _conventionSet.ComplexPropertyFieldChangedConventions)
            {
                propertyConvention.ProcessComplexPropertyFieldChanged(
                    propertyBuilder, newFieldInfo, oldFieldInfo, _fieldInfoConventionContext);
                if (_fieldInfoConventionContext.ShouldStopProcessing())
                {
                    return _fieldInfoConventionContext.Result;
                }
#if DEBUG
                Check.DebugAssert(
                    initialValue == propertyBuilder.Metadata.FieldInfo,
                    $"Convention {propertyConvention.GetType().Name} changed value without terminating");
#endif
            }

            return _fieldInfoConventionContext.Result;
        }

        public override IConventionAnnotation? OnComplexPropertyAnnotationChanged(
            IConventionComplexPropertyBuilder propertyBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            if (!propertyBuilder.Metadata.IsInModel)
            {
                return null;
            }
#if DEBUG
            var initialValue = propertyBuilder.Metadata[name];
#endif
            using (_dispatcher.DelayConventions())
            {
                _annotationConventionContext.ResetState(annotation);
                foreach (var propertyConvention in _conventionSet.ComplexPropertyAnnotationChangedConventions)
                {
                    propertyConvention.ProcessComplexPropertyAnnotationChanged(
                        propertyBuilder, name, annotation, oldAnnotation, _annotationConventionContext);
                    if (_annotationConventionContext.ShouldStopProcessing())
                    {
                        return _annotationConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        propertyBuilder.Metadata.IsInModel
                        && initialValue == propertyBuilder.Metadata[name],
                        $"Convention {propertyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !propertyBuilder.Metadata.IsInModel ? null : annotation;
        }

        public override IConventionForeignKeyBuilder? OnForeignKeyAdded(IConventionForeignKeyBuilder relationshipBuilder)
        {
            if (!relationshipBuilder.Metadata.DeclaringEntityType.IsInModel
                || !relationshipBuilder.Metadata.PrincipalEntityType.IsInModel
                || !relationshipBuilder.Metadata.IsInModel)
            {
                return null;
            }

            using (_dispatcher.DelayConventions())
            {
                _relationshipBuilderConventionContext.ResetState(relationshipBuilder);
                foreach (var foreignKeyConvention in _conventionSet.ForeignKeyAddedConventions)
                {
                    if (!relationshipBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    foreignKeyConvention.ProcessForeignKeyAdded(relationshipBuilder, _relationshipBuilderConventionContext);
                    if (_relationshipBuilderConventionContext.ShouldStopProcessing())
                    {
                        return _relationshipBuilderConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        relationshipBuilder.Metadata.DeclaringEntityType.IsInModel
                        && relationshipBuilder.Metadata.PrincipalEntityType.IsInModel
                        && relationshipBuilder.Metadata.IsInModel,
                        $"Convention {foreignKeyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !relationshipBuilder.Metadata.IsInModel ? null : relationshipBuilder;
        }

        public override IConventionForeignKey? OnForeignKeyRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionForeignKey foreignKey)
        {
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

        public override IReadOnlyList<IConventionProperty>? OnForeignKeyPropertiesChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IReadOnlyList<IConventionProperty> oldDependentProperties,
            IConventionKey oldPrincipalKey)
        {
#if DEBUG
            var initialProperties = relationshipBuilder.Metadata.Properties;
            var initialPrincipalKey = relationshipBuilder.Metadata.PrincipalKey;
#endif
            using (_dispatcher.DelayConventions())
            {
                _propertyListConventionContext.ResetState(relationshipBuilder.Metadata.Properties);
                foreach (var foreignKeyConvention in _conventionSet.ForeignKeyPropertiesChangedConventions)
                {
                    // Some conventions rely on this running even if the relationship has been removed
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
#if DEBUG
                    Check.DebugAssert(
                        initialProperties == relationshipBuilder.Metadata.Properties,
                        $"Convention {foreignKeyConvention.GetType().Name} changed value without terminating");
                    Check.DebugAssert(
                        initialPrincipalKey == relationshipBuilder.Metadata.PrincipalKey,
                        $"Convention {foreignKeyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !relationshipBuilder.Metadata.IsInModel ? null : relationshipBuilder.Metadata.Properties;
        }

        public override bool? OnForeignKeyUniquenessChanged(IConventionForeignKeyBuilder relationshipBuilder)
        {
#if DEBUG
            var initialValue = relationshipBuilder.Metadata.IsUnique;
#endif
            using (_dispatcher.DelayConventions())
            {
                _boolConventionContext.ResetState(relationshipBuilder.Metadata.IsUnique);
                foreach (var foreignKeyConvention in _conventionSet.ForeignKeyUniquenessChangedConventions)
                {
                    if (!relationshipBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    foreignKeyConvention.ProcessForeignKeyUniquenessChanged(relationshipBuilder, _boolConventionContext);

                    if (_boolConventionContext.ShouldStopProcessing())
                    {
                        return _boolConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == relationshipBuilder.Metadata.IsUnique,
                        $"Convention {foreignKeyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !relationshipBuilder.Metadata.IsInModel ? null : _boolConventionContext.Result;
        }

        public override bool? OnForeignKeyRequirednessChanged(
            IConventionForeignKeyBuilder relationshipBuilder)
        {
#if DEBUG
            var initialValue = relationshipBuilder.Metadata.IsRequired;
#endif
            using (_dispatcher.DelayConventions())
            {
                _boolConventionContext.ResetState(relationshipBuilder.Metadata.IsRequired);
                foreach (var foreignKeyConvention in _conventionSet.ForeignKeyRequirednessChangedConventions)
                {
                    if (!relationshipBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    foreignKeyConvention.ProcessForeignKeyRequirednessChanged(relationshipBuilder, _boolConventionContext);

                    if (_boolConventionContext.ShouldStopProcessing())
                    {
                        return _boolConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == relationshipBuilder.Metadata.IsRequired,
                        $"Convention {foreignKeyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !relationshipBuilder.Metadata.IsInModel ? null : _boolConventionContext.Result;
        }

        public override bool? OnForeignKeyDependentRequirednessChanged(
            IConventionForeignKeyBuilder relationshipBuilder)
        {
#if DEBUG
            var initialValue = relationshipBuilder.Metadata.IsRequiredDependent;
#endif
            using (_dispatcher.DelayConventions())
            {
                _boolConventionContext.ResetState(relationshipBuilder.Metadata.IsRequiredDependent);
                foreach (var foreignKeyConvention in _conventionSet.ForeignKeyDependentRequirednessChangedConventions)
                {
                    if (!relationshipBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    foreignKeyConvention.ProcessForeignKeyDependentRequirednessChanged(relationshipBuilder, _boolConventionContext);

                    if (_boolConventionContext.ShouldStopProcessing())
                    {
                        return _boolConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == relationshipBuilder.Metadata.IsRequiredDependent,
                        $"Convention {foreignKeyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !relationshipBuilder.Metadata.IsInModel ? null : _boolConventionContext.Result;
        }

        public override bool? OnForeignKeyOwnershipChanged(
            IConventionForeignKeyBuilder relationshipBuilder)
        {
#if DEBUG
            var initialValue = relationshipBuilder.Metadata.IsOwnership;
#endif
            using (_dispatcher.DelayConventions())
            {
                _boolConventionContext.ResetState(relationshipBuilder.Metadata.IsOwnership);
                foreach (var foreignKeyConvention in _conventionSet.ForeignKeyOwnershipChangedConventions)
                {
                    if (!relationshipBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    foreignKeyConvention.ProcessForeignKeyOwnershipChanged(relationshipBuilder, _boolConventionContext);
                    if (_boolConventionContext.ShouldStopProcessing())
                    {
                        return _boolConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == relationshipBuilder.Metadata.IsOwnership,
                        $"Convention {foreignKeyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !relationshipBuilder.Metadata.IsInModel ? null : _boolConventionContext.Result;
        }

        public override IConventionForeignKeyBuilder? OnForeignKeyPrincipalEndChanged(
            IConventionForeignKeyBuilder relationshipBuilder)
        {
            using (_dispatcher.DelayConventions())
            {
                _relationshipBuilderConventionContext.ResetState(relationshipBuilder);
                foreach (var foreignKeyConvention in _conventionSet.ForeignKeyPrincipalEndChangedConventions)
                {
                    if (!relationshipBuilder.Metadata.IsInModel)
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

            return !relationshipBuilder.Metadata.IsInModel ? null : relationshipBuilder;
        }

        public override IConventionAnnotation? OnForeignKeyAnnotationChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            if (!relationshipBuilder.Metadata.IsInModel)
            {
                return null;
            }
#if DEBUG
            var initialValue = relationshipBuilder.Metadata[name];
#endif
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
#if DEBUG
                    Check.DebugAssert(
                        relationshipBuilder.Metadata.IsInModel
                        && initialValue == relationshipBuilder.Metadata[name],
                        $"Convention {foreignKeyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return annotation;
        }

        public override IConventionNavigation? OnForeignKeyNullNavigationSet(
            IConventionForeignKeyBuilder relationshipBuilder,
            bool pointsToPrincipal)
        {
#if DEBUG
            var initialValue = pointsToPrincipal
                ? relationshipBuilder.Metadata.DependentToPrincipal
                : relationshipBuilder.Metadata.PrincipalToDependent;
#endif
            using (_dispatcher.DelayConventions())
            {
                _navigationConventionContext.ResetState(null);
                foreach (var foreignKeyConvention in _conventionSet.ForeignKeyNullNavigationSetConventions)
                {
                    if (!relationshipBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    foreignKeyConvention.ProcessForeignKeyNullNavigationSet(
                        relationshipBuilder, pointsToPrincipal, _navigationConventionContext);
                    if (_navigationConventionContext.ShouldStopProcessing())
                    {
                        return _navigationConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue
                        == (pointsToPrincipal
                            ? relationshipBuilder.Metadata.DependentToPrincipal
                            : relationshipBuilder.Metadata.PrincipalToDependent),
                        $"Convention {foreignKeyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return null;
        }

        public override IConventionNavigationBuilder? OnNavigationAdded(IConventionNavigationBuilder navigationBuilder)
        {
            if (!navigationBuilder.Metadata.IsInModel)
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
#if DEBUG
                    Check.DebugAssert(
                        navigationBuilder.Metadata.IsInModel,
                        $"Convention {navigationConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !navigationBuilder.Metadata.IsInModel ? null : navigationBuilder;
        }

        public override IConventionAnnotation? OnNavigationAnnotationChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionNavigation navigation,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            if (!relationshipBuilder.Metadata.IsInModel
                || relationshipBuilder.Metadata.GetNavigation(navigation.IsOnDependent) != navigation)
            {
                return null;
            }
#if DEBUG
            var initialValue = navigation[name];
#endif
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
#if DEBUG
                    Check.DebugAssert(
                        relationshipBuilder.Metadata.IsInModel
                        && relationshipBuilder.Metadata.GetNavigation(navigation.IsOnDependent) == navigation
                        && initialValue == navigation[name],
                        $"Convention {navigationConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return annotation;
        }

        public override string? OnNavigationRemoved(
            IConventionEntityTypeBuilder sourceEntityTypeBuilder,
            IConventionEntityTypeBuilder targetEntityTypeBuilder,
            string navigationName,
            MemberInfo? memberInfo)
        {
            using (_dispatcher.DelayConventions())
            {
                _stringConventionContext.ResetState(navigationName);
                foreach (var navigationConvention in _conventionSet.NavigationRemovedConventions)
                {
                    navigationConvention.ProcessNavigationRemoved(
                        sourceEntityTypeBuilder, targetEntityTypeBuilder, navigationName, memberInfo, _stringConventionContext);

                    if (_stringConventionContext.ShouldStopProcessing())
                    {
                        return _stringConventionContext.Result;
                    }
                }
            }

            return sourceEntityTypeBuilder.Metadata.FindNavigation(navigationName) != null ? null : navigationName;
        }

        public override IConventionSkipNavigationBuilder? OnSkipNavigationAdded(
            IConventionSkipNavigationBuilder navigationBuilder)
        {
            if (!navigationBuilder.Metadata.DeclaringEntityType.IsInModel)
            {
                return null;
            }

            using (_dispatcher.DelayConventions())
            {
                _skipNavigationBuilderConventionContext.ResetState(navigationBuilder);
                foreach (var skipNavigationConvention in _conventionSet.SkipNavigationAddedConventions)
                {
                    if (!navigationBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    skipNavigationConvention.ProcessSkipNavigationAdded(navigationBuilder, _skipNavigationBuilderConventionContext);
                    if (_skipNavigationBuilderConventionContext.ShouldStopProcessing())
                    {
                        return _skipNavigationBuilderConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        navigationBuilder.Metadata.IsInModel,
                        $"Convention {skipNavigationConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !navigationBuilder.Metadata.IsInModel ? null : navigationBuilder;
        }

        public override IConventionAnnotation? OnSkipNavigationAnnotationChanged(
            IConventionSkipNavigationBuilder navigationBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            if (!navigationBuilder.Metadata.IsInModel)
            {
                return null;
            }
#if DEBUG
            var initialValue = navigationBuilder.Metadata[name];
#endif
            using (_dispatcher.DelayConventions())
            {
                _annotationConventionContext.ResetState(annotation);
                foreach (var skipNavigationConvention in _conventionSet.SkipNavigationAnnotationChangedConventions)
                {
                    if (navigationBuilder.Metadata.IsInModel
                        && navigationBuilder.Metadata.FindAnnotation(name) != annotation)
                    {
                        Check.DebugFail("annotation removed");
                        return null;
                    }

                    skipNavigationConvention.ProcessSkipNavigationAnnotationChanged(
                        navigationBuilder, name, annotation, oldAnnotation, _annotationConventionContext);
                    if (_annotationConventionContext.ShouldStopProcessing())
                    {
                        return _annotationConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        navigationBuilder.Metadata.IsInModel
                        && initialValue == navigationBuilder.Metadata[name],
                        $"Convention {skipNavigationConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return annotation;
        }

        public override IConventionForeignKey? OnSkipNavigationForeignKeyChanged(
            IConventionSkipNavigationBuilder navigationBuilder,
            IConventionForeignKey? foreignKey,
            IConventionForeignKey? oldForeignKey)
        {
            if (!navigationBuilder.Metadata.DeclaringEntityType.IsInModel)
            {
                return null;
            }
#if DEBUG
            var initialValue = navigationBuilder.Metadata.ForeignKey;
#endif
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
#if DEBUG
                    Check.DebugAssert(
                        initialValue == navigationBuilder.Metadata.ForeignKey,
                        $"Convention {skipNavigationConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !navigationBuilder.Metadata.IsInModel ? null : foreignKey;
        }

        public override IConventionSkipNavigation? OnSkipNavigationInverseChanged(
            IConventionSkipNavigationBuilder navigationBuilder,
            IConventionSkipNavigation? inverse,
            IConventionSkipNavigation? oldInverse)
        {
            if (!navigationBuilder.Metadata.DeclaringEntityType.IsInModel)
            {
                return null;
            }

#if DEBUG
            var initialValue = navigationBuilder.Metadata.Inverse;
#endif
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
#if DEBUG
                    Check.DebugAssert(
                        initialValue == navigationBuilder.Metadata.Inverse,
                        $"Convention {skipNavigationConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !navigationBuilder.Metadata.IsInModel ? null : inverse;
        }

        public override IConventionSkipNavigation? OnSkipNavigationRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionSkipNavigation navigation)
        {
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

        public override IConventionTriggerBuilder? OnTriggerAdded(IConventionTriggerBuilder triggerBuilder)
        {
            if (!triggerBuilder.Metadata.EntityType.IsInModel)
            {
                return null;
            }

            using (_dispatcher.DelayConventions())
            {
                _triggerBuilderConventionContext.ResetState(triggerBuilder);
                foreach (var triggerConvention in _conventionSet.TriggerAddedConventions)
                {
                    if (!triggerBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    triggerConvention.ProcessTriggerAdded(triggerBuilder, _triggerBuilderConventionContext);
                    if (_triggerBuilderConventionContext.ShouldStopProcessing())
                    {
                        return _triggerBuilderConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        triggerBuilder.Metadata.IsInModel,
                        $"Convention {triggerConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !triggerBuilder.Metadata.IsInModel ? null : triggerBuilder;
        }

        public override IConventionTrigger? OnTriggerRemoved(IConventionEntityTypeBuilder entityTypeBuilder, IConventionTrigger trigger)
        {
            using (_dispatcher.DelayConventions())
            {
                _triggerConventionContext.ResetState(trigger);
                foreach (var triggerConvention in _conventionSet.TriggerRemovedConventions)
                {
                    triggerConvention.ProcessTriggerRemoved(entityTypeBuilder, trigger, _triggerConventionContext);
                    if (_triggerConventionContext.ShouldStopProcessing())
                    {
                        return _triggerConventionContext.Result;
                    }
                }
            }

            return trigger;
        }

        public override IConventionKeyBuilder? OnKeyAdded(IConventionKeyBuilder keyBuilder)
        {
            if (!keyBuilder.Metadata.DeclaringEntityType.IsInModel)
            {
                return null;
            }

            using (_dispatcher.DelayConventions())
            {
                _keyBuilderConventionContext.ResetState(keyBuilder);
                foreach (var keyConvention in _conventionSet.KeyAddedConventions)
                {
                    if (!keyBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    keyConvention.ProcessKeyAdded(keyBuilder, _keyBuilderConventionContext);
                    if (_keyBuilderConventionContext.ShouldStopProcessing())
                    {
                        return _keyBuilderConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        keyBuilder.Metadata.IsInModel,
                        $"Convention {keyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !keyBuilder.Metadata.IsInModel ? null : keyBuilder;
        }

        public override IConventionKey? OnKeyRemoved(IConventionEntityTypeBuilder entityTypeBuilder, IConventionKey key)
        {
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

        public override IConventionAnnotation? OnKeyAnnotationChanged(
            IConventionKeyBuilder keyBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            if (!keyBuilder.Metadata.IsInModel)
            {
                return null;
            }

#if DEBUG
            var initialValue = keyBuilder.Metadata[name];
#endif
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
#if DEBUG
                    Check.DebugAssert(
                        keyBuilder.Metadata.IsInModel
                        && initialValue == keyBuilder.Metadata[name],
                        $"Convention {keyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return annotation;
        }

        public override IConventionIndexBuilder? OnIndexAdded(IConventionIndexBuilder indexBuilder)
        {
            if (!indexBuilder.Metadata.DeclaringEntityType.IsInModel)
            {
                return null;
            }

            using (_dispatcher.DelayConventions())
            {
                _indexBuilderConventionContext.ResetState(indexBuilder);
                foreach (var indexConvention in _conventionSet.IndexAddedConventions)
                {
                    if (!indexBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    indexConvention.ProcessIndexAdded(indexBuilder, _indexBuilderConventionContext);
                    if (_indexBuilderConventionContext.ShouldStopProcessing())
                    {
                        return _indexBuilderConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        indexBuilder.Metadata.IsInModel,
                        $"Convention {indexConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !indexBuilder.Metadata.IsInModel ? null : indexBuilder;
        }

        public override IConventionIndex? OnIndexRemoved(IConventionEntityTypeBuilder entityTypeBuilder, IConventionIndex index)
        {
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
#if DEBUG
            var initialValue = indexBuilder.Metadata.IsUnique;
#endif
            using (_dispatcher.DelayConventions())
            {
                _boolConventionContext.ResetState(indexBuilder.Metadata.IsUnique);
                foreach (var indexConvention in _conventionSet.IndexUniquenessChangedConventions)
                {
                    if (!indexBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    indexConvention.ProcessIndexUniquenessChanged(indexBuilder, _boolConventionContext);
                    if (_boolConventionContext.ShouldStopProcessing())
                    {
                        return _boolConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == indexBuilder.Metadata.IsUnique,
                        $"Convention {indexConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !indexBuilder.Metadata.IsInModel ? null : _boolConventionContext.Result;
        }

        public override IReadOnlyList<bool>? OnIndexSortOrderChanged(IConventionIndexBuilder indexBuilder)
        {
#if DEBUG
            var initialValue = indexBuilder.Metadata.IsDescending;
#endif
            using (_dispatcher.DelayConventions())
            {
                _boolListConventionContext.ResetState(indexBuilder.Metadata.IsDescending);
                foreach (var indexConvention in _conventionSet.IndexSortOrderChangedConventions)
                {
                    if (!indexBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    indexConvention.ProcessIndexSortOrderChanged(indexBuilder, _boolListConventionContext);
                    if (_boolListConventionContext.ShouldStopProcessing())
                    {
                        return _boolListConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == indexBuilder.Metadata.IsDescending,
                        $"Convention {indexConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !indexBuilder.Metadata.IsInModel ? null : _boolListConventionContext.Result;
        }

        public override IConventionAnnotation? OnIndexAnnotationChanged(
            IConventionIndexBuilder indexBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            if (!indexBuilder.Metadata.IsInModel)
            {
                return null;
            }
#if DEBUG
            var initialValue = indexBuilder.Metadata[name];
#endif
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
#if DEBUG
                    Check.DebugAssert(
                        indexBuilder.Metadata.IsInModel
                        && initialValue == indexBuilder.Metadata[name],
                        $"Convention {indexConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return annotation;
        }

        public override IConventionPropertyBuilder? OnPropertyAdded(IConventionPropertyBuilder propertyBuilder)
        {
            if (!propertyBuilder.Metadata.DeclaringType.IsInModel)
            {
                return null;
            }

            using (_dispatcher.DelayConventions())
            {
                _propertyBuilderConventionContext.ResetState(propertyBuilder);
                foreach (var propertyConvention in _conventionSet.PropertyAddedConventions)
                {
                    if (!propertyBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    propertyConvention.ProcessPropertyAdded(propertyBuilder, _propertyBuilderConventionContext);
                    if (_propertyBuilderConventionContext.ShouldStopProcessing())
                    {
                        return _propertyBuilderConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        propertyBuilder.Metadata.IsInModel,
                        $"Convention {propertyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !propertyBuilder.Metadata.IsInModel ? null : propertyBuilder;
        }

        public override bool? OnPropertyNullabilityChanged(IConventionPropertyBuilder propertyBuilder)
        {
            if (!propertyBuilder.Metadata.DeclaringType.IsInModel)
            {
                return null;
            }
#if DEBUG
            var initialValue = propertyBuilder.Metadata.IsNullable;
#endif
            using (_dispatcher.DelayConventions())
            {
                _boolConventionContext.ResetState(propertyBuilder.Metadata.IsNullable);
                foreach (var propertyConvention in _conventionSet.PropertyNullabilityChangedConventions)
                {
                    if (!propertyBuilder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    propertyConvention.ProcessPropertyNullabilityChanged(propertyBuilder, _boolConventionContext);
                    if (_boolConventionContext.ShouldStopProcessing())
                    {
                        return _boolConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == propertyBuilder.Metadata.IsNullable,
                        $"Convention {propertyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !propertyBuilder.Metadata.IsInModel ? null : _boolConventionContext.Result;
        }

        public override bool? OnElementTypeNullabilityChanged(IConventionElementTypeBuilder builder)
        {
            if (!builder.Metadata.CollectionProperty.IsInModel)
            {
                return null;
            }
#if DEBUG
            var initialValue = builder.Metadata.IsNullable;
#endif
            using (_dispatcher.DelayConventions())
            {
                _boolConventionContext.ResetState(builder.Metadata.IsNullable);
                foreach (var elementConvention in _conventionSet.ElementTypeNullabilityChangedConventions)
                {
                    if (!builder.Metadata.IsInModel)
                    {
                        return null;
                    }

                    elementConvention.ProcessElementTypeNullabilityChanged(builder, _boolConventionContext);
                    if (_boolConventionContext.ShouldStopProcessing())
                    {
                        return _boolConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        initialValue == builder.Metadata.IsNullable,
                        $"Convention {elementConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !builder.Metadata.IsInModel ? null : _boolConventionContext.Result;
        }

        public override FieldInfo? OnPropertyFieldChanged(
            IConventionPropertyBuilder propertyBuilder,
            FieldInfo? newFieldInfo,
            FieldInfo? oldFieldInfo)
        {
            if (!propertyBuilder.Metadata.IsInModel
                || !propertyBuilder.Metadata.DeclaringType.IsInModel)
            {
                return null;
            }
#if DEBUG
            var initialValue = propertyBuilder.Metadata.FieldInfo;
#endif
            _fieldInfoConventionContext.ResetState(newFieldInfo);
            foreach (var propertyConvention in _conventionSet.PropertyFieldChangedConventions)
            {
                propertyConvention.ProcessPropertyFieldChanged(
                    propertyBuilder, newFieldInfo, oldFieldInfo, _fieldInfoConventionContext);
                if (_fieldInfoConventionContext.ShouldStopProcessing())
                {
                    return _fieldInfoConventionContext.Result;
                }
#if DEBUG
                Check.DebugAssert(
                    initialValue == propertyBuilder.Metadata.FieldInfo,
                    $"Convention {propertyConvention.GetType().Name} changed value without terminating");
#endif
            }

            return _fieldInfoConventionContext.Result;
        }

        public override IElementType? OnPropertyElementTypeChanged(
            IConventionPropertyBuilder propertyBuilder,
            IElementType? newElementType,
            IElementType? oldElementType)
        {
            if (!propertyBuilder.Metadata.IsInModel
                || !propertyBuilder.Metadata.DeclaringType.IsInModel)
            {
                return null;
            }
#if DEBUG
            var initialValue = propertyBuilder.Metadata.GetElementType();
#endif
            _elementTypeConventionContext.ResetState(newElementType);
            foreach (var propertyConvention in _conventionSet.PropertyElementTypeChangedConventions)
            {
                propertyConvention.ProcessPropertyElementTypeChanged(
                    propertyBuilder, newElementType, oldElementType, _elementTypeConventionContext);
                if (_elementTypeConventionContext.ShouldStopProcessing())
                {
                    return _elementTypeConventionContext.Result;
                }
#if DEBUG
                Check.DebugAssert(
                    initialValue == propertyBuilder.Metadata.GetElementType(),
                    $"Convention {propertyConvention.GetType().Name} changed value without terminating");
#endif
            }

            return _elementTypeConventionContext.Result;
        }

        public override IConventionAnnotation? OnPropertyAnnotationChanged(
            IConventionPropertyBuilder propertyBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            if (!propertyBuilder.Metadata.IsInModel
                || !propertyBuilder.Metadata.DeclaringType.IsInModel)
            {
                return null;
            }
#if DEBUG
            var initialValue = propertyBuilder.Metadata[name];
#endif
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
#if DEBUG
                    Check.DebugAssert(
                        propertyBuilder.Metadata is { IsInModel: true, DeclaringType.IsInModel: true }
                        && initialValue == propertyBuilder.Metadata[name],
                        $"Convention {propertyConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !propertyBuilder.Metadata.IsInModel ? null : annotation;
        }

        public override IConventionAnnotation? OnElementTypeAnnotationChanged(
            IConventionElementTypeBuilder builder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            if (!builder.Metadata.IsInModel
                || !builder.Metadata.CollectionProperty.IsInModel)
            {
                return null;
            }
#if DEBUG
            var initialValue = builder.Metadata[name];
#endif
            using (_dispatcher.DelayConventions())
            {
                _annotationConventionContext.ResetState(annotation);
                foreach (var elementConvention in _conventionSet.ElementTypeAnnotationChangedConventions)
                {
                    elementConvention.ProcessElementTypeAnnotationChanged(
                        builder, name, annotation, oldAnnotation, _annotationConventionContext);

                    if (_annotationConventionContext.ShouldStopProcessing())
                    {
                        return _annotationConventionContext.Result;
                    }
#if DEBUG
                    Check.DebugAssert(
                        builder.Metadata is { IsInModel: true, CollectionProperty.IsInModel: true }
                        && initialValue == builder.Metadata[name],
                        $"Convention {elementConvention.GetType().Name} changed value without terminating");
#endif
                }
            }

            return !builder.Metadata.IsInModel ? null : annotation;
        }

        public override IConventionProperty? OnPropertyRemoved(
            IConventionTypeBaseBuilder typeBaseBuilder,
            IConventionProperty property)
        {
            using (_dispatcher.DelayConventions())
            {
                _propertyConventionContext.ResetState(property);
                foreach (var propertyConvention in _conventionSet.PropertyRemovedConventions)
                {
                    propertyConvention.ProcessPropertyRemoved(typeBaseBuilder, property, _propertyConventionContext);
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
