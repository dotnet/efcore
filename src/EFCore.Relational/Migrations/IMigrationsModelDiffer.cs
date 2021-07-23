// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     <para>
    ///         A service for finding differences between two <see cref="IRelationalModel" />s and transforming
    ///         those differences into <see cref="MigrationOperation" />s that can be used to
    ///         update the database.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IMigrationsModelDiffer
    {
        /// <summary>
        ///     Checks whether there are differences between the two models.
        /// </summary>
        /// <param name="source"> The first model. </param>
        /// <param name="target"> The second model. </param>
        /// <returns>
        ///     <see langword="true" /> if there are any differences and <see langword="false" /> otherwise.
        /// </returns>
        bool HasDifferences(IRelationalModel? source, IRelationalModel? target);

        /// <summary>
        ///     Finds the differences between two models.
        /// </summary>
        /// <param name="source"> The model as it was before it was possibly modified. </param>
        /// <param name="target"> The model as it is now. </param>
        /// <returns>
        ///     A list of the operations that need to applied to the database to migrate it
        ///     from mapping to the source model so that is now mapping to the target model.
        /// </returns>
        IReadOnlyList<MigrationOperation> GetDifferences(IRelationalModel? source, IRelationalModel? target);
    }
}
