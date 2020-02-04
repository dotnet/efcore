// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a column-like object in a table-like object.
    /// </summary>
    public interface IColumnBase : IAnnotatable
    {
        /// <summary>
        ///     The column name.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The column type.
        /// </summary>
        string Type { get; }

        /// <summary>
        ///     Whether the column can contain NULL.
        /// </summary>
        bool IsNullable { get; }

        /// <summary>
        ///     The containing table-like object.
        /// </summary>
        ITableBase Table { get; }

        /// <summary>
        ///     The property mappings.
        /// </summary>
        IEnumerable<IColumnMappingBase> PropertyMappings { get; }
    }
}
