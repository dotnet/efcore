// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a function parameter.
    /// </summary>
    public interface IDbFunctionParameter : IReadOnlyDbFunctionParameter, IAnnotatable
    {
        /// <summary>
        ///     Gets the function to which this parameter belongs.
        /// </summary>
        new IDbFunction Function { get; }

        /// <summary>
        ///     Gets the store type of this parameter.
        /// </summary>
        new string StoreType { get; }

        /// <summary>
        ///     Gets the <see cref="RelationalTypeMapping" /> for this parameter.
        /// </summary>
        new RelationalTypeMapping TypeMapping { get; }

        /// <summary>
        ///     Gets the associated <see cref="IStoreFunctionParameter" />.
        /// </summary>
        IStoreFunctionParameter StoreFunctionParameter { get; }
    }
}
