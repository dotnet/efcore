// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a mutable database function parameter in an <see cref="IMutableDbFunction" />.
    /// </summary>
    public interface IMutableDbFunctionParameter : IDbFunctionParameter
    {
        /// <summary>
        ///     The <see cref="IMutableDbFunction" /> to which this parameter belongs.
        /// </summary>
        new IMutableDbFunction Function { get; }

        /// <summary>
        ///     The store (database) type of this parameter.
        /// </summary>
        new string StoreType { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The <see cref="RelationalTypeMapping" /> for this parameter.
        /// </summary>
        new RelationalTypeMapping TypeMapping { get; [param: CanBeNull] set; }
    }
}
