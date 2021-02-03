// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Metadata about the shape of entities, the relationships between them, and how they map to
    ///         the database. A model is typically created by overriding the
    ///         <see cref="DbContext.OnModelCreating(ModelBuilder)" /> method on a derived
    ///         <see cref="DbContext" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IModel : IReadOnlyModel, IAnnotatable
    {
        /// <summary>
        ///     Gets the entity with the given name. Returns <see langword="null" /> if no entity type with the given name is found
        ///     or the given CLR type is being used by shared type entity type
        ///     or the entity type has a defining navigation.
        /// </summary>
        /// <param name="name"> The name of the entity type to find. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        new IEntityType? FindEntityType([NotNull] string name);

        /// <summary>
        ///     Gets the entity type for the given name, defining navigation name
        ///     and the defining entity type. Returns <see langword="null" /> if no matching entity type is found.
        /// </summary>
        /// <param name="name"> The name of the entity type to find. </param>
        /// <param name="definingNavigationName"> The defining navigation of the entity type to find. </param>
        /// <param name="definingEntityType"> The defining entity type of the entity type to find. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        IEntityType? FindEntityType(
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] IEntityType definingEntityType);

        /// <summary>
        ///     Gets the entity that maps the given entity class, where the class may be a proxy derived from the
        ///     actual entity type. Returns <see langword="null" /> if no entity type with the given CLR type is found
        ///     or the given CLR type is being used by shared type entity type
        ///     or the entity type has a defining navigation.
        /// </summary>
        /// <param name="type"> The type to find the corresponding entity type for. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        IEntityType? FindRuntimeEntityType([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return FindEntityType(type)
                ?? (type.BaseType == null
                    ? null
                    : FindEntityType(type.BaseType));
        }

        /// <summary>
        ///     Gets all entity types defined in the model.
        /// </summary>
        /// <returns> All entity types defined in the model. </returns>
        new IEnumerable<IEntityType> GetEntityTypes();

        /// <summary>
        ///     The runtime service dependencies.
        /// </summary>
        SingletonModelDependencies? ModelDependencies
            => (SingletonModelDependencies?)FindRuntimeAnnotationValue(CoreAnnotationNames.ModelDependencies);

        /// <summary>
        ///     Gets the runtime service dependencies.
        /// </summary>
        SingletonModelDependencies GetModelDependencies()
        {
            var dependencies = ModelDependencies;
            if (dependencies == null)
            {
                throw new InvalidOperationException(CoreStrings.ModelNotFinalized(nameof(GetModelDependencies)));
            }

            return dependencies;
        }

        /// <summary>
        ///     Set the runtime service dependencies.
        /// </summary>
        /// <param name="modelDependencies"> The runtime service dependencies. </param>
        /// <returns><see langword="true"/> if the runtime service dependencies were set; <see langword="false"/> otherwise. </returns>
        bool SetModelDependencies([NotNull] SingletonModelDependencies modelDependencies)
        {
            if (FindRuntimeAnnotation(CoreAnnotationNames.ModelDependencies) != null)
            {
                return false;
            }

            AddRuntimeAnnotation(CoreAnnotationNames.ModelDependencies, modelDependencies);

            return true;
        }

        /// <summary>
        ///     Gets the entity that maps the given entity class. Returns <see langword="null" /> if no entity type with
        ///     the given CLR type is found or the given CLR type is being used by shared type entity type
        ///     or the entity type has a defining navigation.
        /// </summary>
        /// <param name="type"> The type to find the corresponding entity type for. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        IEntityType? FindEntityType([NotNull] Type type);

        /// <summary>
        ///     Gets the entity type for the given name, defining navigation name
        ///     and the defining entity type. Returns <see langword="null" /> if no matching entity type is found.
        /// </summary>
        /// <param name="type"> The type of the entity type to find. </param>
        /// <param name="definingNavigationName"> The defining navigation of the entity type to find. </param>
        /// <param name="definingEntityType"> The defining entity type of the entity type to find. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        IEntityType? FindEntityType(
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] IEntityType definingEntityType)
            => (IEntityType?)((IReadOnlyModel)this).FindEntityType(type, definingNavigationName, definingEntityType);

        /// <summary>
        ///     Gets the entity types matching the given type.
        /// </summary>
        /// <param name="type"> The type of the entity type to find. </param>
        /// <returns> The entity types found. </returns>
        [DebuggerStepThrough]
        IEnumerable<IEntityType> GetEntityTypes([NotNull] Type type);

        /// <summary>
        ///     Returns the entity types corresponding to the least derived types from the given.
        /// </summary>
        /// <param name="type"> The base type. </param>
        /// <param name="condition"> An optional condition for filtering entity types. </param>
        /// <returns> List of entity types corresponding to the least derived types from the given. </returns>
        IEnumerable<IEntityType> FindLeastDerivedEntityTypes(
            [NotNull] Type type,
            [CanBeNull] Func<IEntityType, bool>? condition = null)
            => ((IReadOnlyModel)this).FindLeastDerivedEntityTypes(type, condition == null ? null : t => condition((IEntityType)t))
                .Cast<IEntityType>();
    }
}
