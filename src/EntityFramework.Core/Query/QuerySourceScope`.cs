// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public class QuerySourceScope<TResult>
        : QuerySourceScope, IEquatable<QuerySourceScope<TResult>>, IComparable<QuerySourceScope<TResult>>
    {
        public static implicit operator TResult([NotNull] QuerySourceScope<TResult> querySourceScope)
        {
            return querySourceScope.Result;
        }

        public readonly TResult Result;

        public QuerySourceScope(
            [NotNull] IQuerySource querySource,
            [CanBeNull] TResult result,
            [CanBeNull] QuerySourceScope parentScope,
            [CanBeNull] IValueReader valueReader)
            : base(querySource, parentScope, valueReader)
        {
            Result = result;
        }

        public override object UntypedResult => Result;

        public virtual bool Equals([NotNull] QuerySourceScope<TResult> other)
        {
            Debug.Assert(other != null);

            return Equals(Result, other.Result);
        }

        public virtual int CompareTo(QuerySourceScope<TResult> other)
        {
            return ((IComparable<TResult>)Result).CompareTo(other.Result);
        }

        public override bool Equals([NotNull] object obj)
        {
            Debug.Assert(obj != null);

            return Equals(Result, ((QuerySourceScope<TResult>)obj).Result);
        }

        public override int GetHashCode()
        {
            return Result?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Result?.ToString() ?? "(null)";
        }
    }
}
