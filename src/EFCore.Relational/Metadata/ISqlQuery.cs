// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a SQL query string.
    /// </summary>
    public interface ISqlQuery : ITableBase
    {
        /// <summary>
        ///     Gets the entity type mappings.
        /// </summary>
        new IEnumerable<ISqlQueryMapping> EntityTypeMappings { get; }

        /// <summary>
        ///     Gets the columns defined for this query.
        /// </summary>
        new IEnumerable<ISqlQueryColumn> Columns { get; }

        /// <summary>
        ///     Gets the column with the given name. Returns <see langword="null" /> if no column with the given name is defined.
        /// </summary>
        new ISqlQueryColumn FindColumn([NotNull] string name);

        /// <summary>
        ///     Gets the column mapped to the given property. Returns <see langword="null" /> if no column is mapped to the given property.
        /// </summary>
        new ISqlQueryColumn FindColumn([NotNull] IProperty property);

        /// <summary>
        ///     Gets the SQL query string.
        /// </summary>
        public string Sql { get; }
    }
}
