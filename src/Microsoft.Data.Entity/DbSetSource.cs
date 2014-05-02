// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class DbSetSource
    {
        private static readonly MethodInfo _genericCreate
            = typeof(DbSetSource).GetTypeInfo().GetDeclaredMethods("CreateConstructor").Single();

        private readonly ThreadSafeDictionaryCache<Type, Func<DbContext, DbSet>> _cache
            = new ThreadSafeDictionaryCache<Type, Func<DbContext, DbSet>>();

        public virtual DbSet Create([NotNull] DbContext context, [NotNull] Type type)
        {
            Check.NotNull(context, "context");
            Check.NotNull("type", "type");

            var factory = _cache.GetOrAdd(
                type,
                t => (Func<DbContext, DbSet>)_genericCreate.MakeGenericMethod(type).Invoke(null, null));

            return factory(context);
        }

        [UsedImplicitly]
        private static Func<DbContext, DbSet> CreateConstructor<TEntity>() where TEntity : class
        {
            return c => new DbSet<TEntity>(c);
        }
    }
}
