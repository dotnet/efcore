// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a relational database function in a model in
///     the form that can be mutated while the model is being built.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
/// </remarks>
public interface IConventionDbFunction : IReadOnlyDbFunction, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the model in which this function is defined.
    /// </summary>
    new IConventionModel Model { get; }

    /// <summary>
    ///     Gets the builder that can be used to configure this function.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the function has been removed from the model.</exception>
    new IConventionDbFunctionBuilder Builder { get; }

    /// <summary>
    ///     Gets the configuration source for this function.
    /// </summary>
    /// <returns>The configuration source for this function.</returns>
    ConfigurationSource GetConfigurationSource();

    /// <summary>
    ///     Sets the name of the function in the database.
    /// </summary>
    /// <param name="name">The name of the function in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    string? SetName(string? name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for <see cref="IReadOnlyDbFunction.Name" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyDbFunction.Name" />.</returns>
    ConfigurationSource? GetNameConfigurationSource();

    /// <summary>
    ///     Sets the schema of the function in the database.
    /// </summary>
    /// <param name="schema">The schema of the function in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    string? SetSchema(string? schema, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for <see cref="IReadOnlyDbFunction.Schema" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyDbFunction.Schema" />.</returns>
    ConfigurationSource? GetSchemaConfigurationSource();

    /// <summary>
    ///     Sets the value indicating whether the database function is built-in or not.
    /// </summary>
    /// <param name="builtIn">The value indicating whether the database function is built-in or not.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    bool SetIsBuiltIn(bool builtIn, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for <see cref="IReadOnlyDbFunction.IsBuiltIn" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyDbFunction.IsBuiltIn" />.</returns>
    ConfigurationSource? GetIsBuiltInConfigurationSource();

    /// <summary>
    ///     Sets the value indicating whether the database function can return null value or not.
    /// </summary>
    /// <param name="nullable">The value indicating whether the database function can return null value or not.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    bool SetIsNullable(bool nullable, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for <see cref="IReadOnlyDbFunction.IsNullable" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyDbFunction.IsNullable" />.</returns>
    ConfigurationSource? GetIsNullableConfigurationSource();

    /// <summary>
    ///     Sets the store type of the function in the database.
    /// </summary>
    /// <param name="storeType">The store type of the function in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    string? SetStoreType(string? storeType, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for <see cref="IReadOnlyDbFunction.StoreType" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyDbFunction.StoreType" />.</returns>
    ConfigurationSource? GetStoreTypeConfigurationSource();

    /// <summary>
    ///     Sets the type mapping of the function in the database.
    /// </summary>
    /// <param name="typeMapping">The type mapping of the function in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    RelationalTypeMapping? SetTypeMapping(RelationalTypeMapping? typeMapping, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for <see cref="IReadOnlyDbFunction.TypeMapping" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyDbFunction.TypeMapping" />.</returns>
    ConfigurationSource? GetTypeMappingConfigurationSource();

    /// <summary>
    ///     Sets the translation callback for performing custom translation of the method call into a SQL expression fragment.
    /// </summary>
    /// <param name="translation">
    ///     The translation callback for performing custom translation of the method call into a SQL expression fragment.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    Func<IReadOnlyList<SqlExpression>, SqlExpression>? SetTranslation(
        Func<IReadOnlyList<SqlExpression>, SqlExpression>? translation,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for <see cref="IReadOnlyDbFunction.Translation" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyDbFunction.Translation" />.</returns>
    ConfigurationSource? GetTranslationConfigurationSource();

    /// <summary>
    ///     Gets the parameters for this function
    /// </summary>
    new IReadOnlyList<IConventionDbFunctionParameter> Parameters { get; }
}
