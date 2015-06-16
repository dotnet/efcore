// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration;
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
                ((EntityType)entityType).FindPrimaryKey()?.Properties.ToList()
                ?? Enumerable.Empty<Property>();

            foreach (var property in primaryKeyProperties)
            {
                yield return property;
            }

            foreach (var property in
                entityType.GetProperties()
                    .Except(primaryKeyProperties)
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

            return foreignKey.PrincipalEntityType.DisplayName();
        }

        public virtual string GetPrincipalEndCandidateNavigationPropertyName([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            return foreignKey.EntityType.DisplayName();
        }

        public virtual string ConstructNavigationConfiguration([NotNull] NavigationConfiguration navigationConfiguration)
        {
            Check.NotNull(navigationConfiguration, nameof(navigationConfiguration));

            var sb = new StringBuilder();
            sb.Append("Reference");
            sb.Append("(d => d.");
            sb.Append(navigationConfiguration.DependentEndNavigationPropertyName);
            sb.Append(")");

            if (navigationConfiguration.ForeignKey.IsUnique)
            {
                sb.Append(".InverseReference(");
            }
            else
            {
                sb.Append(".InverseCollection(");
            }
            if (!string.IsNullOrEmpty(navigationConfiguration.PrincipalEndNavigationPropertyName))
            {
                sb.Append("p => p.");
                sb.Append(navigationConfiguration.PrincipalEndNavigationPropertyName);
            }
            sb.Append(")");

            sb.Append(".ForeignKey");
            if (navigationConfiguration.ForeignKey.IsUnique)
            {
                // If the relationship is 1:1 need to define to which end
                // the ForeignKey properties belong.
                sb.Append("<");
                sb.Append(navigationConfiguration.EntityConfiguration.EntityType.DisplayName());
                sb.Append(">");
            }

            sb.Append("(d => ");
            sb.Append(GenerateLambdaToKey(navigationConfiguration.ForeignKey.Properties, "d"));
            sb.Append(")");

            return sb.ToString();
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
            foreach (var c in firstString)
            {
                foreach (var s in stringsEnumerable)
                {
                    if (s.Length <= prefixLength
                        || s[prefixLength] != c)
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
