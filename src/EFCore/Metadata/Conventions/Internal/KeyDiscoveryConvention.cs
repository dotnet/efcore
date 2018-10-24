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
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class KeyDiscoveryConvention :
        IEntityTypeAddedConvention,
        IPropertyAddedConvention,
        IKeyRemovedConvention,
        IBaseTypeChangedConvention,
        IPropertyFieldChangedConvention,
        IForeignKeyAddedConvention,
        IForeignKeyRemovedConvention,
        IForeignKeyUniquenessChangedConvention,
        IForeignKeyOwnershipChangedConvention
    {
        private const string KeySuffix = "Id";
        private readonly IDiagnosticsLogger<DbLoggerCategory.Model> _logger;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public KeyDiscoveryConvention([CanBeNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            var entityType = entityTypeBuilder.Metadata;
            if (entityType.BaseType != null
                || entityType.IsQueryType
                || !ConfigurationSource.Convention.Overrides(entityType.GetPrimaryKeyConfigurationSource()))
            {
                return entityTypeBuilder;
            }

            IReadOnlyList<Property> keyProperties = null;
            var definingFk = entityType.FindDefiningNavigation()?.ForeignKey
                             ?? entityType.FindOwnership();

            if (definingFk?.IsUnique == false
                && definingFk.DeclaringEntityType == entityType)
            {
                entityTypeBuilder.PrimaryKey((IReadOnlyList<string>)null, ConfigurationSource.Convention);
                return entityTypeBuilder;
            }

            if (definingFk?.IsUnique == true
                && definingFk.DeclaringEntityType == entityType)
            {
                keyProperties = definingFk.Properties;
            }

            if (keyProperties == null)
            {
                var candidateProperties = entityType.GetProperties().Where(
                    p => !p.IsShadowProperty
                         || !ConfigurationSource.Convention.Overrides(p.GetConfigurationSource())).ToList();
                keyProperties = (IReadOnlyList<Property>)DiscoverKeyProperties(entityType, candidateProperties);
                if (keyProperties.Count > 1)
                {
                    _logger?.MultiplePrimaryKeyCandidates(keyProperties[0], keyProperties[1]);
                    return entityTypeBuilder;
                }
            }

            if (keyProperties.Count > 0)
            {
                entityTypeBuilder.PrimaryKey(keyProperties, ConfigurationSource.Convention);
            }

            return entityTypeBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Property> DiscoverKeyProperties(
            [NotNull] EntityType entityType, [NotNull] IReadOnlyList<Property> candidateProperties)
        {
            Check.NotNull(entityType, nameof(entityType));

            var keyProperties = candidateProperties.Where(p => string.Equals(p.Name, KeySuffix, StringComparison.OrdinalIgnoreCase)).ToList();
            if (keyProperties.Count == 0)
            {
                var entityTypeName = entityType.ShortName();
                keyProperties = candidateProperties.Where(
                    p => p.Name.Length == entityTypeName.Length + KeySuffix.Length
                         && p.Name.StartsWith(entityTypeName, StringComparison.OrdinalIgnoreCase)
                         && p.Name.EndsWith(KeySuffix, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return keyProperties;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
            => Apply(entityTypeBuilder) != null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            Apply(propertyBuilder.Metadata.DeclaringEntityType.Builder);

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
        public virtual void Apply(InternalEntityTypeBuilder entityTypeBuilder, Key key)
        {
            if (entityTypeBuilder.Metadata.FindPrimaryKey() == null)
            {
                Apply(entityTypeBuilder);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            if (relationshipBuilder.Metadata.IsOwnership)
            {
                Apply(relationshipBuilder.Metadata.DeclaringEntityType.Builder);
            }

            return relationshipBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        InternalRelationshipBuilder IForeignKeyOwnershipChangedConvention.Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            Apply(relationshipBuilder.Metadata.DeclaringEntityType.Builder);

            return relationshipBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Apply(InternalEntityTypeBuilder entityTypeBuilder, ForeignKey foreignKey)
        {
            if (foreignKey.IsOwnership)
            {
                Apply(entityTypeBuilder);
            }
        }
    }
}
