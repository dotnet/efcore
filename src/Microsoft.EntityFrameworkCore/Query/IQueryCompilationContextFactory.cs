// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Factory for <see cref="QueryCompilationContext" /> instances.
    /// </summary>
    public interface IQueryCompilationContextFactory
    {
        /// <summary>
        ///     Creates a new QueryCompilationContext.
        /// </summary>
        /// <param name="async"> true if the query will be executed asynchronously. </param>
        /// <returns>
        ///     A <see cref="QueryCompilationContext" /> instance.
        /// </returns>
        QueryCompilationContext Create(bool async);
    }
}
