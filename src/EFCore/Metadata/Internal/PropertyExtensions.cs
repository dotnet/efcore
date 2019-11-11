// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class PropertyExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool ForAdd(this ValueGenerated valueGenerated)
            => (valueGenerated & ValueGenerated.OnAdd) != 0;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool ForUpdate(this ValueGenerated valueGenerated)
            => (valueGenerated & ValueGenerated.OnUpdate) != 0;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<IEntityType> GetContainingEntityTypes([NotNull] this IProperty property)
            => property.DeclaringEntityType.GetDerivedTypesInclusive();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<IForeignKey> GetReferencingForeignKeys([NotNull] this IProperty property)
            => property.GetContainingKeys().SelectMany(k => k.GetReferencingForeignKeys());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IProperty GetGenerationProperty([NotNull] this IProperty property)
        {
            var traversalList = new List<IProperty> { property };

            var index = 0;
            while (index < traversalList.Count)
            {
                var currentProperty = traversalList[index];

                if (currentProperty.RequiresValueGenerator())
                {
                    return currentProperty;
                }

                foreach (var foreignKey in currentProperty.GetContainingForeignKeys())
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
        ///     Gets a value indicating whether this property requires a <see cref="ValueGenerator" /> to generate
        ///     values when new entities are added to the context.
        /// </summary>
        public static bool RequiresValueGenerator([NotNull] this IProperty property)
            => ((property.ValueGenerated & ValueGenerated.OnAdd) == ValueGenerated.OnAdd
                    && !property.IsForeignKey()
                    && property.IsKey())
                || property.GetValueGeneratorFactory() != null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool RequiresOriginalValue([NotNull] this IProperty property)
            => property.DeclaringEntityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.ChangingAndChangedNotifications
                || property.IsConcurrencyToken
                || property.IsKey()
                || property.IsForeignKey();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsKeyOrForeignKey([NotNull] this IProperty property)
            => property.IsKey()
                || property.IsForeignKey();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyList<IProperty> FindPrincipals([NotNull] this IProperty property)
        {
            var principals = new List<IProperty> { property };
            AddPrincipals(property, principals);
            return principals;
        }

        private static void AddPrincipals(IProperty property, List<IProperty> visited)
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
                            var principal = foreignKey.PrincipalKey.Properties[propertyIndex];
                            if (!visited.Contains(principal))
                            {
                                visited.Add(principal);

                                AddPrincipals(principal, visited);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string ToDebugString(
            [NotNull] this IProperty property,
            bool singleLine = true,
            bool includeIndexes = false,
            [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            builder.Append(indent);

            if (singleLine)
            {
                builder.Append($"Property: {property.DeclaringEntityType.DisplayName()}.");
            }

            builder.Append(property.Name).Append(" (");

            var field = property.GetFieldName();
            if (field == null)
            {
                builder.Append("no field, ");
            }
            else if (!field.EndsWith(">k__BackingField", StringComparison.Ordinal))
            {
                builder.Append(field).Append(", ");
            }

            builder.Append(property.ClrType.ShortDisplayName()).Append(")");

            if (property.IsShadowProperty())
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

            if (property.GetBeforeSaveBehavior() != PropertySaveBehavior.Save)
            {
                builder.Append(" BeforeSave:").Append(property.GetBeforeSaveBehavior());
            }

            if (property.GetAfterSaveBehavior() != PropertySaveBehavior.Save)
            {
                builder.Append(" AfterSave:").Append(property.GetAfterSaveBehavior());
            }

            if (property.ValueGenerated != ValueGenerated.Never)
            {
                builder.Append(" ValueGenerated.").Append(property.ValueGenerated);
            }

            if (property.GetMaxLength() != null)
            {
                builder.Append(" MaxLength").Append(property.GetMaxLength());
            }

            if (property.IsUnicode() == false)
            {
                builder.Append(" Ansi");
            }

            if (property.GetPropertyAccessMode() != PropertyAccessMode.PreferField)
            {
                builder.Append(" PropertyAccessMode.").Append(property.GetPropertyAccessMode());
            }

            if (includeIndexes)
            {
                var indexes = property.GetPropertyIndexes();
                if (indexes != null)
                {
                    builder.Append(" ").Append(indexes.Index);
                    builder.Append(" ").Append(indexes.OriginalValueIndex);
                    builder.Append(" ").Append(indexes.RelationshipIndex);
                    builder.Append(" ").Append(indexes.ShadowIndex);
                    builder.Append(" ").Append(indexes.StoreGenerationIndex);
                }
            }

            if (!singleLine)
            {
                builder.Append(property.AnnotationsToDebugString(indent + "  "));
            }

            return builder.ToString();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Property AsProperty([NotNull] this IProperty property, [NotNull] [CallerMemberName] string methodName = "")
            => MetadataExtensions.AsConcreteMetadataType<IProperty, Property>(property, methodName);
    }
}
