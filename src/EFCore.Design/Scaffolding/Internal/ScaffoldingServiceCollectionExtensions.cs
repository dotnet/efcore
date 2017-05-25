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
                .AddSingleton<DbContextScaffolder>()
                .AddSingleton<CandidateNamingService>()
                .AddSingleton<IPluralizer, NullPluralizer>()
                .AddSingleton<CSharpUtilities>()
                .AddSingleton<CSharpDbContextGenerator>()
                .AddSingleton<CSharpEntityTypeGenerator>()
                .AddSingleton<ScaffoldingCodeGenerator, CSharpScaffoldingGenerator>()
                .AddSingleton<ILoggingOptions, LoggingOptions>()
                .AddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>()
                .AddSingleton<DiagnosticSource>(new DiagnosticListener(DbLoggerCategory.Root))
                .AddSingleton(typeof(IDiagnosticsLogger<>), typeof(DiagnosticsLogger<>));
    }
}
