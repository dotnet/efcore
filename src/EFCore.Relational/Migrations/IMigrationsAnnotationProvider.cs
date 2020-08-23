// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     <para>
    ///         A service typically implemented by database providers that gives access to annotations used by EF Core Migrations
    ///         when generating removal operations for various elements of the <see cref="IRelationalModel" />. The annotations
    ///         stored in the relational model are provided by <see cref="IRelationalAnnotationProvider" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IMigrationsAnnotationProvider
    {
        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="IRelationalModel" />
        ///     when it is being altered.
        /// </summary>
        /// <param name="model"> The database model. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] IRelationalModel model);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="ITable" />
        ///     when it is being removed.
        /// </summary>
        /// <param name="table"> The table. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] ITable table);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="IColumn" />
        ///     when it is being removed.
        /// </summary>
        /// <param name="column"> The column. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] IColumn column);

        /// <summary>
        ///     Gets provider-specific annotations for the given <see cref="IView" />
        ///     when it is being removed.
        /// </summary>
        /// <param name="view"> The view. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] IView view);

        /// <summary>
        ///     Gets provider-specific annotations for the given <see cref="IViewColumn" />
        ///     when it is being removed.
        /// </summary>
        /// <param name="column"> The column. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] IViewColumn column);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="IUniqueConstraint" />
        ///     when it is being removed.
        /// </summary>
        /// <param name="constraint"> The unique constraint. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] IUniqueConstraint constraint);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="ITableIndex" />
        ///     when it is being removed.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] ITableIndex index);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="IForeignKeyConstraint" />
        ///     when it is being removed.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] IForeignKeyConstraint foreignKey);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="ISequence" />
        ///     when it is being removed.
        /// </summary>
        /// <param name="sequence"> The sequence. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] ISequence sequence);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="ICheckConstraint" />
        ///     when it is being removed.
        /// </summary>
        /// <param name="checkConstraint"> The check constraint. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] ICheckConstraint checkConstraint);
    }
}
