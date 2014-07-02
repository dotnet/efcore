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
    //TODO investigate the performance possibilities such as using reflection instead and/or caching instances
    public class TableEntityAdapterFactory
    {
        public virtual ITableEntity CreateFromStateEntry([NotNull] StateEntry entry)
        {
            Check.NotNull(entry, "entry");

            var entityType = entry.Entity.GetType();
            var ctor = GetOrMakeCreator(entityType);
            return ctor(entry);
        }

        private readonly ThreadSafeDictionaryCache<Type, Func<StateEntry, ITableEntity>> _instanceCreatorCache = new ThreadSafeDictionaryCache<Type, Func<StateEntry, ITableEntity>>();

        private Func<StateEntry, ITableEntity> GetOrMakeCreator(Type objType)
        {
            return _instanceCreatorCache.GetOrAdd(objType, type =>
                {
                    var paramExpression = new[]
                        {
                            Expression.Parameter(typeof(StateEntry), "entry")
                        };
                    var ctorExpression = Expression.New(
                        typeof(StateEntryTableEntityAdapter<>)
                            .MakeGenericType(type)
                            .GetConstructor(new[] { typeof(StateEntry) }),
                        paramExpression
                        );

                    var lambda = Expression.Lambda<Func<StateEntry, ITableEntity>>(ctorExpression, paramExpression);
                    return lambda.Compile();
                });
        }
    }
}
