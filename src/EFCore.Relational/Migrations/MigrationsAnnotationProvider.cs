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
    ///         A base class inherited by database providers that gives access to annotations
    ///         used by EF Core Migrations on various elements of the <see cref="IModel" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
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

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="IModel" />.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> For(IModel model) => Enumerable.Empty<IAnnotation>();

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="IEntityType" />.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> For(IEntityType entityType) => Enumerable.Empty<IAnnotation>();

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="IForeignKey" />.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> For(IForeignKey foreignKey) => Enumerable.Empty<IAnnotation>();

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="IIndex" />.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> For(IIndex index) => Enumerable.Empty<IAnnotation>();

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="IKey" />.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> For(IKey key) => Enumerable.Empty<IAnnotation>();

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="IProperty" />.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> For(IProperty property) => Enumerable.Empty<IAnnotation>();

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="ISequence" />.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="sequence"> The sequence. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> For(ISequence sequence) => Enumerable.Empty<IAnnotation>();

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="ICheckConstraint" />.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="checkConstraint"> The check constraint. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> For(ICheckConstraint checkConstraint) => Enumerable.Empty<IAnnotation>();

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="IModel" />
        ///         when it is being removed/altered.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> ForRemove(IModel model) => Enumerable.Empty<IAnnotation>();

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="IEntityType" />
        ///         when it is being removed/altered.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> ForRemove(IEntityType entityType) => Enumerable.Empty<IAnnotation>();

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="IForeignKey" />
        ///         when it is being removed/altered.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> ForRemove(IForeignKey foreignKey) => Enumerable.Empty<IAnnotation>();

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="IIndex" />
        ///         when it is being removed/altered.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> ForRemove(IIndex index) => Enumerable.Empty<IAnnotation>();

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="IKey" />
        ///         when it is being removed/altered.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> ForRemove(IKey key) => Enumerable.Empty<IAnnotation>();

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="IProperty" />
        ///         when it is being removed/altered.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> ForRemove(IProperty property) => Enumerable.Empty<IAnnotation>();

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="ISequence" />
        ///         when it is being removed/altered.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="sequence"> The sequence. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> ForRemove(ISequence sequence) => Enumerable.Empty<IAnnotation>();

        /// <summary>
        ///     <para>
        ///         Gets provider-specific Migrations annotations for the given <see cref="ICheckConstraint" />
        ///         when it is being removed/altered.
        ///     </para>
        ///     <para>
        ///         The default implementation returns an empty collection.
        ///     </para>
        /// </summary>
        /// <param name="checkConstraint"> The check constraint. </param>
        /// <returns> The annotations. </returns>
        public virtual IEnumerable<IAnnotation> ForRemove(ICheckConstraint checkConstraint) => Enumerable.Empty<IAnnotation>();
    }
}
