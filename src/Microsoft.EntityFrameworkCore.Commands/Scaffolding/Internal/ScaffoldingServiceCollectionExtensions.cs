// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public static class ScaffoldingServiceCollectionExtensions
    {
        public static IServiceCollection AddScaffolding([NotNull] this IServiceCollection serviceCollection)
            => serviceCollection.AddSingleton<IFileService, FileSystemFileService>()
                .AddSingleton<ReverseEngineeringGenerator>()
                .AddSingleton<ScaffoldingUtilities>()
                .AddSingleton<CSharpUtilities>()
                .AddSingleton<ConfigurationFactory>()
                .AddSingleton<DbContextWriter>()
                .AddSingleton<EntityTypeWriter>()
                .AddSingleton<CodeWriter, StringBuilderCodeWriter>();
    }
}
