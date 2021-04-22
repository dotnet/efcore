// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         Parameters for creating a <see cref="IModificationCommand" /> instance.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public sealed record ModificationCommandParameters
    {
        /// <summary>
        ///     The name of the table containing the data to be modified.
        /// </summary>
        public readonly string TableName;

        /// <summary>
        ///     The schema containing the table, or <see langword="null" /> to use the default schema.
        /// </summary>
        public readonly string? Schema;

        /// <summary>
        ///     A delegate to generate parameter names.
        /// </summary>
        public readonly Func<string> GenerateParameterName;

        /// <summary>
        ///     Indicates whether or not potentially sensitive data (e.g. database values) can be logged.
        /// </summary>
        public readonly bool SensitiveLoggingEnabled;

        /// <summary>
        ///     A ColumnModification factory.
        /// </summary>
        public readonly IColumnModificationFactory ColumnModificationFactory;

        /// <summary>
        ///     #DUMMY
        /// </summary>
        public readonly IReadOnlyList<IUpdateEntry> Entries;

        /// <summary>
        ///     The <see cref="EntityFrameworkCore.EntityState" /> that indicates whether the row will be
        ///     inserted (<see cref="Microsoft.EntityFrameworkCore.EntityState.Added" />),
        ///     updated (<see cref="Microsoft.EntityFrameworkCore.EntityState.Modified" />),
        ///     or deleted ((<see cref="Microsoft.EntityFrameworkCore.EntityState.Deleted" />).
        /// </summary>
        public readonly EntityState EntityState;

        /// <summary>
        ///     Creates a new <see cref="ModificationCommandParameters" /> instance.
        /// </summary>
        /// <param name="tableName"> The name of the table containing the data to be modified. </param>
        /// <param name="schemaName"> The schema containing the table, or <see langword="null" /> to use the default schema. </param>
        /// <param name="generateParameterName"> A delegate to generate parameter names. </param>
        /// <param name="sensitiveLoggingEnabled"> Indicates whether or not potentially sensitive data (e.g. database values) can be logged. </param>
        /// <param name="columnModificationFactory"> A ColumnModification factory. </param>
        /// <param name="entries"> List of entries. </param>
        /// <param name="entityState">
        ///     The <see cref="EntityFrameworkCore.EntityState" /> that indicates whether the row will be
        ///     inserted (<see cref="Microsoft.EntityFrameworkCore.EntityState.Added" />),
        ///     updated (<see cref="Microsoft.EntityFrameworkCore.EntityState.Modified" />),
        ///     or deleted ((<see cref="Microsoft.EntityFrameworkCore.EntityState.Deleted" />).
        /// </param>
        public ModificationCommandParameters(
            string tableName,
            string? schemaName,
            Func<string> generateParameterName,
            bool sensitiveLoggingEnabled,
            IColumnModificationFactory columnModificationFactory,
            IReadOnlyList<IUpdateEntry> entries,
            EntityState entityState)
        {
            TableName = tableName;
            Schema = schemaName;
            GenerateParameterName = generateParameterName;
            SensitiveLoggingEnabled = sensitiveLoggingEnabled;
            ColumnModificationFactory = columnModificationFactory;
            Entries = entries;
            EntityState = entityState;
        }
    }
}
