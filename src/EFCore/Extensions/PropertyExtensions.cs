// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IProperty" />.
    /// </summary>
    public static class PropertyExtensions
    {
        /// <summary>
        ///     Returns the <see cref="CoreTypeMapping" /> for the given property from a finalized model.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The type mapping. </returns>
        public static CoreTypeMapping GetTypeMapping([NotNull] this IProperty property)
        {
            var mapping = ((Property)property).TypeMapping;
            if (mapping == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ModelNotFinalized(nameof(GetTypeMapping)));
            }

            return mapping;
        }

        /// <summary>
        ///     Returns the <see cref="CoreTypeMapping" /> for the given property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        public static CoreTypeMapping FindTypeMapping([NotNull] this IProperty property)
            => ((Property)property).TypeMapping;

        /// <summary>
        ///     Finds the first principal property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <param name="property"> The foreign key property. </param>
        /// <returns> The first associated principal property, or <see langword="null" /> if none exists. </returns>
        public static IProperty FindFirstPrincipal([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

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
        ///     Finds the list of principal properties including the given property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <param name="property"> The foreign key property. </param>
        /// <returns> The list of all associated principal properties including the given property. </returns>
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
        ///     Gets a value indicating whether this property is used as a foreign key (or part of a composite foreign key).
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns> <see langword="true" /> if the property is used as a foreign key, otherwise <see langword="false" />. </returns>
        public static bool IsForeignKey([NotNull] this IProperty property)
            => Check.NotNull((Property)property, nameof(property)).ForeignKeys != null;

        /// <summary>
        ///     Gets a value indicating whether this property is used as an index (or part of a composite index).
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns> <see langword="true" /> if the property is used as an index, otherwise <see langword="false" />. </returns>
        public static bool IsIndex([NotNull] this IProperty property)
            => Check.NotNull((Property)property, nameof(property)).Indexes != null;

        /// <summary>
        ///     Gets a value indicating whether this property is used as a unique index (or part of a unique composite index).
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns> <see langword="true" /> if the property is used as an unique index, otherwise <see langword="false" />. </returns>
        public static bool IsUniqueIndex([NotNull] this IProperty property)
            => Check.NotNull(property, nameof(property)).AsProperty().Indexes?.Any(e => e.IsUnique) == true;

        /// <summary>
        ///     Gets a value indicating whether this property is used as the primary key (or part of a composite primary key).
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns> <see langword="true" /> if the property is used as the primary key, otherwise <see langword="false" />. </returns>
        public static bool IsPrimaryKey([NotNull] this IProperty property)
            => FindContainingPrimaryKey(property) != null;

        /// <summary>
        ///     Gets a value indicating whether this property is used as the primary key or alternate key
        ///     (or part of a composite primary or alternate key).
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns> <see langword="true" /> if the property is used as a key, otherwise <see langword="false" />. </returns>
        public static bool IsKey([NotNull] this IProperty property)
            => Check.NotNull((Property)property, nameof(property)).Keys != null;

        /// <summary>
        ///     Gets all foreign keys that use this property (including composite foreign keys in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get foreign keys for. </param>
        /// <returns> The foreign keys that use this property. </returns>
        public static IEnumerable<IForeignKey> GetContainingForeignKeys([NotNull] this IProperty property)
            => Check.NotNull((Property)property, nameof(property)).GetContainingForeignKeys();

        /// <summary>
        ///     Gets all indexes that use this property (including composite indexes in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get indexes for. </param>
        /// <returns> The indexes that use this property. </returns>
        public static IEnumerable<IIndex> GetContainingIndexes([NotNull] this IProperty property)
            => Check.NotNull((Property)property, nameof(property)).GetContainingIndexes();

        /// <summary>
        ///     Gets the primary key that uses this property (including a composite primary key in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get primary key for. </param>
        /// <returns> The primary that use this property, or <see langword="null" /> if it is not part of the primary key. </returns>
        public static IKey FindContainingPrimaryKey([NotNull] this IProperty property)
            => Check.NotNull((Property)property, nameof(property)).PrimaryKey;

        /// <summary>
        ///     Gets all primary or alternate keys that use this property (including composite keys in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get primary and alternate keys for. </param>
        /// <returns> The primary and alternate keys that use this property. </returns>
        public static IEnumerable<IKey> GetContainingKeys([NotNull] this IProperty property)
            => Check.NotNull((Property)property, nameof(property)).GetContainingKeys();

        /// <summary>
        ///     Gets the maximum length of data that is allowed in this property. For example, if the property is a <see cref="string" />
        ///     then this is the maximum number of characters.
        /// </summary>
        /// <param name="property"> The property to get the maximum length of. </param>
        /// <returns> The maximum length, or <see langword="null" /> if none if defined. </returns>
        public static int? GetMaxLength([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return (int?)property[CoreAnnotationNames.MaxLength];
        }

        /// <summary>
        ///     Gets the precision of data that is allowed in this property.
        ///     For example, if the property is a <see cref="decimal" /> then this is the maximum number of digits.
        /// </summary>
        /// <param name="property"> The property to get the precision of. </param>
        /// <returns> The precision, or <see langword="null" /> if none is defined. </returns>
        public static int? GetPrecision([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return (int?)property[CoreAnnotationNames.Precision];
        }

        /// <summary>
        ///     Gets the scale of data that is allowed in this property.
        ///     For example, if the property is a <see cref="decimal" /> then this is the maximum number of decimal places.
        /// </summary>
        /// <param name="property"> The property to get the scale of. </param>
        /// <returns> The scale, or <see langword="null" /> if none is defined. </returns>
        public static int? GetScale([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return (int?)property[CoreAnnotationNames.Scale];
        }

        /// <summary>
        ///     Gets a value indicating whether or not the property can persist Unicode characters.
        /// </summary>
        /// <param name="property"> The property to get the Unicode setting for. </param>
        /// <returns> The Unicode setting, or <see langword="null" /> if none is defined. </returns>
        public static bool? IsUnicode([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return (bool?)property[CoreAnnotationNames.Unicode];
        }

        /// <summary>
        ///     <para>
        ///         Gets a value indicating whether or not this property can be modified before the entity is
        ///         saved to the database.
        ///     </para>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Throw" />, then an exception
        ///         will be thrown if a value is assigned to this property when it is in
        ///         the <see cref="EntityState.Added" /> state.
        ///     </para>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Ignore" />, then any value
        ///         set will be ignored when it is in the <see cref="EntityState.Added" /> state.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        public static PropertySaveBehavior GetBeforeSaveBehavior([NotNull] this IProperty property)
            => (PropertySaveBehavior?)Check.NotNull(property, nameof(property))[CoreAnnotationNames.BeforeSaveBehavior]
                ?? (property.ValueGenerated == ValueGenerated.OnAddOrUpdate
                    ? PropertySaveBehavior.Ignore
                    : PropertySaveBehavior.Save);

        /// <summary>
        ///     <para>
        ///         Gets a value indicating whether or not this property can be modified after the entity is
        ///         saved to the database.
        ///     </para>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Throw" />, then an exception
        ///         will be thrown if a new value is assigned to this property after the entity exists in the database.
        ///     </para>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Ignore" />, then any modification to the
        ///         property value of an entity that already exists in the database will be ignored.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        public static PropertySaveBehavior GetAfterSaveBehavior([NotNull] this IProperty property)
            => (PropertySaveBehavior?)Check.NotNull(property, nameof(property))[CoreAnnotationNames.AfterSaveBehavior]
                ?? (property.IsKey()
                    ? PropertySaveBehavior.Throw
                    : property.ValueGenerated.ForUpdate()
                        ? PropertySaveBehavior.Ignore
                        : PropertySaveBehavior.Save);

        /// <summary>
        ///     Gets the factory that has been set to generate values for this property, if any.
        /// </summary>
        /// <param name="property"> The property to get the value generator factory for. </param>
        /// <returns> The factory, or <see langword="null" /> if no factory has been set. </returns>
        public static Func<IProperty, IEntityType, ValueGenerator> GetValueGeneratorFactory([NotNull] this IProperty property)
            => (Func<IProperty, IEntityType, ValueGenerator>)
                Check.NotNull(property, nameof(property))[CoreAnnotationNames.ValueGeneratorFactory];

        /// <summary>
        ///     Gets the custom <see cref="ValueConverter" /> set for this property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The converter, or <see langword="null" /> if none has been set. </returns>
        public static ValueConverter GetValueConverter([NotNull] this IProperty property)
            => (ValueConverter)Check.NotNull(property, nameof(property))[CoreAnnotationNames.ValueConverter];

        /// <summary>
        ///     Gets the type that the property value will be converted to before being sent to the database provider.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The provider type, or <see langword="null" /> if none has been set. </returns>
        public static Type GetProviderClrType([NotNull] this IProperty property)
            => (Type)Check.NotNull(property, nameof(property))[CoreAnnotationNames.ProviderClrType];

        /// <summary>
        ///     Gets the <see cref="ValueComparer" /> for this property, or <see langword="null" /> if none is set.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The comparer, or <see langword="null" /> if none has been set. </returns>
        public static ValueComparer GetValueComparer([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return (ValueComparer)property[CoreAnnotationNames.ValueComparer]
                ?? property.FindTypeMapping()?.Comparer;
        }

        /// <summary>
        ///     Gets the <see cref="ValueComparer" /> to use with keys for this property, or <see langword="null" /> if none is set.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The comparer, or <see langword="null" /> if none has been set. </returns>
        public static ValueComparer GetKeyValueComparer([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return (ValueComparer)property[CoreAnnotationNames.ValueComparer]
                ?? property.FindTypeMapping()?.KeyComparer;
        }

        /// <summary>
        ///     Gets the <see cref="ValueComparer" /> to use for structural copies for this property, or <see langword="null" /> if none is set.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The comparer, or <see langword="null" /> if none has been set. </returns>
        [Obsolete("Use GetKeyValueComparer. A separate structural comparer is no longer supported.")]
        public static ValueComparer GetStructuralValueComparer([NotNull] this IProperty property)
            => property.GetKeyValueComparer();

        /// <summary>
        ///     Creates an <see cref="IEqualityComparer{T}" /> for values of the given property type.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <typeparam name="TProperty"> The property type. </typeparam>
        /// <returns> A new equality comparer. </returns>
        public static IEqualityComparer<TProperty> CreateKeyEqualityComparer<TProperty>([NotNull] this IProperty property)
        {
            var comparer = property.GetKeyValueComparer();

            return comparer is IEqualityComparer<TProperty> nullableComparer
                ? nullableComparer
                : new NullableComparer<TProperty>(comparer);
        }

        private sealed class NullableComparer<TNullableKey> : IEqualityComparer<TNullableKey>
        {
            private readonly IEqualityComparer _comparer;

            public NullableComparer(IEqualityComparer comparer)
            {
                _comparer = comparer;
            }

            public bool Equals(TNullableKey x, TNullableKey y)
                => (x == null && y == null)
                    || (x != null && y != null && _comparer.Equals(x, y));

            public int GetHashCode(TNullableKey obj)
                => _comparer.GetHashCode(obj);
        }

        /// <summary>
        ///     Creates a formatted string representation of the given properties such as is useful
        ///     when throwing exceptions about keys, indexes, etc. that use the properties.
        /// </summary>
        /// <param name="properties"> The properties to format. </param>
        /// <param name="includeTypes"> If true, then type names are included in the string. The default is <see langword="false" />.</param>
        /// <returns> The string representation. </returns>
        public static string Format([NotNull] this IEnumerable<IPropertyBase> properties, bool includeTypes = false)
            => "{"
                + string.Join(
                    ", ",
                    properties.Select(
                        p => "'" + p.Name + "'" + (includeTypes ? " : " + p.ClrType.DisplayName(fullName: false) : "")))
                + "}";

        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="property"> The metadata item. </param>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        public static string ToDebugString(
            [NotNull] this IProperty property,
            MetadataDebugStringOptions options,
            int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder.Append(indentString);

            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
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

            if (property.IsIndexerProperty())
            {
                builder.Append(" Indexer");
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
                builder.Append(" MaxLength(").Append(property.GetMaxLength()).Append(")");
            }

            if (property.IsUnicode() == false)
            {
                builder.Append(" Ansi");
            }

            if (property.GetPropertyAccessMode() != PropertyAccessMode.PreferField)
            {
                builder.Append(" PropertyAccessMode.").Append(property.GetPropertyAccessMode());
            }

            if ((options & MetadataDebugStringOptions.IncludePropertyIndexes) != 0)
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

            if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(property.AnnotationsToDebugString(indent + 2));
            }

            return builder.ToString();
        }
    }
}
