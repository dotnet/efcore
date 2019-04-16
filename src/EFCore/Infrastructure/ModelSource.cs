// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         An implementation of <see cref="IModelSource" /> that produces a model based on
    ///         the <see cref="DbSet{TEntity}" /> properties exposed on the context. The model is cached to avoid
    ///         recreating it every time it is requested.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class ModelSource : IModelSource
    {
        private readonly ConcurrentDictionary<object, Lazy<IModel>> _models = new ConcurrentDictionary<object, Lazy<IModel>>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ModelSource([NotNull] ModelSourceDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="ModelSource" />
        /// </summary>
        protected virtual ModelSourceDependencies Dependencies { get; }

        /// <summary>
        ///     Returns the model from the cache, or creates a model if it is not present in the cache.
        /// </summary>
        /// <param name="context"> The context the model is being produced for. </param>
        /// <param name="conventionSetBuilder"> The convention set to use when creating the model. </param>
        /// <param name="validator"> The validator to verify the model can be successfully used with the context. </param>
        /// <returns> The model to be used. </returns>
        public virtual IModel GetModel(DbContext context, IConventionSetBuilder conventionSetBuilder, IModelValidator validator)
            => _models.GetOrAdd(
                Dependencies.ModelCacheKeyFactory.Create(context),
                // Using a Lazy here so that OnModelCreating, etc. really only gets called once, since it may not be thread safe.
                k => new Lazy<IModel>(
                    () => CreateModel(context, conventionSetBuilder, validator),
                    LazyThreadSafetyMode.ExecutionAndPublication)).Value;

        /// <summary>
        ///     Creates the model. This method is called when the model was not found in the cache.
        /// </summary>
        /// <param name="context"> The context the model is being produced for. </param>
        /// <param name="conventionSetBuilder"> The convention set to use when creating the model. </param>
        /// <param name="validator"> The validator to verify the model can be successfully used with the context. </param>
        /// <returns> The model to be used. </returns>
        protected virtual IModel CreateModel(
            [NotNull] DbContext context,
            [NotNull] IConventionSetBuilder conventionSetBuilder,
            [NotNull] IModelValidator validator)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(validator, nameof(validator));

            var conventionSet = CreateConventionSet(conventionSetBuilder);
            conventionSet.ModelBuiltConventions.Add(new ValidatingConvention(validator));

            var modelBuilder = new ModelBuilder(conventionSet);

            Dependencies.ModelCustomizer.Customize(modelBuilder, context);

            return modelBuilder.FinalizeModel();
        }

        /// <summary>
        ///     Creates the convention set to be used for the model. Only uses the <see cref="CoreConventionSetBuilder" />
        ///     if <paramref name="conventionSetBuilder" /> is null.
        /// </summary>
        /// <param name="conventionSetBuilder"> The provider convention set builder to be used. </param>
        /// <returns> The convention set to be used. </returns>
        protected virtual ConventionSet CreateConventionSet([NotNull] IConventionSetBuilder conventionSetBuilder)
            => conventionSetBuilder.AddConventions(Dependencies.CoreConventionSetBuilder.CreateConventionSet());
    }
}
