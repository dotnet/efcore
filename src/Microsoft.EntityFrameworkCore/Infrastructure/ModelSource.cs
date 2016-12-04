// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

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
    public abstract class ModelSource : IModelSource, IServiceInjectionSite
    {
        private readonly ConcurrentDictionary<object, IModel> _models = new ConcurrentDictionary<object, IModel>();

        /// <summary>
        ///     Gets the <see cref="IDbSetFinder" /> that will locate the <see cref="DbSet{TEntity}" /> properties
        ///     on the derived context.
        /// </summary>
        protected virtual IDbSetFinder SetFinder { get; }

        /// <summary>
        ///     Gets the <see cref="ICoreConventionSetBuilder" /> that will build the conventions to be used
        ///     to build the model.
        /// </summary>
        protected virtual ICoreConventionSetBuilder CoreConventionSetBuilder { get; }

        /// <summary>
        ///     Gets the <see cref="CoreModelValidator" /> that will validate the built model.
        /// </summary>
        protected virtual CoreModelValidator CoreModelValidator { get; private set; }

        /// <summary>
        ///     Gets the <see cref="IModelCustomizer" /> that will perform additional configuration of the model
        ///     in addition to what is discovered by convention.
        /// </summary>
        protected virtual IModelCustomizer ModelCustomizer { get; }

        /// <summary>
        ///     Gets the <see cref="IModelCacheKeyFactory" /> that will create keys used to store and lookup models
        ///     the model cache.
        /// </summary>
        protected virtual IModelCacheKeyFactory ModelCacheKeyFactory { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Obsolete("Derived classes must be updated to call the new constructor with additional parameters.")]
        protected ModelSource(
            [NotNull] IDbSetFinder setFinder,
            [NotNull] ICoreConventionSetBuilder coreConventionSetBuilder,
            [NotNull] IModelCustomizer modelCustomizer,
            [NotNull] IModelCacheKeyFactory modelCacheKeyFactory)
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected ModelSource(
            [NotNull] IDbSetFinder setFinder,
            [NotNull] ICoreConventionSetBuilder coreConventionSetBuilder,
            [NotNull] IModelCustomizer modelCustomizer,
            [NotNull] IModelCacheKeyFactory modelCacheKeyFactory,
            [NotNull] CoreModelValidator coreModelValidator)
        {
            Check.NotNull(setFinder, nameof(setFinder));
            Check.NotNull(coreConventionSetBuilder, nameof(coreConventionSetBuilder));
            Check.NotNull(modelCustomizer, nameof(modelCustomizer));
            Check.NotNull(modelCacheKeyFactory, nameof(modelCacheKeyFactory));
            Check.NotNull(coreModelValidator, nameof(coreModelValidator));

            SetFinder = setFinder;
            CoreConventionSetBuilder = coreConventionSetBuilder;
            ModelCustomizer = modelCustomizer;
            ModelCacheKeyFactory = modelCacheKeyFactory;
            CoreModelValidator = coreModelValidator;
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
            CoreModelValidator.Validate(modelBuilder.Model);
            validator.Validate(modelBuilder.Model);

            return modelBuilder.Model;
        }

        /// <summary>
        ///     Creates the convention set to be used for the model. Only uses the <see cref="CoreConventionSetBuilder" />
        ///     if <paramref name="conventionSetBuilder" /> is null.
        /// </summary>
        /// <param name="conventionSetBuilder"> The provider convention set builder to be used. </param>
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
        /// <param name="modelBuilder"> The <see cref="ModelBuilder" /> being used to build the model. </param>
        /// <param name="context"> The context to find <see cref="DbSet{TEntity}" /> properties on. </param>
        protected virtual void FindSets([NotNull] ModelBuilder modelBuilder, [NotNull] DbContext context)
        {
            foreach (var setInfo in SetFinder.FindSets(context))
            {
                modelBuilder.Entity(setInfo.ClrType);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void IServiceInjectionSite.InjectServices(IServiceProvider serviceProvider)
            => CoreModelValidator = CoreModelValidator ?? serviceProvider.GetService<CoreModelValidator>();
    }
}
