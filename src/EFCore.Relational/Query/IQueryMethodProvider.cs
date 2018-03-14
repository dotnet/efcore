// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Provides reflection objects for late-binding to relational query operations.
    /// </summary>
    public interface IQueryMethodProvider
    {
        /// <summary>
        ///     Gets the group join method.
        /// </summary>
        /// <value>
        ///     The group join method.
        /// </value>
        MethodInfo GroupJoinMethod { get; }

        /// <summary>
        ///     Gets the group by method.
        /// </summary>
        /// <value>
        ///     The group by method.
        /// </value>
        MethodInfo GroupByMethod { get; }

        /// <summary>
        ///     Gets the shaped query method.
        /// </summary>
        /// <value>
        ///     The shaped query method.
        /// </value>
        MethodInfo ShapedQueryMethod { get; }

        /// <summary>
        ///     Gets the default if empty shaped query method.
        /// </summary>
        /// <value>
        ///     The default if empty shaped query method.
        /// </value>
        MethodInfo DefaultIfEmptyShapedQueryMethod { get; }

        /// <summary>
        ///     Gets the query method.
        /// </summary>
        /// <value>
        ///     The query method.
        /// </value>
        MethodInfo QueryMethod { get; }

        /// <summary>
        ///     Gets the get result method.
        /// </summary>
        /// <value>
        ///     The get result method.
        /// </value>
        MethodInfo GetResultMethod { get; }

        /// <summary>
        ///     Gets the inject parameters method.
        /// </summary>
        /// <value>
        ///     The pre execute method.
        /// </value>
        MethodInfo InjectParametersMethod { get; }

        /// <summary>
        ///     Gets the fast query method.
        /// </summary>
        /// <value>
        ///     The fast query method.
        /// </value>
        MethodInfo FastQueryMethod { get; }
    }
}
