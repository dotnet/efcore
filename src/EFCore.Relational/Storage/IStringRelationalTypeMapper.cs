// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Maps string property types to their corresponding relational database types.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    [Obsolete("Use IRelationalTypeMappingSource.")]
    public interface IStringRelationalTypeMapper
    {
        /// <summary>
        ///     Gets the relational database type for a string property.
        /// </summary>
        /// <param name="unicode"> A value indicating whether the property should handle Unicode data or not. </param>
        /// <param name="keyOrIndex"> A value indicating whether the property is part of a key or not. </param>
        /// <param name="maxLength"> The maximum length of data the property is configured to store, or null if no maximum is configured. </param>
        /// <returns> The type mapping to be used. </returns>
        RelationalTypeMapping FindMapping(bool unicode, bool keyOrIndex, int? maxLength);
    }
}
