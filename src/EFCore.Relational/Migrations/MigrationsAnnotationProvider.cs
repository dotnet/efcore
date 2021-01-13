// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public MigrationsAnnotationProvider([NotNull] MigrationsAnnotationProviderDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
        }

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
    }
}
