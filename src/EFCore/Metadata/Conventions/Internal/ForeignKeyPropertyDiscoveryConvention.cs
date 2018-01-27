// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ForeignKeyPropertyDiscoveryConvention :
        IForeignKeyAddedConvention,
        INavigationAddedConvention,
        IPropertyAddedConvention,
        IPrincipalEndChangedConvention,
        IPropertyFieldChangedConvention,
        IForeignKeyUniquenessChangedConvention,
        IKeyAddedConvention,
        IKeyRemovedConvention,
        IModelBuiltConvention
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Model> _logger;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ForeignKeyPropertyDiscoveryConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            var foreignKey = relationshipBuilder.Metadata;
            if (!ConfigurationSource.Convention.Overrides(foreignKey.GetForeignKeyPropertiesConfigurationSource()))
            {
                var conflictingForeignKeys = foreignKey.DeclaringEntityType.FindForeignKeysInHierarchy(foreignKey.Properties)
                    .Where(fk => ConfigurationSource.Convention.Overrides(fk.GetForeignKeyPropertiesConfigurationSource()))
                    .ToList();
                foreach (var conflictingForeignKey in conflictingForeignKeys)
                {
                    conflictingForeignKey.Builder.HasForeignKey((IReadOnlyList<Property>)null, ConfigurationSource.Convention);
                }
                return relationshipBuilder;
            }

            var invertable = true;
            if (foreignKey.DeclaringEntityType.DefiningEntityType == foreignKey.PrincipalEntityType
                || foreignKey.IsOwnership
                || foreignKey.DeclaringEntityType.IsQueryType)
            {
                relationshipBuilder = relationshipBuilder
                    .RelatedEntityTypes(foreignKey.PrincipalEntityType, foreignKey.DeclaringEntityType, ConfigurationSource.Convention);
                invertable = false;
            } else if (ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource())
                && foreignKey.PrincipalEntityType.DefiningEntityType == foreignKey.DeclaringEntityType)
            {
                var invertedRelationshipBuilder = relationshipBuilder
                    .RelatedEntityTypes(foreignKey.DeclaringEntityType, foreignKey.PrincipalEntityType, ConfigurationSource.Convention);
                if (invertedRelationshipBuilder != null)
                {
                    return invertedRelationshipBuilder;
                }
            }

            var foreignKeyProperties = FindCandidateForeignKeyProperties(relationshipBuilder.Metadata, onDependent: true);
            if (foreignKeyProperties == null)
            {
                // Try to invert if one to one or can be converted to one to one
                if (invertable
                    && (foreignKey.IsUnique || foreignKey.PrincipalToDependent == null)
                    && ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource()))
                {
                    var candidatePropertiesOnPrincipal = FindCandidateForeignKeyProperties(foreignKey, onDependent: false);
                    if (candidatePropertiesOnPrincipal != null
                         && !foreignKey.PrincipalEntityType.FindForeignKeysInHierarchy(candidatePropertiesOnPrincipal).Any())
                    {
                        var invertedRelationshipBuilder = relationshipBuilder
                            .RelatedEntityTypes(foreignKey.DeclaringEntityType, foreignKey.PrincipalEntityType, ConfigurationSource.Convention);

                        if (invertedRelationshipBuilder != null)
                        {
                            return invertedRelationshipBuilder;
                        }

                        return foreignKey.Builder == null ? null : relationshipBuilder;
                    }
                }

                if (foreignKey.IsUnique
                    && !foreignKey.IsSelfReferencing()
                    && foreignKey.DeclaringEntityType.BaseType == null)
                {
                    // Try to use PK properties if principal end is not ambiguous
                    if (!ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource())
                        || foreignKey.DeclaringEntityType.DefiningEntityType == foreignKey.PrincipalEntityType
                        || foreignKey.IsOwnership)
                    {
                        foreignKeyProperties = GetCompatiblePrimaryKeyProperties(
                            foreignKey.DeclaringEntityType,
                            foreignKey.PrincipalEntityType,
                            foreignKey.PrincipalKey.Properties);
                    }
                    else
                    {
                        foreignKeyProperties = FindCandidateForeignKeyProperties(foreignKey, onDependent: true, matchPk: true);
                        var candidatePropertiesOnPrincipal = FindCandidateForeignKeyProperties(foreignKey, onDependent: false, matchPk: true);
                        if (candidatePropertiesOnPrincipal != null)
                        {
                            if (foreignKeyProperties == null)
                            {
                                using (var batch = foreignKey.DeclaringEntityType.Model.ConventionDispatcher.StartBatch())
                                {
                                    var invertedRelationshipBuilder = relationshipBuilder
                                        .RelatedEntityTypes(foreignKey.DeclaringEntityType, foreignKey.PrincipalEntityType, ConfigurationSource.Convention);
                                    return batch.Run(invertedRelationshipBuilder.HasForeignKey(candidatePropertiesOnPrincipal, foreignKey.PrincipalEntityType, ConfigurationSource.Convention));
                                }
                            }

                            foreignKeyProperties = null;
                        }
                    }
                }
            }

            relationshipBuilder = SetForeignKeyProperties(relationshipBuilder, foreignKeyProperties);
            foreignKey = relationshipBuilder?.Metadata;

            if (relationshipBuilder == null
                || foreignKey.GetForeignKeyPropertiesConfigurationSource() != null)
            {
                return relationshipBuilder;
            }

            using (var batch = foreignKey.DeclaringEntityType.Model.ConventionDispatcher.StartBatch())
            {
                var newTemporaryProperties = foreignKey.DeclaringEntityType.Builder.ReUniquifyTemporaryProperties(
                    foreignKey.Properties,
                    foreignKey.PrincipalKey.Properties,
                    foreignKey.IsRequired,
                    GetPropertyBaseName(foreignKey));
                return newTemporaryProperties != null
                    ? batch.Run(relationshipBuilder.HasForeignKey(
                        newTemporaryProperties, foreignKey.DeclaringEntityType, null))
                    : relationshipBuilder;
            }
        }

        private static string GetPropertyBaseName(ForeignKey foreignKey)
        {
            return foreignKey.DependentToPrincipal?.Name ?? foreignKey.PrincipalEntityType.ShortName();
        }

        private InternalRelationshipBuilder SetForeignKeyProperties(
            InternalRelationshipBuilder relationshipBuilder, IReadOnlyList<Property> foreignKeyProperties)
        {
            if (foreignKeyProperties == null)
            {
                return relationshipBuilder;
            }

            var foreignKey = relationshipBuilder.Metadata;
            if (ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource())
                && foreignKey.DeclaringEntityType.DefiningEntityType != foreignKey.PrincipalEntityType
                && !foreignKey.DeclaringEntityType.IsQueryType
                && !foreignKey.IsOwnership
                && !foreignKey.IsSelfReferencing()
                && (foreignKey.PrincipalToDependent?.IsCollection() != true))
            {
                var candidatePropertiesOnPrincipal = FindCandidateForeignKeyProperties(foreignKey, onDependent: false);
                if (candidatePropertiesOnPrincipal != null
                    && !foreignKey.PrincipalEntityType.FindForeignKeysInHierarchy(candidatePropertiesOnPrincipal).Any())
                {
                    // Ambiguous principal end
                    if (relationshipBuilder.Metadata.GetPrincipalEndConfigurationSource() == ConfigurationSource.Convention)
                    {
                        relationshipBuilder.Metadata.SetPrincipalEndConfigurationSource(null);
                    }
                    return relationshipBuilder;
                }
            }

            if (foreignKey.DeclaringEntityType.FindForeignKeysInHierarchy(foreignKeyProperties).Any())
            {
                return relationshipBuilder;
            }

            var newRelationshipBuilder = relationshipBuilder.HasForeignKey(foreignKeyProperties, ConfigurationSource.Convention);
            if (newRelationshipBuilder != null)
            {
                return newRelationshipBuilder;
            }

            if (relationshipBuilder.Metadata.Builder == null)
            {
                return null;
            }

            return relationshipBuilder;
        }

        private IReadOnlyList<Property> FindCandidateForeignKeyProperties(
            ForeignKey foreignKey, bool onDependent, bool matchPk = false)
        {
            var baseNames = new List<string>();
            var navigation = onDependent
                ? foreignKey.DependentToPrincipal
                : foreignKey.PrincipalToDependent;
            if (navigation != null)
            {
                baseNames.Add(navigation.Name);
            }

            var entityTypeToReference = onDependent
                ? foreignKey.PrincipalEntityType
                : foreignKey.DeclaringEntityType;
            baseNames.Add(entityTypeToReference.ShortName());

            if (!matchPk)
            {
                baseNames.Add("");
            }

            foreach (var baseName in baseNames)
            {
                var match = FindMatchingProperties(foreignKey, baseName, onDependent, matchPk);
                if (match != null)
                {
                    // Stop searching if match found, but is incompatible
                    return match.Count == 0 ? null : match;
                }
            }

            return null;
        }

        private static IReadOnlyList<Property> GetCompatiblePrimaryKeyProperties(EntityType dependentEntityType, EntityType principalEntityType, IReadOnlyList<Property> propertiesToReference)
        {
            var dependentPkProperties = dependentEntityType.FindPrimaryKey()?.Properties;
            if ((dependentPkProperties != null)
                && ForeignKey.AreCompatible(
                    propertiesToReference,
                    dependentPkProperties,
                    principalEntityType,
                    dependentEntityType,
                    false))
            {
                return dependentPkProperties;
            }

            return null;
        }

        private IReadOnlyList<Property> FindMatchingProperties(
            ForeignKey foreignKey, string baseName, bool onDependent, bool matchPK = false)
        {
            var dependentEntityType = onDependent
                ? foreignKey.DeclaringEntityType
                : foreignKey.PrincipalEntityType;
            var principalEntityType = onDependent
                ? foreignKey.PrincipalEntityType
                : foreignKey.DeclaringEntityType;
            var propertiesToReference = onDependent
                ? foreignKey.PrincipalKey.Properties
                : foreignKey.DeclaringEntityType.FindPrimaryKey()?.Properties;

            if (propertiesToReference == null)
            {
                return null;
            }

            var foreignKeyProperties = new Property[propertiesToReference.Count];
            var matchFound = true;
            for (var i = 0; i < propertiesToReference.Count; i++)
            {
                var referencedProperty = propertiesToReference[i];
                var property = TryGetProperty(
                    dependentEntityType,
                    baseName, referencedProperty.Name);

                if (property == null)
                {
                    matchFound = false;
                    continue;
                }

                foreignKeyProperties[i] = property;
            }

            if (!matchFound
                && propertiesToReference.Count == 1)
            {
                var property = TryGetProperty(
                    dependentEntityType,
                    baseName, "Id");

                if (property != null)
                {
                    foreignKeyProperties[0] = property;
                    matchFound = true;
                }
            }

            if (!matchFound)
            {
                return null;
            }

            if (!ForeignKey.AreCompatible(
                propertiesToReference,
                foreignKeyProperties,
                principalEntityType,
                dependentEntityType,
                false))
            {
                _logger.IncompatibleMatchingForeignKeyProperties(foreignKeyProperties, propertiesToReference);
                return new Property[0];
            }

            foreach (var key in dependentEntityType.GetKeys())
            {
                if (key.Properties.All(property => foreignKeyProperties.Contains(property))
                    && (!foreignKey.IsUnique
                        || (key.IsPrimaryKey() && !matchPK)))
                {
                    return new Property[0];
                }
            }

            return foreignKeyProperties;
        }

        private static Property TryGetProperty(EntityType entityType, string prefix, string suffix)
        {
            foreach (var property in entityType.GetProperties())
            {
                if ((!property.IsShadowProperty || !ConfigurationSource.Convention.Overrides(property.GetConfigurationSource()))
                    && property.Name.Length == prefix.Length + suffix.Length
                    && property.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    && property.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return property;
                }
            }
            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder, Navigation navigation)
            => Apply(relationshipBuilder);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder)
        {
            if (!((IProperty)propertyBuilder.Metadata).IsShadowProperty)
            {
                var entityType = propertyBuilder.Metadata.DeclaringEntityType;

                foreach (var foreignKey in entityType.GetDeclaredForeignKeys().Concat(entityType.GetDerivedForeignKeys()).ToList())
                {
                    if (foreignKey.Builder != null)
                    {
                        Apply(foreignKey.Builder);
                    }
                }

                foreach (var foreignKey in entityType.GetReferencingForeignKeys().ToList())
                {
                    if (foreignKey.Builder != null)
                    {
                        Apply(foreignKey.Builder);
                    }
                }
            }
            return propertyBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalPropertyBuilder propertyBuilder, FieldInfo oldFieldInfo)
        {
            Apply(propertyBuilder);
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        InternalRelationshipBuilder IForeignKeyUniquenessChangedConvention.Apply(InternalRelationshipBuilder relationshipBuilder)
            => Apply(relationshipBuilder);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalKeyBuilder Apply(InternalKeyBuilder keyBuilder)
        {
            var key = keyBuilder.Metadata;
            foreach (var foreignKey in key.DeclaringEntityType.GetDerivedForeignKeysInclusive().ToList())
            {
                if (key.Properties.All(p => foreignKey.Properties.Contains(p))
                    && (!foreignKey.IsUnique || foreignKey.DeclaringEntityType.BaseType != null))
                {
                    foreignKey.Builder.HasForeignKey((IReadOnlyList<Property>)null, ConfigurationSource.Convention);
                }
            }

            return keyBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Apply(InternalEntityTypeBuilder entityTypeBuilder, Key key)
        {
            var fks = key.DeclaringEntityType.GetDerivedForeignKeysInclusive().ToList();
            foreach (var foreignKey in fks)
            {
                if (foreignKey.Builder != null
                    && (!foreignKey.IsUnique
                        || foreignKey.DeclaringEntityType.BaseType != null))
                {
                    Apply(foreignKey.Builder);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var conflictingFkFound = false;
                foreach (var foreignKey in entityType.GetDeclaredForeignKeys().Where(fk => fk.GetForeignKeyPropertiesConfigurationSource() == null))
                {
                    var foreignKeyProperties = FindCandidateForeignKeyProperties(foreignKey, onDependent: true);
                    if (foreignKeyProperties != null)
                    {
                        var conflictingForeignKey = foreignKey.DeclaringEntityType.FindForeignKeysInHierarchy(foreignKeyProperties)
                            .FirstOrDefault(
                                fk => fk != foreignKey
                                      && ConfigurationSource.Convention.Overrides(fk.GetForeignKeyPropertiesConfigurationSource()));
                        if (conflictingForeignKey != null)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.AmbiguousForeignKeyPropertyCandidates(
                                    conflictingForeignKey.DeclaringEntityType.DisplayName() +
                                    (conflictingForeignKey.DependentToPrincipal == null ? "" : "." + conflictingForeignKey.DependentToPrincipal.Name),
                                    conflictingForeignKey.PrincipalEntityType.DisplayName() +
                                    (conflictingForeignKey.PrincipalToDependent == null ? "" : "." + conflictingForeignKey.PrincipalToDependent.Name),
                                    foreignKey.DeclaringEntityType.DisplayName() +
                                    (foreignKey.DependentToPrincipal == null ? "" : "." + foreignKey.DependentToPrincipal.Name),
                                    foreignKey.PrincipalEntityType.DisplayName() +
                                    (foreignKey.PrincipalToDependent == null ? "" : "." + foreignKey.PrincipalToDependent.Name),
                                    Property.Format(foreignKeyProperties)));
                        }
                    }

                    if (conflictingFkFound)
                    {
                        continue;
                    }

                    if (HasUniquifiedProperties(foreignKey))
                    {
                        var conflictingShadowFk = entityType.GetDeclaredForeignKeys().FirstOrDefault(otherForeignKey =>
                            otherForeignKey != foreignKey
                            && otherForeignKey.PrincipalEntityType == foreignKey.PrincipalEntityType
                            && otherForeignKey.GetForeignKeyPropertiesConfigurationSource() == null);
                        if (conflictingShadowFk != null)
                        {
                            conflictingFkFound = true;
                            _logger.ConflictingShadowForeignKeys(conflictingShadowFk);
                        }
                    }
                }
            }

            return modelBuilder;
        }

        private bool HasUniquifiedProperties(ForeignKey foreignKey)
        {
            var fkBaseName = GetPropertyBaseName(foreignKey);
            for (var i = 0; i < foreignKey.Properties.Count; i++)
            {
                var fkPropertyName = foreignKey.Properties[i].Name;
                var pkPropertyName = foreignKey.PrincipalKey.Properties[i].Name;
                if (fkPropertyName.Length != fkBaseName.Length + pkPropertyName.Length
                    || !fkPropertyName.StartsWith(fkBaseName, StringComparison.Ordinal)
                    || !fkPropertyName.EndsWith(pkPropertyName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
