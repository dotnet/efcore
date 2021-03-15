// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Implemented by database providers to generate the query string for <see cref="EntityFrameworkQueryableExtensions.ToQueryString" />.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IRelationalQueryStringFactory
    {
        /// <summary>
        ///     Returns a formatted query string for the given command.
        /// </summary>
        /// <param name="command"> The command that represents the query. </param>
        /// <returns> The formatted string. </returns>
        string Create([NotNull] DbCommand command);
    }
}
