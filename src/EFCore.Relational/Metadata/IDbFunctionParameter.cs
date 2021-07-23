// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a function parameter.
    /// </summary>
    public interface IDbFunctionParameter : IReadOnlyDbFunctionParameter, IAnnotatable
    {
        /// <summary>
        ///     Gets the store type of this parameter.
        /// </summary>
        new string StoreType { get; }

        /// <summary>
        ///     Gets the function to which this parameter belongs.
        /// </summary>
        new IDbFunction Function { get; }

        /// <summary>
        ///     Gets the associated <see cref="IStoreFunctionParameter" />.
        /// </summary>
        IStoreFunctionParameter StoreFunctionParameter { get; }
    }
}
