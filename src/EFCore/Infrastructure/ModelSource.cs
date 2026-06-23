// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

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
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class ModelSource : IModelSource
{
    private readonly object _syncObject = new();

    /// <summary>
    ///     Creates a new <see cref="ModelSource" /> instance.
    /// </summary>
    /// <param name="dependencies">The dependencies to use.</param>
    public ModelSource(ModelSourceDependencies dependencies)
        => Dependencies = dependencies;

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ModelSourceDependencies Dependencies { get; }

    /// <summary>
    ///     Gets the model to be used.
    /// </summary>
    /// <param name="context">The context the model is being produced for.</param>
    /// <param name="modelCreationDependencies">The dependencies object used during the creation of the model.</param>
    /// <param name="designTime">Whether the model should contain design-time configuration.</param>
    /// <returns>The model to be used.</returns>
    public virtual IModel GetModel(
        DbContext context,
        ModelCreationDependencies modelCreationDependencies,
        bool designTime)
    {
        var cache = Dependencies.MemoryCache;
        var cacheKey = Dependencies.ModelCacheKeyFactory.Create(context, designTime);
        if (!cache.TryGetValue(cacheKey, out IModel? model))
        {
            // Make sure OnModelCreating really only gets called once, since it may not be thread safe.
            lock (_syncObject)
            {
                if (!cache.TryGetValue(cacheKey, out model))
                {
                    var designTimeModel = CreateModel(context, modelCreationDependencies, designTime: true);

                    var runtimeModel = (IModel)designTimeModel.FindRuntimeAnnotationValue(CoreAnnotationNames.ReadOnlyModel)!;

                    var designTimeKey = designTime ? cacheKey : Dependencies.ModelCacheKeyFactory.Create(context, designTime: true);
                    var runtimeKey = designTime ? Dependencies.ModelCacheKeyFactory.Create(context, designTime: false) : cacheKey;

                    cache.Set(
                        designTimeKey, designTimeModel, new MemoryCacheEntryOptions { Size = 150, Priority = CacheItemPriority.High });
                    cache.Set(runtimeKey, runtimeModel, new MemoryCacheEntryOptions { Size = 100, Priority = CacheItemPriority.High });

                    model = designTime ? designTimeModel : runtimeModel;
                }
            }
        }

        return model!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual IModel CreateModel(
        DbContext context,
        ModelCreationDependencies modelCreationDependencies,
        bool designTime)
    {
        var model = CreateModel(context, modelCreationDependencies.ConventionSetBuilder, modelCreationDependencies.ModelDependencies);
        return modelCreationDependencies.ModelRuntimeInitializer.Initialize(
            model, designTime, modelCreationDependencies.ValidationLogger);
    }

    /// <summary>
    ///     Creates the model. This method is called when the model was not found in the cache.
    /// </summary>
    /// <param name="context">The context the model is being produced for.</param>
    /// <param name="conventionSetBuilder">The convention set to use when creating the model.</param>
    /// <param name="modelDependencies">The dependencies object for the model.</param>
    /// <returns>The model to be used.</returns>
    protected virtual IModel CreateModel(
        DbContext context,
        IConventionSetBuilder conventionSetBuilder,
        ModelDependencies modelDependencies)
    {
        Check.DebugAssert(context != null, "context == null");

        var modelConfigurationBuilder = new ModelConfigurationBuilder(
            conventionSetBuilder.CreateConventionSet(),
            context.GetInfrastructure());

        context.ConfigureConventions(modelConfigurationBuilder);

        var modelBuilder = modelConfigurationBuilder.CreateModelBuilder(modelDependencies);

        Dependencies.ModelCustomizer.Customize(modelBuilder, context);

        return (IModel)modelBuilder.Model;
    }
}
