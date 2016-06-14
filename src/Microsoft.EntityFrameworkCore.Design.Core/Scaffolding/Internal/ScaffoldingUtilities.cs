// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Configuration.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ScaffoldingUtilities
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateLambdaToKey(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] string lambdaIdentifier)
        {
            Check.NotNull(properties, nameof(properties));
            Check.NotEmpty(lambdaIdentifier, nameof(lambdaIdentifier));

            var sb = new StringBuilder();

            if (properties.Count > 1)
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
            sb.Append("." + nameof(EntityTypeBuilder<EntityType>.HasOne) + "(");
            sb.Append(dependentEndLambdaIdentifier);
            sb.Append(" => ");
            sb.Append(dependentEndLambdaIdentifier);
            sb.Append(".");
            sb.Append(rc.DependentEndNavigationPropertyName);
            sb.AppendLine(")");

            using (sb.Indent())
            {
                var withMethodName = rc.ForeignKey.IsUnique
                    ? nameof(ReferenceNavigationBuilder<EntityType, EntityType>.WithOne)
                    : nameof(ReferenceNavigationBuilder<EntityType, EntityType>.WithMany);
                sb.Append("." + withMethodName + "(");
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
                    var hasPrincipalKeyMethodName = rc.ForeignKey.IsUnique
                        ? nameof(ReferenceReferenceBuilder<EntityType, EntityType>.HasPrincipalKey)
                        : nameof(ReferenceReferenceBuilder.HasPrincipalKey);
                    sb.Append("." + hasPrincipalKeyMethodName);
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

                var hasForeignKeyMethodName = rc.ForeignKey.IsUnique
                    ? nameof(ReferenceReferenceBuilder<EntityType, EntityType>.HasForeignKey)
                    : nameof(ReferenceCollectionBuilder.HasForeignKey);
                sb.Append("." + hasForeignKeyMethodName);
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
                    var onDeleteMethodName = rc.ForeignKey.IsUnique
                        ? nameof(ReferenceReferenceBuilder.OnDelete)
                        : nameof(ReferenceCollectionBuilder.OnDelete);
                    sb.Append("." + onDeleteMethodName + "(");
                    sb.Append(CSharpUtilities.Instance.GenerateLiteral(rc.OnDeleteAction));
                    sb.Append(")");
                }

                var foreignKey = rc.ForeignKey as ForeignKey;
                if (foreignKey != null
                    && foreignKey.Relational().Name !=
                    RelationalForeignKeyAnnotations.GetDefaultForeignKeyName(
                        foreignKey.DeclaringEntityType.Relational().TableName,
                        foreignKey.PrincipalEntityType.Relational().TableName,
                        foreignKey.Properties.Select(p => p.Relational().ColumnName)))
                {
                    sb.AppendLine();
                    var hasConstraintMethodName = foreignKey.IsUnique
                        ? nameof(RelationalReferenceReferenceBuilderExtensions.HasConstraintName)
                        : nameof(RelationalReferenceCollectionBuilderExtensions.HasConstraintName);
                    sb.Append("." + hasConstraintMethodName + "(");
                    sb.Append(CSharpUtilities.Instance.DelimitString(foreignKey.Relational().Name));
                    sb.Append(")");
                }

                sb.AppendLine(";");
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IProperty> OrderedProperties([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var primaryKeyProperties =
                ((EntityType)entityType).FindPrimaryKey()?.Properties.ToList()
                ?? new List<Property>();

            foreach (var property in primaryKeyProperties)
            {
                yield return property;
            }

            foreach (var property in
                entityType.GetProperties()
                    .Except(primaryKeyProperties)
                    .OrderBy(p => p.Name, StringComparer.Ordinal))
            {
                yield return property;
            }
        }
    }
}
