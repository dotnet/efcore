// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     <para>
    ///         Indicates how a delete operation is applied to dependent entities in a relationship when the
    ///         principal is deleted or the relationship is severed.
    ///     </para>
    ///     <para>
    ///         Behaviors in the database are dependent on the database schema being created
    ///         appropriately. Using Entity Framework Migrations or <see cref="DatabaseFacade.EnsureCreated" />
    ///         will create the appropriate schema.
    ///     </para>
    ///     <para>
    ///         Note that the in-memory behavior for entities that are currently tracked by
    ///         the <see cref="DbContext" /> can be different from the behavior that happens in the database.
    ///         See the <see cref="ClientSetNull" /> behavior for more details.
    ///     </para>
    /// </summary>
    public enum DeleteBehavior
    {
        /// <summary>
        ///     <para>
        ///         For entities being tracked by the <see cref="DbContext" />, the values of foreign key properties in
        ///         dependent entities are set to null. This helps keep the graph of entities in a consistent
        ///         state while they are being tracked, such that a fully consistent graph can then be written to
        ///         the database. If a property cannot be set to null because it is not a nullable type,
        ///         then an exception will be thrown when <see cref="DbContext.SaveChanges()" /> is called.
        ///         This is the same as the <see cref="SetNull" /> behavior.
        ///     </para>
        ///     <para>
        ///         If the database has been created from the model using Entity Framework Migrations or the
        ///         <see cref="DatabaseFacade.EnsureCreated" /> method, then the behavior in the database
        ///         is to generate an error if a foreign key constraint is violated.
        ///         This is the same as the <see cref="Restrict" /> behavior.
        ///     </para>
        ///     <para>
        ///         This is the default for optional relationships. That is, for relationships that have
        ///         nullable foreign keys.
        ///     </para>
        /// </summary>
        ClientSetNull,

        /// <summary>
        ///     <para>
        ///         For entities being tracked by the <see cref="DbContext" />, the values of foreign key properties in
        ///         dependent entities are not changed. This can result in an inconsistent graph of entities
        ///         where the values of foreign key properties do not match the relationships in the
        ///         graph. If a property remains in this state when <see cref="DbContext.SaveChanges()" />
        ///         is called, then an exception will be thrown.
        ///     </para>
        ///     <para>
        ///         If the database has been created from the model using Entity Framework Migrations or the
        ///         <see cref="DatabaseFacade.EnsureCreated" /> method, then the behavior in the database
        ///         is to generate an error if a foreign key constraint is violated.
        ///     </para>
        /// </summary>
        Restrict,

        /// <summary>
        ///     <para>
        ///         For entities being tracked by the <see cref="DbContext" />, the values of foreign key properties in
        ///         dependent entities are set to null. This helps keep the graph of entities in a consistent
        ///         state while they are being tracked, such that a fully consistent graph can then be written to
        ///         the database. If a property cannot be set to null because it is not a nullable type,
        ///         then an exception will be thrown when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         If the database has been created from the model using Entity Framework Migrations or the
        ///         <see cref="DatabaseFacade.EnsureCreated" /> method, then the behavior in the database is
        ///         the same as is described above for tracked entities. Keep in mind that some databases cannot easily
        ///         support this behavior, especially if there are cycles in relationships.
        ///     </para>
        /// </summary>
        SetNull,

        /// <summary>
        ///     <para>
        ///         For entities being tracked by the <see cref="DbContext" />, the dependent entities
        ///         will also be deleted when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         If the database has been created from the model using Entity Framework Migrations or the
        ///         <see cref="DatabaseFacade.EnsureCreated" /> method, then the behavior in the database is
        ///         the same as is described above for tracked entities. Keep in mind that some databases cannot easily
        ///         support this behavior, especially if there are cycles in relationships.
        ///     </para>
        ///     <para>
        ///         This is the default for required relationships. That is, for relationships that have
        ///         non-nullable foreign keys.
        ///     </para>
        /// </summary>
        Cascade
    }
}
