// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Builds a collection of <see cref="IRelationalParameter" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IRelationalParameterBuilder
    {
        /// <summary>
        ///     The collection of parameters.
        /// </summary>
        IReadOnlyList<IRelationalParameter> Parameters { get; }

        /// <summary>
        ///     Adds a parameter.
        /// </summary>
        /// <param name="invariantName">
        ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
        ///     placeholder for a parameter and not the actual value. This is because the same command can be
        ///     reused multiple times with different parameter values.
        /// </param>
        /// <param name="name">
        ///     The name to be used for the parameter when the command is executed against the database.
        /// </param>
        void AddParameter(
            [NotNull] string invariantName,
            [NotNull] string name);

        /// <summary>
        ///     Adds a parameter.
        /// </summary>
        /// <param name="invariantName">
        ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
        ///     placeholder for a parameter and not the actual value. This is because the same command can be
        ///     reused multiple times with different parameter values.
        /// </param>
        /// <param name="name">
        ///     The name to be used for the parameter when the command is executed against the database.
        /// </param>
        /// <param name="typeMapping">
        ///     The type mapping for the property that values for this parameter will come from.
        /// </param>
        /// <param name="nullable">
        ///     A value indicating whether the parameter can contain null values.
        /// </param>
        void AddParameter(
            [NotNull] string invariantName,
            [NotNull] string name,
            [NotNull] RelationalTypeMapping typeMapping,
            bool nullable);

        /// <summary>
        ///     Adds a parameter.
        /// </summary>
        /// <param name="invariantName">
        ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
        ///     placeholder for a parameter and not the actual value. This is because the same command can be
        ///     reused multiple times with different parameter values.
        /// </param>
        /// <param name="name">
        ///     The name to be used for the parameter when the command is executed against the database.
        /// </param>
        /// <param name="property"> The property that the type for this parameter will come from. </param>
        void AddParameter(
            [NotNull] string invariantName,
            [NotNull] string name,
            [NotNull] IProperty property);

        /// <summary>
        ///     Adds a parameter that is ultimately represented as multiple <see cref="DbParameter" />s in the
        ///     final command.
        /// </summary>
        /// <param name="invariantName">
        ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
        ///     placeholder for a parameter and not the actual value. This is because the same command can be
        ///     reused multiple times with different parameter values.
        /// </param>
        /// <param name="buildAction">
        ///     The action to add the multiple parameters that this placeholder represents.
        /// </param>
        void AddCompositeParameter(
            [NotNull] string invariantName,
            [NotNull] Action<IRelationalParameterBuilder> buildAction);

        /// <summary>
        ///     Adds a parameter.
        /// </summary>
        /// <param name="invariantName">
        ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
        ///     placeholder for a parameter and not the actual value. This is because the same command can be
        ///     reused multiple times with different parameter values.
        /// </param>
        /// <param name="name">
        ///     The name to be used for the parameter when the command is executed against the database.
        /// </param>
        /// <param name="property">
        ///     The property that values for this parameter will come from.
        /// </param>
        void AddPropertyParameter(
            [NotNull] string invariantName,
            [NotNull] string name,
            [NotNull] IProperty property);

        /// <summary>
        ///     Adds a parameter.
        /// </summary>
        /// <param name="invariantName">
        ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
        ///     placeholder for a parameter and not the actual value. This is because the same command can be
        ///     reused multiple times with different parameter values.
        /// </param>
        /// <param name="dbParameter">
        ///     The DbParameter being added.
        /// </param>
        void AddRawParameter(
            [NotNull] string invariantName,
            [NotNull] DbParameter dbParameter);
    }
}
