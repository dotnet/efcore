// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <inheritdoc />
    public interface ICollectionLoader<out TEntity> : ICollectionLoader
        where TEntity : class
    {
        /// <inheritdoc cref="ICollectionLoader.Query" />
        new IQueryable<TEntity> Query([NotNull] InternalEntityEntry entry);
    }
}
