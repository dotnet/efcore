// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Properties for relational-specific annotations accessed through
    ///     <see cref="RelationalMetadataExtensions.Relational(IMutableIndex)" />.
    /// </summary>
    public class RelationalIndexAnnotations : IRelationalIndexAnnotations
    {
        /// <summary>
        ///     Constructs an instance for annotations of the given <see cref="IIndex" />.
        /// </summary>
        /// <param name="index"> The <see cref="IIndex" /> to use. </param>
        public RelationalIndexAnnotations([NotNull] IIndex index)
            : this(new RelationalAnnotations(index))
        {
        }

        /// <summary>
        ///     Constructs an instance for annotations of the <see cref="IIndex" />
        ///     represented by the given annotation helper.
        /// </summary>
        /// <param name="annotations">
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IIndex" /> to annotate.
        /// </param>
        protected RelationalIndexAnnotations([NotNull] RelationalAnnotations annotations)
            => Annotations = annotations;

        /// <summary>
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IIndex" /> to annotate.
        /// </summary>
        protected virtual RelationalAnnotations Annotations { get; }

        /// <summary>
        ///     The <see cref="IIndex" /> to annotate.
        /// </summary>
        protected virtual IIndex Index => (IIndex)Annotations.Metadata;

        /// <summary>
        ///     The index name.
        /// </summary>
        public virtual string Name
        {
            get => (string)Annotations.Metadata[RelationalAnnotationNames.Name]
                   ?? ConstraintNamer.GetDefaultName(Index);

            [param: CanBeNull] set => SetName(value);
        }

        /// <summary>
        ///     The index filter expression.
        /// </summary>
        public virtual string Filter
        {
            get => (string)Annotations.Metadata[RelationalAnnotationNames.Filter];
            [param: CanBeNull] set => SetFilter(value);
        }

        /// <summary>
        ///     Attempts to set the <see cref="Filter" /> using the semantics of the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetFilter([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.Filter,
                Check.NullButNotEmpty(value, nameof(value)));

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
