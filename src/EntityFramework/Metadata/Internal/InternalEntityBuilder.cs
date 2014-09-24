// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalEntityBuilder : InternalMetadataItemBuilder<EntityType>
    {
        private readonly LazyRef<Dictionary<ForeignKey, InternalForeignKeyBuilder>> _foreignKeyBuilders =
            new LazyRef<Dictionary<ForeignKey, InternalForeignKeyBuilder>>(() => new Dictionary<ForeignKey, InternalForeignKeyBuilder>());
        private readonly LazyRef<Dictionary<Index, InternalIndexBuilder>> _indexBuilders =
            new LazyRef<Dictionary<Index, InternalIndexBuilder>>(() => new Dictionary<Index, InternalIndexBuilder>());
        private readonly Dictionary<Key, InternalKeyBuilder> _keyBuilders = new Dictionary<Key, InternalKeyBuilder>();
        private readonly Dictionary<Property, InternalPropertyBuilder> _propertyBuilders = new Dictionary<Property, InternalPropertyBuilder>();
        private readonly LazyRef<Dictionary<ForeignKey, InternalRelationshipBuilder>> _relationshipBuilders =
            new LazyRef<Dictionary<ForeignKey, InternalRelationshipBuilder>>(() => new Dictionary<ForeignKey, InternalRelationshipBuilder>());

        public InternalEntityBuilder([NotNull] EntityType metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata, modelBuilder)
        {
        }

        public virtual InternalKeyBuilder Key([NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotEmpty(propertyNames, "propertyNames");

            return Key(GetExistingProperties(propertyNames));
        }

        public virtual InternalKeyBuilder Key([NotNull] IList<PropertyInfo> clrProperties)
        {
            Check.NotEmpty(clrProperties, "clrProperties");

            return Key(GetOrCreateProperties(clrProperties));
        }

        private InternalKeyBuilder Key(IReadOnlyList<Property> properties)
        {
            Debug.Assert(properties != null);

            InternalKeyBuilder keyBuilder;
            var currentPrimaryKey = Metadata.TryGetPrimaryKey();
            Key newKey;
            if (currentPrimaryKey == null)
            {
                newKey = Metadata.SetPrimaryKey(properties);
            }
            else
            {
                newKey = Metadata.GetOrSetPrimaryKey(properties);
                if (ReferenceEquals(currentPrimaryKey, newKey))
                {
                    if (_keyBuilders.TryGetValue(currentPrimaryKey, out keyBuilder))
                    {
                        return keyBuilder;
                    }
                }
                else
                {
                    _keyBuilders.Remove(currentPrimaryKey);
                }
            }

            keyBuilder = new InternalKeyBuilder(newKey, ModelBuilder);
            _keyBuilders.Add(newKey, keyBuilder);
            return keyBuilder;
        }

        public virtual InternalPropertyBuilder Property(
            [NotNull] Type propertyType, [NotNull] string name)
        {
            Check.NotNull(propertyType, "propertyType");
            Check.NotEmpty(name, "name");

            return InternalProperty(propertyType, name, shadowProperty: true);
        }

        public virtual InternalPropertyBuilder Property([NotNull] PropertyInfo clrProperty)
        {
            Check.NotNull(clrProperty, "clrProperty");

            return InternalProperty(clrProperty.PropertyType, clrProperty.Name, shadowProperty: false);
        }

        private InternalPropertyBuilder InternalProperty(Type propertyType, string name, bool shadowProperty)
        {
            InternalPropertyBuilder propertyBuilder;
            var property = Metadata.TryGetProperty(name);
            if (property == null)
            {
                property = Metadata.AddProperty(name, propertyType, shadowProperty);
            }
            else
            {
                if (_propertyBuilders.TryGetValue(property, out propertyBuilder))
                {
                    return propertyBuilder;
                }
            }

            propertyBuilder = new InternalPropertyBuilder(property, ModelBuilder);
            _propertyBuilders.Add(property, propertyBuilder);
            return propertyBuilder;
        }

        public virtual InternalForeignKeyBuilder ForeignKey(
            [NotNull] string referencedEntityTypeName, [NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotEmpty(referencedEntityTypeName, "referencedEntityTypeName");
            Check.NotNull(propertyNames, "propertyNames");

            var principalType = ModelBuilder.Metadata.GetEntityType(referencedEntityTypeName);

            return ForeignKey(principalType, GetExistingProperties(propertyNames));
        }

        public virtual InternalForeignKeyBuilder ForeignKey([NotNull] Type referencedType, [NotNull] IList<PropertyInfo> clrProperties)
        {
            Check.NotNull(referencedType, "referencedType");
            Check.NotNull(clrProperties, "clrProperties");

            var principalType = ModelBuilder.Entity(referencedType).Metadata;

            return ForeignKey(principalType, GetOrCreateProperties(clrProperties));
        }

        private InternalForeignKeyBuilder ForeignKey(EntityType principalType, IReadOnlyList<Property> dependentProperties)
        {
            InternalForeignKeyBuilder foreignKeyBuilder;
            var foreignKey = Metadata.TryGetForeignKey(dependentProperties);
            if (foreignKey == null)
            {
                // TODO: This code currently assumes that the FK maps to a PK on the principal end
                // Issue #756
                foreignKey = Metadata.AddForeignKey(dependentProperties, principalType.GetPrimaryKey());
            }
            else
            {
                if (_foreignKeyBuilders.Value.TryGetValue(foreignKey, out foreignKeyBuilder))
                {
                    return foreignKeyBuilder;
                }
            }

            foreignKeyBuilder = new InternalForeignKeyBuilder(foreignKey, ModelBuilder);
            _foreignKeyBuilders.Value.Add(foreignKey, foreignKeyBuilder);
            return foreignKeyBuilder;
        }

        public virtual InternalIndexBuilder Index([NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(propertyNames, "propertyNames");

            return Index(GetExistingProperties(propertyNames));
        }

        public virtual InternalIndexBuilder Index([NotNull] IList<PropertyInfo> clrProperties)
        {
            Check.NotNull(clrProperties, "clrProperties");

            return Index(GetOrCreateProperties(clrProperties));
        }

        private InternalIndexBuilder Index(IReadOnlyList<Property> properties)
        {
            // TODO: This code currently assumes that the FK maps to a PK on the principal end
            InternalIndexBuilder indexBuilder;
            var index = Metadata.TryGetIndex(properties);
            if (index == null)
            {
                index = Metadata.AddIndex(properties);
            }
            else
            {
                if (_indexBuilders.Value.TryGetValue(index, out indexBuilder))
                {
                    return indexBuilder;
                }
            }
            
            indexBuilder = new InternalIndexBuilder(index, ModelBuilder);
            _indexBuilders.Value.Add(index, indexBuilder);
            return indexBuilder;
        }

        public virtual InternalRelationshipBuilder BuildRelationship(
            [NotNull] Type principalType, [NotNull] Type dependentType,
            [CanBeNull] string navNameToPrincipal, [CanBeNull] string navNameToDependent, bool oneToOne)
        {
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            var dependentEntityType = ModelBuilder.Entity(dependentType).Metadata;
            var principalEntityType = ModelBuilder.Entity(principalType).Metadata;

            return BuildRelationship(principalEntityType, dependentEntityType, navNameToPrincipal, navNameToDependent, oneToOne);
        }

        public virtual InternalRelationshipBuilder BuildRelationship(
            [NotNull] EntityType principalEntityType, [NotNull] EntityType dependentEntityType,
            [CanBeNull] string navNameToPrincipal, [CanBeNull] string navNameToDependent, bool oneToOne)
        {
            Check.NotNull(principalEntityType, "principalEntityType");
            Check.NotNull(dependentEntityType, "dependentEntityType");

            var navToDependent = navNameToDependent == null ? null : principalEntityType.TryGetNavigation(navNameToDependent);
            var navToPrincipal = navNameToPrincipal == null ? null : dependentEntityType.TryGetNavigation(navNameToPrincipal);

            // Find the associated FK on an already existing navigation, or create one by convention
            // TODO: If FK isn't already specified, then creating the navigation should cause it to be found/created
            // by convention, but this part of conventions is not done yet, so we do it here instead--kind of h.acky
            // Issue #213
            var foreignKey = navToDependent != null
                ? navToDependent.ForeignKey
                : navToPrincipal != null
                    ? navToPrincipal.ForeignKey
                    : new ForeignKeyConvention()
                        .FindOrCreateForeignKey(principalEntityType, dependentEntityType, navNameToPrincipal, navNameToDependent, oneToOne);

            if (navNameToDependent != null
                && navToDependent == null)
            {
                navToDependent = principalEntityType.AddNavigation(navNameToDependent, foreignKey, pointsToPrincipal: false);
            }

            if (navNameToPrincipal != null
                && navToPrincipal == null)
            {
                navToPrincipal = dependentEntityType.AddNavigation(navNameToPrincipal, foreignKey, pointsToPrincipal: true);
            }

            InternalRelationshipBuilder builder;
            var owner = this;
            if (Metadata != foreignKey.EntityType)
            {
                owner = ModelBuilder.Entity(foreignKey.EntityType.Name);
            }

            if (owner._relationshipBuilders.Value.TryGetValue(foreignKey, out builder))
            {
                return builder;
            }

            builder = new InternalRelationshipBuilder(
                foreignKey, ModelBuilder, principalEntityType, dependentEntityType, navToPrincipal, navToDependent);
            owner._relationshipBuilders.Value.Add(foreignKey, builder);
            return builder;
        }

        public InternalRelationshipBuilder ReplaceForeignKey(
            [NotNull] InternalRelationshipBuilder relationshipBuilder,
            [NotNull] IReadOnlyList<Property> dependentProperties,
            [NotNull] IReadOnlyList<Property> principalProperties)
        {
            var removed = _relationshipBuilders.Value.Remove(relationshipBuilder.Metadata);
            Debug.Assert(removed);

            // TODO: avoid removing and readding the navigation property
            if (relationshipBuilder.NavigationToPrincipal != null)
            {
                relationshipBuilder.DependentType.RemoveNavigation(relationshipBuilder.NavigationToPrincipal);
            }

            if (relationshipBuilder.NavigationToDependent != null)
            {
                relationshipBuilder.PrincipalType.RemoveNavigation(relationshipBuilder.NavigationToDependent);
            }

            var entityType = relationshipBuilder.Metadata.EntityType;

            // TODO: Remove FK only if it was added by convention
            // Issue #213
            entityType.RemoveForeignKey(relationshipBuilder.Metadata);

            var newForeignKey = new ForeignKeyConvention().FindOrCreateForeignKey(
                relationshipBuilder.PrincipalType,
                relationshipBuilder.DependentType,
                relationshipBuilder.NavigationToPrincipal != null ? relationshipBuilder.NavigationToPrincipal.Name : null,
                relationshipBuilder.NavigationToDependent != null ? relationshipBuilder.NavigationToDependent.Name : null,
                dependentProperties,
                principalProperties,
                relationshipBuilder.Metadata.IsUnique);

            // TODO: Remove principal key only if it was added by convention
            // Issue #213
            var currentPrincipalKey = relationshipBuilder.Metadata.ReferencedKey;
            if (currentPrincipalKey != newForeignKey.ReferencedKey
                && currentPrincipalKey != currentPrincipalKey.EntityType.TryGetPrimaryKey()
                && currentPrincipalKey.Properties.All(p => p.IsShadowProperty))
            {
                currentPrincipalKey.EntityType.RemoveKey(currentPrincipalKey);
            }

            var propertiesInUse = entityType.Keys.SelectMany(k => k.Properties)
                .Concat(entityType.ForeignKeys.SelectMany(k => k.Properties))
                .Concat(relationshipBuilder.Metadata.ReferencedEntityType.Keys.SelectMany(k => k.Properties))
                .Concat(relationshipBuilder.Metadata.ReferencedEntityType.ForeignKeys.SelectMany(k => k.Properties))
                .Concat(dependentProperties)
                .Concat(principalProperties)
                .Where(p => p.IsShadowProperty)
                .Distinct();

            var propertiesToRemove = Metadata.Properties
                .Concat(relationshipBuilder.Metadata.ReferencedKey.Properties)
                .Where(p => p.IsShadowProperty)
                .Distinct()
                .Except(propertiesInUse)
                .ToList();

            // TODO: Remove property only if it was added by convention
            // Issue #213
            foreach (var property in propertiesToRemove)
            {
                property.EntityType.RemoveProperty(property);
            }

            var navigationToPrincipal = relationshipBuilder.NavigationToPrincipal;
            if (navigationToPrincipal != null)
            {
                navigationToPrincipal = relationshipBuilder.DependentType.AddNavigation(
                    navigationToPrincipal.Name, newForeignKey, navigationToPrincipal.PointsToPrincipal);
            }

            var navigationToDependent = relationshipBuilder.NavigationToDependent;
            if (navigationToDependent != null)
            {
                navigationToDependent = relationshipBuilder.PrincipalType.AddNavigation(
                    navigationToDependent.Name, newForeignKey, navigationToDependent.PointsToPrincipal);
            }

            InternalRelationshipBuilder builder;
            var owner = this;
            if (Metadata != newForeignKey.EntityType)
            {
                owner = ModelBuilder.Entity(newForeignKey.EntityType.Name);
            }

            if (owner._relationshipBuilders.Value.TryGetValue(newForeignKey, out builder))
            {
                if (builder.PrincipalType == relationshipBuilder.PrincipalType
                    && builder.DependentType == relationshipBuilder.DependentType
                    && builder.NavigationToPrincipal == navigationToPrincipal
                    && builder.NavigationToDependent == navigationToDependent)
                {
                    return builder;
                }

                owner._relationshipBuilders.Value.Remove(newForeignKey);
            }

            builder = new InternalRelationshipBuilder(
                relationshipBuilder,
                newForeignKey,
                navigationToPrincipal,
                navigationToDependent);
            owner._relationshipBuilders.Value.Add(newForeignKey, builder);
            return builder;
        }

        private IReadOnlyList<Property> GetExistingProperties(IEnumerable<string> propertyNames)
        {
            return propertyNames.Select(n => Metadata.GetProperty(n)).ToList();
        }

        private IReadOnlyList<Property> GetOrCreateProperties(IEnumerable<PropertyInfo> clrProperties)
        {
            return clrProperties.Select(p => Metadata.GetOrAddProperty(p.Name, p.PropertyType)).ToList();
        }
    }
}
