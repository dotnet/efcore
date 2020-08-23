// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a function in the database.
    /// </summary>
    public interface IStoreFunction : ITableBase
    {
        /// <summary>
        ///     Gets the associated <see cref="IDbFunction" />s.
        /// </summary>
        IEnumerable<IDbFunction> DbFunctions { get; }

        /// <summary>
        ///     Gets the value indicating whether the database function is built-in.
        /// </summary>
        bool IsBuiltIn { get; }

        /// <summary>
        ///     Gets the parameters for this function.
        /// </summary>
        IEnumerable<IStoreFunctionParameter> Parameters { get; }

        /// <summary>
        ///     Gets the scalar return type.
        /// </summary>
        string ReturnType { get; }

        /// <summary>
        ///     Gets the entity type mappings for the returned row set.
        /// </summary>
        new IEnumerable<IFunctionMapping> EntityTypeMappings { get; }

        /// <summary>
        ///     Gets the columns defined for the returned row set.
        /// </summary>
        new IEnumerable<IFunctionColumn> Columns { get; }

        /// <summary>
        ///     Gets the column with the given name. Returns <see langword="null" />
        ///     if no column with the given name is defined for the returned row set.
        /// </summary>
        new IFunctionColumn FindColumn([NotNull] string name);

        /// <summary>
        ///     Gets the column mapped to the given property. Returns <see langword="null" /> if no column is mapped to the given property.
        /// </summary>
        new IFunctionColumn FindColumn([NotNull] IProperty property);
    }
}
