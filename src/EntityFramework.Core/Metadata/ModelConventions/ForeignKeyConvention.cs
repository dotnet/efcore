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
            [NotNull] IReadOnlyList<Property> foreignKeyProperties,
            [NotNull] IReadOnlyList<Property> referencedProperties,
            bool isUnique)
        {
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            IReadOnlyList<IReadOnlyList<Property>> foreignKeyCandidates;
            if (foreignKeyProperties.Any())
            {
                foreignKeyCandidates = new List<IReadOnlyList<Property>> { foreignKeyProperties };
            }
            else if (referencedProperties.Count <= 1)
            {
                foreignKeyCandidates = GetCandidateForeignKeyProperties(principalType, dependentType, navigationToPrincipal, isUnique);
                foreignKeyProperties = foreignKeyCandidates.FirstOrDefault() ?? ImmutableList<Property>.Empty;
            }
            else
            {
                foreignKeyCandidates = new List<IReadOnlyList<Property>>();
            }

            // If existing FK matches, then use it
            if (!foreignKeyCandidates.Any())
            {
                var foreignKey = dependentType.ForeignKeys
                    .FirstOrDefault(fk => ((IForeignKey)fk).IsUnique == isUnique
                                          && fk.ReferencedEntityType == principalType
                                          && fk.EntityType.Navigations.All(n => n.ForeignKey != fk || !n.PointsToPrincipal || n.Name == navigationToPrincipal)
                                          && fk.ReferencedEntityType.Navigations.All(n => n.ForeignKey != fk || n.PointsToPrincipal || n.Name == navigationToDependent)
                                          && (!referencedProperties.Any()
                                              || fk.ReferencedKey.Properties.SequenceEqual(referencedProperties)));

                if (foreignKey != null)
                {
                    return foreignKey;
                }
            }

            foreach (var properties in foreignKeyCandidates)
            {
                var foreignKey = dependentType.ForeignKeys
                    .FirstOrDefault(fk => fk.Properties.SequenceEqual(properties)
                                          && ((IForeignKey)fk).IsUnique == isUnique
                                          && fk.ReferencedEntityType == principalType
                                          && fk.EntityType.Navigations.All(n => n.ForeignKey != fk || !n.PointsToPrincipal || n.Name == navigationToPrincipal)
                                          && fk.ReferencedEntityType.Navigations.All(n => n.ForeignKey != fk || n.PointsToPrincipal || n.Name == navigationToDependent)
                                          && (!referencedProperties.Any()
                                              || fk.ReferencedKey.Properties.SequenceEqual(referencedProperties)));

                if (foreignKey != null)
                {
                    return foreignKey;
                }
            }

            Key principalKey;
            if (referencedProperties.Any())
            {
                principalKey = principalType.GetOrAddKey(referencedProperties);
            }
            else
            {
                // Use the primary key if it is compatible with the FK properties, otherwise create a principal key
                principalKey = principalType.TryGetPrimaryKey();
                if (principalKey == null
                    || (foreignKeyProperties.Any()
                        && !principalKey.Properties.Select(p => p.UnderlyingType).SequenceEqual(foreignKeyProperties.Select(p => p.UnderlyingType))))
                {
                    // TODO: Convention property naming/disambiguation
                    // Issue #213
                    if (foreignKeyProperties.Any())
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

            // Create foreign key properties in shadow state
            if (!foreignKeyProperties.Any()
                || dependentType.TryGetForeignKey(foreignKeyProperties) != null)
            {
                var baseName = (navigationToPrincipal ?? principalType.SimpleName) + "Id";
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
            EntityType principalType, EntityType dependentType, string navigationToPrincipal, bool isUnique)
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

            if (isUnique)
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
