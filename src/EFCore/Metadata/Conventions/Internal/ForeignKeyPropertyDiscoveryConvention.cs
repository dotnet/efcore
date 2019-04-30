// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ForeignKeyPropertyDiscoveryConvention :
        IForeignKeyAddedConvention,
        INavigationAddedConvention,
        IPropertyAddedConvention,
        IEntityTypeMemberIgnoredConvention,
        IPropertyNullabilityChangedConvention,
        IPropertyFieldChangedConvention,
        IForeignKeyPropertiesChangedConvention,
        IForeignKeyPrincipalEndChangedConvention,
        IForeignKeyUniquenessChangedConvention,
        IForeignKeyRequirednessChangedConvention,
        IKeyAddedConvention,
        IKeyRemovedConvention,
        IPrimaryKeyChangedConvention,
        IModelBuiltConvention
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ForeignKeyPropertyDiscoveryConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            Logger = logger;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IDiagnosticsLogger<DbLoggerCategory.Model> Logger { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            var shouldBeRequired = true;
            foreach (var property in relationshipBuilder.Metadata.Properties)
            {
                if (property.IsNullable)
                {
                    shouldBeRequired = false;
                    relationshipBuilder.IsRequired(false, ConfigurationSource.Convention);
                    break;
                }
            }

            if (shouldBeRequired)
            {
                relationshipBuilder.IsRequired(true, ConfigurationSource.Convention);
            }

            var newRelationshipBuilder = DiscoverProperties(relationshipBuilder);

            // If new properties were used for this relationship we have to examine the other foreign keys
            // in case they can use the properties used previously.
            var fksToProcess = newRelationshipBuilder.Metadata.DeclaringEntityType.GetForeignKeysInHierarchy()
                .Where(fk => fk != newRelationshipBuilder.Metadata)
                .ToList();

            foreach (var fk in fksToProcess)
            {
                if (fk.Builder != null)
                {
                    DiscoverProperties(fk.Builder);
                }
            }

            return newRelationshipBuilder;
        }

        private InternalRelationshipBuilder DiscoverProperties(InternalRelationshipBuilder relationshipBuilder)
        {
            var foreignKey = relationshipBuilder.Metadata;
            if (!ConfigurationSource.Convention.Overrides(foreignKey.GetPropertiesConfigurationSource()))
            {
                using (var batch = foreignKey.DeclaringEntityType.Model.ConventionDispatcher.StartBatch())
                {
                    using (var foreignKeyReference = foreignKey.DeclaringEntityType.Model.ConventionDispatcher.Tracker.Track(foreignKey))
                    {
                        foreach (var fkProperty in foreignKey.Properties)
                        {
                            if (ConfigurationSource.Convention.Overrides(fkProperty.GetTypeConfigurationSource())
                                && fkProperty.IsShadowProperty()
                                && fkProperty.ClrType.IsNullableType() == foreignKey.IsRequired
                                && fkProperty.GetContainingForeignKeys().All(otherFk => otherFk.IsRequired == foreignKey.IsRequired))
                            {
                                var newType = fkProperty.ClrType.MakeNullable(!foreignKey.IsRequired);
                                if (fkProperty.ClrType != newType)
                                {
                                    fkProperty.DeclaringEntityType.Builder.Property(
                                        fkProperty.ClrType.MakeNullable(!foreignKey.IsRequired),
                                        fkProperty.Name,
                                        fkProperty.GetConfigurationSource(), ConfigurationSource.Convention);
                                }
                            }
                        }

                        batch.Run();
                        return foreignKeyReference.Object?.Builder;
                    }
                }
            }

            var invertable = true;
            if (foreignKey.DeclaringEntityType.DefiningEntityType == foreignKey.PrincipalEntityType
                || foreignKey.IsOwnership
                || foreignKey.DeclaringEntityType.IsKeyless
                || (!foreignKey.IsUnique && !ConfigurationSource.Convention.Overrides(foreignKey.GetIsUniqueConfigurationSource()))
                || foreignKey.PrincipalToDependent?.IsCollection() == true
                || foreignKey.DeclaringEntityType.FindOwnership() != null)
            {
                relationshipBuilder = relationshipBuilder.HasEntityTypes(
                    foreignKey.PrincipalEntityType, foreignKey.DeclaringEntityType, ConfigurationSource.Convention);
                invertable = false;
            }
            else if (ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource())
                     && (foreignKey.PrincipalEntityType.DefiningEntityType == foreignKey.DeclaringEntityType
                         || (foreignKey.PrincipalEntityType.FindOwnership() != null
                             && foreignKey.PrincipalToDependent != null
                             && foreignKey.DependentToPrincipal == null)))
            {
                var invertedRelationshipBuilder = relationshipBuilder.HasEntityTypes(
                    foreignKey.DeclaringEntityType, foreignKey.PrincipalEntityType, ConfigurationSource.Convention);
                if (invertedRelationshipBuilder != null)
                {
                    return invertedRelationshipBuilder;
                }
            }

            var foreignKeyProperties = FindCandidateForeignKeyProperties(relationshipBuilder.Metadata, onDependent: true);
            if (foreignKeyProperties == null)
            {
                if (invertable
                    && ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource()))
                {
                    var candidatePropertiesOnPrincipal = FindCandidateForeignKeyProperties(foreignKey, onDependent: false);
                    if (candidatePropertiesOnPrincipal != null)
                    {
                        var invertedRelationshipBuilder = relationshipBuilder
                            .HasEntityTypes(foreignKey.DeclaringEntityType, foreignKey.PrincipalEntityType, ConfigurationSource.Convention);
                        var invertedFk = invertedRelationshipBuilder?.Metadata;
                        if (invertedFk?.IsSelfReferencing() == true)
                        {
                            invertedRelationshipBuilder = invertedRelationshipBuilder.HasNavigations(
                                invertedFk.PrincipalToDependent?.Name, invertedFk.DependentToPrincipal?.Name,
                                ConfigurationSource.Convention);
                        }

                        return invertedRelationshipBuilder ?? (foreignKey.Builder == null ? null : relationshipBuilder);
                    }
                }

                if (foreignKey.IsUnique
                    && foreignKey.DeclaringEntityType.BaseType == null
                    && !foreignKey.IsSelfReferencing())
                {
                    // Try to use PK properties if principal end is not ambiguous
                    if (!foreignKey.IsOwnership
                        && (!ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource())
                            || foreignKey.DeclaringEntityType.DefiningEntityType == foreignKey.PrincipalEntityType))
                    {
                        foreignKeyProperties = GetCompatiblePrimaryKeyProperties(
                            foreignKey.DeclaringEntityType,
                            foreignKey.PrincipalEntityType,
                            foreignKey.PrincipalKey.Properties);
                    }
                    else if (invertable)
                    {
                        foreignKeyProperties = FindCandidateForeignKeyProperties(foreignKey, onDependent: true, matchPk: true);
                        var candidatePropertiesOnPrincipal =
                            FindCandidateForeignKeyProperties(foreignKey, onDependent: false, matchPk: true);
                        if (candidatePropertiesOnPrincipal != null)
                        {
                            if (foreignKeyProperties == null)
                            {
                                using (var batch = foreignKey.DeclaringEntityType.Model.ConventionDispatcher.StartBatch())
                                {
                                    var invertedRelationshipBuilder = relationshipBuilder
                                        .HasEntityTypes(
                                            foreignKey.DeclaringEntityType, foreignKey.PrincipalEntityType, ConfigurationSource.Convention);
                                    return batch.Run(
                                        invertedRelationshipBuilder.HasForeignKey(
                                            candidatePropertiesOnPrincipal, foreignKey.PrincipalEntityType,
                                            ConfigurationSource.Convention));
                                }
                            }

                            foreignKeyProperties = null;
                            relationshipBuilder.Metadata.SetPrincipalEndConfigurationSource(null);
                        }
                    }
                }

                if (foreignKeyProperties == null
                    && invertable
                    && ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource()))
                {
                    relationshipBuilder.Metadata.SetPrincipalEndConfigurationSource(null);
                }
            }
            else if (invertable
                     && ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource()))
            {
                var candidatePropertiesOnPrincipal = FindCandidateForeignKeyProperties(foreignKey, onDependent: false);
                if (candidatePropertiesOnPrincipal != null)
                {
                    // Principal end is ambiguous
                    foreignKeyProperties = null;
                    relationshipBuilder.Metadata.SetPrincipalEndConfigurationSource(null);
                }
            }

            if (foreignKeyProperties == null)
            {
                return ReuniquifyTemporaryProperties(foreignKey, force: false);
            }

            var conflictingFKCount = foreignKey.DeclaringEntityType.FindForeignKeysInHierarchy(foreignKeyProperties).Count();
            if (foreignKey.Properties.SequenceEqual(foreignKeyProperties))
            {
                return conflictingFKCount > 1
                    ? ReuniquifyTemporaryProperties(foreignKey, force: true)
                    : relationshipBuilder;
            }

            if (conflictingFKCount > 0)
            {
                return ReuniquifyTemporaryProperties(foreignKey, force: false);
            }

            var newRelationshipBuilder = relationshipBuilder.HasForeignKey(foreignKeyProperties, ConfigurationSource.Convention);
            if (newRelationshipBuilder != null)
            {
                return newRelationshipBuilder;
            }

            return relationshipBuilder.Metadata.Builder == null ? null : relationshipBuilder;
        }

        private static string GetPropertyBaseName(ForeignKey foreignKey)
            => foreignKey.DependentToPrincipal?.Name ?? foreignKey.PrincipalEntityType.ShortName();

        private InternalRelationshipBuilder ReuniquifyTemporaryProperties(ForeignKey foreignKey, bool force)
        {
            if (!force
                && (foreignKey.GetPropertiesConfigurationSource() != null
                    || !foreignKey.DeclaringEntityType.Builder.ShouldReuniquifyTemporaryProperties(
                        foreignKey.Properties,
                        foreignKey.PrincipalKey.Properties,
                        foreignKey.IsRequired && foreignKey.GetIsRequiredConfigurationSource().Overrides(ConfigurationSource.Convention),
                        GetPropertyBaseName(foreignKey))))
            {
                return foreignKey.Builder;
            }

            var relationshipBuilder = foreignKey.Builder;
            using (var batch = foreignKey.DeclaringEntityType.Model.ConventionDispatcher.StartBatch())
            {
                var temporaryProperties = foreignKey.Properties.Where(
                    p => p.IsShadowProperty()
                         && ConfigurationSource.Convention.Overrides(p.GetConfigurationSource())).ToList();

                var keysToDetach = temporaryProperties.SelectMany(
                        p => p.GetContainingKeys()
                            .Where(k => ConfigurationSource.Convention.Overrides(k.GetConfigurationSource())))
                    .Distinct().ToList();

                List<RelationshipSnapshot> detachedRelationships = null;
                foreach (var key in keysToDetach)
                {
                    foreach (var referencingForeignKey in key.GetReferencingForeignKeys().ToList())
                    {
                        if (detachedRelationships == null)
                        {
                            detachedRelationships = new List<RelationshipSnapshot>();
                        }

                        detachedRelationships.Add(InternalEntityTypeBuilder.DetachRelationship(referencingForeignKey));
                    }
                }

                var detachedKeys = InternalEntityTypeBuilder.DetachKeys(keysToDetach);

                var detachedIndexes = InternalEntityTypeBuilder.DetachIndexes(
                    temporaryProperties.SelectMany(p => p.GetContainingIndexes()).Distinct());

                relationshipBuilder = relationshipBuilder.HasForeignKey((IReadOnlyList<Property>)null, ConfigurationSource.Convention);

                if (detachedIndexes != null)
                {
                    foreach (var indexBuilderTuple in detachedIndexes)
                    {
                        indexBuilderTuple.Attach(indexBuilderTuple.Metadata.DeclaringEntityType.Builder);
                    }
                }

                if (detachedKeys != null)
                {
                    foreach (var detachedKeyTuple in detachedKeys)
                    {
                        detachedKeyTuple.Item1.Attach(foreignKey.DeclaringEntityType.RootType().Builder, detachedKeyTuple.Item2);
                    }
                }

                if (detachedRelationships != null)
                {
                    foreach (var detachedRelationship in detachedRelationships)
                    {
                        detachedRelationship.Attach();
                    }
                }

                return batch.Run(relationshipBuilder);
            }
        }

        private IReadOnlyList<Property> FindCandidateForeignKeyProperties(
            ForeignKey foreignKey, bool onDependent, bool matchPk = false)
        {
            IReadOnlyList<Property> match;
            var navigation = onDependent
                ? foreignKey.DependentToPrincipal
                : foreignKey.PrincipalToDependent;
            if (navigation != null)
            {
                if (TryFindMatchingProperties(foreignKey, navigation.Name, onDependent, matchPk, out match))
                {
                    return match;
                }
            }

            var entityTypeToReference = onDependent
                ? foreignKey.PrincipalEntityType
                : foreignKey.DeclaringEntityType;
            if (TryFindMatchingProperties(foreignKey, entityTypeToReference.ShortName(), onDependent, matchPk, out match))
            {
                return match;
            }

            return match;
        }

        private static IReadOnlyList<Property> GetCompatiblePrimaryKeyProperties(
            EntityType dependentEntityType, EntityType principalEntityType, IReadOnlyList<Property> propertiesToReference)
        {
            var dependentPkProperties = dependentEntityType.FindPrimaryKey()?.Properties;
            return (dependentPkProperties != null)
                   && ForeignKey.AreCompatible(
                       propertiesToReference,
                       dependentPkProperties,
                       principalEntityType,
                       dependentEntityType,
                       false)
                ? dependentPkProperties
                : null;
        }

        private bool TryFindMatchingProperties(
            ForeignKey foreignKey, string baseName, bool onDependent, bool matchPK, out IReadOnlyList<Property> match)
        {
            match = null;
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
                return false;
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
                && propertiesToReference.Count == 1
                && baseName.Length > 0)
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
                return false;
            }

            if (!ForeignKey.AreCompatible(
                propertiesToReference,
                foreignKeyProperties,
                principalEntityType,
                dependentEntityType,
                false))
            {
                if (propertiesToReference.All(
                    p => !p.IsShadowProperty()
                         || p.GetConfigurationSource().Overrides(ConfigurationSource.DataAnnotation)))
                {
                    Logger.IncompatibleMatchingForeignKeyProperties(foreignKeyProperties, propertiesToReference);
                }

                // Stop searching if match found, but is incompatible
                return true;
            }

            foreach (var key in dependentEntityType.GetKeys())
            {
                var isKeyContainedInForeignKey = true;
                // ReSharper disable once LoopCanBeConvertedToQuery
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < key.Properties.Count; i++)
                {
                    if (!foreignKeyProperties.Contains(key.Properties[i]))
                    {
                        isKeyContainedInForeignKey = false;
                        break;
                    }

                    if (!foreignKey.IsUnique)
                    {
                        // Stop searching if match found, but is incompatible
                        return true;
                    }
                }

                if (isKeyContainedInForeignKey
                    && key.IsPrimaryKey()
                    && !matchPK)
                {
                    // Stop searching if match found, but is incompatible
                    return true;
                }
            }

            match = foreignKeyProperties;
            return true;
        }

        private static Property TryGetProperty(EntityType entityType, string prefix, string suffix)
        {
            foreach (var property in entityType.GetProperties())
            {
                if ((!property.IsShadowProperty() || !ConfigurationSource.Convention.Overrides(property.GetConfigurationSource()))
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder, Navigation navigation)
            => DiscoverProperties(relationshipBuilder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder)
        {
            var property = propertyBuilder.Metadata;
            if (property.IsShadowProperty()
                && ConfigurationSource.Convention.Overrides(property.GetConfigurationSource()))
            {
                return propertyBuilder;
            }

            var entityType = propertyBuilder.Metadata.DeclaringEntityType;

            Apply(entityType);

            return propertyBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, string ignoredMemberName)
        {
            Apply(entityTypeBuilder.Metadata);
            return true;
        }

        private void Apply(EntityType entityType)
        {
            foreach (var foreignKey in entityType.GetDeclaredForeignKeys().Concat(entityType.GetDerivedForeignKeys()).ToList())
            {
                if (foreignKey.Builder != null)
                {
                    DiscoverProperties(foreignKey.Builder);
                }
            }

            foreach (var foreignKey in entityType.GetReferencingForeignKeys().ToList())
            {
                if (foreignKey.Builder != null
                    && ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource()))
                {
                    DiscoverProperties(foreignKey.Builder);
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Apply(InternalPropertyBuilder propertyBuilder, FieldInfo oldFieldInfo)
        {
            Apply(propertyBuilder);
            return true;
        }

        bool IPropertyNullabilityChangedConvention.Apply(InternalPropertyBuilder propertyBuilder)
        {
            var nullable = propertyBuilder.Metadata.IsNullable;
            foreach (var containingForeignKey in propertyBuilder.Metadata.GetContainingForeignKeys())
            {
                if (containingForeignKey.IsRequired != nullable
                    || (!nullable
                        && containingForeignKey.Properties.Any(p => p.IsNullable)))
                {
                    continue;
                }

                containingForeignKey.Builder.IsRequired(!nullable, ConfigurationSource.Convention);
            }

            return true;
        }

        InternalRelationshipBuilder IForeignKeyUniquenessChangedConvention.Apply(InternalRelationshipBuilder relationshipBuilder)
            => DiscoverProperties(relationshipBuilder);

        InternalRelationshipBuilder IForeignKeyRequirednessChangedConvention.Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            var isRequired = relationshipBuilder.Metadata.IsRequired;
            var model = relationshipBuilder.ModelBuilder.Metadata;
            using (model.ConventionDispatcher.StartBatch())
            {
                foreach (var property in relationshipBuilder.Metadata.Properties.Where(p => p.ClrType.IsNullableType()))
                {
                    var requiredSet = property.Builder.IsRequired(isRequired, ConfigurationSource.Convention) != null;
                    if (requiredSet
                        && isRequired != true)
                    {
                        break;
                    }
                }

                return DiscoverProperties(relationshipBuilder);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(
            InternalRelationshipBuilder relationshipBuilder,
            IReadOnlyList<Property> oldDependentProperties,
            Key oldPrincipalKey)
        {
            if (relationshipBuilder.Metadata.Properties == oldDependentProperties
                || relationshipBuilder.Metadata.Builder == null)
            {
                return relationshipBuilder;
            }

            return Apply(relationshipBuilder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Apply(InternalEntityTypeBuilder entityTypeBuilder, Key key)
        {
            var foreignKeys = key.DeclaringEntityType.GetDerivedForeignKeysInclusive().ToList();
            foreach (var foreignKey in foreignKeys)
            {
                if ((!foreignKey.IsUnique
                     || foreignKey.DeclaringEntityType.BaseType != null))
                {
                    DiscoverProperties(foreignKey.Builder);
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IPrimaryKeyChangedConvention.Apply(InternalEntityTypeBuilder entityTypeBuilder, Key previousPrimaryKey)
        {
            var foreignKeys = entityTypeBuilder.Metadata.GetDerivedForeignKeysInclusive().ToList();
            foreach (var foreignKey in foreignKeys)
            {
                if (foreignKey.IsUnique)
                {
                    DiscoverProperties(foreignKey.Builder);
                }
            }

            var referencingForeignKeys = entityTypeBuilder.Metadata.GetDerivedReferencingForeignKeysInclusive().ToList();
            foreach (var referencingForeignKey in referencingForeignKeys)
            {
                DiscoverProperties(referencingForeignKey.Builder);
            }

            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var conflictingFkFound = false;
                foreach (var foreignKey in entityType.GetDeclaredForeignKeys().Where(fk => fk.GetPropertiesConfigurationSource() == null))
                {
                    var foreignKeyProperties = FindCandidateForeignKeyProperties(foreignKey, onDependent: true);
                    if (foreignKeyProperties != null)
                    {
                        var conflictingForeignKey = foreignKey.DeclaringEntityType.FindForeignKeysInHierarchy(foreignKeyProperties)
                            .FirstOrDefault(
                                fk => fk != foreignKey
                                      && ConfigurationSource.Convention.Overrides(fk.GetPropertiesConfigurationSource()));
                        if (conflictingForeignKey != null)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.AmbiguousForeignKeyPropertyCandidates(
                                    conflictingForeignKey.DeclaringEntityType.DisplayName() +
                                    (conflictingForeignKey.DependentToPrincipal == null
                                        ? ""
                                        : "." + conflictingForeignKey.DependentToPrincipal.Name),
                                    conflictingForeignKey.PrincipalEntityType.DisplayName() +
                                    (conflictingForeignKey.PrincipalToDependent == null
                                        ? ""
                                        : "." + conflictingForeignKey.PrincipalToDependent.Name),
                                    foreignKey.DeclaringEntityType.DisplayName() +
                                    (foreignKey.DependentToPrincipal == null ? "" : "." + foreignKey.DependentToPrincipal.Name),
                                    foreignKey.PrincipalEntityType.DisplayName() +
                                    (foreignKey.PrincipalToDependent == null ? "" : "." + foreignKey.PrincipalToDependent.Name),
                                    foreignKeyProperties.Format()));
                        }
                    }

                    if (conflictingFkFound)
                    {
                        continue;
                    }

                    if (HasUniquifiedProperties(foreignKey))
                    {
                        var conflictingShadowFk = entityType.GetDeclaredForeignKeys().FirstOrDefault(
                            otherForeignKey =>
                                otherForeignKey != foreignKey
                                && otherForeignKey.PrincipalEntityType == foreignKey.PrincipalEntityType
                                && otherForeignKey.GetPropertiesConfigurationSource() == null);
                        if (conflictingShadowFk != null)
                        {
                            conflictingFkFound = true;
                            Logger.ConflictingShadowForeignKeysWarning(conflictingShadowFk);
                        }
                    }
                }
            }

            return modelBuilder;
        }

        private static bool HasUniquifiedProperties(ForeignKey foreignKey)
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
