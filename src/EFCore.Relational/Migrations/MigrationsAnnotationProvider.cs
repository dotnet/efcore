// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     <para>
    ///         A base class inherited by database providers that gives access to annotations used by EF Core Migrations
    ///         when generating removal operations for various elements of the <see cref="IRelationalModel" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class MigrationsAnnotationProvider : IMigrationsAnnotationProvider
    {
        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="relationalDependencies"> Parameter object containing dependencies for this service. </param>
        public MigrationsAnnotationProvider(MigrationsAnnotationProviderDependencies relationalDependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     Relational provider-specific dependencies for this service.
        /// </summary>
        protected virtual MigrationsAnnotationProviderDependencies RelationalDependencies { get; }

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> ForRemove(IRelationalModel model)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> ForRemove(ITable table)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> ForRemove(IColumn column)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> ForRemove(IView view)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> ForRemove(IViewColumn column)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> ForRemove(IUniqueConstraint constraint)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> ForRemove(ITableIndex index)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> ForRemove(IForeignKeyConstraint foreignKey)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> ForRemove(ISequence sequence)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> ForRemove(ICheckConstraint checkConstraint)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> ForRename(ITable table)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> ForRename(IColumn column)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> ForRename(ITableIndex index)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> ForRename(ISequence sequence)
            => Enumerable.Empty<IAnnotation>();
    }
}
