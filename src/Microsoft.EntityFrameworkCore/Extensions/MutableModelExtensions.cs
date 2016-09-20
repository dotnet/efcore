// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
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
        /// <param name="type"> The type of the entity class to find the type for. </param>
        /// <returns> The entity type, or null if none if found. </returns>
        public static IMutableEntityType FindEntityType([NotNull] this IMutableModel model, [NotNull] Type type)
            => (IMutableEntityType)((IModel)model).FindEntityType(type);

        /// <summary>
        ///     Gets the entity type with the given name or adds a new entity type if none is found.
        /// </summary>
        /// <param name="model"> The model to find or add the entity type to. </param>
        /// <param name="name"> The name of the entity type. </param>
        /// <returns> The existing or newly created entity type. </returns>
        public static IMutableEntityType GetOrAddEntityType([NotNull] this IMutableModel model, [NotNull] string name)
            => Check.NotNull(model, nameof(model)).FindEntityType(name) ?? model.AddEntityType(name);

        /// <summary>
        ///     Gets the entity type with the given .NET type or adds a new entity type if none is found.
        /// </summary>
        /// <param name="model"> The model to find or add the entity type to. </param>
        /// <param name="type"> The .NET type of the entity type. </param>
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

            return model.RemoveEntityType(type.DisplayName());
        }

        /// <summary>
        ///     Gets the complex type definition that maps the given type. Returns null if no complex type definition with the given name is found.
        /// </summary>
        /// <param name="model"> The model to find the complex type definition in. </param>
        /// <param name="type"> The CLR type to find the complex type definition for. </param>
        /// <returns> The complex type definition, or null if none if found. </returns>
        public static IMutableComplexTypeDefinition FindComplexTypeDefinition([NotNull] this IMutableModel model, [NotNull] Type type)
            => model.AsModel().FindComplexTypeDefinition(type);

        /// <summary>
        ///     Gets the complex type definition with the given name or adds a new complex type definition if none is found.
        /// </summary>
        /// <param name="model"> The model to find or add the complex type definition to. </param>
        /// <param name="name"> The name of the complex type definition. </param>
        /// <returns> The existing or newly created complex type definition. </returns>
        public static IMutableComplexTypeDefinition GetOrAddComplexTypeDefinition([NotNull] this IMutableModel model, [NotNull] string name)
            => model.AsModel().GetOrAddComplexTypeDefinition(name);

        /// <summary>
        ///     Gets the complex type definition with the given .NET type or adds a new complex type definition if none is found.
        /// </summary>
        /// <param name="model"> The model to find or add the complex type definition to. </param>
        /// <param name="type"> The .NET type of the complex type definition. </param>
        /// <returns> The existing or newly created complex type definition. </returns>
        public static IMutableComplexTypeDefinition GetOrAddComplexTypeDefinition([NotNull] this IMutableModel model, [NotNull] Type type)
            => model.AsModel().GetOrAddComplexTypeDefinition(type);

        /// <summary>
        ///     Removes an complex type definition from the model.
        /// </summary>
        /// <param name="model"> The model to remove the complex type definition from. </param>
        /// <param name="type"> The complex type definition to be removed. </param>
        /// <returns> The complex type definition that was removed. </returns>
        public static IMutableComplexTypeDefinition RemoveComplexTypeDefinition([NotNull] this IMutableModel model, [NotNull] Type type)
            => model.AsModel().RemoveComplexTypeDefinition(type);

        /// <summary>
        ///     Gets the entity type or complex type definition with the given name. Returns null if no type with the given name is found.
        /// </summary>
        /// <param name="model"> The model to find the type in. </param>
        /// <param name="name"> The the name to look up. </param>
        /// <returns> The type, or null if none if found. </returns>
        public static IMutableTypeBase FindMappedType([NotNull] this IMutableModel model, [NotNull] string name)
            => model.AsModel().FindMappedType(name);

        /// <summary>
        ///     Gets the entity type or complex type definition that maps the given type. Returns null if no type with the given name is found.
        /// </summary>
        /// <param name="model"> The model to find the type in. </param>
        /// <param name="type"> The CLR type to find the type definition for. </param>
        /// <returns> The type, or null if none if found. </returns>
        public static IMutableTypeBase FindMappedType([NotNull] this IMutableModel model, [NotNull] Type type)
            => model.AsModel().FindMappedType(type);

        /// <summary>
        ///     Removes an entity type or complex type definition with a given name from the model.
        /// </summary>
        /// <param name="model"> The model to remove the type from. </param>
        /// <param name="name"> The name of the entity type or complex type definition to be removed. </param>
        /// <returns> The type that was removed. </returns>
        public static IMutableTypeBase RemoveMappedType([NotNull] this IMutableModel model, [NotNull] string name)
            => model.AsModel().RemoveMappedType(name);

        /// <summary>
        ///     Removes an entity type or complex type definition from the model.
        /// </summary>
        /// <param name="model"> The model to remove the type from. </param>
        /// <param name="type"> The entity type or complex type definition to be removed. </param>
        /// <returns> The type that was removed. </returns>
        public static IMutableTypeBase RemoveMappedType([NotNull] this IMutableModel model, [NotNull] Type type)
            => model.AsModel().RemoveMappedType(type);

        /// <summary>
        /// Gets the entity types and complex type definitions contained in the model.
        /// </summary>
        /// <param name="model"> The model to get types from. </param>
        /// <returns> All mapped types in the model. </returns>
        public static IEnumerable<IMutableTypeBase> GetMappedTypes([NotNull] this IMutableModel model)
            => model.AsModel().GetMappedTypes();

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
    }
}
