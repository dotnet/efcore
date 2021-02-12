// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

#nullable enable

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
        ///     Gets the entity types matching the given name.
        /// </summary>
        /// <param name="model"> The model to find the entity type in. </param>
        /// <param name="name"> The name of the entity type to find. </param>
        /// <returns> The entity types found. </returns>
        [DebuggerStepThrough]
        [Obsolete("Use GetEntityTypes(Type) or FindEntityType(string)")]
        public static IReadOnlyCollection<IReadOnlyEntityType> GetEntityTypes([NotNull] this IReadOnlyModel model, [NotNull] string name)
            => ((Model)model).GetEntityTypes(name);

        /// <summary>
        ///     Gets a value indicating whether the model contains a corresponding entity type with a defining navigation.
        /// </summary>
        /// <param name="model"> The model to find the entity type in. </param>
        /// <param name="type"> The type used to find an entity type a defining navigation. </param>
        /// <returns> <see langword="true" /> if the model contains a corresponding entity type with a defining navigation. </returns>
        [DebuggerStepThrough]
        [Obsolete("Use IsShared(Type)")]
        public static bool HasEntityTypeWithDefiningNavigation([NotNull] this IReadOnlyModel model, [NotNull] Type type)
            => model.IsShared(type);

        /// <summary>
        ///     Gets a value indicating whether the model contains a corresponding entity type with a defining navigation.
        /// </summary>
        /// <param name="model"> The model to find the entity type in. </param>
        /// <param name="name"> The name used to find an entity type with a defining navigation. </param>
        /// <returns> <see langword="true" /> if the model contains a corresponding entity type with a defining navigation. </returns>
        [DebuggerStepThrough]
        [Obsolete("Use FindEntityType(string)?.HasSharedClrType")]
        public static bool HasEntityTypeWithDefiningNavigation([NotNull] this IReadOnlyModel model, [NotNull] string name)
            => model.FindEntityType(name)?.HasSharedClrType ?? false;
    }
}
