// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ResultOperators
{
    /// <summary>
    ///     Represents an annotation on a query.
    /// </summary>
    public interface IQueryAnnotation
    {
        /// <summary>
        ///     Gets the query source.
        /// </summary>
        /// <value>
        ///     The query source.
        /// </value>
        IQuerySource QuerySource { get; [param: NotNull] set; }

        /// <summary>
        ///     Gets the query model.
        /// </summary>
        /// <value>
        ///     The query model.
        /// </value>
        QueryModel QueryModel { get; [param: NotNull] set; }
    }
}
