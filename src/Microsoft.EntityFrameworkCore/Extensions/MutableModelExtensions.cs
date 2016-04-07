// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
        {
            Check.NotNull(model, nameof(model));

            return model.FindEntityType(name) ?? model.AddEntityType(name);
        }

        public static IMutableEntityType GetOrAddEntityType([NotNull] this IMutableModel model, [NotNull] Type type)
            => model.FindEntityType(type) ?? model.AddEntityType(type);

        public static IMutableEntityType AddEntityType([NotNull] this IMutableModel model, [NotNull] Type type)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(type, nameof(type));

            var canFindEntityType = model as ICanFindEntityType;

            var entityType = canFindEntityType != null
                ? canFindEntityType.AddEntityType(type.DisplayName(), type) :
                model.AddEntityType(type.DisplayName());

            entityType.ClrType = type;

            return entityType;
        }

        public static IMutableEntityType RemoveEntityType([NotNull] this IMutableModel model, [NotNull] Type type)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(type, nameof(type));

            return model.RemoveEntityType(type.DisplayName());
        }
    }
}
