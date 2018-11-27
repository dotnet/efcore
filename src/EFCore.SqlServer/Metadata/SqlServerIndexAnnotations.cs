// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Properties for relational-specific annotations accessed through
    ///     <see cref="SqlServerMetadataExtensions.SqlServer(IMutableIndex)" />.
    /// </summary>
    public class SqlServerIndexAnnotations : RelationalIndexAnnotations, ISqlServerIndexAnnotations
    {
        /// <summary>
        ///     Constructs an instance for annotations of the given <see cref="IIndex" />.
        /// </summary>
        /// <param name="index"> The <see cref="IIndex" /> to use. </param>
        public SqlServerIndexAnnotations([NotNull] IIndex index)
            : base(index)
        {
        }

        /// <summary>
        ///     Constructs an instance for annotations of the <see cref="IIndex" />
        ///     represented by the given annotation helper.
        /// </summary>
        /// <param name="annotations">
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IIndex" /> to annotate.
        /// </param>
        protected SqlServerIndexAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        /// <summary>
        ///     Indicates whether or not the index is clustered, or <c>null</c> if clustering has not
        ///     been specified.
        /// </summary>
        public virtual bool? IsClustered
        {
            get => (bool?)Annotations.Metadata[SqlServerAnnotationNames.Clustered];
            set => SetIsClustered(value);
        }

        /// <summary>
        ///     Attempts to set clustering using the semantics of the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetIsClustered(bool? value) => Annotations.SetAnnotation(
            SqlServerAnnotationNames.Clustered,
            value);

        /// <summary>
        ///     Returns included property names, or <c>null</c> if they have not been specified.
        /// </summary>
        public virtual IReadOnlyList<string> IncludeProperties
        {
            get => (string[])Annotations.Metadata[SqlServerAnnotationNames.Include];
            set => SetInclude(value);
        }

        /// <summary>
        ///     Attempts to set included property names using the semantics of the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="properties"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetInclude([CanBeNull] IReadOnlyList<string> properties)
            => Annotations.SetAnnotation(
                SqlServerAnnotationNames.Include,
                properties);

        /// <summary>
        ///     Indicates whether or not the index is online, or <c>null</c> if online option has not
        ///     been specified.
        /// </summary>
        public virtual bool? IsOnline
        {
            get => (bool?)Annotations.Metadata[SqlServerAnnotationNames.Online];
            set => SetIsOnline(value);
        }

        /// <summary>
        ///     Attempts to set online option using the semantics of the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetIsOnline(bool? value) => Annotations.SetAnnotation(
            SqlServerAnnotationNames.Online,
            value);
    }
}
