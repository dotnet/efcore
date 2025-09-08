// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGQueryCompilationContextFactory : IQueryCompilationContextFactory
    {
        private readonly QueryCompilationContextDependencies _dependencies;
        private readonly RelationalQueryCompilationContextDependencies _relationalDependencies;

        public XGQueryCompilationContextFactory(
            [NotNull] QueryCompilationContextDependencies dependencies,
            [NotNull] RelationalQueryCompilationContextDependencies relationalDependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
        }

        public virtual QueryCompilationContext Create(bool async)
            => new XGQueryCompilationContext(_dependencies, _relationalDependencies, async);

        public virtual QueryCompilationContext CreatePrecompiled(bool async, IReadOnlySet<string> nonNullableReferenceTypeParameters)
            => new XGQueryCompilationContext(
                _dependencies, _relationalDependencies, async, precompiling: true, nonNullableReferenceTypeParameters);
    }
}
