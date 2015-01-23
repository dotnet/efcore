using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Query
{
    public interface ICompiledQueryCache
    {
        Func<QueryContext, IEnumerable<TResult>> GetOrAdd<TResult>(
            [NotNull] QueryModel queryModel,
            [NotNull] Func<QueryModel, Func<QueryContext, IEnumerable<TResult>>> queryCompiler);
    }
}