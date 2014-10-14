// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class AsyncQueryMethodProvider : IQueryMethodProvider
    {
        public virtual MethodInfo IncludeCollectionMethod
        {
            get { return _includeCollectionMethodInfo; }
        }

        private static readonly MethodInfo _includeCollectionMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod("_IncludeCollection");

        [UsedImplicitly]
        private static IAsyncEnumerable<TResult> _IncludeCollection<TResult>(
            QueryContext queryContext,
            IAsyncEnumerable<TResult> source,
            INavigation navigation,
            IAsyncEnumerable<IValueReader> relatedValueReaders,
            Func<TResult, object> accessorLambda)
        {
            return new IncludeCollectionAsyncEnumerable<TResult>(
                queryContext,
                source,
                navigation,
                relatedValueReaders,
                accessorLambda);
        }

        public virtual MethodInfo QueryMethod
        {
            get { return _queryMethodInfo; }
        }

        private static readonly MethodInfo _queryMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod("_Query");

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _Query<T>(
            QueryContext queryContext, CommandBuilder commandBuilder, Func<DbDataReader, T> shaper)
        {
            return new AsyncQueryingEnumerable<T>(
                ((RelationalQueryContext)queryContext).Connection,
                commandBuilder,
                shaper,
                queryContext.Logger);
        }
    }
}
