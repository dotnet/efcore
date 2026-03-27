// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Indicates how a delete operation is applied to dependent entities in a relationship when the
///     principal is deleted or the relationship is severed.
/// </summary>
/// <remarks>
///     <para>
///         Behaviors in the database are dependent on the database schema being created appropriately. The database is created appropriately
///         when using Entity Framework Migrations or using one of
///         <see href="https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade.ensurecreated" /> or
///         <see href="https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade.ensurecreatedasync" />
///         .
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-cascading">Cascade delete and deleting orphans in EF Core</see> for more information and
///         examples.
///     </para>
/// </remarks>
public enum DeleteBehavior
{
    /// <summary>
    ///     Sets foreign key values to <see langword="null" /> as appropriate when changes are made to tracked entities and creates
    ///     a non-cascading foreign key constraint in the database. This is the default for optional relationships.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <see cref="ClientSetNull" />, <see cref="Restrict" />, and <see cref="NoAction" /> all have essentially the same behavior, except
    ///         that <see cref="Restrict" /> and <see cref="NoAction" /> will configure the foreign key constraint as "RESTRICT" or "NO ACTION"
    ///         respectively for databases that support this, while <see cref="ClientSetNull" /> uses the default database setting for foreign
    ///         key constraints.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-cascading">EF Core cascade deletes and deleting orphans</see> for more information
    ///         and examples.
    ///     </para>
    /// </remarks>
    ClientSetNull,

    /// <summary>
    ///     Sets foreign key values to <see langword="null" /> as appropriate when changes are made to tracked entities and creates
    ///     a non-cascading foreign key constraint in the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <see cref="ClientSetNull" />, <see cref="Restrict" />, and <see cref="NoAction" /> all have essentially the same behavior, except
    ///         that <see cref="Restrict" /> and <see cref="NoAction" /> will configure the foreign key constraint as "RESTRICT" or "NO ACTION"
    ///         respectively for databases that support this, while <see cref="ClientSetNull" /> uses the default database setting for foreign
    ///         key constraints.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-cascading">EF Core cascade deletes and deleting orphans</see> for more information
    ///         and examples.
    ///     </para>
    /// </remarks>
    Restrict,

    /// <summary>
    ///     Sets foreign key values to <see langword="null" /> as appropriate when changes are made to tracked entities and creates
    ///     a foreign key constraint in the database that propagates <see langword="null" /> values from principals to dependents.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Not all database support propagation of <see langword="null" /> values, and some databases that do have restrictions
    ///         on when it can be used. For example, when using SQL Server, it is difficult to use <see langword="null" /> propagation
    ///         without creating multiple cascade paths.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-cascading">EF Core cascade deletes and deleting orphans</see> for more information
    ///         and examples.
    ///     </para>
    /// </remarks>
    SetNull,

    /// <summary>
    ///     Automatically deletes dependent entities when the principal is deleted or the relationship to the principal is severed,
    ///     and creates a foreign key constraint in the database with cascading deletes enabled. This is the default for
    ///     required relationships.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Some databases have restrictions on when cascading deletes can be used. For example, SQL Server has limited
    ///         support for multiple cascade paths. Consider using <see cref="ClientCascade" /> instead for these cases.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-cascading">EF Core cascade deletes and deleting orphans</see> for more information
    ///         and examples.
    ///     </para>
    /// </remarks>
    Cascade,

    /// <summary>
    ///     Automatically deletes dependent entities when the principal is deleted or the relationship to the principal is severed,
    ///     but creates a non-cascading foreign key constraint in the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Consider using this option when database restrictions prevent the use of <see cref="Cascade" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-cascading">EF Core cascade deletes and deleting orphans</see> for more information
    ///         and examples.
    ///     </para>
    /// </remarks>
    ClientCascade,

    /// <summary>
    ///     Sets foreign key values to <see langword="null" /> as appropriate when changes are made to tracked entities and creates
    ///     a non-cascading foreign key constraint in the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <see cref="ClientSetNull" />, <see cref="Restrict" />, and <see cref="NoAction" /> all have essentially the same behavior, except
    ///         that <see cref="Restrict" /> and <see cref="NoAction" /> will configure the foreign key constraint as "RESTRICT" or "NO ACTION"
    ///         respectively for databases that support this, while <see cref="ClientSetNull" /> uses the default database setting for foreign
    ///         key constraints.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-cascading">EF Core cascade deletes and deleting orphans</see> for more information
    ///         and examples.
    ///     </para>
    /// </remarks>
    NoAction,

    /// <summary>
    ///     Tracked dependents are not deleted and their foreign key values are not set to <see langword="null" /> when deleting
    ///     principal entities. A non-cascading foreign key constraint is created in the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         It is unusual to use this option and will often result in exceptions when saving changes to the database unless
    ///         additional work is done.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-cascading">EF Core cascade deletes and deleting orphans</see> for more information
    ///         and examples.
    ///     </para>
    /// </remarks>
    ClientNoAction
}
