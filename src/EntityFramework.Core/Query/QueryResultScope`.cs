// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public class QueryResultScope<TResult> : QueryResultScope
    {
        public readonly TResult Result;

        public QueryResultScope(
            [NotNull] IQuerySource querySource,
            [NotNull] TResult result,
            [CanBeNull] QueryResultScope parentScope)
            : base(querySource, parentScope)
        {
            Result = result;
        }

        public override object UntypedResult => Result;
    }
}
