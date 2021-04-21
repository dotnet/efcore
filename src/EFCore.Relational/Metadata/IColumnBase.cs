// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        ///     Gets the column name.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the column type.
        /// </summary>
        string StoreType { get; }

        /// <summary>
        ///     Gets the value indicating whether the column can contain NULL.
        /// </summary>
        bool IsNullable { get; }

        /// <summary>
        ///     Gets the containing table-like object.
        /// </summary>
        ITableBase Table { get; }

        /// <summary>
        ///     Gets the property mappings.
        /// </summary>
        IEnumerable<IColumnMappingBase> PropertyMappings { get; }
    }
}
