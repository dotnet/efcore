// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    /// <summary>
    /// Supports queryable Include/ThenInclude chaining operators.
    /// </summary>
    /// <typeparam name="TEntity"> The entity type. </typeparam>
    /// <typeparam name="TProperty"> The property type. </typeparam>
    // ReSharper disable once UnusedTypeParameter
    public interface IIncludableQueryable<out TEntity, out TProperty> : IQueryable<TEntity>
    {
    }
}
