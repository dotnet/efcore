// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Query.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class XGJsonMicrosoftServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds the services required for Microsoft JSON support (System.Text.Json) in Pomelo's XuGu provider for Entity Framework Core.
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns> The same service collection so that multiple calls can be chained. </returns>
        public static IServiceCollection AddEntityFrameworkXGJsonMicrosoft(
            [NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<IRelationalTypeMappingSourcePlugin, XGJsonMicrosoftTypeMappingSourcePlugin>()
                .TryAdd<IMethodCallTranslatorPlugin, XGJsonMicrosoftMethodCallTranslatorPlugin>()
                .TryAdd<IMemberTranslatorPlugin, XGJsonMicrosoftMemberTranslatorPlugin>()
                .TryAddProviderSpecificServices(
                    x => x.TryAddScopedEnumerable<IXGJsonPocoTranslator, XGJsonMicrosoftPocoTranslator>());

            return serviceCollection;
        }
    }
}
