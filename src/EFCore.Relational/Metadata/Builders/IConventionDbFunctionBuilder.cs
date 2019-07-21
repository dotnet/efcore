// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API for configuring a <see cref="IConventionDbFunction" />.
    /// </summary>
    public interface IConventionDbFunctionBuilder
    {
        /// <summary>
        ///     The function being configured.
        /// </summary>
        IConventionDbFunction Metadata { get; }

        /// <summary>
        ///     Sets the name of the database function.
        /// </summary>
        /// <param name="name"> The name of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionDbFunctionBuilder HasName([CanBeNull] string name, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given name can be set for the database function.
        /// </summary>
        /// <param name="name"> The name of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given name can be set for the database function. </returns>
        bool CanSetName([CanBeNull] string name, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the schema of the database function.
        /// </summary>
        /// <param name="schema"> The schema of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionDbFunctionBuilder HasSchema([CanBeNull] string schema, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given schema can be set for the database function.
        /// </summary>
        /// <param name="schema"> The schema of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given schema can be set for the database function. </returns>
        bool CanSetSchema([CanBeNull] string schema, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the store type of the function in the database.
        /// </summary>
        /// <param name="storeType"> The store type of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionDbFunctionBuilder HasStoreType([CanBeNull] string storeType, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given store type can be set for the database function.
        /// </summary>
        /// <param name="storeType"> The store type of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given store type can be set for the database function. </returns>
        bool CanSetStoreType([CanBeNull] string storeType, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the return type mapping of the database function.
        /// </summary>
        /// <param name="typeMapping"> The return type mapping of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionDbFunctionBuilder HasTypeMapping([CanBeNull] RelationalTypeMapping typeMapping, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given return type mapping can be set for the database function.
        /// </summary>
        /// <param name="typeMapping"> The return type mapping of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given return type mapping can be set for the database function. </returns>
        bool CanSetTypeMapping([CanBeNull] RelationalTypeMapping typeMapping, bool fromDataAnnotation = false);

        /// <summary>
        ///     <para>
        ///         Sets a callback that will be invoked to perform custom translation of this
        ///         function. The callback takes a collection of expressions corresponding to
        ///         the parameters passed to the function call. The callback should return an
        ///         expression representing the desired translation.
        ///     </para>
        ///     <para>
        ///         See https://go.microsoft.com/fwlink/?linkid=852477 for more information.
        ///     </para>
        /// </summary>
        /// <param name="translation"> The translation to use. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionDbFunctionBuilder HasTranslation(
            [CanBeNull] Func<IReadOnlyCollection<SqlExpression>, SqlExpression> translation, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given translation can be set for the database function.
        /// </summary>
        /// <param name="translation"> The translation to use. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given translation can be set for the database function. </returns>
        bool CanSetTranslation(
            [CanBeNull] Func<IReadOnlyCollection<SqlExpression>, SqlExpression> translation, bool fromDataAnnotation = false);
    }
}
