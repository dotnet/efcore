// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A base implementation of <see cref="IModelSource" /> that produces a model based on the <see cref="DbSet{TEntity}" /> properties
    ///         exposed on the context. The model is cached to avoid recreating it every time it is requested.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class ModelSource : IModelSource
    {
        private readonly ConcurrentDictionary<object, IModel> _models = new ConcurrentDictionary<object, IModel>();
        protected virtual IDbSetFinder SetFinder { get; }
        protected virtual ICoreConventionSetBuilder CoreConventionSetBuilder { get; }

        protected virtual IModelCustomizer ModelCustomizer { get; }
        protected virtual IModelCacheKeyFactory ModelCacheKeyFactory { get; }

        protected ModelSource([NotNull] IDbSetFinder setFinder, [NotNull] ICoreConventionSetBuilder coreConventionSetBuilder, [NotNull] IModelCustomizer modelCustomizer, [NotNull] IModelCacheKeyFactory modelCacheKeyFactory)
        {
            Check.NotNull(setFinder, nameof(setFinder));
            Check.NotNull(coreConventionSetBuilder, nameof(coreConventionSetBuilder));
            Check.NotNull(modelCustomizer, nameof(modelCustomizer));
            Check.NotNull(modelCacheKeyFactory, nameof(modelCacheKeyFactory));

            SetFinder = setFinder;
            CoreConventionSetBuilder = coreConventionSetBuilder;
            ModelCustomizer = modelCustomizer;
            ModelCacheKeyFactory = modelCacheKeyFactory;
        }

        /// <summary>
        ///     Returns the model from the cache, or creates a model if it is not present in the cache.
        /// </summary>
        /// <param name="context"> The context the model is being produced for. </param>
        /// <param name="conventionSetBuilder"> The convention set to use when creating the model. </param>
        /// <param name="validator"> The validator to verify the model can be successfully used with the context. </param>
        /// <returns> The model to be used. </returns>
        public virtual IModel GetModel(DbContext context, IConventionSetBuilder conventionSetBuilder, IModelValidator validator)
            => _models.GetOrAdd(ModelCacheKeyFactory.Create(context), k => CreateModel(context, conventionSetBuilder, validator));

        /// <summary>
        ///     Creates the model. This method is called when the model was not found in the cache.
        /// </summary>
        /// <param name="context"> The context the model is being produced for. </param>
        /// <param name="conventionSetBuilder"> The convention set to use when creating the model. </param>
        /// <param name="validator"> The validator to verify the model can be successfully used with the context. </param>
        /// <returns> The model to be used. </returns>
        protected virtual IModel CreateModel(
            [NotNull] DbContext context,
            [CanBeNull] IConventionSetBuilder conventionSetBuilder,
            [NotNull] IModelValidator validator)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(validator, nameof(validator));

            var conventionSet = CreateConventionSet(conventionSetBuilder);

            var modelBuilder = new ModelBuilder(conventionSet);
            var internalModelBuilder = ((IInfrastructure<InternalModelBuilder>)modelBuilder).Instance;

            internalModelBuilder.Metadata.SetProductVersion(ProductInfo.GetVersion());

            FindSets(modelBuilder, context);

            ModelCustomizer.Customize(modelBuilder, context);

            internalModelBuilder.Validate();

            validator.Validate(modelBuilder.Model);

            return modelBuilder.Model;
        }

        /// <summary>
        ///     Creates the convention set to be used for the model. Uses the <see cref="CoreConventionSetBuilder" />
        ///     if <paramref name="conventionSetBuilder" /> is null.
        /// </summary>
        /// <param name="conventionSetBuilder"> The convention set builder to be used. </param>
        /// <returns> The convention set to be used. </returns>
        protected virtual ConventionSet CreateConventionSet([CanBeNull] IConventionSetBuilder conventionSetBuilder)
        {
            var conventionSet = CoreConventionSetBuilder.CreateConventionSet();
            return conventionSetBuilder == null
                ? conventionSet
                : conventionSetBuilder.AddConventions(conventionSet);
        }

        /// <summary>
        ///     Adds the entity types found in <see cref="DbSet{TEntity}" /> properties on the context to the model.
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <param name="context"></param>
        protected virtual void FindSets([NotNull] ModelBuilder modelBuilder, [NotNull] DbContext context)
        {
            foreach (var setInfo in SetFinder.FindSets(context))
            {
                modelBuilder.Entity(setInfo.EntityType);
            }
        }

    }
}

