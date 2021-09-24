// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IMutableModel" />.
    /// </summary>
    [Obsolete("Use IMutableModel")]
    public static class MutableModelExtensions
    {
        /// <summary>
        ///     Gets the entity types matching the given type.
        /// </summary>
        /// <param name="model">The model to find the entity type in.</param>
        /// <param name="type">The type of the entity type to find.</param>
        /// <returns>The entity types found.</returns>
        [DebuggerStepThrough]
        [Obsolete("Use IMutableEntityType.FindEntityTypes")]
        public static IEnumerable<IMutableEntityType> GetEntityTypes(this IMutableModel model, Type type)
            => model.FindEntityTypes(type);

        /// <summary>
        ///     Gets the entity types matching the given name.
        /// </summary>
        /// <param name="model">The model to find the entity type in.</param>
        /// <param name="name">The name of the entity type to find.</param>
        /// <returns>The entity types found.</returns>
        [DebuggerStepThrough]
        [Obsolete("Use FindEntityTypes(Type) or FindEntityType(string)")]
        public static IReadOnlyCollection<IMutableEntityType> GetEntityTypes(this IMutableModel model, string name)
            => ((Model)model).GetEntityTypes(name);
    }
}
