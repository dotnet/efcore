// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query
{
    public interface ILinqOperatorProvider
    {
        MethodInfo TrackEntities { get; }
        MethodInfo TrackGroupedEntities { get; }
        MethodInfo InterceptExceptions { get; }

        MethodInfo SelectMany { get; }
        MethodInfo Join { get; }
        MethodInfo GroupJoin { get; }
        MethodInfo Select { get; }
        MethodInfo OrderBy { get; }
        MethodInfo ThenBy { get; }
        MethodInfo Where { get; }
        MethodInfo Any { get; }
        MethodInfo All { get; }
        MethodInfo Cast { get; }
        MethodInfo CastWrappedResult { get; }
        MethodInfo Count { get; }
        MethodInfo Contains { get; }
        MethodInfo DefaultIfEmpty { get; }
        MethodInfo DefaultIfEmptyArg { get; }
        MethodInfo Distinct { get; }
        MethodInfo First { get; }
        MethodInfo FirstOrDefault { get; }
        MethodInfo GroupBy { get; }
        MethodInfo Last { get; }
        MethodInfo LastOrDefault { get; }
        MethodInfo LongCount { get; }
        MethodInfo Single { get; }
        MethodInfo SingleOrDefault { get; }
        MethodInfo Skip { get; }
        MethodInfo Take { get; }
        MethodInfo OfType { get; }
        MethodInfo OfTypeWrappedResult { get; }

        MethodInfo UnwrapQueryResults { get; }
        MethodInfo UnwrapGroupedQueryResults { get; }
        MethodInfo UnwrapGrouping { get; }
        MethodInfo RewrapQueryResults { get; }

        MethodInfo ToSequence { get; }
        MethodInfo ToOrdered { get; }
        MethodInfo ToEnumerable { get; }
        MethodInfo ToQueryable { get; }
        Expression AdjustSequenceType([NotNull] Expression expression);

        MethodInfo GetAggregateMethod([NotNull] string methodName, [NotNull] Type elementType);
    }
}
