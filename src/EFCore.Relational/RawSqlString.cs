// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     A string representing a raw SQL query. This type enables overload resolution between
    ///     the regular and interpolated <see cref="RelationalQueryableExtensions.FromSql{TEntity}(IQueryable{TEntity},RawSqlString,object[])" />
    ///     and <see cref="RelationalDatabaseFacadeExtensions.ExecuteSqlCommand(Infrastructure.DatabaseFacade,RawSqlString,object[])" />
    /// </summary>
    [Obsolete("For the execution of SQL queries using plain strings, use FromSqlRaw, ExecuteSqlRaw, or ExecuteSqlRawAsync instead. For the execution of SQL queries using interpolated string syntax to create parameters, use FromSqlInterpolated, ExecuteSqlInterpolated, or ExecuteSqlInterpolatedAsync instead.")]
    public readonly struct RawSqlString
    {
        /// <summary>
        ///     Implicitly converts a <see cref="string" /> to a <see cref="RawSqlString" />
        /// </summary>
        /// <param name="s"> The string. </param>
        public static implicit operator RawSqlString([NotNull] string s) => new RawSqlString(s);

        /// <summary>
        ///     Implicitly converts a <see cref="FormattableString" /> to a <see cref="RawSqlString" />
        /// </summary>
        /// <param name="fs"> The string format. </param>
        public static implicit operator RawSqlString([NotNull] FormattableString fs) => default;

        /// <summary>
        ///     Constructs a <see cref="RawSqlString" /> from a <see cref="string" />
        /// </summary>
        /// <param name="s"> The string. </param>
        public RawSqlString([NotNull] string s) => Format = s;

        /// <summary>
        ///     The string format.
        /// </summary>
        public string Format { get; }
    }
}
