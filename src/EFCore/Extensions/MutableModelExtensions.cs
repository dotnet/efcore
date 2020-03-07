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
    ///     Extension methods for <see cref="IMutableModel" />.
    /// </summary>
    public static class MutableModelExtensions
    {
        /// <summary>
        ///     Gets the entity that maps the given entity class. Returns <c>null</c> if no entity type with the given name is found.
        /// </summary>
        /// <param name="model"> The model to find the entity type in. </param>
        /// <param name="type"> The type to find the corresponding entity type for. </param>
        /// <returns> The entity type, or <c>null</c> if none if found. </returns>
        public static IMutableEntityType FindEntityType([NotNull] this IMutableModel model, [NotNull] Type type)
            => (IMutableEntityType)((IModel)model).FindEntityType(type);

        /// <summary>
        ///     Gets the entity type for the given name, defining navigation name
        ///     and the defining entity type. Returns <c>null</c> if no matching entity type is found.
        /// </summary>
        /// <param name="model"> The model to find the entity type in. </param>
        /// <param name="type"> The type of the entity type to find. </param>
        /// <param name="definingNavigationName"> The defining navigation of the entity type to find. </param>
        /// <param name="definingEntityType"> The defining entity type of the entity type to find. </param>
        /// <returns> The entity type, or <c>null</c> if none are found. </returns>
        public static IMutableEntityType FindEntityType(
            [NotNull] this IMutableModel model,
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] IMutableEntityType definingEntityType)
            => (IMutableEntityType)((IModel)model).FindEntityType(type, definingNavigationName, definingEntityType);

        /// <summary>
        ///     Gets the entity types matching the given type.
        /// </summary>
        /// <param name="model"> The model to find the entity type in. </param>
        /// <param name="type"> The type of the entity type to find. </param>
        /// <returns> The entity types found. </returns>
        [DebuggerStepThrough]
        public static IReadOnlyCollection<IMutableEntityType> GetEntityTypes([NotNull] this IMutableModel model, [NotNull] Type type)
            => ((Model)model).GetEntityTypes(type);

        /// <summary>
        ///     Gets the entity types matching the given name.
        /// </summary>
        /// <param name="model"> The model to find the entity type in. </param>
        /// <param name="name"> The name of the entity type to find. </param>
        /// <returns> The entity types found. </returns>
        [DebuggerStepThrough]
        public static IReadOnlyCollection<IMutableEntityType> GetEntityTypes([NotNull] this IMutableModel model, [NotNull] string name)
            => ((Model)model).GetEntityTypes(name);

        /// <summary>
        ///     Removes an entity type from the model.
        /// </summary>
        /// <param name="model"> The model to remove the entity type from. </param>
        /// <param name="type"> The entity type to be removed. </param>
        /// <returns> The entity type that was removed. </returns>
        public static IMutableEntityType RemoveEntityType([NotNull] this IMutableModel model, [NotNull] Type type)
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
        public static IMutableEntityType RemoveEntityType(
            [NotNull] this IMutableModel model,
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] IMutableEntityType definingEntityType)
            => Check.NotNull((Model)model, nameof(model)).RemoveEntityType(
                Check.NotNull(type, nameof(type)),
                Check.NotNull(definingNavigationName, nameof(definingNavigationName)),
                (EntityType)Check.NotNull(definingEntityType, nameof(definingEntityType)));

        /// <summary>
        ///     Removes an entity type without a defining navigation from the model.
        /// </summary>
        /// <param name="model"> The model to remove the entity type from. </param>
        /// <param name="name"> The name of the entity type to be removed. </param>
        /// <returns> The entity type that was removed. </returns>
        public static IMutableEntityType RemoveEntityType(
            [NotNull] this IMutableModel model,
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
        public static IMutableEntityType RemoveEntityType(
            [NotNull] this IMutableModel model,
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] IMutableEntityType definingEntityType)
        {
            Check.NotNull(model, nameof(model));
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(definingNavigationName, nameof(definingNavigationName));
            Check.NotNull(definingEntityType, nameof(definingEntityType));

            return ((Model)model).RemoveEntityType(name);
        }

        /// <summary>
        ///     Returns the entity types corresponding to the least derived types from the given.
        /// </summary>
        /// <param name="model"> The model to find the entity types in. </param>
        /// <param name="type"> The base type. </param>
        /// <param name="condition"> An optional condition for filtering entity types. </param>
        /// <returns> List of entity types corresponding to the least derived types from the given. </returns>
        public static IReadOnlyList<IMutableEntityType> FindLeastDerivedEntityTypes(
            [NotNull] this IMutableModel model,
            [NotNull] Type type,
            [CanBeNull] Func<IMutableEntityType, bool> condition = null)
            => Check.NotNull((Model)model, nameof(model))
                .FindLeastDerivedEntityTypes(type, condition);

        /// <summary>
        ///     Removes the ignored entity type.
        /// </summary>
        /// <param name="model"> The model to remove the ignored entity type from. </param>
        /// <param name="type"> The ignored entity type to be removed. </param>
        public static void RemoveIgnored([NotNull] this IMutableModel model, [NotNull] Type type)
            => Check.NotNull((Model)model, nameof(model)).RemoveIgnored(
                Check.NotNull(type, nameof(type)));

        /// <summary>
        ///     Indicates whether the given entity type name is ignored.
        /// </summary>
        /// <param name="model"> The model to check for ignored type. </param>
        /// <param name="type"> The entity type that might be ignored. </param>
        /// <returns> <c>true</c> if the given entity type name is ignored. </returns>
        public static bool IsIgnored([NotNull] this IMutableModel model, [NotNull] Type type)
            => Check.NotNull((Model)model, nameof(model)).IsIgnored(
                Check.NotNull(type, nameof(type)));

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
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or <c>null</c> to clear the mode set.</param>
        public static void SetPropertyAccessMode(
            [NotNull] this IMutableModel model, PropertyAccessMode? propertyAccessMode)
            => Check.NotNull((Model)model, nameof(model))
                .SetPropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the default change tracking strategy to use for entities in the model. This strategy indicates how the
        ///     context detects changes to properties for an instance of an entity type.
        /// </summary>
        /// <param name="model"> The model to set the default change tracking strategy for. </param>
        /// <param name="changeTrackingStrategy"> The strategy to use. </param>
        public static void SetChangeTrackingStrategy(
            [NotNull] this IMutableModel model, ChangeTrackingStrategy? changeTrackingStrategy)
            => Check.NotNull((Model)model, nameof(model))
                .SetChangeTrackingStrategy(changeTrackingStrategy, ConfigurationSource.Explicit);

        /// <summary>
        ///     Marks the given entity type as ignored, preventing conventions from adding a matching entity type to the model.
        /// </summary>
        /// <param name="model"> The model to add the ignored type to. </param>
        /// <param name="clrType"> The entity type to be ignored. </param>
        public static void AddIgnored([NotNull] this IMutableModel model, [NotNull] Type clrType)
            => Check.NotNull((Model)model, nameof(model)).AddIgnored(
                Check.NotNull(clrType, nameof(clrType)), ConfigurationSource.Explicit);

        /// <summary>
        ///     Returns a value indicating whether the entity types using the given type should be configured
        ///     as owned types when discovered by conventions.
        /// </summary>
        /// <param name="model"> The model to get the value from. </param>
        /// <param name="clrType"> The type of the entity type that might be owned. </param>
        /// <returns>
        ///     <c>true</c> if a matching entity type should be configured as owned when discovered,
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool IsOwned([NotNull] this IMutableModel model, [NotNull] Type clrType)
            => Check.NotNull((Model)model, nameof(model)).IsOwned(
                Check.NotNull(clrType, nameof(clrType)));

        /// <summary>
        ///     Marks the given entity type as owned, indicating that when discovered matching entity types
        ///     should be configured as owned.
        /// </summary>
        /// <param name="model"> The model to add the owned type to. </param>
        /// <param name="clrType"> The type of the entity type that should be owned. </param>
        public static void AddOwned([NotNull] this IMutableModel model, [NotNull] Type clrType)
            => Check.NotNull((Model)model, nameof(model)).AddOwned(
                Check.NotNull(clrType, nameof(clrType)), ConfigurationSource.Explicit);

        /// <summary>
        ///     Removes the given owned type, indicating that when discovered matching entity types
        ///     should not be configured as owned.
        /// </summary>
        /// <param name="model"> The model to remove the owned type name from. </param>
        /// <param name="clrType"> The type of the entity type that should not be owned. </param>
        public static void RemoveOwned([NotNull] this IMutableModel model, [NotNull] Type clrType)
            => Check.NotNull((Model)model, nameof(model)).RemoveOwned(
                Check.NotNull(clrType, nameof(clrType)));

        /// <summary>
        ///     Forces post-processing on the model such that it is ready for use by the runtime. This post
        ///     processing happens automatically when using <see cref="DbContext.OnModelCreating" />; this method allows it to be run
        ///     explicitly in cases where the automatic execution is not possible.
        /// </summary>
        /// <param name="model"> The model to finalize. </param>
        /// <returns> The finalized <see cref="IModel" />. </returns>
        public static IModel FinalizeModel([NotNull] this IMutableModel model) => ((Model)model).FinalizeModel();
    }
}
