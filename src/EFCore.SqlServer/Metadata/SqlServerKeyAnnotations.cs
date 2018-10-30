// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Properties for relational-specific annotations accessed through
    ///     <see cref="SqlServerMetadataExtensions.SqlServer(IMutableKey)" />.
    /// </summary>
    public class SqlServerKeyAnnotations : RelationalKeyAnnotations, ISqlServerKeyAnnotations
    {
        /// <summary>
        ///     Constructs an instance for annotations of the given <see cref="IKey" />.
        /// </summary>
        /// <param name="key"> The <see cref="IKey" /> to use. </param>
        public SqlServerKeyAnnotations([NotNull] IKey key)
            : base(key)
        {
        }

        /// <summary>
        ///     Constructs an instance for annotations of the <see cref="IKey" />
        ///     represented by the given annotation helper.
        /// </summary>
        /// <param name="annotations">
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IKey" /> to annotate.
        /// </param>
        protected SqlServerKeyAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        /// <summary>
        ///     Gets or sets whether or not the key is clustered, or <c>null</c> if clustering has not
        ///     been specified.
        /// </summary>
        public virtual bool? IsClustered
        {
            get => (bool?)Annotations.Metadata[SqlServerAnnotationNames.Clustered] ?? DefaultIsClustered;
            set => SetIsClustered(value);
        }

        private bool? DefaultIsClustered
        {
            get
            {
                var sharedTablePrincipalPrimaryKeyProperty = Key.Properties[0].FindSharedTableRootPrimaryKeyProperty();
                if (sharedTablePrincipalPrimaryKeyProperty != null)
                {
                    return sharedTablePrincipalPrimaryKeyProperty.GetContainingPrimaryKey().SqlServer().IsClustered;
                }

                return null;
            }
        }

        /// <summary>
        ///     Attempts to set clustering using the semantics of the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetIsClustered(bool? value)
            => Annotations.SetAnnotation(SqlServerAnnotationNames.Clustered, value);
    }
}
