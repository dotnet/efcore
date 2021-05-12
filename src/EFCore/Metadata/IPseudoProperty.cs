// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents a property mapped to the database, but fully accessed through an outer property.
    ///         For relational databases, pseudo-properties represent the columns in the database when a single
    ///         property maps to multiple columns.
    ///     </para>
    /// </summary>
    public interface IPseudoProperty : IProperty
    {
        /// <summary>
        ///     The outer property used to access values of this pseudo-property.
        /// </summary>
        IProperty OuterProperty { get; }

        /// <summary>
        ///     A delegate that accepts a value from the <see cref="OuterProperty"/> and generates the value
        ///     for this pseudo-property.
        /// </summary>
        Func<object?, object?> ValueExtractor { get; }
    }
}
