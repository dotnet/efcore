// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        public static IServiceCollection AddScaffolding([NotNull] this IServiceCollection serviceCollection, [NotNull] IOperationReporter reporter)
            => serviceCollection
                .AddSingleton<AnnotationCodeGeneratorDependencies>()
                .AddSingleton<IFileService, FileSystemFileService>()
                .AddSingleton<RelationalTypeMapperDependencies>()
                .AddSingleton<IReverseEngineerScaffolder, ReverseEngineerScaffolder>()
                .AddSingleton<ICandidateNamingService, CandidateNamingService>()
                .AddSingleton<IPluralizer, NullPluralizer>()
                .AddSingleton<ICSharpUtilities, CSharpUtilities>()
                .AddSingleton<ICSharpDbContextGenerator, CSharpDbContextGenerator>()
                .AddSingleton<ICSharpEntityTypeGenerator, CSharpEntityTypeGenerator>()
                .AddSingleton<IScaffoldingTypeMapper, ScaffoldingTypeMapper>()
                .AddSingleton<ScaffoldingCodeGeneratorSelector>()
                .AddSingleton<IScaffoldingCodeGenerator, CSharpScaffoldingGenerator>()
                .AddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>()
                .AddSingleton<ILoggingOptions, LoggingOptions>()
                .AddSingleton<DiagnosticSource>(new DiagnosticListener(DbLoggerCategory.Name))
                .AddSingleton(typeof(IDiagnosticsLogger<>), typeof(DiagnosticsLogger<>))
                .AddLogging(b => b.SetMinimumLevel(LogLevel.Debug).AddProvider(new OperationLoggerProvider(reporter)));
    }
}
