// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable 0169 

using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Storage;
using Remotion.Linq.Clauses;

// ReSharper disable AssignNullToNotNullAttribute

// ReSharper disable PossibleNullReferenceException

// ReSharper disable InconsistentNaming

namespace Microsoft.Data.Entity.Utilities
{
    internal class ImplyLinqOperator<T>
    {
        public void ImplyMethods()
        {
            LinqOperatorProvider._Where<T>(null, null);

            AsyncLinqOperatorProvider._Where<T>(null, null);
        }
    }

    internal class ImplyLinqOperator<T1, T2>
    {
        public void ImplyMethods()
        {
            ((IDatabase)null).CompileQuery<T2>(null); // for projections
            ((IDatabase)null).CompileAsyncQuery<T2>(null);

            LinqOperatorProvider._OrderBy<T1, T2>(null, null, OrderingDirection.Asc);
            AsyncLinqOperatorProvider._OrderBy<T1, T2>(null, null, OrderingDirection.Asc);

            LinqOperatorProvider._TrackGroupedEntities<T1, T2, object>(null, null, null, null);
            AsyncLinqOperatorProvider._TrackGroupedEntities<T1, T2, object>(null, null, null, null);

            AsyncLinqOperatorProvider._Select<T1, T2>(null, null);
            LinqOperatorProvider._Select<T1, T2>(null, null);

            AsyncLinqOperatorProvider._ThenBy<T1, T2>(null, null, OrderingDirection.Asc);
            LinqOperatorProvider._ThenBy<T1, T2>(null, null, OrderingDirection.Asc);
        }
    }

    internal class ImplyLinqOperator<T1, T2, T3>
    {
        public void ImplyMethods()
        {
            LinqOperatorProvider._GroupBy<T1, T2, T3>(null, null, null);
            AsyncLinqOperatorProvider._GroupBy<T1, T2, T3>(null, null, null);
        }
    }

    internal class ImplyLinqOperator<T1, T2, T3, T4>
    {
        public void ImplyMethods()
        {
            LinqOperatorProvider._Join<T1, T2, T3, T4>(null, null, null, null, null);
            AsyncLinqOperatorProvider._Join<T1, T2, T3, T4>(null, null, null, null, null);

            LinqOperatorProvider._GroupJoin<T1, T2, T3, T4>(null, null, null, null, null);
            AsyncLinqOperatorProvider._GroupJoin<T1, T2, T3, T4>(null, null, null, null, null);
        }
    }
}
