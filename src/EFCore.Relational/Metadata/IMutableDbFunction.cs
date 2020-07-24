// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a relational database function in an <see cref="IMutableModel" /> in
    ///     the a form that can be mutated while the model is being built.
    /// </summary>
    public interface IMutableDbFunction : IMutableAnnotatable, IDbFunction
    {
        /// <summary>
        ///     Gets or sets the name of the function in the database.
        /// </summary>
        new string Name { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Gets or sets the schema of the function in the database.
        /// </summary>
        new string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Gets or sets the value indicating whether the database function is built-in or not.
        /// </summary>
        new bool IsBuiltIn { get; set; }

        /// <summary>
        ///     Gets or sets the value indicating whether the database function can return null value or not.
        /// </summary>
        new bool IsNullable { get; set; }

        /// <summary>
        ///     Gets or sets the store type of the function in the database.
        /// </summary>
        new string StoreType { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Gets or sets the type mapping of the function in the database.
        /// </summary>
        new RelationalTypeMapping TypeMapping { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Gets the <see cref="IMutableModel" /> in which this function is defined.
        /// </summary>
        new IMutableModel Model { get; }

        /// <summary>
        ///     Gets the parameters for this function
        /// </summary>
        new IReadOnlyList<IMutableDbFunctionParameter> Parameters { get; }

        /// <summary>
        ///     Gets or sets the translation callback for performing custom translation of the method call into a SQL expression fragment.
        /// </summary>
        new Func<IReadOnlyCollection<SqlExpression>, SqlExpression> Translation { get; [param: CanBeNull] set; }
    }
}
