// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
        ///     Gets the entity that maps the given entity class. Returns null if no entity type with the given name is found.
        /// </summary>
        /// <param name="model"> The model to find the entity type in. </param>
        /// <param name="type"> The type to find the corresponding entity type for. </param>
        /// <returns> The entity type, or null if none if found. </returns>
        public static IMutableEntityType FindEntityType([NotNull] this IMutableModel model, [NotNull] Type type)
            => (IMutableEntityType)((IModel)model).FindEntityType(type);

        /// <summary>
        ///     Gets the entity type for the given name, defining navigation name
        ///     and the defining entity type. Returns null if no matching entity type is found.
        /// </summary>
        /// <param name="model"> The model to find the entity type in. </param>
        /// <param name="type"> The type of the entity type to find. </param>
        /// <param name="definingNavigationName"> The defining navigation of the entity type to find. </param>
        /// <param name="definingEntityType"> The defining entity type of the entity type to find. </param>
        /// <returns> The entity type, or null if none are found. </returns>
        public static IMutableEntityType FindEntityType(
            [NotNull] this IMutableModel model,
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] IMutableEntityType definingEntityType)
            => (IMutableEntityType)((IModel)model).FindEntityType(type, definingNavigationName, definingEntityType);

        /// <summary>
        ///     Gets the entity type with the given name or adds a new entity type if none is found.
        /// </summary>
        /// <param name="model"> The model to find or add the entity type to. </param>
        /// <param name="name"> The name of the entity type. </param>
        /// <returns> The existing or newly created entity type. </returns>
        public static IMutableEntityType GetOrAddEntityType([NotNull] this IMutableModel model, [NotNull] string name)
            => Check.NotNull(model, nameof(model)).FindEntityType(name) ?? model.AddEntityType(name);

        /// <summary>
        ///     Gets the entity type with the given CLR class or adds a new entity type if none is found.
        /// </summary>
        /// <param name="model"> The model to find or add the entity type to. </param>
        /// <param name="type"> The CLR class of the entity type. </param>
        /// <returns> The existing or newly created entity type. </returns>
        public static IMutableEntityType GetOrAddEntityType([NotNull] this IMutableModel model, [NotNull] Type type)
            => Check.NotNull(model, nameof(model)).FindEntityType(type) ?? model.AddEntityType(type);

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

            return model.AsModel().RemoveEntityType(type);
        }

        /// <summary>
        ///     Removes an entity type from the model.
        /// </summary>
        /// <param name="model"> The model to remove the entity type from. </param>
        /// <param name="entityType"> The entity type to be removed. </param>
        /// <returns> The entity type that was removed. </returns>
        public static IMutableEntityType RemoveEntityType(
            [NotNull] this IMutableModel model,
            [NotNull] IMutableEntityType entityType)
            => Check.NotNull(model, nameof(model)).AsModel().RemoveEntityType(
                (EntityType)Check.NotNull(entityType, nameof(entityType)));

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
            => Check.NotNull(model, nameof(model)).AsModel().RemoveEntityType(
                Check.NotNull(type, nameof(type)),
                Check.NotNull(definingNavigationName, nameof(definingNavigationName)),
                (EntityType)Check.NotNull(definingEntityType, nameof(definingEntityType)));

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
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or null to clear the mode set.</param>
        public static void SetPropertyAccessMode(
            [NotNull] this IMutableModel model, PropertyAccessMode? propertyAccessMode)
        {
            Check.NotNull(model, nameof(model));

            model[CoreAnnotationNames.PropertyAccessModeAnnotation] = propertyAccessMode;
        }

        /// <summary>
        ///     Sets the default change tracking strategy to use for entities in the model. This strategy indicates how the
        ///     context detects changes to properties for an instance of an entity type.
        /// </summary>
        /// <param name="model"> The model to set the default change tracking strategy for. </param>
        /// <param name="changeTrackingStrategy"> The strategy to use. </param>
        public static void SetChangeTrackingStrategy(
            [NotNull] this IMutableModel model,
            ChangeTrackingStrategy changeTrackingStrategy)
            => Check.NotNull(model, nameof(model)).AsModel().ChangeTrackingStrategy = changeTrackingStrategy;
    }
}
