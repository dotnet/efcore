// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class PrecompiledQueryFactoryRegistry
{
    public static bool ArePrecompiledQueriesEnabled { get; private set; }

    public static void RegisterBootstrapper(Type contextType, Action<DbContext, ConcurrentDictionary<object, Func<DbContext, Delegate>>> bootstrapper)
    {
        var contextTypeEntry = Registry.GetOrAdd(contextType, _ => new());

        lock (contextTypeEntry.Bootstrappers)
        {
            contextTypeEntry.Bootstrappers.Add(bootstrapper);
        }

        ArePrecompiledQueriesEnabled = true;
    }

    private static readonly ConcurrentDictionary<Type, ContextTypeEntry> Registry = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool TryGetPrecompiledQueryFactory(
        DbContext context,
        object queryCacheKey,
        [NotNullWhen(true)] out Func<DbContext, Delegate>? precompiledQueryFactory)
    {
        if (Registry.TryGetValue(context.GetType(), out var contextTypeEntry))
        {
            if (contextTypeEntry.Factories.TryGetValue(queryCacheKey, out precompiledQueryFactory))
            {
                return true;
            }

            // We haven't found a factory for the given cache key. Check if there are any un-applied boostrappers and apply them
            // (startup flow)
            if (contextTypeEntry.Bootstrappers.Count > 0)
            {
                lock (contextTypeEntry.Bootstrappers)
                {
                    foreach (var bootstrapper in contextTypeEntry.Bootstrappers)
                    {
                        bootstrapper(context, contextTypeEntry.Factories);
                    }

                    contextTypeEntry.Bootstrappers.Clear();
                }

                // And retry looking up the factory after bootstrapping
                if (contextTypeEntry.Factories.TryGetValue(queryCacheKey, out precompiledQueryFactory))
                {
                    return true;
                }
            }
        }


        precompiledQueryFactory = null;
        return false;
    }

    private readonly struct ContextTypeEntry
    {
        public ContextTypeEntry()
        {
        }

        public List<Action<DbContext, ConcurrentDictionary<object, Func<DbContext, Delegate>>>> Bootstrappers { get; } = new();

        /// <summary>
        ///     Maps query cache keys to a factory method that can create a query executor given a context instance.
        /// </summary>
        public ConcurrentDictionary<object, Func<DbContext, Delegate>> Factories { get; } = new();
    }
}
