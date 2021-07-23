// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    /// <summary>
    ///     A simple model for a database sequence used when reverse engineering an existing database.
    /// </summary>
    public class DatabaseSequence : Annotatable
    {
        /// <summary>
        ///     The database that contains the sequence.
        /// </summary>
        public virtual DatabaseModel Database { get; set; } = null!;

        /// <summary>
        ///     The sequence name.
        /// </summary>
        public virtual string Name { get; set; } = null!;

        /// <summary>
        ///     The schema that contains the sequence, or <see langword="null" /> to use the default schema.
        /// </summary>
        public virtual string? Schema { get; set; }

        /// <summary>
        ///     The database/store type of the sequence, or <see langword="null" /> if not set.
        /// </summary>
        public virtual string? StoreType { get; set; }

        /// <summary>
        ///     The start value for the sequence, or <see langword="null" /> if not set.
        /// </summary>
        public virtual long? StartValue { get; set; }

        /// <summary>
        ///     The amount to increment by to generate the next value in, the sequence, or <see langword="null" /> if not set.
        /// </summary>
        public virtual int? IncrementBy { get; set; }

        /// <summary>
        ///     The minimum value supported by the sequence, or <see langword="null" /> if not set.
        /// </summary>
        public virtual long? MinValue { get; set; }

        /// <summary>
        ///     The maximum value supported by the sequence, or <see langword="null" /> if not set.
        /// </summary>
        public virtual long? MaxValue { get; set; }

        /// <summary>
        ///     Indicates whether or not the sequence will start over when the max value is reached, or <see langword="null" /> if not set.
        /// </summary>
        public virtual bool? IsCyclic { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var name = Name ?? "<UNKNOWN>";
            return Schema == null ? name : $"{Schema}.{name}";
        }
    }
}
