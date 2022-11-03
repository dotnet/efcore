// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a stored procedure in a model.
/// </summary>
public interface IStoredProcedure : IReadOnlyStoredProcedure, IAnnotatable
{
    /// <summary>
    ///     Gets the name of the stored procedure in the database.
    /// </summary>
    new string Name { get; }

    /// <summary>
    ///     Gets the entity type in which this stored procedure is defined.
    /// </summary>
    new IEntityType EntityType { get; }

    /// <summary>
    ///     Gets the associated database stored procedure.
    /// </summary>
    IStoreStoredProcedure StoreStoredProcedure { get; }

    /// <summary>
    ///     Gets the parameters for this stored procedure.
    /// </summary>
    new IReadOnlyList<IStoredProcedureParameter> Parameters { get; }

    /// <summary>
    ///     Returns the parameter corresponding to the given property.
    /// </summary>
    /// <param name="propertyName">The name of a property.</param>
    /// <returns>The parameter corresponding to the given property if found; <see langword="null" /> otherwise.</returns>
    new IStoredProcedureParameter? FindParameter(string propertyName);

    /// <summary>
    ///     Returns the original value parameter corresponding to the given property.
    /// </summary>
    /// <param name="propertyName">The name of a property.</param>
    /// <returns>
    ///     The original value parameter corresponding to the given property if found; <see langword="null" /> otherwise.
    /// </returns>
    new IStoredProcedureParameter? FindOriginalValueParameter(string propertyName);

    /// <summary>
    ///     Returns the rows affected parameter.
    /// </summary>
    /// <returns>
    ///     The rows affected parameter if found; <see langword="null" /> otherwise.
    /// </returns>
    new IStoredProcedureParameter? FindRowsAffectedParameter();

    /// <summary>
    ///     Gets the columns of the result for this stored procedure.
    /// </summary>
    new IReadOnlyList<IStoredProcedureResultColumn> ResultColumns { get; }

    /// <summary>
    ///     Returns the result column corresponding to the given property.
    /// </summary>
    /// <param name="propertyName">The name of a property.</param>
    /// <returns>The result column corresponding to the given property if found; <see langword="null" /> otherwise.</returns>
    new IStoredProcedureResultColumn? FindResultColumn(string propertyName);

    /// <summary>
    ///     Returns the rows affected result column.
    /// </summary>
    /// >
    /// <returns>The rows affected result column if found; <see langword="null" /> otherwise.</returns>
    new IStoredProcedureResultColumn? FindRowsAffectedResultColumn();

    /// <summary>
    ///     Returns the store identifier of this stored procedure.
    /// </summary>
    /// <returns>The store identifier.</returns>
    new StoreObjectIdentifier GetStoreIdentifier()
        => ((IReadOnlyStoredProcedure)this).GetStoreIdentifier()!.Value;
}
