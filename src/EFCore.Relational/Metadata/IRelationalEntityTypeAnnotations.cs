// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Properties for relational-specific annotations accessed through
    ///     <see cref="RelationalMetadataExtensions.Relational(IEntityType)" />.
    /// </summary>
    public interface IRelationalEntityTypeAnnotations
    {
        /// <summary>
        ///     The name of the table to which the entity type is mapped..
        /// </summary>
        string TableName { get; }

        /// <summary>
        ///     The database schema that contains the mapped table.
        /// </summary>
        string Schema { get; }

        /// <summary>
        ///     The <see cref="IProperty" /> that will be used for storing a discriminator value.
        /// </summary>
        IProperty DiscriminatorProperty { get; }

        /// <summary>
        ///     The discriminator value to use.
        /// </summary>
        object DiscriminatorValue { get; }
    }
}
