// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryTranslationPostprocessor
    {
        public QueryTranslationPostprocessor(QueryTranslationPostprocessorDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        protected virtual QueryTranslationPostprocessorDependencies Dependencies { get; }

        public virtual Expression Process(Expression query)
        {
            return query;
        }
    }
}
