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
    ///         A service typically implemented by database providers that gives access to annotations
    ///         used by EF Core Migrations on various elements of the <see cref="IModel" />.
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
        ///     Gets provider-specific Migrations annotations for the given <see cref="IModel" />.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> For([NotNull] IModel model);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="IIndex" />.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> For([NotNull] IIndex index);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="IProperty" />.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> For([NotNull] IProperty property);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="IKey" />.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> For([NotNull] IKey key);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="IForeignKey" />.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> For([NotNull] IForeignKey foreignKey);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="IEntityType" />.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> For([NotNull] IEntityType entityType);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="ISequence" />.
        /// </summary>
        /// <param name="sequence"> The sequence. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> For([NotNull] ISequence sequence);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="ICheckConstraint" />.
        /// </summary>
        /// <param name="checkConstraint"> The check constraint. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> For([NotNull] ICheckConstraint checkConstraint);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="IModel" />
        ///     when it is being removed/altered.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] IModel model);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="IIndex" />
        ///     when it is being removed/altered.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] IIndex index);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="IProperty" />
        ///     when it is being removed/altered.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] IProperty property);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="IKey" />
        ///     when it is being removed/altered.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] IKey key);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="IForeignKey" />
        ///     when it is being removed/altered.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] IForeignKey foreignKey);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="IEntityType" />
        ///     when it is being removed/altered.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] IEntityType entityType);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="ISequence" />
        ///     when it is being removed/altered.
        /// </summary>
        /// <param name="sequence"> The sequence. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] ISequence sequence);

        /// <summary>
        ///     Gets provider-specific Migrations annotations for the given <see cref="ICheckConstraint" />
        ///     when it is being removed/altered.
        /// </summary>
        /// <param name="checkConstraint"> The check constraint. </param>
        /// <returns> The annotations. </returns>
        IEnumerable<IAnnotation> ForRemove([NotNull] ICheckConstraint checkConstraint);
    }
}
