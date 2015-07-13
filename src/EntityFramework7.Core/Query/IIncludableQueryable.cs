// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Microsoft.Data.Entity.Query
{
    // ReSharper disable once UnusedTypeParameter
    public interface IIncludableQueryable<out TEntity, out TProperty> : IQueryable<TEntity>
    {
    }
}
