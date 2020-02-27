// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     An interface to identify FromSql query roots in LINQ.
    /// </summary>
    public interface IFromSqlQueryable : IEntityQueryable
    {
        /// <summary>
        ///     Return Sql used to get data for this query root.
        /// </summary>
        string Sql { get; }

        /// <summary>
        ///     Return arguments for the Sql.
        /// </summary>
        Expression Argument { get; }
    }
}
