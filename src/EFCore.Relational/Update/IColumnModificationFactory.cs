// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         A ColumnModification factory.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public interface IColumnModificationFactory
    {
        /// <summary>
        ///     Creates a new <see cref="ColumnModification" /> instance.
        /// </summary>
        /// <param name="columnModificationParameters"> Creation parameters. </param>
        /// <returns> A new instance of ColumnModification. </returns>
        ColumnModification CreateColumnModification(in ColumnModificationParameters columnModificationParameters);
    }
}
