// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <inheritdoc />
    public interface IEntityFinder<TEntity> : IEntityFinder
        where TEntity : class
    {
        /// <inheritdoc cref="IEntityFinder.Find" />
        new TEntity Find([CanBeNull] object[] keyValues);

        /// <inheritdoc cref="IEntityFinder.FindAsync" />
        new ValueTask<TEntity> FindAsync([CanBeNull] object[] keyValues, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="IEntityFinder.Query" />
        new IQueryable<TEntity> Query([NotNull] INavigation navigation, [NotNull] InternalEntityEntry entry);
    }
}
