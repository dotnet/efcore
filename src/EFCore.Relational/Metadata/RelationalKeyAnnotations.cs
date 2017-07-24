// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Properties for relational-specific annotations accessed through
    ///     <see cref="RelationalMetadataExtensions.Relational(IMutableKey)" />.
    /// </summary>
    public class RelationalKeyAnnotations : IRelationalKeyAnnotations
    {
        /// <summary>
        ///     Constructs an instance for annotations of the given <see cref="IKey" />.
        /// </summary>
        /// <param name="key"> The <see cref="IKey" /> to use. </param>
        public RelationalKeyAnnotations([NotNull] IKey key)
            : this(new RelationalAnnotations(key))
        {
        }

        /// <summary>
        ///     Constructs an instance for annotations of the <see cref="IKey" />
        ///     represented by the given annotation helper.
        /// </summary>
        /// <param name="annotations">
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IKey" /> to annotate.
        /// </param>
        protected RelationalKeyAnnotations([NotNull] RelationalAnnotations annotations)
            => Annotations = annotations;

        /// <summary>
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IKey" /> to annotate.
        /// </summary>
        protected virtual RelationalAnnotations Annotations { get; }

        /// <summary>
        ///     The <see cref="IKey" /> to annotate.
        /// </summary>
        protected virtual IKey Key => (IKey)Annotations.Metadata;

        /// <summary>
        ///     The key constraint name.
        /// </summary>
        public virtual string Name
        {
            get => (string)Annotations.Metadata[RelationalAnnotationNames.Name]
                   ?? ConstraintNamer.GetDefaultName(Key);

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
