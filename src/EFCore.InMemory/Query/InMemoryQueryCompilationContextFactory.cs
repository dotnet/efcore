// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query.Internal;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query
{

    /// <summary>
    ///     A relational factory for instances of <see cref="QueryCompilationContext" />.
    /// </summary>
    public class InMemoryQueryCompilationContextFactory : QueryCompilationContextFactory
    {
        public InMemoryQueryCompilationContextFactory([NotNull] QueryCompilationContextDependencies dependencies)
            : base(dependencies)
        {
        }

        public override QueryCompilationContext Create(bool async)
            => new InMemoryQueryCompilationContext(
                Dependencies,
                async ? (ILinqOperatorProvider)new AsyncLinqOperatorProvider() : new LinqOperatorProvider(),
                TrackQueryResults);
    }
}
