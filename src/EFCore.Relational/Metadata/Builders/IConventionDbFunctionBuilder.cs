// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API for configuring a <see cref="IConventionDbFunction" />.
    /// </summary>
    public interface IConventionDbFunctionBuilder : IConventionAnnotatableBuilder
    {
        /// <summary>
        ///     The function being configured.
        /// </summary>
        new IConventionDbFunction Metadata { get; }

        /// <summary>
        ///     Sets the name of the database function.
        /// </summary>
        /// <param name="name"> The name of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionDbFunctionBuilder? HasName(string? name, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given name can be set for the database function.
        /// </summary>
        /// <param name="name"> The name of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given name can be set for the database function. </returns>
        bool CanSetName(string? name, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the schema of the database function.
        /// </summary>
        /// <param name="schema"> The schema of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionDbFunctionBuilder? HasSchema(string? schema, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given schema can be set for the database function.
        /// </summary>
        /// <param name="schema"> The schema of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given schema can be set for the database function. </returns>
        bool CanSetSchema(string? schema, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the value indicating whether the database function is built-in or not.
        /// </summary>
        /// <param name="builtIn"> The value indicating whether the database function is built-in or not. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionDbFunctionBuilder? IsBuiltIn(bool builtIn, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given built-in can be set for the database function.
        /// </summary>
        /// <param name="builtIn"> The value indicating whether the database function is built-in or not. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given schema can be set for the database function. </returns>
        bool CanSetIsBuiltIn(bool builtIn, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the value indicating whether the database function can return null value or not.
        /// </summary>
        /// <param name="nullable"> The value indicating whether the database function is built-in or not. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionDbFunctionBuilder? IsNullable(bool nullable, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given nullable can be set for the database function.
        /// </summary>
        /// <param name="nullable"> The value indicating whether the database function can return null value or not. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given schema can be set for the database function. </returns>
        bool CanSetIsNullable(bool nullable, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the store type of the function in the database.
        /// </summary>
        /// <param name="storeType"> The store type of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionDbFunctionBuilder? HasStoreType(string? storeType, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given store type can be set for the database function.
        /// </summary>
        /// <param name="storeType"> The store type of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given store type can be set for the database function. </returns>
        bool CanSetStoreType(string? storeType, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the return type mapping of the database function.
        /// </summary>
        /// <param name="typeMapping"> The return type mapping of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionDbFunctionBuilder? HasTypeMapping(RelationalTypeMapping? typeMapping, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given return type mapping can be set for the database function.
        /// </summary>
        /// <param name="typeMapping"> The return type mapping of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given return type mapping can be set for the database function. </returns>
        bool CanSetTypeMapping(RelationalTypeMapping? typeMapping, bool fromDataAnnotation = false);

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
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionDbFunctionBuilder? HasTranslation(
            Func<IReadOnlyList<SqlExpression>, SqlExpression>? translation,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given translation can be set for the database function.
        /// </summary>
        /// <param name="translation"> The translation to use. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given translation can be set for the database function. </returns>
        bool CanSetTranslation(
            Func<IReadOnlyList<SqlExpression>, SqlExpression>? translation,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns an object that can be used to configure a parameter with the given name.
        /// </summary>
        /// <param name="name"> The parameter name. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The builder to use for further parameter configuration. </returns>
        IConventionDbFunctionParameterBuilder HasParameter(string name, bool fromDataAnnotation = false);
    }
}
