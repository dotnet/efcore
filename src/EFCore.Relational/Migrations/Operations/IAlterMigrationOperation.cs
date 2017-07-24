// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     <para>
    ///         An interface for any <see cref="MigrationOperation" /> that alters some existing database object.
    ///     </para>
    ///     <para>
    ///         All such operations contain an 'Old...' property that provides access to the configuration to the
    ///         database object as it was before being altered. This interface provides a common way to access
    ///         annotations on that 'old' database object.
    ///     </para>
    /// </summary>
    public interface IAlterMigrationOperation
    {
        /// <summary>
        ///     Annotations on the database object as they were before being altered.
        /// </summary>
        IMutableAnnotatable OldAnnotations { get; }
    }
}
