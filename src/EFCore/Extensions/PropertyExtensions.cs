// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
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
            var mapping = (CoreTypeMapping)property[CoreAnnotationNames.TypeMapping];

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
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        public static CoreTypeMapping FindTypeMapping([NotNull] this IProperty property)
            => (CoreTypeMapping)property[CoreAnnotationNames.TypeMapping];

        /// <summary>
        ///     Returns the <see cref="CoreTypeMapping" /> for the given property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        [Obsolete("Use FindTypeMapping instead")]
        public static CoreTypeMapping FindMapping([NotNull] this IProperty property)
            => property.FindTypeMapping();

        /// <summary>
        ///     Finds the first principal property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <param name="property"> The foreign key property. </param>
        /// <returns> The first associated principal property, or <c>null</c> if none exists. </returns>
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
        ///     Gets a value indicating whether this property is used as a foreign key (or part of a composite foreign key).
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns>
        ///     <c>true</c> if the property is used as a foreign key, otherwise <c>false</c>.
        /// </returns>
        public static bool IsForeignKey([NotNull] this IProperty property)
            => Check.NotNull(property, nameof(property)).AsProperty().ForeignKeys != null;

        /// <summary>
        ///     Gets a value indicating whether this property is used as an index (or part of a composite index).
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns>
        ///     <c>true</c> if the property is used as an index, otherwise <c>false</c>.
        /// </returns>
        public static bool IsIndex([NotNull] this IProperty property)
            => Check.NotNull(property, nameof(property)).AsProperty().Indexes != null;

        /// <summary>
        ///     Gets a value indicating whether this property is used as the primary key (or part of a composite primary key).
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns>
        ///     <c>true</c> if the property is used as the primary key, otherwise <c>false</c>.
        /// </returns>
        public static bool IsPrimaryKey([NotNull] this IProperty property)
            => FindContainingPrimaryKey(property) != null;

        /// <summary>
        ///     Gets a value indicating whether this property is used as part of a primary or alternate key
        ///     (or part of a composite primary or alternate key).
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns>
        ///     <c>true</c> if the property is part of a key, otherwise <c>false</c>.
        /// </returns>
        public static bool IsKey([NotNull] this IProperty property)
            => Check.NotNull(property, nameof(property)).AsProperty().Keys != null;

        /// <summary>
        ///     Gets all foreign keys that use this property (including composite foreign keys in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get foreign keys for. </param>
        /// <returns>
        ///     The foreign keys that use this property.
        /// </returns>
        public static IEnumerable<IForeignKey> GetContainingForeignKeys([NotNull] this IProperty property)
            => Check.NotNull(property, nameof(property)).AsProperty().GetContainingForeignKeys();

        /// <summary>
        ///     Gets all indexes that use this property (including composite indexes in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get indexes for. </param>
        /// <returns>
        ///     The indexes that use this property.
        /// </returns>
        public static IEnumerable<IIndex> GetContainingIndexes([NotNull] this IProperty property)
            => Check.NotNull(property, nameof(property)).AsProperty().GetContainingIndexes();

        /// <summary>
        ///     Gets the primary key that uses this property (including a composite primary key in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get primary key for. </param>
        /// <returns>
        ///     The primary that use this property, or <c>null</c> if it is not part of the primary key.
        /// </returns>
        [Obsolete("Use FindContainingPrimaryKey()")]
        public static IKey GetContainingPrimaryKey([NotNull] this IProperty property)
            => property.FindContainingPrimaryKey();

        /// <summary>
        ///     Gets the primary key that uses this property (including a composite primary key in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get primary key for. </param>
        /// <returns>
        ///     The primary that use this property, or <c>null</c> if it is not part of the primary key.
        /// </returns>
        public static IKey FindContainingPrimaryKey([NotNull] this IProperty property)
            => Check.NotNull(property, nameof(property)).AsProperty().PrimaryKey;

        /// <summary>
        ///     Gets all primary or alternate keys that use this property (including composite keys in which this property
        ///     is included).
        /// </summary>
        /// <param name="property"> The property to get primary and alternate keys for. </param>
        /// <returns>
        ///     The primary and alternate keys that use this property.
        /// </returns>
        public static IEnumerable<IKey> GetContainingKeys([NotNull] this IProperty property)
            => Check.NotNull(property, nameof(property)).AsProperty().GetContainingKeys();

        /// <summary>
        ///     Gets the maximum length of data that is allowed in this property. For example, if the property is a <see cref="string" /> '
        ///     then this is the maximum number of characters.
        /// </summary>
        /// <param name="property"> The property to get the maximum length of. </param>
        /// <returns> The maximum length, or <c>null</c> if none if defined. </returns>
        public static int? GetMaxLength([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return (int?)property[CoreAnnotationNames.MaxLength];
        }

        /// <summary>
        ///     Gets a value indicating whether or not the property can persist Unicode characters.
        /// </summary>
        /// <param name="property"> The property to get the Unicode setting for. </param>
        /// <returns> The Unicode setting, or <c>null</c> if none if defined. </returns>
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
        /// <returns> The factory, or <c>null</c> if no factory has been set. </returns>
        public static Func<IProperty, IEntityType, ValueGenerator> GetValueGeneratorFactory([NotNull] this IProperty property)
            => (Func<IProperty, IEntityType, ValueGenerator>)
                Check.NotNull(property, nameof(property))[CoreAnnotationNames.ValueGeneratorFactory];

        /// <summary>
        ///     Gets the custom <see cref="ValueConverter" /> set for this property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The converter, or <c>null</c> if none has been set. </returns>
        public static ValueConverter GetValueConverter([NotNull] this IProperty property)
            => (ValueConverter)Check.NotNull(property, nameof(property))[CoreAnnotationNames.ValueConverter];

        /// <summary>
        ///     Gets the type that the property value will be converted to before being sent to the database provider.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The provider type, or <c>null</c> if none has been set. </returns>
        public static Type GetProviderClrType([NotNull] this IProperty property)
            => (Type)Check.NotNull(property, nameof(property))[CoreAnnotationNames.ProviderClrType];

        /// <summary>
        ///     Gets the <see cref="ValueComparer" /> for this property, or <c>null</c> if none is set.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The comparer, or <c>null</c> if none has been set. </returns>
        public static ValueComparer GetValueComparer([NotNull] this IProperty property)
            => (ValueComparer)Check.NotNull(property, nameof(property))[CoreAnnotationNames.ValueComparer];

        /// <summary>
        ///     Gets the <see cref="ValueComparer" /> to use with keys for this property, or <c>null</c> if none is set.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The comparer, or <c>null</c> if none has been set. </returns>
        public static ValueComparer GetKeyValueComparer([NotNull] this IProperty property)
            => (ValueComparer)Check.NotNull(property, nameof(property))[CoreAnnotationNames.KeyValueComparer];

        /// <summary>
        ///     Gets the <see cref="ValueComparer" /> to use for structural copies for this property, or <c>null</c> if none is set.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The comparer, or <c>null</c> if none has been set. </returns>
        public static ValueComparer GetStructuralValueComparer([NotNull] this IProperty property)
            => (ValueComparer)Check.NotNull(property, nameof(property))[CoreAnnotationNames.KeyValueComparer];

        /// <summary>
        ///     Creates a formatted string representation of the given properties such as is useful
        ///     when throwing exceptions about keys, indexes, etc. that use the properties.
        /// </summary>
        /// <param name="properties"> The properties to format. </param>
        /// <param name="includeTypes"> If <c>true</c>, then type names are included in the string. The default is <c>false</c>. </param>
        /// <returns> The string representation. </returns>
        public static string Format([NotNull] this IEnumerable<IPropertyBase> properties, bool includeTypes = false)
            => "{"
               + string.Join(
                   ", ",
                   properties.Select(
                       p => "'" + p.Name + "'" + (includeTypes ? " : " + p.ClrType.DisplayName(fullName: false) : "")))
               + "}";
    }
}
