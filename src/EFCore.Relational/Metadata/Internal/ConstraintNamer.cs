// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            var baseName = new StringBuilder()
                .Append("FK_")
                .Append(foreignKey.DeclaringEntityType.Relational().TableName)
                .Append("_")
                .Append(foreignKey.PrincipalEntityType.Relational().TableName)
                .Append("_")
                .AppendJoin(foreignKey.Properties.Select(p => p.Relational().ColumnName), "_")
                .ToString();

            return Truncate(baseName, null, foreignKey.DeclaringEntityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string GetDefaultName([NotNull] IIndex index)
        {
            var baseName = new StringBuilder()
                .Append("IX_")
                .Append(index.DeclaringEntityType.Relational().TableName)
                .Append("_")
                .AppendJoin(index.Properties.Select(p => p.Relational().ColumnName), "_")
                .ToString();

            return Truncate(baseName, null, index.DeclaringEntityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string GetDefaultName([NotNull] IKey key)
        {
            var sharedTablePrincipalPrimaryKeyProperty = key.Properties[0].FindSharedTableRootPrimaryKeyProperty();
            if (sharedTablePrincipalPrimaryKeyProperty != null)
            {
                return sharedTablePrincipalPrimaryKeyProperty.GetContainingPrimaryKey().Relational().Name;
            }

            var builder = new StringBuilder();
            var tableName = key.DeclaringEntityType.Relational().TableName;

            if (key.IsPrimaryKey())
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
                    .AppendJoin(key.Properties.Select(p => p.Relational().ColumnName), "_");
            }

            return Truncate(builder.ToString(), null, key.DeclaringEntityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string GetDefaultName([NotNull] IProperty property)
        {
            var sharedTablePrincipalPrimaryKeyProperty = property.FindSharedTableRootPrimaryKeyProperty();
            if (sharedTablePrincipalPrimaryKeyProperty != null)
            {
                return sharedTablePrincipalPrimaryKeyProperty.Relational().ColumnName;
            }

            var entityType = property.DeclaringEntityType;
            StringBuilder builder = null;
            do
            {
                var ownership = entityType.GetForeignKeys().SingleOrDefault(fk => fk.IsOwnership);
                if (ownership == null)
                {
                    entityType = null;
                }
                else
                {
                    var ownerType = ownership.PrincipalEntityType;
                    var entityTypeAnnotations = entityType.Relational();
                    var ownerTypeAnnotations = ownerType.Relational();
                    if (entityTypeAnnotations.TableName == ownerTypeAnnotations.TableName
                        && entityTypeAnnotations.Schema == ownerTypeAnnotations.Schema)
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder();
                        }

                        builder.Insert(0, "_");
                        builder.Insert(0, ownership.PrincipalToDependent.Name);
                        entityType = ownerType;
                    }
                    else
                    {
                        entityType = null;
                    }
                }
            }
            while (entityType != null);

            var baseName = property.Name;
            if (builder != null)
            {
                builder.Append(property.Name);
                baseName = builder.ToString();
            }

            return Truncate(baseName, null, property.DeclaringEntityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string Truncate([NotNull] string name, int? uniquifier, int maxLength)
        {
            var uniquifierLength = GetLength(uniquifier);
            var maxNameLength = maxLength - uniquifierLength;

            var builder = new StringBuilder();
            if (name.Length <= maxNameLength)
            {
                builder.Append(name);
            }
            else
            {
                builder.Append(name, 0, maxNameLength - 1);
                builder.Append("~");
            }

            if (uniquifier != null)
            {
                builder.Append(uniquifier.Value);
            }

            return builder.ToString();
        }

        private static int GetLength(int? number)
        {
            if (number == null)
            {
                return 0;
            }

            var length = 0;
            do
            {
                number /= 10;
                length++;
            }
            while (number.Value >= 1);

            return length;
        }
    }
}
