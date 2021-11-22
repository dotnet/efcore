// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public interface ISetSource
    {
        IQueryable<TEntity> Set<TEntity>()
            where TEntity : class;
    }
}
