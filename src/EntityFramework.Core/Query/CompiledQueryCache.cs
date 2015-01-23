// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Query
{
    public class CompiledQueryCache : ICompiledQueryCache
    {
        private readonly ThreadSafeDictionaryCache<QueryModel, Delegate> _cache
            = new ThreadSafeDictionaryCache<QueryModel, Delegate>();

        public virtual Func<QueryContext, IEnumerable<TResult>> GetOrAdd<TResult>(
            QueryModel queryModel,
            Func<QueryModel, Func<QueryContext, IEnumerable<TResult>>> queryCompiler)
        {
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(queryCompiler, "queryCompiler");

            return (Func<QueryContext, IEnumerable<TResult>>)
                _cache.GetOrAdd(queryModel, queryCompiler);
        }
    }
}
