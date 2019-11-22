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
    ///     </para>
    /// </summary>
    public enum DeleteBehavior
    {
        /// <summary>
        ///     <para>
        ///         For entities being tracked by the <see cref="DbContext" />, the values of foreign key properties in
        ///         dependent entities are set to null when the related principal is deleted.
        ///         This helps keep the graph of entities in a consistent state while they are being tracked, such that a
        ///         fully consistent graph can then be written to the database. If a property cannot be set to null because
        ///         it is not a nullable type, then an exception will be thrown when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         If the database has been created from the model using Entity Framework Migrations or the
        ///         <see cref="DatabaseFacade.EnsureCreated" /> method, then the behavior in the database
        ///         is to generate an error if a foreign key constraint is violated.
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
        ///         dependent entities are set to null when the related principal is deleted.
        ///         This helps keep the graph of entities in a consistent state while they are being tracked, such that a
        ///         fully consistent graph can then be written to the database. If a property cannot be set to null because
        ///         it is not a nullable type, then an exception will be thrown when <see cref="DbContext.SaveChanges()" /> is called.
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
        ///         dependent entities are set to null when the related principal is deleted.
        ///         This helps keep the graph of entities in a consistent state while they are being tracked, such that a
        ///         fully consistent graph can then be written to the database. If a property cannot be set to null because
        ///         it is not a nullable type, then an exception will be thrown when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         If the database has been created from the model using Entity Framework Migrations or the
        ///         <see cref="DatabaseFacade.EnsureCreated" /> method, then the behavior in the database is
        ///         the same as is described above for tracked entities. Keep in mind that some databases cannot easily
        ///         support this behavior, especially if there are cycles in relationships, in which case it may
        ///         be better to use <see cref="ClientSetNull" /> which will allow EF to cascade null values
        ///         on loaded entities even if the database does not support this.
        ///     </para>
        /// </summary>
        SetNull,

        /// <summary>
        ///     <para>
        ///         For entities being tracked by the <see cref="DbContext" />, dependent entities
        ///         will deleted when the related principal is deleted.
        ///     </para>
        ///     <para>
        ///         If the database has been created from the model using Entity Framework Migrations or the
        ///         <see cref="DatabaseFacade.EnsureCreated" /> method, then the behavior in the database is
        ///         the same as is described above for tracked entities. Keep in mind that some databases cannot easily
        ///         support this behavior, especially if there are cycles in relationships, in which case it may
        ///         be better to use <see cref="ClientCascade" /> which will allow EF to perform cascade deletes
        ///         on loaded entities even if the database does not support this.
        ///     </para>
        ///     <para>
        ///         This is the default for required relationships. That is, for relationships that have
        ///         non-nullable foreign keys.
        ///     </para>
        /// </summary>
        Cascade,

        /// <summary>
        ///     <para>
        ///         For entities being tracked by the <see cref="DbContext" />, dependent entities
        ///         will deleted when the related principal is deleted.
        ///     </para>
        ///     <para>
        ///         If the database has been created from the model using Entity Framework Migrations or the
        ///         <see cref="DatabaseFacade.EnsureCreated" /> method, then the behavior in the database
        ///         is to generate an error if a foreign key constraint is violated.
        ///     </para>
        /// </summary>
        ClientCascade,

        /// <summary>
        ///     <para>
        ///         For entities being tracked by the <see cref="DbContext" />, the values of foreign key properties in
        ///         dependent entities are set to null when the related principal is deleted.
        ///         This helps keep the graph of entities in a consistent state while they are being tracked, such that a
        ///         fully consistent graph can then be written to the database. If a property cannot be set to null because
        ///         it is not a nullable type, then an exception will be thrown when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         If the database has been created from the model using Entity Framework Migrations or the
        ///         <see cref="DatabaseFacade.EnsureCreated" /> method, then the behavior in the database
        ///         is to generate an error if a foreign key constraint is violated.
        ///     </para>
        /// </summary>
        NoAction,

        /// <summary>
        ///     <para>
        ///         Note: it is unusual to use this value. Consider using <see cref="ClientSetNull" /> instead to match
        ///         the behavior of EF6 with cascading deletes disabled.
        ///     </para>
        ///     <para>
        ///         For entities being tracked by the <see cref="DbContext" />, the values of foreign key properties in
        ///         dependent entities are not changed when the related principal entity is deleted.
        ///         This can result in an inconsistent graph of entities where the values of foreign key properties do
        ///         not match the relationships in the graph.
        ///     </para>
        ///     <para>
        ///         If the database has been created from the model using Entity Framework Migrations or the
        ///         <see cref="DatabaseFacade.EnsureCreated" /> method, then the behavior in the database
        ///         is to generate an error if a foreign key constraint is violated.
        ///     </para>
        /// </summary>
        ClientNoAction
    }
}
