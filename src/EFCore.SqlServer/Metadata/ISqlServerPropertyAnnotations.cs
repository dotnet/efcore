// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     API for SQL Server-specific annotations accessed through
    ///     <see cref="SqlServerMetadataExtensions.SqlServer(IProperty)" />.
    /// </summary>
    public interface ISqlServerPropertyAnnotations : IRelationalPropertyAnnotations
    {
        /// <summary>
        ///     <para>
        ///         Gets the <see cref="SqlServerValueGenerationStrategy" /> to use for the property.
        ///     </para>
        ///     <para>
        ///         If no strategy is set for the property, then the strategy to use will be taken from the <see cref="IModel" />
        ///     </para>
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

        /// <summary>
        ///     Finds the <see cref="ISequence" /> in the model to use with
        ///     <see cref="SqlServerPropertyBuilderExtensions.ForSqlServerUseSequenceHiLo" />
        /// </summary>
        /// <returns> The sequence to use, or <c>null</c> if no sequence exists in the model. </returns>
        ISequence FindHiLoSequence();
    }
}
