// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for <see cref="EntityTypeBuilder" />.
    /// </summary>
    public static class RelationalEntityTypeBuilderExtensions
    {
        private static readonly string DefaultDiscriminatorName = "Discriminator";

        // ReSharper disable once InconsistentNaming
        private static readonly Type DefaultDiscriminatorType = typeof(string);

        /// <summary>
        ///     Configures the view or table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToTable(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            entityTypeBuilder.Metadata.SetTableName(name);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the view or table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToTable((EntityTypeBuilder)entityTypeBuilder, name);

        /// <summary>
        ///     Configures the view or table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToTable(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            entityTypeBuilder.Metadata.SetTableName(name);
            entityTypeBuilder.Metadata.SetSchema(schema);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the view or table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToTable((EntityTypeBuilder)entityTypeBuilder, name, schema);

        /// <summary>
        ///     Configures the view or table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder ToTable(
            [NotNull] this OwnedNavigationBuilder referenceOwnershipBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(referenceOwnershipBuilder, nameof(referenceOwnershipBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            referenceOwnershipBuilder.OwnedEntityType.SetTableName(name);

            return referenceOwnershipBuilder;
        }

        /// <summary>
        ///     Configures the view or table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder<TEntity, TRelatedEntity> ToTable<TEntity, TRelatedEntity>(
            [NotNull] this OwnedNavigationBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TRelatedEntity : class
            => (OwnedNavigationBuilder<TEntity, TRelatedEntity>)ToTable((OwnedNavigationBuilder)referenceOwnershipBuilder, name);

        /// <summary>
        ///     Configures the view or table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder ToTable(
            [NotNull] this OwnedNavigationBuilder referenceOwnershipBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(referenceOwnershipBuilder, nameof(referenceOwnershipBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            referenceOwnershipBuilder.OwnedEntityType.SetTableName(name);
            referenceOwnershipBuilder.OwnedEntityType.SetSchema(schema);

            return referenceOwnershipBuilder;
        }

        /// <summary>
        ///     Configures the view or table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder<TEntity, TRelatedEntity> ToTable<TEntity, TRelatedEntity>(
            [NotNull] this OwnedNavigationBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
            where TEntity : class
            where TRelatedEntity : class
            => (OwnedNavigationBuilder<TEntity, TRelatedEntity>)ToTable((OwnedNavigationBuilder)referenceOwnershipBuilder, name, schema);

        /// <summary>
        ///     Configures the view or table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder ToTable(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, [CanBeNull] string name, bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetTable(name, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetTableName(name, fromDataAnnotation);
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the view or table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder ToTable(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name, [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetTable(name, fromDataAnnotation)
                || !entityTypeBuilder.CanSetSchema(schema, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetTableName(name, fromDataAnnotation);
            entityTypeBuilder.Metadata.SetSchema(schema, fromDataAnnotation);
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the view or table name can be set for this entity type
        ///     from the current configuration source
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the configuration can be applied. </returns>
        public static bool CanSetTable(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, [CanBeNull] string name, bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.TableName, name, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the schema of the view or table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder ToSchema(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetSchema(schema, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetSchema(schema, fromDataAnnotation);
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the schema of the view or table name can be set for this entity type
        ///     from the current configuration source
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the configuration can be applied. </returns>
        public static bool CanSetSchema(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(schema, nameof(schema));

            return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.Schema, schema, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the view that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToView(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            entityTypeBuilder.Metadata.SetViewName(name);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the view that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToView<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToView((EntityTypeBuilder)entityTypeBuilder, name);

        /// <summary>
        ///     Configures the view that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the query type being configured. </param>
        /// <param name="name"> The name of the view. </param>
        /// <param name="schema"> The schema of the view. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToView(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            entityTypeBuilder.Metadata.SetViewName(name);
            // TODO:
            //entityTypeBuilder.Metadata.SetViewSchema(schema);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the view that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The query type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the query type being configured. </param>
        /// <param name="name"> The name of the view. </param>
        /// <param name="schema"> The schema of the view. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToView<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToView((EntityTypeBuilder)entityTypeBuilder, name, schema);

        /// <summary>
        ///     Configures the discriminator column used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <returns> A builder that allows the discriminator column to be configured. </returns>
        public static DiscriminatorBuilder HasDiscriminator([NotNull] this EntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            var propertyBuilder = new PropertyBuilder(
                (IMutableProperty)GetOrCreateDiscriminatorProperty(
                    ((IConventionEntityType)entityTypeBuilder.Metadata).Builder, null, null, true).Metadata);
            return DiscriminatorBuilder(entityTypeBuilder.Metadata, propertyBuilder);
        }

        /// <summary>
        ///     Configures the discriminator column used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the discriminator column. </param>
        /// <param name="type"> The type of values stored in the discriminator column. </param>
        /// <returns> A builder that allows the discriminator column to be configured. </returns>
        public static DiscriminatorBuilder HasDiscriminator(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [NotNull] string name,
            [NotNull] Type type)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(type, nameof(type));

            return DiscriminatorBuilder(
                entityTypeBuilder.Metadata, entityTypeBuilder.Property(type, name));
        }

        /// <summary>
        ///     Configures the discriminator column used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <typeparam name="TDiscriminator"> The type of values stored in the discriminator column. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the discriminator column. </param>
        /// <returns> A builder that allows the discriminator column to be configured. </returns>
        public static DiscriminatorBuilder<TDiscriminator> HasDiscriminator<TDiscriminator>(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [NotNull] string name)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotEmpty(name, nameof(name));

            return new DiscriminatorBuilder<TDiscriminator>(
                DiscriminatorBuilder(
                    entityTypeBuilder.Metadata, entityTypeBuilder.Property(typeof(TDiscriminator), name)));
        }

        /// <summary>
        ///     Configures the discriminator column used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TDiscriminator"> The type of values stored in the discriminator column. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to be used as the discriminator (
        ///     <c>blog => blog.Discriminator</c>).
        /// </param>
        /// <returns> A builder that allows the discriminator column to be configured. </returns>
        public static DiscriminatorBuilder<TDiscriminator> HasDiscriminator<TEntity, TDiscriminator>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [NotNull] Expression<Func<TEntity, TDiscriminator>> propertyExpression)
            where TEntity : class
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(propertyExpression, nameof(propertyExpression));

            return new DiscriminatorBuilder<TDiscriminator>(
                DiscriminatorBuilder(entityTypeBuilder.Metadata, entityTypeBuilder.Property(propertyExpression)));
        }

        /// <summary>
        ///     Configures the discriminator column used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> A builder that allows the discriminator column to be configured. </returns>
        public static IConventionDiscriminatorBuilder HasDiscriminator(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, bool fromDataAnnotation = false)
            => DiscriminatorBuilder(
                entityTypeBuilder, GetOrCreateDiscriminatorProperty(entityTypeBuilder, type: null, name: null, fromDataAnnotation: false),
                fromDataAnnotation);

        /// <summary>
        ///     Configures the discriminator column used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="type"> The type of values stored in the discriminator column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the discriminator was configured,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionDiscriminatorBuilder HasDiscriminator(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, [NotNull] Type type, bool fromDataAnnotation = false)
            => CanSetDiscriminator(entityTypeBuilder, type, fromDataAnnotation)
                ? DiscriminatorBuilder(
                    entityTypeBuilder, GetOrCreateDiscriminatorProperty(entityTypeBuilder, type, name: null, fromDataAnnotation),
                    fromDataAnnotation)
                : null;

        /// <summary>
        ///     Configures the discriminator column used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the discriminator column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the discriminator was configured,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionDiscriminatorBuilder HasDiscriminator(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, [NotNull] string name, bool fromDataAnnotation = false)
            => CanSetDiscriminator(entityTypeBuilder, name, fromDataAnnotation)
                ? DiscriminatorBuilder(
                    entityTypeBuilder, GetOrCreateDiscriminatorProperty(entityTypeBuilder, type: null, name, fromDataAnnotation),
                    fromDataAnnotation)
                : null;

        /// <summary>
        ///     Configures the discriminator column used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the discriminator column. </param>
        /// <param name="type"> The type of values stored in the discriminator column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the discriminator was configured,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionDiscriminatorBuilder HasDiscriminator(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, [NotNull] string name, [NotNull] Type type,
            bool fromDataAnnotation = false)
            => CanSetDiscriminator(entityTypeBuilder, type, name, fromDataAnnotation)
                ? DiscriminatorBuilder(
                    entityTypeBuilder, entityTypeBuilder.Metadata.RootType().Builder.Property(type, name, fromDataAnnotation),
                    fromDataAnnotation)
                : null;

        /// <summary>
        ///     Configures the discriminator column used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="memberInfo"> The property mapped to the discriminator column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the discriminator was configured,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionDiscriminatorBuilder HasDiscriminator(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, [NotNull] MemberInfo memberInfo, bool fromDataAnnotation = false)
            => CanSetDiscriminator(entityTypeBuilder, memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName(), fromDataAnnotation)
                ? DiscriminatorBuilder(
                    entityTypeBuilder, entityTypeBuilder.Metadata.RootType().Builder.Property(memberInfo, fromDataAnnotation),
                    fromDataAnnotation)
                : null;

        /// <summary>
        ///     Removes the discriminator property from this entity type.
        ///     This method is usually called when the entity type is no longer mapped to the same table as any other type in
        ///     the hierarchy or when this entity type is no longer the root type.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the discriminator was configured,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder HasNoDeclaredDiscriminator(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, bool fromDataAnnotation = false)
        {
            var discriminatorName = (string)entityTypeBuilder.Metadata[RelationalAnnotationNames.DiscriminatorProperty];
            if (discriminatorName == null)
            {
                return entityTypeBuilder;
            }

            var discriminatorProperty = entityTypeBuilder.Metadata.FindProperty(discriminatorName);
            if (discriminatorProperty != null)
            {
                if (!CanSetDiscriminator(entityTypeBuilder, discriminatorProperty, null, null, fromDataAnnotation))
                {
                    return null;
                }

                discriminatorProperty.DeclaringEntityType.Builder.RemoveUnusedShadowProperties(
                    new[]
                    {
                        discriminatorProperty
                    });
            }

            entityTypeBuilder.Metadata.SetDiscriminatorProperty(null, fromDataAnnotation);
            return entityTypeBuilder;
        }

        private static IConventionPropertyBuilder GetOrCreateDiscriminatorProperty(
            IConventionEntityTypeBuilder entityTypeBuilder, Type type, string name, bool fromDataAnnotation)
        {
            var discriminatorProperty = entityTypeBuilder.Metadata.GetDiscriminatorProperty();
            if ((name != null && discriminatorProperty?.Name != name)
                || (type != null && discriminatorProperty?.ClrType != type))
            {
                discriminatorProperty = null;
            }

            return entityTypeBuilder.Metadata.RootType().Builder.Property(
                type ?? discriminatorProperty?.ClrType ?? DefaultDiscriminatorType,
                name ?? discriminatorProperty?.Name ?? DefaultDiscriminatorName,
                setTypeConfigurationSource: type != null,
                fromDataAnnotation);
        }

        private static DiscriminatorBuilder DiscriminatorBuilder(
            IMutableEntityType entityType,
            [NotNull] PropertyBuilder discriminatorPropertyBuilder)
        {
            var rootTypeBuilder = new EntityTypeBuilder(entityType.RootType());

            var discriminatorProperty = (IConventionProperty)discriminatorPropertyBuilder.Metadata;
            // Make sure the property is on the root type
            discriminatorPropertyBuilder = discriminatorProperty.GetTypeConfigurationSource() != null
                ? rootTypeBuilder.Property(discriminatorProperty.ClrType, discriminatorProperty.Name)
                : rootTypeBuilder.Property(discriminatorProperty.Name);

            var oldDiscriminatorProperty = entityType.GetDiscriminatorProperty() as IConventionProperty;
            if (oldDiscriminatorProperty?.Builder != null
                && oldDiscriminatorProperty != discriminatorProperty)
            {
                oldDiscriminatorProperty.DeclaringEntityType.Builder.RemoveUnusedShadowProperties(
                    new[]
                    {
                        oldDiscriminatorProperty
                    });
            }

            rootTypeBuilder.Metadata.SetDiscriminatorProperty(discriminatorProperty);
            discriminatorPropertyBuilder.IsRequired();
            discriminatorPropertyBuilder.HasValueGenerator(DiscriminatorValueGenerator.Factory);

            return new DiscriminatorBuilder(entityType);
        }

        private static IConventionDiscriminatorBuilder DiscriminatorBuilder(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionPropertyBuilder discriminatorPropertyBuilder,
            bool fromDataAnnotation)
        {
            if (discriminatorPropertyBuilder == null)
            {
                return null;
            }

            var rootTypeBuilder = entityTypeBuilder.Metadata.RootType().Builder;
            var discriminatorProperty = discriminatorPropertyBuilder.Metadata;
            // Make sure the property is on the root type
            discriminatorPropertyBuilder = rootTypeBuilder.Property(
                discriminatorProperty.ClrType, discriminatorProperty.Name, setTypeConfigurationSource: false);

            var oldDiscriminatorProperty = entityTypeBuilder.Metadata.GetDiscriminatorProperty() as IConventionProperty;
            if (oldDiscriminatorProperty?.Builder != null
                && oldDiscriminatorProperty != discriminatorProperty)
            {
                oldDiscriminatorProperty.Builder.IsRequired(null, fromDataAnnotation);
                oldDiscriminatorProperty.Builder.HasValueGenerator((Type)null, fromDataAnnotation);
                oldDiscriminatorProperty.DeclaringEntityType.Builder.RemoveUnusedShadowProperties(
                    new[]
                    {
                        oldDiscriminatorProperty
                    });
            }

            rootTypeBuilder.Metadata.SetDiscriminatorProperty(discriminatorProperty, fromDataAnnotation);
            discriminatorPropertyBuilder.IsRequired(true, fromDataAnnotation);
            discriminatorPropertyBuilder.HasValueGenerator(DiscriminatorValueGenerator.Factory, fromDataAnnotation);

            return new DiscriminatorBuilder((IMutableEntityType)entityTypeBuilder.Metadata);
        }

        /// <summary>
        ///     Returns a value indicating whether the discriminator column can be configured.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the discriminator column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the configuration can be applied. </returns>
        public static bool CanSetDiscriminator(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, [CanBeNull] string name, bool fromDataAnnotation = false)
            => CanSetDiscriminator(
                entityTypeBuilder, entityTypeBuilder.Metadata.GetDiscriminatorProperty(), name, discriminatorType: null,
                fromDataAnnotation);

        /// <summary>
        ///     Returns a value indicating whether the discriminator column can be configured.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="type"> The type of values stored in the discriminator column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the configuration can be applied. </returns>
        public static bool CanSetDiscriminator(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, [CanBeNull] Type type, bool fromDataAnnotation = false)
            => CanSetDiscriminator(
                entityTypeBuilder, entityTypeBuilder.Metadata.GetDiscriminatorProperty(), name: null, type, fromDataAnnotation);

        /// <summary>
        ///     Returns a value indicating whether the discriminator column can be configured.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="type"> The type of values stored in the discriminator column. </param>
        /// <param name="name"> The name of the discriminator column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the configuration can be applied. </returns>
        public static bool CanSetDiscriminator(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] Type type,
            [NotNull] string name,
            bool fromDataAnnotation = false)
            => CanSetDiscriminator(
                entityTypeBuilder, entityTypeBuilder.Metadata.GetDiscriminatorProperty(), name, type, fromDataAnnotation);

        private static bool CanSetDiscriminator(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IProperty discriminatorProperty,
            string name,
            Type discriminatorType,
            bool fromDataAnnotation)
            => discriminatorProperty == null
               || ((name != null || discriminatorType != null)
                   && (name == null || discriminatorProperty.Name == name)
                   && (discriminatorType == null || discriminatorProperty.ClrType == discriminatorType))
               || (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
               .Overrides(entityTypeBuilder.Metadata.GetDiscriminatorPropertyConfigurationSource());

        /// <summary>
        ///     Configures a database check constraint when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The entity type builder. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <returns> A builder to further configure the entity type. </returns>
        public static EntityTypeBuilder HasCheckConstraint(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [NotNull] string name,
            [CanBeNull] string sql)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(sql, nameof(sql));

            var entityType = entityTypeBuilder.Metadata;

            var tableName = entityType.GetTableName();
            var schema = entityType.GetSchema();

            var constraint = entityType.Model.FindCheckConstraint(name, tableName, schema);
            if (constraint != null)
            {
                if (constraint.Sql == sql)
                {
                    ((CheckConstraint)constraint).UpdateConfigurationSource(ConfigurationSource.Explicit);
                    return entityTypeBuilder;
                }

                entityType.Model.RemoveCheckConstraint(name, tableName, schema);
            }

            if (sql != null)
            {
                entityType.Model.AddCheckConstraint(sql, name, tableName, schema);
            }

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures a database check constraint when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The entity type builder. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <returns> A builder to further configure the entity type. </returns>
        public static EntityTypeBuilder<TEntity> HasCheckConstraint<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [NotNull] string name,
            [CanBeNull] string sql)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)HasCheckConstraint((EntityTypeBuilder)entityTypeBuilder, name, sql);

        /// <summary>
        ///     Configures a database check constraint when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The entity type builder. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the check constraint was configured,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder HasCheckConstraint(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] string name,
            [CanBeNull] string sql,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(sql, nameof(sql));

            var entityType = entityTypeBuilder.Metadata;

            var tableName = entityType.GetTableName();
            var schema = entityType.GetSchema();

            var constraint = entityType.Model.FindCheckConstraint(name, tableName, schema);
            if (constraint != null)
            {
                if (constraint.Sql == sql)
                {
                    ((CheckConstraint)constraint).UpdateConfigurationSource(
                        fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
                    return entityTypeBuilder;
                }

                if (!(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                    .Overrides(entityTypeBuilder.Metadata.GetDiscriminatorPropertyConfigurationSource()))
                {
                    return null;
                }

                entityType.Model.RemoveCheckConstraint(name, tableName, schema);
            }

            if (sql != null)
            {
                entityType.Model.AddCheckConstraint(sql, name, tableName, schema, fromDataAnnotation);
            }

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the discriminator column can be configured.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the configuration can be applied. </returns>
        public static bool CanSetCheckConstraint(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] string name,
            [CanBeNull] string sql,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(sql, nameof(sql));

            var entityType = entityTypeBuilder.Metadata;
            var tableName = entityType.GetTableName();
            var schema = entityType.GetSchema();

            var constraint = entityType.Model.FindCheckConstraint(name, tableName, schema);

            return constraint == null
                   || constraint.Sql == sql
                   || (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                   .Overrides(entityTypeBuilder.Metadata.GetDiscriminatorPropertyConfigurationSource());
        }
    }
}
