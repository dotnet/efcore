// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         Represents a conceptual database command to insert/update/delete a row.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public interface IReadOnlyModificationCommand
{
    /// <summary>
    ///     The table containing the data to be modified.
    /// </summary>
    public ITable? Table { get; }

    /// <summary>
    ///     The stored procedure to use for updating the data.
    /// </summary>
    public IStoreStoredProcedure? StoreStoredProcedure { get; }

    /// <summary>
    ///     The name of the table containing the data to be modified.
    /// </summary>
    public string TableName { get; }

    /// <summary>
    ///     The schema containing the table, or <see langword="null" /> to use the default schema.
    /// </summary>
    public string? Schema { get; }

    /// <summary>
    ///     The list of <see cref="IColumnModification" /> needed to perform the insert, update, or delete.
    /// </summary>
    public IReadOnlyList<IColumnModification> ColumnModifications { get; }

    /// <summary>
    ///     The <see cref="IUpdateEntry" /> that represent the entities that are mapped to the row to update.
    /// </summary>
    public IReadOnlyList<IUpdateEntry> Entries { get; }

    /// <summary>
    ///     The <see cref="EntityFrameworkCore.EntityState" /> that indicates whether the row will be
    ///     inserted (<see cref="Microsoft.EntityFrameworkCore.EntityState.Added" />),
    ///     updated (<see cref="Microsoft.EntityFrameworkCore.EntityState.Modified" />),
    ///     or deleted ((<see cref="Microsoft.EntityFrameworkCore.EntityState.Deleted" />).
    /// </summary>
    public EntityState EntityState { get; }

    /// <summary>
    ///     When using a stored procedure, this optionally points to the output parameter or result column containing the rows affected.
    /// </summary>
    public IColumnBase? RowsAffectedColumn { get; }

    /// <summary>
    ///     Reads result set columns returned from the database in the given <paramref name="relationalReader" /> and propagates them back
    ///     to into the appropriate <see cref="IColumnModification" /> from which the values can be propagated on to tracked entities.
    /// </summary>
    /// <param name="relationalReader">The relational reader containing the values read from the database.</param>
    public void PropagateResults(RelationalDataReader relationalReader);

    /// <summary>
    ///     Reads output parameters returned from the database in the given <paramref name="parameterCollection" /> and propagates them back
    ///     to into the appropriate <see cref="IColumnModification" /> from which the values can be propagated on to tracked entities.
    /// </summary>
    /// <param name="parameterCollection">The parameter collection from which to propagate output values.</param>
    /// <param name="baseParameterIndex">
    ///     The index in <paramref name="parameterCollection" /> on which parameters for this <see cref="ModificationCommand" /> begin.
    /// </param>
    public void PropagateOutputParameters(DbParameterCollection parameterCollection, int baseParameterIndex);
}
