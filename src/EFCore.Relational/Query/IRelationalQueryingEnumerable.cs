// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Data.Common;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Interface that can be implemented by a database provider's <see cref="IEnumerable" /> implementation to
    ///         provide the query string for debugging purposes.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IRelationalQueryingEnumerable : IQueryingEnumerable
    {
        /// <summary>
        ///     <para>
        ///         Creates a <see cref="DbCommand"/> set up to execute this query.
        ///     </para>
        ///     <para>
        ///         Warning: there is no guarantee that executing this command directly will result in the same behavior as if EF Core had
        ///         executed the command.
        ///     </para>
        ///     <para>
        ///         Note that DbCommand is an <see cref="IDisposable"/> object. The caller is responsible for disposing the returned
        ///         command.
        ///     </para>
        /// </summary>
        /// <returns> The newly created command. </returns>
        DbCommand CreateDbCommand();
    }
}
