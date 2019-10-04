// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a query type.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class QueryTypeBuilder : IInfrastructure<IMutableModel>, IInfrastructure<InternalEntityTypeBuilder>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryTypeBuilder([NotNull] InternalEntityTypeBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            Builder = builder;
        }

        private InternalEntityTypeBuilder Builder { [DebuggerStepThrough] get; }

        /// <summary>
        ///     The query type being configured.
        /// </summary>
        public virtual IMutableEntityType Metadata => Builder.Metadata;

        /// <summary>
        ///     The model that the query type belongs to.
        /// </summary>
        IMutableModel IInfrastructure<IMutableModel>.Instance => Builder.ModelBuilder.Metadata;

        /// <summary>
        ///     Gets the internal builder being used to configure the query type.
        /// </summary>
        InternalEntityTypeBuilder IInfrastructure<InternalEntityTypeBuilder>.Instance => Builder;

        /// <summary>
        ///     Adds or updates an annotation on the query type. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists its value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual QueryTypeBuilder HasAnnotation([NotNull] string annotation, [NotNull] object value)
        {
            Check.NotEmpty(annotation, nameof(annotation));
            Check.NotNull(value, nameof(value));

            Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Sets the base type of this query type in an inheritance hierarchy.
        /// </summary>
        /// <param name="name"> The name of the base type. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual QueryTypeBuilder HasBaseType([CanBeNull] string name)
            => new QueryTypeBuilder(Builder.HasBaseType(name, ConfigurationSource.Explicit));

        /// <summary>
        ///     Sets the base type of this query type in an inheritance hierarchy.
        /// </summary>
        /// <param name="queryType"> The base type. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual QueryTypeBuilder HasBaseType([CanBeNull] Type queryType)
            => new QueryTypeBuilder(Builder.HasBaseType(queryType, ConfigurationSource.Explicit));

        /// <summary>
        ///     <para>
        ///         Returns an object that can be used to configure a property of the query type.
        ///         If no property with the given name exists, then a new property will be added.
        ///     </para>
        ///     <para>
        ///         When adding a new property with this overload the property name must match the
        ///         name of a CLR property or field on the query type. This overload cannot be used to
        ///         add a new shadow state property.
        ///     </para>
        /// </summary>
        /// <param name="propertyName"> The name of the property to be configured. </param>
        /// <returns> An object that can be used to configure the property. </returns>
        public virtual PropertyBuilder Property([NotNull] string propertyName)
            => new PropertyBuilder(
                Builder.Property(
                    Check.NotEmpty(propertyName, nameof(propertyName)),
                    ConfigurationSource.Explicit));

        /// <summary>
        ///     <para>
        ///         Returns an object that can be used to configure a property of the query type.
        ///         If no property with the given name exists, then a new property will be added.
        ///     </para>
        ///     <para>
        ///         When adding a new property, if a property with the same name exists in the query type class
        ///         then it will be added to the model. If no property exists in the query type class, then
        ///         a new shadow state property will be added. A shadow state property is one that does not have a
        ///         corresponding property in the query type class. The current value for the property is stored in
        ///         the <see cref="ChangeTracker" /> rather than being stored in instances of the query type class.
        ///     </para>
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property to be configured. </typeparam>
        /// <param name="propertyName"> The name of the property to be configured. </param>
        /// <returns> An object that can be used to configure the property. </returns>
        public virtual PropertyBuilder<TProperty> Property<TProperty>([NotNull] string propertyName)
            => new PropertyBuilder<TProperty>(
                Builder.Property(
                    Check.NotEmpty(propertyName, nameof(propertyName)),
                    typeof(TProperty),
                    ConfigurationSource.Explicit));

        /// <summary>
        ///     <para>
        ///         Returns an object that can be used to configure a property of the query type.
        ///         If no property with the given name exists, then a new property will be added.
        ///     </para>
        ///     <para>
        ///         When adding a new property, if a property with the same name exists in the query type class
        ///         then it will be added to the model. If no property exists in the query type class, then
        ///         a new shadow state property will be added. A shadow state property is one that does not have a
        ///         corresponding property in the query type class. The current value for the property is stored in
        ///         the <see cref="ChangeTracker" /> rather than being stored in instances of the query type class.
        ///     </para>
        /// </summary>
        /// <param name="propertyType"> The type of the property to be configured. </param>
        /// <param name="propertyName"> The name of the property to be configured. </param>
        /// <returns> An object that can be used to configure the property. </returns>
        public virtual PropertyBuilder Property([NotNull] Type propertyType, [NotNull] string propertyName)
            => new PropertyBuilder(
                Builder.Property(
                    Check.NotEmpty(propertyName, nameof(propertyName)),
                    Check.NotNull(propertyType, nameof(propertyType)),
                    ConfigurationSource.Explicit));

        /// <summary>
        ///     Excludes the given property from the query type. This method is typically used to remove properties
        ///     from the query type that were added by convention.
        /// </summary>
        /// <param name="propertyName"> The name of then property to be removed from the query type. </param>
        public virtual QueryTypeBuilder Ignore([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            Builder.Ignore(propertyName, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Specifies a LINQ predicate expression that will automatically be applied to any queries targeting
        ///     this query type.
        /// </summary>
        /// <param name="filter">The LINQ predicate expression.</param>
        /// <returns> An object that can be used to configure the query type. </returns>
        public virtual QueryTypeBuilder HasQueryFilter([CanBeNull] LambdaExpression filter)
        {
            Builder.HasQueryFilter(filter);

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
        ///         <see cref="ReferenceNavigationBuilder.WithMany" />
        ///         or <see cref="ReferenceNavigationBuilder.WithOne" /> to fully configure
        ///         the relationship. Calling just this method without the chained call will not
        ///         produce a valid relationship.
        ///     </para>
        /// </summary>
        /// <param name="relatedTypeName"> The name of the query type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this query type that represents the relationship. If
        ///     no property is specified, the relationship will be configured without a navigation property on this
        ///     end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual ReferenceNavigationBuilder HasOne(
            [NotNull] string relatedTypeName,
            [CanBeNull] string navigationName = null)
        {
            Check.NotEmpty(relatedTypeName, nameof(relatedTypeName));
            Check.NullButNotEmpty(navigationName, nameof(navigationName));

            var relatedEntityType = Builder.ModelBuilder.Entity(relatedTypeName, ConfigurationSource.Explicit).Metadata;

            return new ReferenceNavigationBuilder(
                Builder.Metadata,
                relatedEntityType,
                navigationName,
                Builder.Navigation(
                    relatedEntityType.Builder, navigationName, ConfigurationSource.Explicit,
                    Builder.Metadata == relatedEntityType));
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this query type has a reference that points
        ///         to a single instance of the other type in the relationship.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="ReferenceNavigationBuilder.WithMany" />
        ///         or <see cref="ReferenceNavigationBuilder.WithOne" /> to fully configure
        ///         the relationship. Calling just this method without the chained call will not
        ///         produce a valid relationship.
        ///     </para>
        /// </summary>
        /// <param name="relatedType"> The query type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this query type that represents the relationship. If
        ///     no property is specified, the relationship will be configured without a navigation property on this
        ///     end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual ReferenceNavigationBuilder HasOne(
            [NotNull] Type relatedType,
            [CanBeNull] string navigationName = null)
        {
            Check.NotNull(relatedType, nameof(relatedType));
            Check.NullButNotEmpty(navigationName, nameof(navigationName));

            var relatedEntityType = Builder.ModelBuilder.Entity(relatedType, ConfigurationSource.Explicit).Metadata;

            return new ReferenceNavigationBuilder(
                Builder.Metadata,
                relatedEntityType,
                navigationName,
                Builder.Navigation(
                    relatedEntityType.Builder, navigationName, ConfigurationSource.Explicit,
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
        public virtual QueryTypeBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
        {
            Builder.UsePropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);

            return this;
        }

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
