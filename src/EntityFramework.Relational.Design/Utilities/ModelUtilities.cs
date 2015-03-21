// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.Utilities
{
    public class ModelUtilities
    {
        public virtual string GenerateLambdaToKey(
            [NotNull] IEnumerable<IProperty> properties,
            [NotNull] string lambdaIdentifier)
        {
            Check.NotNull(properties, nameof(properties));
            Check.NotEmpty(lambdaIdentifier, nameof(lambdaIdentifier));

            var sb = new StringBuilder();

            if (properties.Count() > 1)
            {
                sb.Append("new { ");
                sb.Append(string.Join(", ", properties.Select(p => lambdaIdentifier + "." + p.Name)));
                sb.Append(" }");
            }
            else
            {
                sb.Append(lambdaIdentifier + "." + properties.ElementAt(0).Name);
            }

            return sb.ToString();
        }

        public virtual IEnumerable<IProperty> OrderedProperties([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var primaryKeyProperties =
                entityType.TryGetPrimaryKey()?.Properties.ToList()
                ?? new List<IProperty>();
            foreach (var property in primaryKeyProperties)
            {
                yield return property;
            }

            var foreignKeyProperties = entityType.GetForeignKeys().SelectMany(fk => fk.Properties).Distinct().ToList();
            foreach (var property in
                entityType.GetProperties()
                .Except(primaryKeyProperties)
                .Except(foreignKeyProperties)
                .OrderBy(p => p.Name))
            {
                yield return property;
            }
        }

        public virtual string GetDependentEndCandidateNavigationPropertyName([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            var candidateName = StripId(
                FindCommonPrefix(foreignKey.Properties.Select(p => p.Name)));

            if (!string.IsNullOrEmpty(candidateName))
            {
                return candidateName;
            }

            return foreignKey.ReferencedEntityType.DisplayName();
        }

        public virtual string GetPrincipalEndCandidateNavigationPropertyName([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            return foreignKey.EntityType.DisplayName();
        }

        private string FindCommonPrefix(IEnumerable<string> stringsEnumerable)
        {
            if (stringsEnumerable.Count() == 0)
            {
                return string.Empty;
            }

            if (stringsEnumerable.Count() == 1)
            {
                return stringsEnumerable.Single();
            }

            var prefixLength = 0;
            var firstString = stringsEnumerable.First();
            foreach (char c in firstString)
            {
                foreach(var s in stringsEnumerable)
                {
                    if (s.Length <= prefixLength || s[prefixLength] != c)
                    {
                        return firstString.Substring(0, prefixLength);
                    }
                }

                prefixLength++;
            }

            return firstString;
        }

        private string StripId(string identifier)
        {
            if (identifier.EndsWith("_id", StringComparison.OrdinalIgnoreCase))
            {
                return identifier.Substring(0, identifier.Length - 3);
            }

            if (identifier.EndsWith("Id", StringComparison.Ordinal)
                || identifier.EndsWith("ID", StringComparison.Ordinal))
            {
                return identifier.Substring(0, identifier.Length - 2);
            }

            return identifier;
        }
    }
}