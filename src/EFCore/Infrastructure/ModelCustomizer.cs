// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Builds the model for a given context. This default implementation builds the model by calling
    ///         <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> on the context.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class ModelCustomizer : IModelCustomizer
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelCustomizer" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public ModelCustomizer([NotNull] ModelCustomizerDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="ModelCustomizer" />
        /// </summary>
        protected virtual ModelCustomizerDependencies Dependencies { get; }

        /// <summary>
        ///     Performs additional configuration of the model in addition to what is discovered by convention. This default implementation
        ///     builds the model for a given context by calling <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" />
        ///     on the context.
        /// </summary>
        /// <param name="modelBuilder">
        ///     The builder being used to construct the model.
        /// </param>
        /// <param name="context">
        ///     The context instance that the model is being created for.
        /// </param>
        public virtual void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            FindSets(modelBuilder, context);

            context.OnModelCreating(modelBuilder);
        }

        /// <summary>
        ///     Adds the entity types found in <see cref="DbSet{TEntity}" /> properties on the context to the model.
        /// </summary>
        /// <param name="modelBuilder"> The <see cref="ModelBuilder" /> being used to build the model. </param>
        /// <param name="context"> The context to find <see cref="DbSet{TEntity}" /> properties on. </param>
        protected virtual void FindSets([NotNull] ModelBuilder modelBuilder, [NotNull] DbContext context)
        {
            foreach (var setInfo in Dependencies.SetFinder.FindSets(context))
            {
                if (setInfo.IsKeyless)
                {
                    modelBuilder.Entity(setInfo.ClrType).HasNoKey();
                }
                else
                {
                    modelBuilder.Entity(setInfo.ClrType);
                }
            }
        }
    }
}
