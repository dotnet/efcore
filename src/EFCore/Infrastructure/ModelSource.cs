// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Text;
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
    private Dictionary<object, IModel?> _dictionary = new();

    /// <summary>
    ///     Creates a new <see cref="ModelSource" /> instance.
    /// </summary>
    /// <param name="dependencies">The dependencies to use.</param>
    public ModelSource(ModelSourceDependencies dependencies)
    {
        Dependencies = dependencies;
    }

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
                    if (_dictionary.TryGetValue(cacheKey, out var existingModel))
                    {
                        var builder = new StringBuilder();
                        builder.AppendLine(
                            cacheKey is ModelCacheKey modelCacheKey
                                ? $"Dictionary contains key not in cache: {modelCacheKey.ContextType.Name} (Design time: {modelCacheKey.DesignTime})"
                                : $"Dictionary contains key not in cache: {cacheKey.GetType()}: {cacheKey}");

                        var memoryCache = (MemoryCache)cache;
                        builder.AppendLine($"Count: {memoryCache.Count}");

                        var size = typeof(MemoryCache).GetRuntimeProperties().Single(e => e.Name == "Size")
                            .GetValue(memoryCache)!;
                        builder.AppendLine($"Size: {size}");

                        var options = (MemoryCacheOptions)typeof(MemoryCache).GetRuntimeFields().Single(e => e.Name == "_options")
                            .GetValue(memoryCache)!;
                        builder.AppendLine($"SizeLimit: {options.SizeLimit}");

                        var coherentState = typeof(MemoryCache).GetRuntimeFields().Single(e => e.Name == "_coherentState")
                            .GetValue(memoryCache)!;
                        var entries = (IEnumerable)coherentState.GetType().GetRuntimeFields().Single(e => e.Name == "_entries")
                            .GetValue(coherentState)!;

                        foreach (var entry in entries)
                        {
                            var key = entry.GetType().GetProperty("Key")!.GetValue(entry);
                            var cacheEntry = entry.GetType().GetProperty("Value")!.GetValue(entry)!;
                            var value = cacheEntry.GetType().GetRuntimeFields().Single(e => e.Name == "_value").GetValue(cacheEntry);

                            if (key is ModelCacheKey modelCacheKeyFromCache)
                            {
                                builder.AppendLine(
                                    $"{modelCacheKeyFromCache.ContextType.Name} (Design time: {modelCacheKeyFromCache.DesignTime}: {value}");

                                if (key.Equals(cacheKey))
                                {
                                    builder.AppendLine("Match!");
                                }
                            }
                            else
                            {
                                builder.AppendLine($"{key}: {value}");
                            }
                        }

                        throw new Exception(builder.ToString());
                    }

                    model = CreateModel(
                        context, modelCreationDependencies.ConventionSetBuilder, modelCreationDependencies.ModelDependencies);

                    model = modelCreationDependencies.ModelRuntimeInitializer.Initialize(
                        model, designTime, modelCreationDependencies.ValidationLogger);

                    model = cache.Set(cacheKey, model, new MemoryCacheEntryOptions { Size = 100, Priority = CacheItemPriority.High });

                    _dictionary[cacheKey] = model;
                }
            }
        }

        return model!;
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
