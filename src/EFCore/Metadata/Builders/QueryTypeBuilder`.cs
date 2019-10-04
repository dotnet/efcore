// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring an <see cref="EntityType" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="TQuery"> The query type being configured. </typeparam>
    public class QueryTypeBuilder<TQuery> : QueryTypeBuilder
        where TQuery : class
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryTypeBuilder([NotNull] InternalEntityTypeBuilder builder)
            : base(builder)
        {
        }

        private InternalEntityTypeBuilder Builder => this.GetInfrastructure<InternalEntityTypeBuilder>();

        /// <summary>
        ///     Adds or updates an annotation on the query type. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists its value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same typeBuilder instance so that multiple configuration calls can be chained. </returns>
        public new virtual QueryTypeBuilder<TQuery> HasAnnotation([NotNull] string annotation, [NotNull] object value) =>
            (QueryTypeBuilder<TQuery>)base.HasAnnotation(annotation, value);

        /// <summary>
        ///     Sets the base type of this query type in an inheritance hierarchy.
        /// </summary>
        /// <param name="name"> The name of the base type. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual QueryTypeBuilder<TQuery> HasBaseType([CanBeNull] string name) =>
            new QueryTypeBuilder<TQuery>(Builder.HasBaseType(name, ConfigurationSource.Explicit));

        /// <summary>
        ///     Sets the base type of this query type in an inheritance hierarchy.
        /// </summary>
        /// <param name="queryType"> The base type. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual QueryTypeBuilder<TQuery> HasBaseType([CanBeNull] Type queryType) =>
            new QueryTypeBuilder<TQuery>(Builder.HasBaseType(queryType, ConfigurationSource.Explicit));

        /// <summary>
        ///     Sets the base type of this query type in an inheritance hierarchy.
        /// </summary>
        /// <typeparam name="TBaseType"> The base type. </typeparam>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual QueryTypeBuilder<TQuery> HasBaseType<TBaseType>() => HasBaseType(typeof(TBaseType));

        /// <summary>
        ///     Returns an object that can be used to configure a property of the query type.
        ///     If the specified property is not already part of the model, it will be added.
        /// </summary>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to be configured (
        ///     <c>blog => blog.Url</c>).
        /// </param>
        /// <returns> An object that can be used to configure the property. </returns>
        public virtual PropertyBuilder<TProperty> Property<TProperty>(
            [NotNull] Expression<Func<TQuery, TProperty>> propertyExpression) => new PropertyBuilder<TProperty>(
                Builder.Property(
                    Check.NotNull(propertyExpression, nameof(propertyExpression)).GetPropertyAccess(),
                    ConfigurationSource.Explicit));

        /// <summary>
        ///     Excludes the given property from the query type. This method is typically used to remove properties
        ///     from the query type that were added by convention.
        /// </summary>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to be ignored
        ///     (<c>blog => blog.Url</c>).
        /// </param>
        public virtual QueryTypeBuilder<TQuery> Ignore([NotNull] Expression<Func<TQuery, object>> propertyExpression)
            => (QueryTypeBuilder<TQuery>)base.Ignore(
                Check.NotNull(propertyExpression, nameof(propertyExpression)).GetPropertyAccess().GetSimpleMemberName());

        /// <summary>
        ///     Excludes the given property from the query type. This method is typically used to remove properties
        ///     from the query type that were added by convention.
        /// </summary>
        /// <param name="propertyName"> The name of then property to be removed from the query type. </param>
        public new virtual QueryTypeBuilder<TQuery> Ignore([NotNull] string propertyName)
            => (QueryTypeBuilder<TQuery>)base.Ignore(propertyName);

        /// <summary>
        ///     Specifies a LINQ predicate expression that will automatically be applied to any queries targeting
        ///     this query type.
        /// </summary>
        /// <param name="filter">The LINQ predicate expression.</param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual QueryTypeBuilder<TQuery> HasQueryFilter([CanBeNull] Expression<Func<TQuery, bool>> filter)
            => (QueryTypeBuilder<TQuery>)base.HasQueryFilter(filter);

        /// <summary>
        ///     Configures a query used to provide data for a query type.
        /// </summary>
        /// <param name="query"> The query that will provider the underlying data for the query type. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual QueryTypeBuilder<TQuery> ToQuery([NotNull] Expression<Func<IQueryable<TQuery>>> query)
        {
            Check.NotNull(query, nameof(query));

            Builder.HasDefiningQuery(query);

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this query type has a reference that points
        ///         to a single instance of the other type in the relationship.
        ///     </para>
        ///     <para>
        ///         Note that calling this method with no parameters will explicitly configure this side
        ///         of the relationship to use no navigation property, even if such a property exists on the
        ///         entity type. If the navigation property is to be used, then it must be specified.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see
        ///             cref="ReferenceNavigationBuilder{TQuery,TRelatedEntity}.WithMany(Expression{Func{TRelatedEntity,IEnumerable{TQuery}}})" />
        ///         or
        ///         <see
        ///             cref="ReferenceNavigationBuilder{TQuery,TRelatedEntity}.WithOne(Expression{Func{TRelatedEntity,TQuery}})" />
        ///         to fully configure the relationship. Calling just this method without the chained call will not
        ///         produce a valid relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TRelatedEntity"> The query type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this query type that represents
        ///     the relationship (<c>post => post.Blog</c>). If no property is specified, the relationship will be
        ///     configured without a navigation property on this end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual ReferenceNavigationBuilder<TQuery, TRelatedEntity> HasOne<TRelatedEntity>(
            [CanBeNull] Expression<Func<TQuery, TRelatedEntity>> navigationExpression = null)
            where TRelatedEntity : class
        {
            var relatedEntityType = Builder.Metadata.FindInDefinitionPath(typeof(TRelatedEntity)) ??
                                    Builder.ModelBuilder.Entity(typeof(TRelatedEntity), ConfigurationSource.Explicit)
                                        .Metadata;
            var navigation = navigationExpression?.GetPropertyAccess();

            return new ReferenceNavigationBuilder<TQuery, TRelatedEntity>(
                Builder.Metadata,
                relatedEntityType,
                navigation,
                Builder.Navigation(
                    relatedEntityType.Builder, navigation, ConfigurationSource.Explicit,
                    Builder.Metadata == relatedEntityType));
        }

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for all properties of this query type.
        ///     </para>
        ///     <para>
        ///         By default, the backing field, if one is found by convention or has been specified, is used when
        ///         new objects are constructed, typically when entities are queried from the database.
        ///         Properties are used for all other accesses.  Calling this method will change that behavior
        ///         for all properties of this query type as described in the <see cref="PropertyAccessMode" /> enum.
        ///     </para>
        ///     <para>
        ///         Calling this method overrides for all properties of this query type any access mode that was
        ///         set on the model.
        ///     </para>
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" /> to use for properties of this query type. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual QueryTypeBuilder<TQuery> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode) =>
            (QueryTypeBuilder<TQuery>)base.UsePropertyAccessMode(propertyAccessMode);
    }
}
