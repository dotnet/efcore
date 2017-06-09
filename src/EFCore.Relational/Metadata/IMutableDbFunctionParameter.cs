// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a db function parameter in an <see cref="IDbFunction" />.
    /// </summary>
    public interface IMutableDbFunctionParameter : IDbFunctionParameter
    {
        /// <summary>
        ///     The name of the parameter on the .Net method.
        /// </summary>
        new string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The index of the parameter on the mapped datastore method.
        /// </summary>
        new int Index { get; set; }

        /// <summary>
        ///     The .Net parameter type. 
        /// </summary>
        new Type ParameterType { get; [param: NotNull] set; }
    }
}
