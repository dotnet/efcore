// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Properties for SQLite-specific annotations accessed through
    ///     <see cref="SqliteMetadataExtensions.Sqlite(IMutableProperty)" />.
    /// </summary>
    public class SqlitePropertyAnnotations : RelationalPropertyAnnotations, ISqlitePropertyAnnotations
    {
        /// <summary>
        ///     Constructs an instance for annotations of the given <see cref="IProperty" />.
        /// </summary>
        /// <param name="property"> The <see cref="IProperty" /> to use. </param>
        public SqlitePropertyAnnotations([NotNull] IProperty property)
            : base(property)
        {
        }

        /// <summary>
        ///     Constructs an instance for annotations of the <see cref="IProperty" />
        ///     represented by the given annotation helper.
        /// </summary>
        /// <param name="annotations">
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IProperty" /> to annotate.
        /// </param>
        protected SqlitePropertyAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        /// <summary>
        ///     Gets or sets the SRID to use when creating a column for this property.
        /// </summary>
        public virtual int? Srid
        {
            get => (int?)Annotations.Metadata[SqliteAnnotationNames.Srid];
            set => SetSrid(value);
        }

        /// <summary>
        ///     Sets the SRID to use when creating a column for this property.
        /// </summary>
        /// <param name="value"> The SRID. </param>
        /// <returns> true if the annotation was set; otherwise, false. </returns>
        protected virtual bool SetSrid(int? value)
            => Annotations.SetAnnotation(SqliteAnnotationNames.Srid, value);

        /// <summary>
        ///     Gets or sets the dimension to use when creating a column for this property.
        /// </summary>
        public virtual string Dimension
        {
            get => (string)Annotations.Metadata[SqliteAnnotationNames.Dimension];
            set => SetDimension(value);
        }

        /// <summary>
        ///     Sets the dimension to use when creating a column for this property.
        /// </summary>
        /// <param name="value"> The dimension. </param>
        /// <returns> true if the annotation was set; otherwise, false. </returns>
        protected virtual bool SetDimension(string value)
            => Annotations.SetAnnotation(SqliteAnnotationNames.Dimension, value);
    }
}
