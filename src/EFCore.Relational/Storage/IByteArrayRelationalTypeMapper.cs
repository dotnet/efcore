// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Determines the type mapping to use for byte array properties.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    [Obsolete("Use IRelationalTypeMappingSource.")]
    public interface IByteArrayRelationalTypeMapper
    {
        /// <summary>
        ///     Gets the mapping for a property.
        /// </summary>
        /// <param name="rowVersion">
        ///     A value indicating whether the property is being used as a row version.
        /// </param>
        /// <param name="keyOrIndex">
        ///     A value indicating whether the property is being used as a key and/or index.
        /// </param>
        /// <param name="size">
        ///     The configured length of the property, or null if it is unbounded.
        /// </param>
        /// <returns> The mapping to be used for the property. </returns>
        RelationalTypeMapping FindMapping(bool rowVersion, bool keyOrIndex, int? size);
    }
}
