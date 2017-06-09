// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a db function parameter in an <see cref="IDbFunction" />.
    /// </summary>
    public interface IDbFunctionParameter
    {
        /// <summary>
        ///     The name of the parameter on the .Net method.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The index of the parameter on the mapped datastore method.
        /// </summary>
        int Index { get; }

        /// <summary>
        ///     The .Net parameter type. 
        /// </summary>
        Type ParameterType { get; }
    }
}
