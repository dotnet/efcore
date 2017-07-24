// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Properties for relational-specific annotations accessed through
    ///     <see cref="RelationalMetadataExtensions.Relational(IMutableForeignKey)" />.
    /// </summary>
    public class RelationalForeignKeyAnnotations : IRelationalForeignKeyAnnotations
    {
        /// <summary>
        ///     Constructs an instance for annotations of the given <see cref="IForeignKey" />.
        /// </summary>
        /// <param name="foreignKey"> The <see cref="IForeignKey" /> to use. </param>
        public RelationalForeignKeyAnnotations([NotNull] IForeignKey foreignKey)
            : this(new RelationalAnnotations(foreignKey))
        {
        }

        /// <summary>
        ///     Constructs an instance for annotations of the <see cref="IForeignKey" />
        ///     represented by the given annotation helper.
        /// </summary>
        /// <param name="annotations">
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IForeignKey" /> to annotate.
        /// </param>
        protected RelationalForeignKeyAnnotations([NotNull] RelationalAnnotations annotations)
            => Annotations = annotations;

        /// <summary>
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IForeignKey" /> to annotate.
        /// </summary>
        protected virtual RelationalAnnotations Annotations { get; }

        /// <summary>
        ///     The <see cref="IForeignKey" /> to annotate.
        /// </summary>
        protected virtual IForeignKey ForeignKey => (IForeignKey)Annotations.Metadata;

        /// <summary>
        ///     The foreign key constraint name.
        /// </summary>
        public virtual string Name
        {
            get => (string)Annotations.Metadata[RelationalAnnotationNames.Name]
                   ?? ConstraintNamer.GetDefaultName(ForeignKey);

            [param: CanBeNull] set => SetName(value);
        }

        /// <summary>
        ///     Attempts to set the <see cref="Name" /> using the semantics of the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(value, nameof(value)));
    }
}
