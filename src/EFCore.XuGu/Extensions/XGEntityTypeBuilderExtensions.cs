// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     MySQL specific extension methods for <see cref="EntityTypeBuilder" />.
    /// </summary>
    public static class XGEntityTypeBuilderExtensions
    {
        #region CharSet and delegation

        /// <summary>
        /// Sets the MySQL character set on the table associated with this entity. When you only specify the character set, MySQL implicitly
        /// uses its default collation.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="charSet"> The name of the character set. </param>
        /// <param name="delegationModes">
        /// Finely controls where to recursively apply the character set and where not (including this entity/table).
        /// Implicitly uses <see cref="DelegationModes.ApplyToAll"/> if set to <see langword="null"/>.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder HasCharSet(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string charSet,
            DelegationModes? delegationModes = null)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(charSet, nameof(charSet));
            Check.NullOrEnumValue(delegationModes, nameof(delegationModes));

            entityTypeBuilder.Metadata.SetCharSet(charSet);
            entityTypeBuilder.Metadata.SetCharSetDelegation(delegationModes);

            return entityTypeBuilder;
        }

        /// <summary>
        /// Sets the MySQL character set on the table associated with this entity. When you only specify the character set, MySQL implicitly
        /// uses its default collation.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="charSet"> The name of the character set. </param>
        /// <param name="explicitlyDelegateToChildren">
        /// Properties/columns don't explicitly inherit the character set if set to <see langword="false"/>.
        /// They will explicitly inherit the character set if set to <see langword="true"/>.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder HasCharSet(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string charSet,
            bool explicitlyDelegateToChildren)
            => entityTypeBuilder.HasCharSet(
                charSet,
                explicitlyDelegateToChildren == false
                    ? DelegationModes.ApplyToTables
                    : DelegationModes.ApplyToAll);

        /// <summary>
        /// Sets the MySQL character set on the table associated with this entity. When you only specify the character set, MySQL implicitly
        /// uses its default collation.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="charSet"> The name of the character set. </param>
        /// <param name="delegationModes">
        /// Finely controls where to recursively apply the character set and where not (including this entity/table).
        /// Implicitly uses <see cref="DelegationModes.ApplyToAll"/> if set to <see langword="null"/>.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> HasCharSet<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string charSet,
            DelegationModes? delegationModes = null)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)HasCharSet((EntityTypeBuilder)entityTypeBuilder, charSet, delegationModes);

        /// <summary>
        /// Sets the MySQL character set on the table associated with this entity. When you only specify the character set, MySQL implicitly
        /// uses its default collation.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="charSet"> The name of the character set. </param>
        /// <param name="explicitlyDelegateToChildren">
        /// Properties/columns don't explicitly inherit the character set if set to <see langword="false"/>.
        /// They will explicitly inherit the character set if set to <see langword="true"/>.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> HasCharSet<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string charSet,
            bool explicitlyDelegateToChildren)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)HasCharSet((EntityTypeBuilder)entityTypeBuilder, charSet, explicitlyDelegateToChildren);

        /// <summary>
        /// Sets the MySQL character set on the table associated with this entity. When you only specify the character set, MySQL implicitly
        /// uses its default collation.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="charSet"> The name of the character set. </param>
        /// <param name="delegationModes">
        /// Finely controls where to recursively apply the character set and where not (including this entity/table).
        /// Implicitly uses <see cref="DelegationModes.ApplyToAll"/> if set to <see langword="null"/>.
        /// </param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IConventionEntityTypeBuilder HasCharSet(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string charSet,
            DelegationModes? delegationModes = null,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(charSet, nameof(charSet));
            Check.NullOrEnumValue(delegationModes, nameof(delegationModes));

            if (entityTypeBuilder.CanSetCharSet(charSet, fromDataAnnotation) &&
                entityTypeBuilder.CanSetCharSetDelegation(delegationModes, fromDataAnnotation))
            {
                entityTypeBuilder.Metadata.SetCharSet(charSet, fromDataAnnotation);
                entityTypeBuilder.Metadata.SetCharSetDelegation(delegationModes, fromDataAnnotation);

                return entityTypeBuilder;
            }

            return null;
        }

        /// <summary>
        /// Sets the MySQL character set on the table associated with this entity. When you only specify the character set, MySQL implicitly
        /// uses its default collation.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="charSet"> The name of the character set. </param>
        /// <param name="explicitlyDelegateToChildren">
        /// Properties/columns don't explicitly inherit the character set if set to <see langword="false"/>.
        /// They will explicitly inherit the character set if set to <see langword="true"/>.
        /// </param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IConventionEntityTypeBuilder HasCharSet(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string charSet,
            bool explicitlyDelegateToChildren,
            bool fromDataAnnotation = false)
            => entityTypeBuilder.HasCharSet(
                charSet,
                explicitlyDelegateToChildren == false
                    ? DelegationModes.ApplyToTables
                    : DelegationModes.ApplyToAll,
                fromDataAnnotation);

        /// <summary>
        /// Returns a value indicating whether the MySQL character set can be set on the table associated with this entity.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="charSet"> The name of the character set. </param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><see langword="true"/> if the mapped table can be configured with the collation.</returns>
        public static bool CanSetCharSet(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string charSet,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(charSet, nameof(charSet));

            return entityTypeBuilder.CanSetAnnotation(XGAnnotationNames.CharSet, charSet, fromDataAnnotation);
        }

        /// <summary>
        ///     Returns a value indicating whether the given character set delegation modes can be set.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="delegationModes"> The character set delegation modes. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given character set delegation modes can be set as default. </returns>
        public static bool CanSetCharSetDelegation(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] DelegationModes? delegationModes,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullOrEnumValue(delegationModes, nameof(delegationModes));

            return entityTypeBuilder.CanSetAnnotation(XGAnnotationNames.CharSetDelegation, delegationModes, fromDataAnnotation);
        }

        #endregion CharSet and delegation

        #region Collation and delegation

        /// <summary>
        /// Sets the MySQL collation on the table associated with this entity. When you only specify the collation, MySQL implicitly sets
        /// the proper character set as well.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="collation"> The name of the collation. </param>
        /// <param name="delegationModes">
        /// Finely controls where to recursively apply the character set and where not (including this entity/table).
        /// Implicitly uses <see cref="DelegationModes.ApplyToAll"/> if set to <see langword="null"/>.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder UseCollation(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string collation,
            DelegationModes? delegationModes = null)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(collation, nameof(collation));
            Check.NullOrEnumValue(delegationModes, nameof(delegationModes));

            entityTypeBuilder.Metadata.SetCollation(collation);
            entityTypeBuilder.Metadata.SetCollationDelegation(delegationModes);

            return entityTypeBuilder;
        }

        /// <summary>
        /// Sets the MySQL collation on the table associated with this entity. When you only specify the collation, MySQL implicitly sets
        /// the proper character set as well.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="collation"> The name of the collation. </param>
        /// <param name="explicitlyDelegateToChildren">
        /// Properties/columns don't explicitly inherit the collation if set to <see langword="false"/>.
        /// They will explicitly inherit the collation if set to <see langword="null"/> or <see langword="true"/>.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder UseCollation(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string collation,
            bool explicitlyDelegateToChildren)
            => entityTypeBuilder.UseCollation(
                collation,
                explicitlyDelegateToChildren == false
                    ? DelegationModes.ApplyToTables
                    : DelegationModes.ApplyToAll);

        /// <summary>
        /// Sets the MySQL collation on the table associated with this entity. When you only specify the collation, MySQL implicitly sets
        /// the proper character set as well.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="collation"> The name of the collation. </param>
        /// <param name="delegationModes">
        /// Finely controls where to recursively apply the character set and where not (including this entity/table).
        /// Implicitly uses <see cref="DelegationModes.ApplyToAll"/> if set to <see langword="null"/>.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> UseCollation<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string collation,
            DelegationModes? delegationModes = null)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)UseCollation((EntityTypeBuilder)entityTypeBuilder, collation, delegationModes);

        /// <summary>
        /// Sets the MySQL collation on the table associated with this entity. When you only specify the collation, MySQL implicitly sets
        /// the proper character set as well.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="collation"> The name of the collation. </param>
        /// <param name="explicitlyDelegateToChildren">
        /// Properties/columns don't explicitly inherit the collation if set to <see langword="false"/>.
        /// They will explicitly inherit the collation if set to <see langword="null"/> or <see langword="true"/>.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> UseCollation<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string collation,
            bool explicitlyDelegateToChildren)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)UseCollation((EntityTypeBuilder)entityTypeBuilder, collation, explicitlyDelegateToChildren);

        /// <summary>
        /// Sets the MySQL collation on the table associated with this entity. When you only specify the collation, MySQL implicitly sets
        /// the proper character set as well.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="collation"> The name of the collation. </param>
        /// <param name="delegationModes">
        /// Finely controls where to recursively apply the character set and where not (including this entity/table).
        /// Implicitly uses <see cref="DelegationModes.ApplyToAll"/> if set to <see langword="null"/>.
        /// </param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IConventionEntityTypeBuilder UseCollation(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string collation,
            DelegationModes? delegationModes = null,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(collation, nameof(collation));
            Check.NullOrEnumValue(delegationModes, nameof(delegationModes));

            if (entityTypeBuilder.CanSetCollation(collation, fromDataAnnotation) &&
                entityTypeBuilder.CanSetCollationDelegation(delegationModes, fromDataAnnotation))
            {
                entityTypeBuilder.Metadata.SetCollation(collation, fromDataAnnotation);
                entityTypeBuilder.Metadata.SetCollationDelegation(delegationModes, fromDataAnnotation);

                return entityTypeBuilder;
            }

            return null;
        }

        /// <summary>
        /// Sets the MySQL collation on the table associated with this entity. When you only specify the collation, MySQL implicitly sets
        /// the proper character set as well.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="collation"> The name of the collation. </param>
        /// <param name="explicitlyDelegateToChildren">
        /// Properties/columns don't explicitly inherit the collation if set to <see langword="false"/>.
        /// They will explicitly inherit the collation if set to <see langword="null"/> or <see langword="true"/>.
        /// </param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IConventionEntityTypeBuilder UseCollation(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string collation,
            bool explicitlyDelegateToChildren,
            bool fromDataAnnotation = false)
            => entityTypeBuilder.UseCollation(
                collation,
                explicitlyDelegateToChildren == false
                    ? DelegationModes.ApplyToTables
                    : DelegationModes.ApplyToAll,
                fromDataAnnotation);

        /// <summary>
        /// Returns a value indicating whether the MySQL collation can be set on the table associated with this entity.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="collation"> The name of the collation. </param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><see langword="true"/> if the mapped table can be configured with the collation.</returns>
        public static bool CanSetCollation(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string collation,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(collation, nameof(collation));

            return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.Collation, collation, fromDataAnnotation);
        }

        /// <summary>
        ///     Returns a value indicating whether the given collation delegation modes can be set.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="delegationModes"> The collation delegation modes. </param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns> <see langword="true" /> if the given collation delegation modes can be set as default. </returns>
        public static bool CanSetCollationDelegation(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            DelegationModes? delegationModes = null,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullOrEnumValue(delegationModes, nameof(delegationModes));

            return entityTypeBuilder.CanSetAnnotation(XGAnnotationNames.CollationDelegation, delegationModes, fromDataAnnotation);
        }

        #endregion Collation and delegation

        #region Table options

        /// <summary>
        /// Sets a table option for the table associated with this entity.
        /// Can be called more then once to set multiple table options.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table options. </param>
        /// <param name="value"> The value of the table options. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder HasTableOption(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string value)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            var options = entityTypeBuilder.Metadata.GetTableOptions();
            UpdateTableOption(name, value, options);
            entityTypeBuilder.Metadata.SetTableOptions(options);

            return entityTypeBuilder;
        }

        /// <summary>
        /// Sets a table option for the table associated with this entity.
        /// Can be called more then once to set multiple table options.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table options. </param>
        /// <param name="value"> The value of the table options. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> HasTableOption<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string value)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)HasTableOption((EntityTypeBuilder)entityTypeBuilder, name, value);

        /// <summary>
        /// Sets a table option for the table associated with this entity.
        /// Can be called more then once to set multiple table options.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table options. </param>
        /// <param name="value"> The value of the table options. </param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IConventionEntityTypeBuilder HasTableOption(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string value,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            if (entityTypeBuilder.CanSetTableOption(name, value, fromDataAnnotation))
            {
                var options = entityTypeBuilder.Metadata.GetTableOptions();
                UpdateTableOption(name, value, options);
                entityTypeBuilder.Metadata.SetTableOptions(options, fromDataAnnotation);

                return entityTypeBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the table options for the table associated with this entity can be set.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table options. </param>
        /// <param name="value"> The value of the table options. </param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><see langword="true"/> if the value can be set.</returns>
        public static bool CanSetTableOption(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string value,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            var options = entityTypeBuilder.Metadata.GetTableOptions();
            UpdateTableOption(name, value, options);
            var optionsString = XGEntityTypeExtensions.SerializeTableOptions(options);

            return entityTypeBuilder.CanSetAnnotation(XGAnnotationNames.StoreOptions, optionsString, fromDataAnnotation);
        }

        private static void UpdateTableOption(string key, string value, Dictionary<string, string> options)
        {
            if (key == null)
            {
                if (value != null)
                {
                    throw new ArgumentException(nameof(value));
                }

                options.Clear();
                return;
            }

            if (value == null)
            {
                options.Remove(key);
                return;
            }

            options[key] = value;
        }

        #endregion Table options
    }
}
