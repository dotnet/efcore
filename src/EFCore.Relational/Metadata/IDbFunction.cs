// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a db function in an <see cref="IModel" />.
    /// </summary>
    public interface IDbFunction
    {
        /// <summary>
        ///     The schema where the function lives in the underlying datastore.
        /// </summary>
        string Schema { get; }

        /// <summary>
        ///     The name of the function in the underlying datastore.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The list of parameters which are passed to the underlying datastores function.
        /// </summary>
        IReadOnlyList<DbFunctionParameter> Parameters { get; }

        /// <summary>
        ///     The .Net method which maps to the function in the underlying datastore
        /// </summary>
        MethodInfo MethodInfo { get; }

        /// <summary>
        ///     The return type of the mapped .Net method
        /// </summary>
        Type ReturnType { get; }

        /// <summary>
        ///    A translate callback for converting a method call into a sql function
        /// </summary>
        Func<IReadOnlyCollection<Expression>, IDbFunction, SqlFunctionExpression> TranslateCallback { get; }
    }
}
