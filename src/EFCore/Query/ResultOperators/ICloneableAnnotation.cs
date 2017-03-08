// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ResultOperators
{
    /// <summary>
    ///     A query annotation that can be cloned.
    /// </summary>
    public interface ICloneableQueryAnnotation : IQueryAnnotation
    {
        /// <summary>
        ///     Clones this annotation.
        /// </summary>
        /// <param name="querySource">The new query source.</param>
        /// <param name="queryModel">The new query model.</param>
        /// <returns>The cloned annotation.</returns>
        ICloneableQueryAnnotation Clone([NotNull] IQuerySource querySource, [NotNull] QueryModel queryModel);
    }
}
