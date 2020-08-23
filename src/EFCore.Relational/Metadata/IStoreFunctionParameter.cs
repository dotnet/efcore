// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a <see cref="IStoreFunction" /> parameter.
    /// </summary>
    public interface IStoreFunctionParameter : IAnnotatable
    {
        /// <summary>
        ///     Gets the <see cref="IStoreFunction" /> to which this parameter belongs.
        /// </summary>
        IStoreFunction Function { get; }

        /// <summary>
        ///     Gets the associated <see cref="IDbFunctionParameter" />s.
        /// </summary>
        IEnumerable<IDbFunctionParameter> DbFunctionParameters { get; }

        /// <summary>
        ///     Gets the parameter name.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the store type of this parameter.
        /// </summary>
        string Type { get; }
    }
}
