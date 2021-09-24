// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IReadOnlyModel" />.
    /// </summary>
    [Obsolete("Use IReadOnlyModel")]
    public static class ModelExtensions
    {
        /// <summary>
        ///     Gets the entity that maps the given entity class. Returns <see langword="null" /> if no entity type with
        ///     the given CLR type is found or the given CLR type is being used by shared type entity type
        ///     or the entity type has a defining navigation.
        /// </summary>
        /// <param name="model">The model to find the entity type in.</param>
        /// <param name="type">The type to find the corresponding entity type for.</param>
        /// <returns>The entity type, or <see langword="null" /> if none is found.</returns>
        [DebuggerStepThrough]
        [Obsolete("Use IReadOnlyEntityType.FindEntityType")]
        public static IReadOnlyEntityType? FindEntityType(this IModel model, Type type)
            => model.FindEntityType(Check.NotNull(type, nameof(type)));

        /// <summary>
        ///     Gets the entity types matching the given type.
        /// </summary>
        /// <param name="model">The model to find the entity type in.</param>
        /// <param name="type">The type of the entity type to find.</param>
        /// <returns>The entity types found.</returns>
        [DebuggerStepThrough]
        [Obsolete("Use IReadOnlyEntityType.FindEntityTypes")]
        public static IEnumerable<IReadOnlyEntityType> GetEntityTypes(this IModel model, Type type)
            => model.FindEntityTypes(type);

        /// <summary>
        ///     Gets the entity types matching the given name.
        /// </summary>
        /// <param name="model">The model to find the entity type in.</param>
        /// <param name="name">The name of the entity type to find.</param>
        /// <returns>The entity types found.</returns>
        [DebuggerStepThrough]
        [Obsolete("Use FindEntityTypes(Type) or FindEntityType(string)")]
        public static IReadOnlyCollection<IReadOnlyEntityType> GetEntityTypes(this IModel model, string name)
            => ((Model)model).GetEntityTypes(name);

        /// <summary>
        ///     Gets a value indicating whether the model contains a corresponding entity type with a defining navigation.
        /// </summary>
        /// <param name="model">The model to find the entity type in.</param>
        /// <param name="type">The type used to find an entity type a defining navigation.</param>
        /// <returns><see langword="true" /> if the model contains a corresponding entity type with a defining navigation.</returns>
        [DebuggerStepThrough]
        [Obsolete("Use IsShared(Type)")]
        public static bool HasEntityTypeWithDefiningNavigation(this IModel model, Type type)
            => model.IsShared(type);

        /// <summary>
        ///     Gets a value indicating whether the model contains a corresponding entity type with a defining navigation.
        /// </summary>
        /// <param name="model">The model to find the entity type in.</param>
        /// <param name="name">The name used to find an entity type with a defining navigation.</param>
        /// <returns><see langword="true" /> if the model contains a corresponding entity type with a defining navigation.</returns>
        [DebuggerStepThrough]
        [Obsolete("Use FindEntityType(string)?.HasSharedClrType")]
        public static bool HasEntityTypeWithDefiningNavigation(this IModel model, string name)
            => model.FindEntityType(name)?.HasSharedClrType ?? false;
    }
}
