// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IProperty" /> for Cosmos metadata.
    /// </summary>
    public static class CosmosPropertyExtensions
    {
        /// <summary>
        ///     Returns the property name that the property is mapped to when targeting Cosmos.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> Returns the property name that the property is mapped to when targeting Cosmos. </returns>
        public static string GetJsonPropertyName([NotNull] this IProperty property)
            => (string)property[CosmosAnnotationNames.PropertyName]
                ?? GetDefaultJsonPropertyName(property);

        private static string GetDefaultJsonPropertyName(IProperty property)
        {
            var entityType = property.DeclaringEntityType;
            var ownership = entityType.FindOwnership();

            if (ownership != null
                && !entityType.IsDocumentRoot())
            {
                var pk = property.FindContainingPrimaryKey();
                if (pk != null
                    && (property.ClrType == typeof(int) || ownership.Properties.Contains(property))
                    && pk.Properties.Count == ownership.Properties.Count + (ownership.IsUnique ? 0 : 1)
                    && ownership.Properties.All(fkProperty => pk.Properties.Contains(fkProperty)))
                {
                    return "";
                }
            }

            return property.Name;
        }

        /// <summary>
        ///     Sets the property name that the property is mapped to when targeting Cosmos.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The name to set. </param>
        public static void SetJsonPropertyName([NotNull] this IMutableProperty property, [CanBeNull] string name)
            => property.SetOrRemoveAnnotation(
                CosmosAnnotationNames.PropertyName,
                name);

        /// <summary>
        ///     Sets the property name that the property is mapped to when targeting Cosmos.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetJsonPropertyName(
            [NotNull] this IConventionProperty property,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                CosmosAnnotationNames.PropertyName,
                name,
                fromDataAnnotation);

            return name;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> the property name that the property is mapped to when targeting Cosmos.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns>
        ///     The <see cref="ConfigurationSource" /> the property name that the property is mapped to when targeting Cosmos.
        /// </returns>
        public static ConfigurationSource? GetJsonPropertyNameConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(CosmosAnnotationNames.PropertyName)?.GetConfigurationSource();
    }
}
