// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a column in a table.
    /// </summary>
    public interface IColumn : IColumnBase
    {
        /// <summary>
        ///     The containing table.
        /// </summary>
        new ITable Table { get; }

        /// <summary>
        ///     The property mappings.
        /// </summary>
        new IEnumerable<IColumnMapping> PropertyMappings { get; }
    }
}
