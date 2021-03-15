// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
