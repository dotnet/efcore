// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
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
        {
            Check.NotNull(type, nameof(type));

            return model.FindEntityType(type.DisplayName());
        }
    }
}
