// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Indicates which values should be appended to the store type name.
    /// </summary>
    public enum StoreTypePostfix
    {
        /// <summary>
        ///     Append nothing.
        /// </summary>
        None,

        /// <summary>
        ///     Append only the size.
        /// </summary>
        Size,

        /// <summary>
        ///     Append only the precision.
        /// </summary>
        Precision,

        /// <summary>
        ///     Append the precision and scale.
        /// </summary>
        PrecisionAndScale
    }
}
