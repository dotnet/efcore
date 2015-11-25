// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Scaffolding.Internal.Configuration;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Internal
{
    public class ScaffoldingUtilities
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

        public virtual void LayoutRelationshipConfigurationLines(
            [NotNull] IndentedStringBuilder sb,
            [NotNull] string entityLambdaIdentifier,
            [NotNull] RelationshipConfiguration rc,
            [NotNull] string dependentEndLambdaIdentifier,
            [NotNull] string principalEndLambdaIdentifier)
        {
            Check.NotNull(sb, nameof(sb));
            Check.NotEmpty(entityLambdaIdentifier, nameof(entityLambdaIdentifier));
            Check.NotNull(rc, nameof(rc));
            Check.NotEmpty(dependentEndLambdaIdentifier, nameof(dependentEndLambdaIdentifier));
            Check.NotEmpty(principalEndLambdaIdentifier, nameof(principalEndLambdaIdentifier));

            sb.Append(entityLambdaIdentifier);
            sb.Append(".HasOne(");
            sb.Append(dependentEndLambdaIdentifier);
            sb.Append(" => ");
            sb.Append(dependentEndLambdaIdentifier);
            sb.Append(".");
            sb.Append(rc.DependentEndNavigationPropertyName);
            sb.AppendLine(")");

            using (sb.Indent())
            {
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
                sb.AppendLine(")");

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
                        .AppendLine(")");
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
                    sb.AppendLine();
                    sb.Append(".OnDelete(");
                    sb.Append(CSharpUtilities.Instance.GenerateLiteral(rc.OnDeleteAction));
                    sb.Append(")");
                }

                sb.AppendLine(";");
            }
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
    }
}
