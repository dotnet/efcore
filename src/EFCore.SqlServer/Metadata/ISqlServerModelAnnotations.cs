// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     API for SQL Server-specific annotations accessed through
    ///     <see cref="SqlServerMetadataExtensions.SqlServer(IModel)" />.
    /// </summary>
    public interface ISqlServerModelAnnotations : IRelationalModelAnnotations
    {
        /// <summary>
        ///     The <see cref="SqlServerValueGenerationStrategy" /> to use for properties
        ///     of keys in the model, unless the property has a different strategy explicitly set.
        /// </summary>
        SqlServerValueGenerationStrategy? ValueGenerationStrategy { get; }

        /// <summary>
        ///     Gets the sequence name to use with
        ///     <see cref="SqlServerPropertyBuilderExtensions.ForSqlServerUseSequenceHiLo" />
        /// </summary>
        string HiLoSequenceName { get; }

        /// <summary>
        ///     Gets the schema for the sequence to use with
        ///     <see cref="SqlServerPropertyBuilderExtensions.ForSqlServerUseSequenceHiLo" />
        /// </summary>
        string HiLoSequenceSchema { get; }
    }
}
