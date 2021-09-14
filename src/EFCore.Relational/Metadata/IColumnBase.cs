// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a column-like object in a table-like object.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information.
    /// </remarks>
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
