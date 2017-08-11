// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a relational database function in an <see cref="IModel" />.
    /// </summary>
    public interface IDbFunction
    {
        /// <summary>
        ///     The name of the function in the database.
        /// </summary>
        string FunctionName { get; }

        /// <summary>
        ///     The schema of the function in the database.
        /// </summary>
        string Schema { get; }

        /// <summary>
        ///     The CLR method which maps to the function in the database.
        /// </summary>
        MethodInfo MethodInfo { get; }

        /// <summary>
        ///     A translation callback for performing custom translation of the method call into a SQL expression fragment.
        /// </summary>
        Func<IReadOnlyCollection<Expression>, Expression> Translation { get; }
    }
}
