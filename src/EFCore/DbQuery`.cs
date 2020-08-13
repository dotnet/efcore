// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     <para>
    ///         A <see cref="DbQuery{TQuery}" /> can be used to query instances of <typeparamref name="TQuery" />.
    ///         LINQ queries against a <see cref="DbQuery{TQuery}" /> will be translated into queries against the database.
    ///     </para>
    ///     <para>
    ///         The results of a LINQ query against a <see cref="DbQuery{TQuery}" /> will contain the results
    ///         returned from the database and may not reflect changes made in the context that have not
    ///         been persisted to the database. For example, the results will not contain newly added views
    ///         and may still contain views that are marked for deletion.
    ///     </para>
    ///     <para>
    ///         Depending on the database being used, some parts of a LINQ query against a <see cref="DbQuery{TQuery}" />
    ///         may be evaluated in memory rather than being translated into a database query.
    ///     </para>
    ///     <para>
    ///         <see cref="DbQuery{TQuery}" /> objects are usually obtained from a <see cref="DbQuery{TQuery}" />
    ///         property on a derived <see cref="DbContext" /> or from the <see cref="DbContext.Query{TQuery}" />
    ///         method.
    ///     </para>
    /// </summary>
    /// <typeparam name="TQuery"> The type of view being operated on by this view. </typeparam>
    [Obsolete("Use DbSet<T> instead")]
    public abstract class DbQuery<TQuery> : DbSet<TQuery>
        where TQuery : class
    {
    }
}
