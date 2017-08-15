// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
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
        public virtual DatabaseModel Database { get; [param: NotNull] set; }

        /// <summary>
        ///     The sequence name.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the sequence, or <c>null</c> to use the default schema.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The database/store type of the sequence, or <c>null</c> if not set.
        /// </summary>
        public virtual string StoreType { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The start value for the sequence, or <c>null</c> if not set.
        /// </summary>
        public virtual long? StartValue { get; set; }

        /// <summary>
        ///     The amount to increment by to generate the next value in, the sequence, or <c>null</c> if not set.
        /// </summary>
        public virtual int? IncrementBy { get; set; }

        /// <summary>
        ///     The minimum value supported by the sequence, or <c>null</c> if not set.
        /// </summary>
        public virtual long? MinValue { get; set; }

        /// <summary>
        ///     The maximum value supported by the sequence, or <c>null</c> if not set.
        /// </summary>
        public virtual long? MaxValue { get; set; }

        /// <summary>
        ///     Indicates whether or not the sequence will start over when the max value is reached, or <c>null</c> if not set.
        /// </summary>
        public virtual bool? IsCyclic { get; set; }
    }
}
