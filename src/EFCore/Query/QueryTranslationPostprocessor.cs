// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryTranslationPostprocessor
    {
        public QueryTranslationPostprocessor([NotNull] QueryTranslationPostprocessorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        protected virtual QueryTranslationPostprocessorDependencies Dependencies { get; }

        public virtual Expression Process([NotNull] Expression query)
        {
            Check.NotNull(query, nameof(query));

            return query;
        }
    }
}
