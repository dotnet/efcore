// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class ScaffoldingServiceCollectionExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IServiceCollection AddScaffolding([NotNull] this IServiceCollection serviceCollection)
            => serviceCollection.AddSingleton<IFileService, FileSystemFileService>()
                .AddSingleton<RelationalTypeMapperDependencies>()
                .AddSingleton<ReverseEngineeringGenerator>()
                .AddSingleton<ScaffoldingUtilities>()
                .AddSingleton<CandidateNamingService>()
                .AddSingleton<IPluralizer, NullPluralizer>()
                .AddSingleton<CSharpUtilities>()
                .AddSingleton<ConfigurationFactory>()
                .AddSingleton<DbContextWriter>()
                .AddSingleton<EntityTypeWriter>()
                .AddSingleton<CodeWriter, StringBuilderCodeWriter>()
                .AddSingleton<ILoggingOptions, LoggingOptions>()
                .AddSingleton<DiagnosticSource>(new DiagnosticListener(DbLoggerCategory.Name))
                .AddSingleton(typeof(IDiagnosticsLogger<>), typeof(DiagnosticsLogger<>));
    }
}
