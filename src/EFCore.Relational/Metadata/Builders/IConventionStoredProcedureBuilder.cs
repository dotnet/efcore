// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="IConventionStoredProcedure" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionStoredProcedureBuilder : IConventionAnnotatableBuilder
{
    /// <summary>
    ///     The function being configured.
    /// </summary>
    new IConventionStoredProcedure Metadata { get; }

    /// <summary>
    ///     Sets the name of the stored procedure.
    /// </summary>
    /// <param name="name">The name of the stored procedure in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionStoredProcedureBuilder? HasName(string? name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the name and schema of the stored procedure.
    /// </summary>
    /// <param name="name">The name of the stored procedure in the database.</param>
    /// <param name="schema">The schema of the stored procedure in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionStoredProcedureBuilder? HasName(string? name, string? schema, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given name can be set for the stored procedure.
    /// </summary>
    /// <param name="name">The name of the stored procedure in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given name can be set for the stored procedure.</returns>
    bool CanSetName(string? name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the schema of the stored procedure.
    /// </summary>
    /// <param name="schema">The schema of the stored procedure in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionStoredProcedureBuilder? HasSchema(string? schema, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given schema can be set for the stored procedure.
    /// </summary>
    /// <param name="schema">The schema of the stored procedure in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given schema can be set for the database function.</returns>
    bool CanSetSchema(string? schema, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a new parameter if no parameter mapped to the given property exists.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionStoredProcedureBuilder? HasParameter(string propertyName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether a parameter mapped to the given property can be used for stored procedure.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the parameter can be used for the stored procedure.</returns>
    bool CanHaveParameter(string propertyName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a new column of the result for this stored procedure. This is used for database generated columns.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionStoredProcedureBuilder? HasResultColumn(string propertyName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether a column of the result mapped to the given property can be used for stored procedure.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the column of the result can be used for the stored procedure.</returns>
    bool CanHaveResultColumn(string propertyName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Prevents automatically creating a transaction when executing this stored procedure.
    /// </summary>
    /// <param name="suppress">A value indicating whether the automatic transactions should be prevented.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    IConventionStoredProcedureBuilder? SuppressTransactions(bool suppress, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the transaction suppression can be set for stored procedure.
    /// </summary>
    /// <param name="suppress">A value indicating whether the automatic transactions should be prevented.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the column of the result can be used for the stored procedure.</returns>
    bool CanSetSuppressTransactions(bool suppress, bool fromDataAnnotation = false);
}
