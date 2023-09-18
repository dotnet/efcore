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
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionStoredProcedureBuilder" /> to continue configuration if the annotation was set, <see langword="null" />
    ///     otherwise.
    /// </returns>
    new IConventionStoredProcedureBuilder? HasAnnotation(string name, object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    ///     Removes the annotation if <see langword="null" /> value is specified.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation. <see langword="null" /> to remove the annotations.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionStoredProcedureBuilder" /> to continue configuration if the annotation was set or removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionStoredProcedureBuilder? HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the annotation with the given name from this object.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionStoredProcedureBuilder" /> to continue configuration if the annotation was set, <see langword="null" />
    ///     otherwise.
    /// </returns>
    new IConventionStoredProcedureBuilder? HasNoAnnotation(string name, bool fromDataAnnotation = false);

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
    IConventionStoredProcedureParameterBuilder? HasParameter(string propertyName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether a parameter mapped to the given property can be used for stored procedure.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the parameter can be used for the stored procedure.</returns>
    bool CanHaveParameter(string propertyName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a new parameter that holds the original value of the property with the given name
    ///     if no parameter mapped to the given property exists.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The builder instance if the configuration was applied, <see langword="null" /> otherwise.
    /// </returns>
    IConventionStoredProcedureParameterBuilder? HasOriginalValueParameter(string propertyName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether a parameter holds the original value of the mapped property
    ///     can be used for stored procedure.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the parameter can be used for the stored procedure.</returns>
    bool CanHaveOriginalValueParameter(string propertyName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a new parameter that returns the rows affected if no such parameter exists.
    /// </summary>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The builder instance if the configuration was applied, <see langword="null" /> otherwise.
    /// </returns>
    IConventionStoredProcedureParameterBuilder? HasRowsAffectedParameter(bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether a parameter that returns the rows affected can be used for stored procedure.
    /// </summary>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the parameter can be used for the stored procedure.</returns>
    bool CanHaveRowsAffectedParameter(bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a new column of the result for this stored procedure. This is used for database generated columns.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The builder instance if the configuration was applied, <see langword="null" /> otherwise.
    /// </returns>
    IConventionStoredProcedureResultColumnBuilder? HasResultColumn(string propertyName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether a column of the result mapped to the given property can be used for stored procedure.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the column of the result can be used for the stored procedure.</returns>
    bool CanHaveResultColumn(string propertyName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures a new column that contains the rows affected for this stored procedure if no such column exists.
    /// </summary>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The builder instance if the configuration was applied, <see langword="null" /> otherwise.
    /// </returns>
    IConventionStoredProcedureResultColumnBuilder? HasRowsAffectedResultColumn(bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether a column that contains the rows affected can be used for stored procedure.
    /// </summary>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the column of the result can be used for the stored procedure.</returns>
    bool CanHaveRowsAffectedResultColumn(bool fromDataAnnotation = false);
}
