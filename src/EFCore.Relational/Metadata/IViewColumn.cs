// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a column in a view.
    /// </summary>
    public interface IViewColumn : IColumnBase
    {
        /// <summary>
        ///     Gets the containing view.
        /// </summary>
        IView View { get; }

        /// <summary>
        ///     Gets the property mappings.
        /// </summary>
        new IEnumerable<IViewColumnMapping> PropertyMappings { get; }
    }
}
