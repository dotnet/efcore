// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Properties for relational-specific annotations accessed through
    ///     <see cref="RelationalMetadataExtensions.Relational(IForeignKey)" />.
    /// </summary>
    public interface IRelationalForeignKeyAnnotations
    {
        /// <summary>
        ///     The foreign key constraint name.
        /// </summary>
        string Name { get; }
    }
}
