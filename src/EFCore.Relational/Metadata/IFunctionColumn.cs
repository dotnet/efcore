// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a column in a table-valued function.
    /// </summary>
    public interface IFunctionColumn : IColumnBase
    {
        /// <summary>
        ///     Gets the containing function.
        /// </summary>
        IStoreFunction Function { get; }

        /// <summary>
        ///     Gets the property mappings.
        /// </summary>
        new IEnumerable<IFunctionColumnMapping> PropertyMappings { get; }
    }
}
