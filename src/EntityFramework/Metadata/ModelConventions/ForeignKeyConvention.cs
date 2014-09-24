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
    // Instead this is the basic logic for discovering an FK based on navigation properties, etc. such that this
    // logic can be used in the model builder to create an FK by convention when needed.
    public class ForeignKeyConvention
    {
        public virtual ForeignKey FindOrCreateForeignKey(
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] string navigationToDependent,
            bool isUnqiue)
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
                isUnqiue);
        }

        public virtual ForeignKey FindOrCreateForeignKey(
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] string navigationToDependent,
            [NotNull] IReadOnlyList<Property> foreignKeyProperties,
            [NotNull] IReadOnlyList<Property> referencedProperties,
            bool isUnqiue)
        {
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            IReadOnlyList<IReadOnlyList<Property>> foreignKeyCandidates;
            if (foreignKeyProperties.Count != 0)
            {
                foreignKeyCandidates = new List<IReadOnlyList<Property>> { foreignKeyProperties };
            }
            else if (referencedProperties.Count <= 1)
            {
                foreignKeyCandidates = GetCandidateForeignKeyProperties(principalType, dependentType, navigationToPrincipal, isUnqiue);
                foreignKeyProperties = foreignKeyCandidates.FirstOrDefault() ?? ImmutableList<Property>.Empty;
            }
            else
            {
                foreignKeyCandidates = new List<IReadOnlyList<Property>>();
            }

            // If existing FK matches, then use it
            foreach (var properties in foreignKeyCandidates)
            {
                var foreignKey = dependentType.ForeignKeys
                    .FirstOrDefault(fk => fk.IsUnique == isUnqiue
                                          && fk.Properties.SequenceEqual(properties)
                                          && !fk.EntityType.Navigations.Any(n => n.ForeignKey == fk && n.Name != navigationToPrincipal)
                                          && !fk.ReferencedEntityType.Navigations.Any(n => n.ForeignKey == fk && n.Name != navigationToDependent)
                                          && (referencedProperties.Count == 0
                                              || fk.ReferencedKey.Properties.SequenceEqual(referencedProperties)));

                if (foreignKey != null)
                {
                    return foreignKey;
                }
            }

            Key principalKey;
            if (referencedProperties.Count != 0)
            {
                principalKey = principalType.GetOrAddKey(referencedProperties);
            }
            else
            {
                // Use the primary key if it is compatible with the FK properties, otherwise create a principal key
                principalKey = principalType.TryGetPrimaryKey();
                if (principalKey == null
                    || (foreignKeyProperties.Count != 0
                        && !principalKey.Properties.Select(p => p.UnderlyingType).SequenceEqual(foreignKeyProperties.Select(p => p.UnderlyingType))))
                {
                    // TODO: Convention property naming/disambiguation
                    if (foreignKeyProperties.Count != 0)
                    {
                        principalKey = principalType.GetOrAddKey(
                            foreignKeyProperties.Select(p => principalType.GetOrAddProperty(p.Name + "Key", p.UnderlyingType, shadowProperty: true))
                                .ToArray());
                    }
                    else
                    {
                        var shadowId = principalType.GetOrAddProperty(
                            (navigationToPrincipal ?? principalType.SimpleName) + "IdKey", typeof(int), shadowProperty: true);
                        principalKey = principalType.GetOrAddKey(new[] { shadowId });
                    }
                }
            }

            if (foreignKeyProperties.Count == 0
                || dependentType.TryGetForeignKey(foreignKeyProperties) != null)
            {
                // Create foreign key properties in shadow state
                // TODO: Convention property naming/disambiguation
                var baseName = (navigationToPrincipal ?? principalType.SimpleName) + "Id";
                var isComposite = principalKey.Properties.Count > 1;

                foreignKeyProperties = principalKey.Properties.Select((p, i) => dependentType.GetOrAddProperty(
                    baseName + (isComposite ? i.ToString() : ""),
                    // TODO: Make nullable
                    principalKey.Properties[i].PropertyType,
                    shadowProperty: true)).ToList();
            }

            var newForeignKey = dependentType.AddForeignKey(foreignKeyProperties, principalKey);
            newForeignKey.IsUnique = isUnqiue;

            return newForeignKey;
        }

        private IReadOnlyList<IReadOnlyList<Property>> GetCandidateForeignKeyProperties(
            EntityType principalType, EntityType dependentType, string navigationToPrincipal, bool isUnqiue)
        {
            var pk = principalType.TryGetPrimaryKey();
            var pkPropertyName = pk != null && pk.Properties.Count == 1 ? pk.Properties[0].Name : null;

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

            if (isUnqiue)
            {
                var dependentPk = dependentType.TryGetPrimaryKey();
                if (dependentPk != null)
                {
                    matches.Add(dependentPk.Properties.ToArray());
                }
            }

            return matches;
        }
    }
}
