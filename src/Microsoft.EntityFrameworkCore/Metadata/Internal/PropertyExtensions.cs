// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class PropertyExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IEntityType> GetContainingEntityTypes([NotNull] this IProperty property)
            => property.DeclaringEntityType.GetDerivedTypesInclusive();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IForeignKey> GetReferencingForeignKeys([NotNull] this IProperty property)
            => property.GetContainingKeys().SelectMany(k => k.GetReferencingForeignKeys());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IProperty GetGenerationProperty([NotNull] this IProperty property)
        {
            var traversalList = new List<IProperty> { property };

            var index = 0;
            while (index < traversalList.Count)
            {
                var currentProperty = traversalList[index];

                if (currentProperty.RequiresValueGenerator)
                {
                    return currentProperty;
                }

                foreach (var foreignKey in currentProperty.DeclaringEntityType.GetForeignKeys())
                {
                    for (var propertyIndex = 0; propertyIndex < foreignKey.Properties.Count; propertyIndex++)
                    {
                        if (currentProperty == foreignKey.Properties[propertyIndex])
                        {
                            var nextProperty = foreignKey.PrincipalKey.Properties[propertyIndex];
                            if (!traversalList.Contains(nextProperty))
                            {
                                traversalList.Add(nextProperty);
                            }
                        }
                    }
                }
                index++;
            }
            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int GetOriginalValueIndex([NotNull] this IProperty property)
            => property.GetPropertyIndexes().OriginalValueIndex;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool MayBeStoreGenerated([NotNull] this IProperty property)
        {
            if (property.ValueGenerated != ValueGenerated.Never)
            {
                return true;
            }

            if (property.IsKeyOrForeignKey())
            {
                var generationProperty = property.GetGenerationProperty();
                return (generationProperty != null)
                       && (generationProperty.ValueGenerated != ValueGenerated.Never);
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool RequiresOriginalValue([NotNull] this IProperty property)
            => property.DeclaringEntityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.ChangingAndChangedNotifications
               || property.IsConcurrencyToken
               || property.IsKey()
               || property.IsForeignKey();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsKeyOrForeignKey([NotNull] this IProperty property)
            => property.IsKey()
               || property.IsForeignKey();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IProperty FindPrincipal([NotNull] this IProperty property)
        {
            var concreteProperty = property.AsProperty();
            if (concreteProperty.ForeignKeys != null)
            {
                foreach (var foreignKey in concreteProperty.ForeignKeys)
                {
                    for (var propertyIndex = 0; propertyIndex < foreignKey.Properties.Count; propertyIndex++)
                    {
                        if (property == foreignKey.Properties[propertyIndex])
                        {
                            return foreignKey.PrincipalKey.Properties[propertyIndex];
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string ToDebugString([NotNull] this IProperty property, bool singleLine = true, [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            builder.Append(indent);

            if (singleLine)
            {
                builder.Append("Property: ").Append(property.DeclaringEntityType.DisplayName()).Append(".");
            }

            builder.Append(property.Name).Append(" (");

            var field = property.GetFieldName();
            if (field == null)
            {
                builder.Append("no field, ");
            }
            else if (!field.EndsWith(">k__BackingField"))
            {
                builder.Append(field).Append(", ");
            }

            builder.Append(property.ClrType.ShortDisplayName()).Append(")");

            if (property.IsShadowProperty)
            {
                builder.Append(" Shadow");
            }

            if (!property.IsNullable)
            {
                builder.Append(" Required");
            }

            if (property.IsPrimaryKey())
            {
                builder.Append(" PK");
            }

            if (property.IsForeignKey())
            {
                builder.Append(" FK");
            }

            if (property.IsKey()
                && !property.IsPrimaryKey())
            {
                builder.Append(" AlternateKey");
            }

            if (property.IsIndex())
            {
                builder.Append(" Index");
            }

            if (property.IsConcurrencyToken)
            {
                builder.Append(" Concurrency");
            }

            if (property.IsReadOnlyAfterSave)
            {
                builder.Append(" ReadOnlyAfterSave");
            }

            if (property.IsReadOnlyBeforeSave)
            {
                builder.Append(" ReadOnlyBeforeSave");
            }

            if (property.RequiresValueGenerator)
            {
                builder.Append(" RequiresValueGenerator");
            }

            if (property.ValueGenerated != ValueGenerated.Never)
            {
                builder.Append(" ValueGenerated.").Append(property.ValueGenerated);
            }

            if (property.IsStoreGeneratedAlways)
            {
                builder.Append(" StoreGeneratedAlways");
            }

            if (property.GetMaxLength() != null)
            {
                builder.Append(" MaxLength").Append(property.GetMaxLength());
            }

            if (property.IsUnicode() == false)
            {
                builder.Append(" Ansi");
            }

            if (property.GetPropertyAccessMode() != null)
            {
                builder.Append(" PropertyAccessMode.").Append(property.GetPropertyAccessMode());
            }

            var indexes = property.GetPropertyIndexes();
            builder.Append(" ").Append(indexes.Index);
            builder.Append(" ").Append(indexes.OriginalValueIndex);
            builder.Append(" ").Append(indexes.RelationshipIndex);
            builder.Append(" ").Append(indexes.ShadowIndex);
            builder.Append(" ").Append(indexes.StoreGenerationIndex);

            if (!singleLine)
            {
                builder.Append(property.AnnotationsToDebugString(indent + "  "));
            }

            return builder.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Property AsProperty([NotNull] this IProperty property, [NotNull] [CallerMemberName] string methodName = "")
            => MetadataExtensions.AsConcreteMetadataType<IProperty, Property>(property, methodName);
    }
}
