// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents a property mapped to the database, but fully accessed through an outer property.
    ///         For relational databases, pseudo-properties represent the columns in the database when a single
    ///         property maps to multiple columns.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IProperty" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IConventionPseudoProperty : IConventionProperty, IPseudoProperty
    {
        /// <summary>
        ///     The outer property used to access values of this pseudo-property.
        /// </summary>
        new IMutableProperty OuterProperty { get; }
    }
}
