// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IConventionModel" />.
    /// </summary>
    public static class ConventionModelExtensions
    {
        /// <summary>
        ///     Gets the entity that maps the given entity class. Returns <see langword="null" /> if no entity type with the given name is found.
        /// </summary>
        /// <param name="model"> The model to find the entity type in. </param>
        /// <param name="type"> The type to find the corresponding entity type for. </param>
        /// <returns> The entity type, or <see langword="null" /> if none if found. </returns>
        public static IConventionEntityType FindEntityType([NotNull] this IConventionModel model, [NotNull] Type type)
            => ((Model)model).FindEntityType(type);

        /// <summary>
        ///     Gets the entity type for the given name, defining navigation name
        ///     and the defining entity type. Returns <see langword="null" /> if no matching entity type is found.
        /// </summary>
        /// <param name="model"> The model to find the entity type in. </param>
        /// <param name="type"> The type of the entity type to find. </param>
        /// <param name="definingNavigationName"> The defining navigation of the entity type to find. </param>
        /// <param name="definingEntityType"> The defining entity type of the entity type to find. </param>
        /// <returns> The entity type, or <see langword="null" /> if none are found. </returns>
        public static IConventionEntityType FindEntityType(
            [NotNull] this IConventionModel model,
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] IConventionEntityType definingEntityType)
            => (IConventionEntityType)((IModel)model).FindEntityType(type, definingNavigationName, definingEntityType);

        /// <summary>
        ///     Gets the entity types matching the given type.
        /// </summary>
        /// <param name="model"> The model to find the entity type in. </param>
        /// <param name="type"> The type of the entity type to find. </param>
        /// <returns> The entity types found. </returns>
        [DebuggerStepThrough]
        public static IReadOnlyCollection<IConventionEntityType> GetEntityTypes([NotNull] this IConventionModel model, [NotNull] Type type)
            => ((Model)model).GetEntityTypes(type);

        /// <summary>
        ///     Gets the entity types matching the given name.
        /// </summary>
        /// <param name="model"> The model to find the entity type in. </param>
        /// <param name="name"> The name of the entity type to find. </param>
        /// <returns> The entity types found. </returns>
        [DebuggerStepThrough]
        public static IReadOnlyCollection<IConventionEntityType> GetEntityTypes(
            [NotNull] this IConventionModel model,
            [NotNull] string name)
            => ((Model)model).GetEntityTypes(name);

        /// <summary>
        ///     Removes an entity type without a defining navigation from the model.
        /// </summary>
        /// <param name="model"> The model to remove the entity type from. </param>
        /// <param name="name"> The name of the entity type to be removed. </param>
        /// <returns> The entity type that was removed. </returns>
        public static IConventionEntityType RemoveEntityType(
            [NotNull] this IConventionModel model,
            [NotNull] string name)
        {
            Check.NotNull(model, nameof(model));
            Check.NotEmpty(name, nameof(name));

            return ((Model)model).RemoveEntityType(name);
        }

        /// <summary>
        ///     Removes an entity type with a defining navigation from the model.
        /// </summary>
        /// <param name="model"> The model to remove the entity type from. </param>
        /// <param name="name"> The name of the entity type to be removed. </param>
        /// <param name="definingNavigationName"> The defining navigation. </param>
        /// <param name="definingEntityType"> The defining entity type. </param>
        /// <returns> The entity type that was removed. </returns>
        public static IConventionEntityType RemoveEntityType(
            [NotNull] this IConventionModel model,
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] IConventionEntityType definingEntityType)
        {
            Check.NotNull(model, nameof(model));
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(definingNavigationName, nameof(definingNavigationName));
            Check.NotNull(definingEntityType, nameof(definingEntityType));

            return ((Model)model).RemoveEntityType(name);
        }

        /// <summary>
        ///     Removes an entity type from the model.
        /// </summary>
        /// <param name="model"> The model to remove the entity type from. </param>
        /// <param name="type"> The entity type to be removed. </param>
        /// <returns> The entity type that was removed. </returns>
        public static IConventionEntityType RemoveEntityType([NotNull] this IConventionModel model, [NotNull] Type type)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(type, nameof(type));

            return ((Model)model).RemoveEntityType(type);
        }

        /// <summary>
        ///     Removes an entity type with a defining navigation from the model.
        /// </summary>
        /// <param name="model"> The model to remove the entity type from. </param>
        /// <param name="type"> The CLR class that is used to represent instances of this entity type. </param>
        /// <param name="definingNavigationName"> The defining navigation. </param>
        /// <param name="definingEntityType"> The defining entity type. </param>
        /// <returns> The entity type that was removed. </returns>
        public static IConventionEntityType RemoveEntityType(
            [NotNull] this IConventionModel model,
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] IConventionEntityType definingEntityType)
            => Check.NotNull((Model)model, nameof(model)).RemoveEntityType(
                Check.NotNull(type, nameof(type)),
                Check.NotNull(definingNavigationName, nameof(definingNavigationName)),
                (EntityType)Check.NotNull(definingEntityType, nameof(definingEntityType)));

        /// <summary>
        ///     Returns the entity types corresponding to the least derived types from the given.
        /// </summary>
        /// <param name="model"> The model to find the entity types in. </param>
        /// <param name="type"> The base type. </param>
        /// <param name="condition"> An optional condition for filtering entity types. </param>
        /// <returns> List of entity types corresponding to the least derived types from the given. </returns>
        public static IReadOnlyList<IConventionEntityType> FindLeastDerivedEntityTypes(
            [NotNull] this IConventionModel model,
            [NotNull] Type type,
            [CanBeNull] Func<IConventionEntityType, bool> condition = null)
            => Check.NotNull((Model)model, nameof(model))
                .FindLeastDerivedEntityTypes(type, condition);

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for properties of all entity types
        ///         in this model.
        ///     </para>
        ///     <para>
        ///         Note that individual entity types can override this access mode, and individual properties of
        ///         entity types can override the access mode set on the entity type. The value set here will
        ///         be used for any property for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <param name="model"> The model to set the access mode for. </param>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or <see langword="null" /> to clear the mode set.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static PropertyAccessMode? SetPropertyAccessMode(
            [NotNull] this IConventionModel model,
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation = false)
            => Check.NotNull((Model)model, nameof(model))
                .SetPropertyAccessMode(
                    propertyAccessMode,
                    fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="ModelExtensions.GetPropertyAccessMode" />.
        /// </summary>
        /// <param name="model"> The model to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="ModelExtensions.GetPropertyAccessMode" />. </returns>
        public static ConfigurationSource? GetPropertyAccessModeConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(CoreAnnotationNames.PropertyAccessMode)?.GetConfigurationSource();

        /// <summary>
        ///     Sets the default change tracking strategy to use for entities in the model. This strategy indicates how the
        ///     context detects changes to properties for an instance of an entity type.
        /// </summary>
        /// <param name="model"> The model to set the default change tracking strategy for. </param>
        /// <param name="changeTrackingStrategy"> The strategy to use. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static ChangeTrackingStrategy? SetChangeTrackingStrategy(
            [NotNull] this IConventionModel model,
            ChangeTrackingStrategy? changeTrackingStrategy,
            bool fromDataAnnotation = false)
            => ((Model)model).SetChangeTrackingStrategy(
                    changeTrackingStrategy,
                    fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="ModelExtensions.GetChangeTrackingStrategy" />.
        /// </summary>
        /// <param name="model"> The model to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="ModelExtensions.GetChangeTrackingStrategy" />. </returns>
        [DebuggerStepThrough]
        public static ConfigurationSource? GetChangeTrackingStrategyConfigurationSource([NotNull] this IConventionModel model)
            => ((Model)model).GetChangeTrackingStrategyConfigurationSource();

        /// <summary>
        ///     Returns a value indicating whether the entity types using the given type should be configured
        ///     as owned types when discovered.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="type"> The type of the entity type that could be owned. </param>
        /// <returns>
        ///     <see langword="true" /> if the given type name is marked as owned,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static bool IsOwned([NotNull] this IConventionModel model, [NotNull] Type type)
            => model.FindIsOwnedConfigurationSource(type) != null;

        /// <summary>
        ///     Returns a value indicating whether the entity types using the given type should be configured
        ///     as owned types when discovered.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="type"> The type of the entity type that could be owned. </param>
        /// <returns>
        ///     The configuration source if the given type name is marked as owned,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static ConfigurationSource? FindIsOwnedConfigurationSource([NotNull] this IConventionModel model, [NotNull] Type type)
            => Check.NotNull((Model)model, nameof(model)).FindIsOwnedConfigurationSource(
                Check.NotNull(type, nameof(type)));

        /// <summary>
        ///     Marks the given entity type as owned, indicating that when discovered entity types using the given type
        ///     should be configured as owned.
        /// </summary>
        /// <param name="model"> The model to add the owned type to. </param>
        /// <param name="type"> The type of the entity type that should be owned. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void AddOwned([NotNull] this IConventionModel model, [NotNull] Type type, bool fromDataAnnotation = false)
            => Check.NotNull((Model)model, nameof(model)).AddOwned(
                Check.NotNull(type, nameof(type)),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Indicates whether the given entity type name is ignored.
        /// </summary>
        /// <param name="model"> The model to check for ignored type. </param>
        /// <param name="typeName"> The name of the entity type that could be ignored. </param>
        /// <returns> <see langword="true" /> if the given entity type name is ignored. </returns>
        public static bool IsIgnored([NotNull] this IConventionModel model, [NotNull] string typeName)
            => model.FindIgnoredConfigurationSource(typeName) != null;

        /// <summary>
        ///     Indicates whether the given entity type is ignored.
        /// </summary>
        /// <param name="model"> The model to check for ignored type. </param>
        /// <param name="type"> The entity type that might be ignored. </param>
        /// <returns> <see langword="true" /> if the given entity type is ignored. </returns>
        public static bool IsIgnored([NotNull] this IConventionModel model, [NotNull] Type type)
            => Check.NotNull((Model)model, nameof(model)).IsIgnored(
                Check.NotNull(type, nameof(type)));

        /// <summary>
        ///     Indicates whether the given entity type is ignored.
        /// </summary>
        /// <param name="model"> The model to check for ignored type. </param>
        /// <param name="type"> The entity type that might be ignored. </param>
        /// <returns>
        ///     The configuration source if the given entity type is ignored,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static ConfigurationSource? FindIgnoredConfigurationSource(
            [NotNull] this IConventionModel model,
            [NotNull] Type type)
            => Check.NotNull((Model)model, nameof(model)).FindIgnoredConfigurationSource(
                Check.NotNull(type, nameof(type)));

        /// <summary>
        ///     Removes the given owned type, indicating that when discovered matching entity types
        ///     should not be configured as owned.
        /// </summary>
        /// <param name="model"> The model to remove the owned type name from. </param>
        /// <param name="type"> The type of the entity type that should not be owned. </param>
        /// <returns> The name of the removed owned type. </returns>
        public static string RemoveOwned([NotNull] this IConventionModel model, [NotNull] Type type)
            => Check.NotNull((Model)model, nameof(model)).RemoveOwned(
                Check.NotNull(type, nameof(type)));

        /// <summary>
        ///     Marks the given entity type as ignored.
        /// </summary>
        /// <param name="model"> The model to add the ignored type to. </param>
        /// <param name="type"> The entity type to be ignored. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The name of the ignored entity type. </returns>
        public static string AddIgnored([NotNull] this IConventionModel model, [NotNull] Type type, bool fromDataAnnotation = false)
            => Check.NotNull((Model)model, nameof(model)).AddIgnored(
                Check.NotNull(type, nameof(type)),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Marks the given entity type as shared, indicating that when discovered matching entity types
        ///     should be configured as shared type entity type.
        /// </summary>
        /// <param name="model"> The model to add the shared type to. </param>
        /// <param name="type"> The type of the entity type that should be shared. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void AddShared([NotNull] this IConventionModel model, [NotNull] Type type, bool fromDataAnnotation = false)
            => Check.NotNull((Model)model, nameof(model)).AddShared(
                Check.NotNull(type, nameof(type)),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Forces post-processing on the model such that it is ready for use by the runtime. This post
        ///     processing happens automatically when using <see cref="DbContext.OnModelCreating" />; this method allows it to be run
        ///     explicitly in cases where the automatic execution is not possible.
        /// </summary>
        /// <param name="model"> The model to finalize. </param>
        /// <returns> The finalized <see cref="IModel" />. </returns>
        public static IModel FinalizeModel([NotNull] this IConventionModel model)
            => ((Model)model).FinalizeModel();
    }
}
