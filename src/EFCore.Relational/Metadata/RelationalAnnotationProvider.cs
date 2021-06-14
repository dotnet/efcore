// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         A base class inherited by database providers that gives access to annotations
    ///         used by relational EF Core components on various elements of the <see cref="IReadOnlyModel" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class RelationalAnnotationProvider : IRelationalAnnotationProvider
    {
        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public RelationalAnnotationProvider(RelationalAnnotationProviderDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
        }

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IRelationalModel model, bool designTime)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(ITable table, bool designTime)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IColumn column, bool designTime)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IView view, bool designTime)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IViewColumn column, bool designTime)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(ISqlQuery sqlQuery, bool designTime)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(ISqlQueryColumn column, bool designTime)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IStoreFunction function, bool designTime)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IFunctionColumn column, bool designTime)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IForeignKeyConstraint foreignKey, bool designTime)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(ITableIndex index, bool designTime)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IUniqueConstraint constraint, bool designTime)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(ISequence sequence, bool designTime)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(ICheckConstraint checkConstraint, bool designTime)
            => Enumerable.Empty<IAnnotation>();
    }
}
