// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query
{
    public interface ILinqOperatorProvider
    {
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
        MethodInfo InterceptExceptions { get; }
        MethodInfo OfType { get; }

        MethodInfo ToSequence { get; }
        MethodInfo ToOrdered { get; }
        MethodInfo ToEnumerable { get; }
        MethodInfo ToQueryable { get; }

        MethodInfo TrackEntities { get; }
        MethodInfo TrackGroupedEntities { get; }

        MethodInfo GetAggregateMethod([NotNull] string methodName, [NotNull] Type elementType);
        Expression AdjustSequenceType([NotNull] Expression expression);
    }
}
