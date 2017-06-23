// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a db function in an <see cref="IModel" />.
    /// </summary>
    public interface IDbFunction
    {
        /// <summary>
        ///     The schema where the function lives in the underlying database.
        /// </summary>
        string Schema { get; }

        /// <summary>
        ///     The name of the function in the underlying database.
        /// </summary>
        string FunctionName { get; }

        /// <summary>
        ///     The method which maps to the function in the underlying database.
        /// </summary>
        MethodInfo MethodInfo { get; }

        /// <summary>
        ///    A method for converting a method call into sql.
        /// </summary>
        Func<IReadOnlyCollection<Expression>, Expression> Translation { get; }
    }
}
