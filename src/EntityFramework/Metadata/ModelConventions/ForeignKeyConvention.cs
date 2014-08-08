// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
                GetCandidateForeignKeyProperties(principalType, dependentType, navigationToPrincipal, isUnqiue),
                isUnqiue);
        }

        public virtual ForeignKey FindOrCreateForeignKey(
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] string navigationToDependent,
            [NotNull] IReadOnlyList<Property[]> candidateProperties,
            bool isUnqiue)
        {
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            foreach (var properties in candidateProperties)
            {
                var foreignKey = dependentType
                    .ForeignKeys
                    .FirstOrDefault(fk => fk.IsUnique == isUnqiue
                                          && fk.Properties.SequenceEqual(properties)
                                          && !fk.EntityType.Navigations.Any(n => n.ForeignKey == fk && n.Name != navigationToPrincipal)
                                          && !fk.ReferencedEntityType.Navigations.Any(n => n.ForeignKey == fk && n.Name != navigationToDependent));
                if (foreignKey != null)
                {
                    return foreignKey;
                }
            }

            // TODO: Handle case where principal key is not defined
            // TODO: What if foreignKey exists but is associated with different navigations

            var fkProperty = candidateProperties.FirstOrDefault()
                             ?? new[]
                                 {
                                     dependentType.AddProperty(
                                         (navigationToPrincipal ?? principalType.Name) + "Id",
                                         // TODO: Make nullable
                                         principalType.GetKey().Properties.First().PropertyType,
                                         shadowProperty: true,
                                         concurrencyToken: false)
                                 };

            var newForeignKey = dependentType.AddForeignKey(principalType.GetKey(), fkProperty);
            newForeignKey.IsUnique = isUnqiue;
            
            return newForeignKey;
        }

        private IReadOnlyList<Property[]> GetCandidateForeignKeyProperties(
            EntityType principalType, EntityType dependentType, string navigationToPrincipal, bool isUnqiue)
        {
            var pk = principalType.TryGetKey();
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

            candidateNames.Add((principalType.Name + "Id"));

            if (pkPropertyName != null)
            {
                candidateNames.Add((principalType.Name + pkPropertyName));
            }

            var matches = new List<Property[]>();
            foreach (var name in candidateNames)
            {
                foreach (var property in dependentType.Properties)
                {
                    if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        matches.Add(new [] { property });
                    }
                }
            }

            if (isUnqiue)
            {
                var dependentPk = dependentType.TryGetKey();
                if (dependentPk != null)
                {
                    matches.Add(dependentPk.Properties.ToArray());
                }
            }

            return matches;
        }
    }
}
