// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Utilities;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Adapters
{
    public class TableEntityAdapterFactory
    {
        public virtual ITableEntity CreateFromStateEntry([NotNull] StateEntry entry)
        {
            Check.NotNull(entry, "entry");

            var ctor = GetOrMakeAdapter(entry);
            return ctor(entry);
        }

        private readonly ThreadSafeDictionaryCache<StateEntry, Func<StateEntry, ITableEntity>> _instanceCreatorCache
            = new ThreadSafeDictionaryCache<StateEntry, Func<StateEntry, ITableEntity>>();

        private Func<StateEntry, ITableEntity> GetOrMakeAdapter(StateEntry entry)
        {
            return _instanceCreatorCache.GetOrAdd(entry, e =>
                {
                    var paramExpression = new[]
                        {
                            Expression.Parameter(typeof(StateEntry), "entry")
                        };
                    var ctorExpression = Expression.New(
                        typeof(StateEntryTableEntityAdapter<>)
                            .MakeGenericType(e.Entity.GetType())
                            .GetConstructor(new[] { typeof(StateEntry) }),
                        paramExpression
                        );

                    var lambda = Expression.Lambda<Func<StateEntry, ITableEntity>>(ctorExpression, paramExpression);
                    return lambda.Compile();
                });
        }
    }
}
