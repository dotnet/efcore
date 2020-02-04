// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents property mapping to a column-like object.
    /// </summary>
    public interface IColumnMappingBase : IAnnotatable
    {
        /// <summary>
        ///     The mapped property.
        /// </summary>
        IProperty Property { get; }

        /// <summary>
        ///     The target column-like object.
        /// </summary>
        IColumnBase Column { get; }

        /// <summary>
        ///     The type mapping for the column-like object.
        /// </summary>
        RelationalTypeMapping TypeMapping { get; }

        /// <summary>
        ///     The containing table mapping.
        /// </summary>
        ITableMappingBase TableMapping { get; }
    }
}
