// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class ConstraintNamer
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string GetDefaultName([NotNull] IForeignKey foreignKey)
        {
            var otherForeignKeyNames = new HashSet<string>(
                foreignKey.DeclaringEntityType.RootType().GetDerivedTypesInclusive()
                    .SelectMany(et => et.GetDeclaredForeignKeys())
                    .Where(fk => fk != foreignKey)
                    .Where(fk => !ConfigurationSource.Convention.Overrides(
                        (fk as ForeignKey)
                            ?.FindAnnotation(RelationalAnnotationNames.Name)
                            ?.GetConfigurationSource()))
                    .Select(fk => fk.Relational().Name),
                StringComparer.OrdinalIgnoreCase);

            var baseName = new StringBuilder()
                .Append("FK_")
                .Append(foreignKey.DeclaringEntityType.Relational().TableName)
                .Append("_")
                .Append(foreignKey.PrincipalEntityType.Relational().TableName)
                .Append("_")
                .AppendJoin(foreignKey.Properties.Select(p => p.Relational().ColumnName), "_")
                .ToString();

            var name = baseName;
            var uniquifier = 0;
            while (otherForeignKeyNames.Contains(name))
            {
                name = baseName + uniquifier++;
            }

            return name;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string GetDefaultName([NotNull] IIndex index)
        {
            var otherIndexNames = new HashSet<string>(
                index.DeclaringEntityType.RootType().GetDerivedTypesInclusive()
                    .SelectMany(et => et.GetDeclaredIndexes())
                    .Where(i => i != index)
                    .Where(i => !ConfigurationSource.Convention.Overrides(
                        (i as Index)
                            ?.FindAnnotation(RelationalAnnotationNames.Name)
                            ?.GetConfigurationSource()))
                    .Select(i => i.Relational().Name),
                StringComparer.OrdinalIgnoreCase);

            var baseName = new StringBuilder()
                .Append("IX_")
                .Append(index.DeclaringEntityType.Relational().TableName)
                .Append("_")
                .AppendJoin(index.Properties.Select(p => p.Relational().ColumnName), "_")
                .ToString();

            var name = baseName;
            var uniquifier = 0;
            while (otherIndexNames.Contains(name))
            {
                name = baseName + uniquifier++;
            }

            return name;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string GetDefaultName([NotNull] IKey Key)
        {
            var builder = new StringBuilder();
            var tableName = Key.DeclaringEntityType.Relational().TableName;

            if (Key.IsPrimaryKey())
            {
                builder
                    .Append("PK_")
                    .Append(tableName);
            }
            else
            {
                builder
                    .Append("AK_")
                    .Append(tableName)
                    .Append("_")
                    .AppendJoin(Key.Properties.Select(p => p.Relational().ColumnName), "_");
            }

            return builder.ToString();
        }
    }
}
