// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGQueryCompilationContext : RelationalQueryCompilationContext
    {
        public XGQueryCompilationContext(
            [NotNull] QueryCompilationContextDependencies dependencies,
            [NotNull] RelationalQueryCompilationContextDependencies relationalDependencies,
            bool async)
            : base(dependencies, relationalDependencies, async)
        {
        }

        public XGQueryCompilationContext(
            [NotNull] QueryCompilationContextDependencies dependencies,
            [NotNull] RelationalQueryCompilationContextDependencies relationalDependencies,
            bool async,
            bool precompiling,
            IReadOnlySet<string> nonNullableReferenceTypeParameters)
            : base(dependencies, relationalDependencies, async, precompiling, nonNullableReferenceTypeParameters)
        {
        }

        public override bool IsBuffering
            => base.IsBuffering ||
               QuerySplittingBehavior == Microsoft.EntityFrameworkCore.QuerySplittingBehavior.SplitQuery;

        /// <inheritdoc />
        public override bool SupportsPrecompiledQuery => false;
    }
}
