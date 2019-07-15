// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Defines a source for generating <see cref="Expression" /> trees that read values from
    ///         a <see cref="ValueBuffer" /> or creates entity instances.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IEntityMaterializerSource
    {
        /// <summary>
        ///     <para>
        ///         Creates an <see cref="Expression" /> tree representing reading a value from a <see cref="ValueBuffer" />
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="valueBuffer"> The expression that exposes the <see cref="ValueBuffer" />. </param>
        /// <param name="type"> The type to read. </param>
        /// <param name="index"> The index in the buffer to read from. </param>
        /// <param name="property"> The IPropertyBase being read if any. </param>
        /// <returns> An expression to read the value. </returns>
        Expression CreateReadValueExpression(
            [NotNull] Expression valueBuffer,
            [NotNull] Type type,
            int index,
            [CanBeNull] IPropertyBase property);

        /// <summary>
        ///     <para>
        ///         Creates an <see cref="Expression" /> tree representing creating an entity instance.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type being materialized. </param>
        /// <param name="entityInstanceName"> The name of the instance being materialized. </param>
        /// <param name="materializationExpression"> The materialization expression to build on. </param>
        /// <param name="indexMap"> An optional index map for reading values. </param>
        /// <returns> An expression to read the value. </returns>
        Expression CreateMaterializeExpression(
            [NotNull] IEntityType entityType,
            [NotNull] string entityInstanceName,
            [NotNull] Expression materializationExpression,
            [CanBeNull] int[] indexMap = null);

        /// <summary>
        ///     <para>
        ///         Returns a cached delegate that creates instances of the given entity type.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type being materialized. </param>
        /// <returns> A delegate to create instances. </returns>
        Func<MaterializationContext, object> GetMaterializer([NotNull] IEntityType entityType);
    }
}
