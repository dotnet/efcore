// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public class QuerySourceScope<TResult> : QuerySourceScope
    {
        public readonly TResult Result;

        public QuerySourceScope(
            [NotNull] IQuerySource querySource,
            [NotNull] TResult result,
            [CanBeNull] QuerySourceScope parentScope)
            : base(querySource, parentScope)
        {
            Result = result;
        }

        public override object UntypedResult => Result;
    }
}
