// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents property facet overrides for a particular table-like store object.
    /// </summary>
    public class SlimRelationalPropertyOverrides : AnnotatableBase, IRelationalPropertyOverrides
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public SlimRelationalPropertyOverrides(
            SlimProperty property,
            string? columnName,
            bool columnNameOverriden)
        {
            Property = property;
            if (columnNameOverriden)
            {
                SetAnnotation(RelationalAnnotationNames.ColumnName, columnName);
            }
        }

        /// <summary>
        ///     Gets the property for which the overrides are applied.
        /// </summary>
        public virtual SlimProperty Property { get; }

        /// <inheritdoc/>
        IProperty IRelationalPropertyOverrides.Property
        {
            [DebuggerStepThrough]
            get => Property;
        }

        /// <inheritdoc/>
        string? IRelationalPropertyOverrides.ColumnName
        {
            [DebuggerStepThrough]
            get => (string?)this[RelationalAnnotationNames.ColumnName];
        }

        /// <inheritdoc/>
        bool IRelationalPropertyOverrides.ColumnNameOverriden
        {
            [DebuggerStepThrough]
            get => FindAnnotation(RelationalAnnotationNames.ColumnName) != null;
        }
    }
}
