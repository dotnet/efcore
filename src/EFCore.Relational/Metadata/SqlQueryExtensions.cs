// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Extension methods for <see cref="ISqlQuery" />.
    /// </summary>
    public static class SqlQueryExtensions
    {
        /// <summary>
        ///     Gets the name used for the <see cref="ISqlQuery" /> mapped using
        ///     <see cref="M:RelationalEntityTypeBuilderExtensions.ToSqlQuery" />.
        /// </summary>
        [Obsolete("Use RelationalEntityTypeExtensions.DefaultQueryNameBase")]
        public static readonly string DefaultQueryNameBase = RelationalEntityTypeExtensions.DefaultQueryNameBase;
    }
}
