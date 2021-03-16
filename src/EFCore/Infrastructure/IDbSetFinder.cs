// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A service for finding <see cref="DbSet{TEntity}" /> properties on a type that inherits from <see cref="DbContext" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IDbSetFinder
    {
        /// <summary>
        ///     Finds <see cref="DbSet{TEntity}" /> properties on a type that inherits from <see cref="DbContext" />.
        /// </summary>
        /// <param name="contextType"> A type that inherits from <see cref="DbContext" /> </param>
        /// <returns> A list of the found properties. </returns>
        IReadOnlyList<DbSetProperty> FindSets([NotNull] Type contextType);
    }
}
