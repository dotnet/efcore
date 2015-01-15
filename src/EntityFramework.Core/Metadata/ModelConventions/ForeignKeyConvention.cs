// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    // TODO: This is not yet a "real" convention because the convention infrastructure is not in place
    // Issue #213
    // Instead this is the basic logic for discovering an FK based on navigation properties, etc. such that this
    // logic can be used in the model builder to create an FK by convention when needed.
    public class ForeignKeyConvention
    {
        public virtual ForeignKey FindOrCreateForeignKey(
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] string navigationToDependent,
            bool isUnique)
        {
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            return FindOrCreateForeignKey(
                principalType,
                dependentType,
                navigationToPrincipal,
                navigationToDependent,
                ImmutableList<Property>.Empty,
                ImmutableList<Property>.Empty,
                isUnique);
        }

        public virtual ForeignKey FindOrCreateForeignKey(
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] string navigationToDependent,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> referencedProperties,
            bool isUnique)
        {
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            var foreignKey =
                TryGetForeignKey(
                    principalType,
                    dependentType,
                    navigationToPrincipal,
                    navigationToDependent,
                    foreignKeyProperties,
                    referencedProperties,
                    isUnique);

            if (foreignKey != null)
            {
                return foreignKey;
            }

            return CreateForeignKeyByConvention(
                principalType,
                dependentType,
                navigationToPrincipal,
                foreignKeyProperties,
                referencedProperties,
                isUnique);
        }

        public virtual ForeignKey TryGetForeignKey(
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] string navigationToDependent,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> referencedProperties,
            bool isUnique)
        {
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            return dependentType.ForeignKeys.FirstOrDefault(fk =>
                fk.IsCompatible(
                    principalType,
                    dependentType,
                    navigationToPrincipal,
                    navigationToDependent,
                    foreignKeyProperties,
                    referencedProperties,
                    isUnique));
        }

        public virtual ForeignKey CreateForeignKeyByConvention(
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> referencedProperties,
            bool? isUnique)
        {
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            if ((foreignKeyProperties == null || !foreignKeyProperties.Any()))
            {
                var principalKeyProperties = referencedProperties != null && referencedProperties.Any()
                    ? referencedProperties
                    : principalType.TryGetPrimaryKey()?.Properties;

                if (principalKeyProperties != null
                    && principalKeyProperties.Count == 1)
                {
                    var foreignKeyCandidates = GetCandidateForeignKeyProperties(principalType, dependentType, navigationToPrincipal, isUnique);
                    foreignKeyProperties = foreignKeyCandidates.FirstOrDefault() ?? ImmutableList<Property>.Empty;

                    // If the best candidate fk is not available no further guesses will be performed
                    if (foreignKeyProperties.Any()
                        && dependentType.TryGetForeignKey(foreignKeyProperties) != null)
                    {
                        foreignKeyProperties = null;
                    }
                }
            }

            return CreateForeignKey(
                principalType,
                dependentType,
                navigationToPrincipal,
                foreignKeyProperties,
                referencedProperties,
                isUnique);
        }

        public virtual ForeignKey CreateForeignKey(
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> referencedProperties,
            bool? isUnique)
        {
            foreignKeyProperties = foreignKeyProperties ?? ImmutableList<Property>.Empty;

            if (foreignKeyProperties.Any()
                && dependentType.TryGetForeignKey(foreignKeyProperties) != null)
            {
                return null;
            }

            Key principalKey;
            if (referencedProperties != null
                && referencedProperties.Any())
            {
                // TODO: Use ConfigurationSource
                principalKey = principalType.GetOrAddKey(referencedProperties);
            }
            else
            {
                // Use the primary key if it is compatible with the FK properties, otherwise return null
                principalKey = principalType.TryGetPrimaryKey();
                if (principalKey == null
                    || (foreignKeyProperties.Any()
                        && !principalKey.Properties.Select(p => p.UnderlyingType).SequenceEqual(foreignKeyProperties.Select(p => p.UnderlyingType))))
                {
                    return null;
                }
            }

            // Create foreign key properties in shadow state if no properties are specified
            if (!foreignKeyProperties.Any())
            {
                var baseName = (string.IsNullOrEmpty(navigationToPrincipal) ? principalType.SimpleName : navigationToPrincipal) + "Id";
                var isComposite = principalKey.Properties.Count > 1;

                var fkProperties = new List<Property>();
                var index = 0;
                foreach (var keyProperty in principalKey.Properties)
                {
                    string name;
                    do
                    {
                        name = baseName + (isComposite || index > 0 ? index.ToString() : "");
                        index++;
                    }
                    while (dependentType.TryGetProperty(name) != null);

                    fkProperties.Add(dependentType.AddProperty(name, keyProperty.PropertyType.MakeNullable(), shadowProperty: true));
                }

                foreignKeyProperties = fkProperties;
            }

            var newForeignKey = dependentType.AddForeignKey(foreignKeyProperties, principalKey);
            newForeignKey.IsUnique = isUnique;

            return newForeignKey;
        }

        private IReadOnlyList<IReadOnlyList<Property>> GetCandidateForeignKeyProperties(
            EntityType principalType, EntityType dependentType, string navigationToPrincipal, bool? isUnique)
        {
            var pk = principalType.TryGetPrimaryKey();
            if (pk == null)
            {
                return new List<Property[]>();
            }
            
            var pkPropertyName =  pk.Properties.Count == 1 ? pk.Properties[0].Name : null;

            var candidateNames = new List<string>();
            if (navigationToPrincipal != null)
            {
                candidateNames.Add((navigationToPrincipal + "Id"));

                if (pkPropertyName != null)
                {
                    candidateNames.Add((navigationToPrincipal + pkPropertyName));
                }
            }

            candidateNames.Add((principalType.SimpleName + "Id"));

            if (pkPropertyName != null)
            {
                candidateNames.Add((principalType.SimpleName + pkPropertyName));
            }

            var matches = new List<Property[]>();
            foreach (var name in candidateNames)
            {
                foreach (var property in dependentType.Properties)
                {
                    if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        matches.Add(new[] { property });
                    }
                }
            }

            if (isUnique.HasValue && isUnique.Value)
            {
                var dependentPk = dependentType.TryGetPrimaryKey();
                if (dependentPk != null)
                {
                    matches.Add(dependentPk.Properties.ToArray());
                }
            }
            
            return matches.Where(m => pk.Properties.Select(p => p.UnderlyingType).SequenceEqual(m.Select(p => p.UnderlyingType))).ToList();
        }
    }
}
