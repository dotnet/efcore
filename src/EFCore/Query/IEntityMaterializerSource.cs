// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
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
        /// <returns> An expression to read the value. </returns>
        Expression CreateMaterializeExpression(
            IEntityType entityType,
            string entityInstanceName,
            Expression materializationExpression);

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
        Func<MaterializationContext, object> GetMaterializer(IEntityType entityType);
    }
}
