// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         A parameter in an <see cref="IRelationalCommand" />. Note that this interface just represents a
    ///         placeholder for a parameter and not the actual value. This is because the same command can be
    ///         reused multiple times with different parameter values.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IRelationalParameter
    {
        /// <summary>
        ///     The name of the parameter.
        /// </summary>
        string InvariantName { get; }

        /// <summary>
        ///     Adds the parameter as a <see cref="DbParameter" /> to a <see cref="DbCommand" />.
        /// </summary>
        /// <param name="command"> The command to add the parameter to. </param>
        /// <param name="value"> The value to be assigned to the parameter. </param>
        void AddDbParameter([NotNull] DbCommand command, [CanBeNull] object value);

        /// <summary>
        ///     Adds the parameter as a <see cref="DbParameter" /> to a <see cref="DbCommand" />.
        /// </summary>
        /// <param name="command"> The command to add the parameter to. </param>
        /// <param name="parameterValues"> The map of parameter values </param>
        void AddDbParameter([NotNull] DbCommand command, [NotNull] IReadOnlyDictionary<string, object> parameterValues);
    }
}
