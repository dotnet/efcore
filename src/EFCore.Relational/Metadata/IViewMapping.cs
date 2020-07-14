// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents entity type mapping to a view.
    /// </summary>
    public interface IViewMapping : ITableMappingBase
    {
        /// <summary>
        ///     Gets the target view.
        /// </summary>
        IView View { get; }

        /// <summary>
        ///     Gets the properties mapped to columns on the target view.
        /// </summary>
        new IEnumerable<IViewColumnMapping> ColumnMappings { get; }
    }
}
