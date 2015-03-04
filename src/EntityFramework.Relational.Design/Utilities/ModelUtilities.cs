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

            var foreignKeyProperties = entityType.ForeignKeys.SelectMany(fk => fk.Properties).Distinct().ToList();
            foreach (var property in
                entityType
                .Properties
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

            if (foreignKey.Properties.Count == 1)
            {
                var foreignKeyPropertyName = foreignKey.Properties.Single().Name;
                if (foreignKeyPropertyName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    foreignKeyPropertyName =
                        foreignKeyPropertyName.Substring(0, foreignKeyPropertyName.Length - 2);
                }

                if (!string.IsNullOrEmpty(foreignKeyPropertyName))
                {
                    return foreignKeyPropertyName;
                }
            }

            return foreignKey.ReferencedEntityType.SimpleName;
        }

        public virtual string GetPrincipalEndCandidateNavigationPropertyName([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            return foreignKey.EntityType.SimpleName;
        }
    }
}