// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IModel" />.
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        ///     Gets the entity that maps the given entity class. Returns null if no entity type with the given name is found.
        /// </summary>
        /// <param name="model"> The model to find the entity type in. </param>
        /// <param name="type"> The type of the entity class to find the type for. </param>
        /// <returns> The entity type, or null if none if found. </returns>
        public static IEntityType FindEntityType([NotNull] this IModel model, [NotNull] Type type)
            => model.AsModel().FindEntityType(Check.NotNull(type, nameof(type)));

        /// <summary>
        ///     Gets the complex type definition that maps the given type. Returns null if no complex type definition with the given name is found.
        /// </summary>
        /// <param name="model"> The model to find the complex type definition in. </param>
        /// <param name="type"> The CLR type to find the complex type definition for. </param>
        /// <returns> The complex type definition, or null if none if found. </returns>
        public static IComplexTypeDefinition FindComplexTypeDefinition([NotNull] this IModel model, [NotNull] Type type)
            => model.AsModel().FindComplexTypeDefinition(type);

        /// <summary>
        ///     Gets the entity type or complex type definition with the given name. Returns null if no type with the given name is found.
        /// </summary>
        /// <param name="model"> The model to find the type in. </param>
        /// <param name="name"> The the name to look up. </param>
        /// <returns> The type, or null if none if found. </returns>
        public static ITypeBase FindMappedType([NotNull] this IModel model, [NotNull] string name)
            => model.AsModel().FindMappedType(name);

        /// <summary>
        ///     Gets the entity type or complex type definition that maps the given type. Returns null if no type with the given name is found.
        /// </summary>
        /// <param name="model"> The model to find the type in. </param>
        /// <param name="type"> The CLR type to find the type definition for. </param>
        /// <returns> The type, or null if none if found. </returns>
        public static ITypeBase FindMappedType([NotNull] this IModel model, [NotNull] Type type)
            => model.AsModel().FindMappedType(type);

        /// <summary>
        /// Gets the entity types and complex type definitions contained in the model.
        /// </summary>
        /// <param name="model"> The model to get types from. </param>
        /// <returns> All mapped types in the model. </returns>
        public static IEnumerable<ITypeBase> GetMappedTypes([NotNull] this IModel model)
            => model.AsModel().GetMappedTypes();

        /// <summary>
        ///     Gets the default change tracking strategy being used for entities in the model. This strategy indicates how the
        ///     context detects changes to properties for an instance of an entity type.
        /// </summary>
        /// <param name="model"> The model to get the default change tracking strategy for. </param>
        /// <returns> The change tracking strategy. </returns>
        public static ChangeTrackingStrategy GetChangeTrackingStrategy(
            [NotNull] this IModel model)
            => model.AsModel().ChangeTrackingStrategy;

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="PropertyAccessMode" /> being used for properties of entity types in this model.
        ///         Null indicates that the default property access mode is being used.
        ///     </para>
        ///     <para>
        ///         Note that individual entity types can override this access mode, and individual properties of
        ///         entity types can override the access mode set on the entity type. The value returned here will
        ///         be used for any property for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <param name="model"> The model to get the access mode for. </param>
        /// <returns> The access mode being used, or null if the default access mode is being used. </returns>
        public static PropertyAccessMode? GetPropertyAccessMode(
            [NotNull] this IModel model)
            => (PropertyAccessMode?)Check.NotNull(model, nameof(model))[CoreAnnotationNames.PropertyAccessModeAnnotation];
    }
}
