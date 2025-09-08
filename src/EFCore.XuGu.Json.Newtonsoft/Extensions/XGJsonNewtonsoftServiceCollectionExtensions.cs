// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Query.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class XGJsonNewtonsoftServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds the services required for JSON.NET support (Newtonsoft.Json) in Pomelo's XuGu provider for Entity Framework Core.
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns> The same service collection so that multiple calls can be chained. </returns>
        public static IServiceCollection AddEntityFrameworkXGJsonNewtonsoft(
            [NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<IRelationalTypeMappingSourcePlugin, XGJsonNewtonsoftTypeMappingSourcePlugin>()
                .TryAdd<IMethodCallTranslatorPlugin, XGJsonNewtonsoftMethodCallTranslatorPlugin>()
                .TryAdd<IMemberTranslatorPlugin, XGJsonNewtonsoftMemberTranslatorPlugin>()
                .TryAddProviderSpecificServices(
                    x => x.TryAddScopedEnumerable<IXGJsonPocoTranslator, XGJsonNewtonsoftPocoTranslator>());

            return serviceCollection;
        }
    }
}
