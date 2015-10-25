// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Scaffolding.Internal.Configuration;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Internal
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

        public virtual string GetDependentEndCandidateNavigationPropertyName([NotNull] IForeignKey foreignKey)
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

        public virtual string GetPrincipalEndCandidateNavigationPropertyName([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            return foreignKey.DeclaringEntityType.DisplayName();
        }

        public virtual List<string> LayoutPropertyConfigurationLines(
            [NotNull] PropertyConfiguration pc,
            [NotNull] string propertyLambdaIdentifier,
            [NotNull] string indent,
            bool useFluentApi)
        {
            Check.NotNull(pc, nameof(pc));
            Check.NotEmpty(propertyLambdaIdentifier, nameof(propertyLambdaIdentifier));
            Check.NotNull(indent, nameof(indent));

            var lines = new List<string>();
            foreach (var keyValuePair in pc.GetFluentApiConfigurations(useFluentApi))
            {
                var forMethod = keyValuePair.Key;
                var fluentApiConfigurationList = keyValuePair.Value;
                if (fluentApiConfigurationList.Count == 0)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(forMethod))
                {
                    foreach (var fluentApiConfiguration in fluentApiConfigurationList)
                    {
                        lines.Add("." + fluentApiConfiguration.MethodBody);
                    }
                }
                else
                {
                    if (fluentApiConfigurationList.Count == 1)
                    {
                        lines.Add("." + forMethod + "()." + fluentApiConfigurationList.First().MethodBody);
                    }
                    else
                    {
                        lines.Add("." + forMethod + "(" + propertyLambdaIdentifier + " =>");
                        lines.Add("{");
                        foreach (var fluentApiConfiguration in fluentApiConfigurationList)
                        {
                            lines.Add(indent + propertyLambdaIdentifier + "." + fluentApiConfiguration.MethodBody + ";");
                        }
                        lines.Add("})");
                    }
                }
            }

            return lines;
        }

        public virtual string LayoutRelationshipConfigurationLine(
            [NotNull] RelationshipConfiguration rc,
            [NotNull] string dependentEndLambdaIdentifier,
            [NotNull] string principalEndLambdaIdentifier)
        {
            Check.NotNull(rc, nameof(rc));
            Check.NotEmpty(dependentEndLambdaIdentifier, nameof(dependentEndLambdaIdentifier));
            Check.NotEmpty(principalEndLambdaIdentifier, nameof(principalEndLambdaIdentifier));

            var sb = new StringBuilder();
            sb.Append("HasOne(");
            sb.Append(dependentEndLambdaIdentifier);
            sb.Append(" => ");
            sb.Append(dependentEndLambdaIdentifier);
            sb.Append(".");
            sb.Append(rc.DependentEndNavigationPropertyName);
            sb.Append(")");

            if (rc.ForeignKey.IsUnique)
            {
                sb.Append(".WithOne(");
            }
            else
            {
                sb.Append(".WithMany(");
            }
            if (!string.IsNullOrEmpty(rc.PrincipalEndNavigationPropertyName))
            {
                sb.Append(principalEndLambdaIdentifier);
                sb.Append(" => ");
                sb.Append(principalEndLambdaIdentifier);
                sb.Append(".");
                sb.Append(rc.PrincipalEndNavigationPropertyName);
            }
            sb.Append(")");

            if (!rc.ForeignKey.PrincipalKey.IsPrimaryKey())
            {
                sb.Append(".HasPrincipalKey");
                if (rc.ForeignKey.IsUnique)
                {
                    // If the relationship is 1:1 need to define to which end
                    // the PrincipalKey properties belong.
                    sb.Append("<");
                    sb.Append(rc.ForeignKey.PrincipalEntityType.DisplayName());
                    sb.Append(">");
                }
                sb.Append("(")
                    .Append(principalEndLambdaIdentifier)
                    .Append(" => ")
                    .Append(GenerateLambdaToKey(rc.ForeignKey.PrincipalKey.Properties, principalEndLambdaIdentifier))
                    .Append(")");
            }

            sb.Append(".HasForeignKey");
            if (rc.ForeignKey.IsUnique)
            {
                // If the relationship is 1:1 need to define to which end
                // the ForeignKey properties belong.
                sb.Append("<");
                sb.Append(rc.EntityConfiguration.EntityType.DisplayName());
                sb.Append(">");
            }

            sb.Append("(");
            sb.Append(dependentEndLambdaIdentifier);
            sb.Append(" => ");
            sb.Append(GenerateLambdaToKey(rc.ForeignKey.Properties, dependentEndLambdaIdentifier));
            sb.Append(")");

            var defaultOnDeleteAction = rc.ForeignKey.IsRequired
                ? DeleteBehavior.Cascade
                : DeleteBehavior.Restrict;

            if (rc.OnDeleteAction != defaultOnDeleteAction)
            {
                sb.Append(".OnDelete(");
                sb.Append(CSharpUtilities.Instance.GenerateLiteral(rc.OnDeleteAction));
                sb.Append(")");
            }

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
