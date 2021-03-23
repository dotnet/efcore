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
    public class RuntimeRelationalPropertyOverrides : AnnotatableBase, IRelationalPropertyOverrides
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SlimRelationalPropertyOverrides"/> class.
        /// </summary>
        /// <param name="property"> The property for which the overrides are applied. </param>
        /// <param name="columnNameOverriden"> Whether the column name is overriden. </param>
        /// <param name="columnName"> The column name. </param>
        public RuntimeRelationalPropertyOverrides(
            RuntimeProperty property,
            bool columnNameOverriden,
            string? columnName)
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
        public virtual RuntimeProperty Property { get; }

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
