// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a column in a SQL query.
    /// </summary>
    public interface ISqlQueryColumn : IColumnBase
    {
        /// <summary>
        ///     Gets the containing SQL query.
        /// </summary>
        ISqlQuery SqlQuery { get; }

        /// <summary>
        ///     Gets the property mappings.
        /// </summary>
        new IEnumerable<ISqlQueryColumnMapping> PropertyMappings { get; }
    }
}
