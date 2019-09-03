// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Cosmos-specific extension methods for <see cref="PropertyBuilder" />.
    /// </summary>
    public static class CosmosPropertyBuilderExtensions
    {
        /// <summary>
        ///     Configures the property name that the property is mapped to when targeting Azure Cosmos.
        /// </summary>
        /// <remarks> If an empty string is supplied then the property will not be persisted. </remarks>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the container. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ToJsonProperty(
            [NotNull] this PropertyBuilder propertyBuilder,
            [NotNull] string name)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(name, nameof(name));

            propertyBuilder.Metadata.SetPropertyName(name);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property name that the property is mapped to when targeting Azure Cosmos.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the container. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ToJsonProperty<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [NotNull] string name)
            => (PropertyBuilder<TProperty>)ToJsonProperty((PropertyBuilder)propertyBuilder, name);

        /// <summary>
        ///     <para>
        ///         Configures the property name that the property is mapped to when targeting Azure Cosmos.
        ///     </para>
        ///     <para>
        ///         If an empty string is supplied then the property will not be persisted.
        ///     </para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the container. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder ToJsonProperty(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.CanSetJsonProperty(name, fromDataAnnotation))
            {
                return null;
            }

            propertyBuilder.Metadata.SetPropertyName(name, fromDataAnnotation);

            return propertyBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the given property name can be set.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the container. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the property name can be set. </returns>
        public static bool CanSetJsonProperty(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
            => propertyBuilder.CanSetAnnotation(CosmosAnnotationNames.PropertyName, name, fromDataAnnotation);
    }
}
