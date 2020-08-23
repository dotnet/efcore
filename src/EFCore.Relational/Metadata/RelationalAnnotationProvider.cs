// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         A base class inherited by database providers that gives access to annotations
    ///         used by relational EF Core components on various elements of the <see cref="IModel" />.
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
        public RelationalAnnotationProvider([NotNull] RelationalAnnotationProviderDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
        }

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IRelationalModel model)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(ITable table)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IColumn column)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IView view)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IViewColumn column)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(ISqlQuery sqlQuery)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(ISqlQueryColumn column)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IStoreFunction function)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IFunctionColumn column)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IForeignKeyConstraint foreignKey)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(ITableIndex index)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(IUniqueConstraint constraint)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(ISequence sequence)
            => Enumerable.Empty<IAnnotation>();

        /// <inheritdoc />
        public virtual IEnumerable<IAnnotation> For(ICheckConstraint checkConstraint)
            => Enumerable.Empty<IAnnotation>();
    }
}
