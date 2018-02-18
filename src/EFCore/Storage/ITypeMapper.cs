// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Maps .NET types to their corresponding provider database types.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    [Obsolete("Use ITypeMappingSource.")]
    public interface ITypeMapper
    {
        /// <summary>
        ///     Gets a value indicating whether the given .NET type is mapped.
        /// </summary>
        /// <param name="clrType"> The .NET type. </param>
        /// <returns> True if the type can be mapped; otherwise false. </returns>
        bool IsTypeMapped([NotNull] Type clrType);
    }
}
