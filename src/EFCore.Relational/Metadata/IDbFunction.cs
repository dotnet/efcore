// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a relational database function in an <see cref="IModel" />.
    /// </summary>
    public interface IDbFunction : IAnnotatable
    {
        /// <summary>
        ///     Gets the name of the function in the database.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the schema of the function in the database.
        /// </summary>
        string Schema { get; }

        /// <summary>
        ///     Gets the name of the function in the model.
        /// </summary>
        string ModelName { get; }

        /// <summary>
        ///     Gets the <see cref="IModel" /> in which this function is defined.
        /// </summary>
        IModel Model { get; }

        /// <summary>
        ///     Gets the CLR method which maps to the function in the database.
        /// </summary>
        MethodInfo MethodInfo { get; }

        /// <summary>
        ///     Gets the value indicating whether the database function is built-in.
        /// </summary>
        bool IsBuiltIn { get; }

        /// <summary>
        ///     Gets the value indicating whether this function returns scalar value.
        /// </summary>
        bool IsScalar { get; }

        /// <summary>
        ///     Gets the value indicating whether this function is an aggregate function.
        /// </summary>
        bool IsAggregate { get; }

        /// <summary>
        ///     Gets the value indicating whether the database function can return null.
        /// </summary>
        bool IsNullable { get; }

        /// <summary>
        ///     Gets the configured store type string.
        /// </summary>
        string StoreType { get; }

        /// <summary>
        ///     Gets the returned CLR type.
        /// </summary>
        Type ReturnType { get; }

        /// <summary>
        ///     Gets the type mapping for the function's return type.
        /// </summary>
        RelationalTypeMapping TypeMapping { get; }

        /// <summary>
        ///     Gets the parameters for this function.
        /// </summary>
        IReadOnlyList<IDbFunctionParameter> Parameters { get; }

        /// <summary>
        ///     Gets the translation callback for performing custom translation of the method call into a SQL expression fragment.
        /// </summary>
        Func<IReadOnlyCollection<SqlExpression>, SqlExpression> Translation { get; }

        /// <summary>
        ///     Gets the associated <see cref="IStoreFunction" />.
        /// </summary>
        IStoreFunction StoreFunction { get; }
    }
}
