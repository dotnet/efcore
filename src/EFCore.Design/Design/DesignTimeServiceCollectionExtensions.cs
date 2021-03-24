// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Extension methods for adding Entity Framework Core design-time services to an
    ///     <see cref="IServiceCollection" />.
    /// </summary>
    public static class DesignTimeServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds the Entity Framework Core design-time services.
        /// </summary>
        /// <param name="services"> The <see cref="IServiceCollection" /> the services will be added to. </param>
        /// <param name="reporter"> Used to report design-time messages. </param>
        /// <param name="applicationServiceProviderAccessor"> An accessor to the application service provider. </param>
        /// <returns> The <paramref name="services" />. This enables chaining additional method calls. </returns>
        public static IServiceCollection AddEntityFrameworkDesignTimeServices(
            this IServiceCollection services,
            IOperationReporter? reporter = null,
            Func<IServiceProvider>? applicationServiceProviderAccessor = null)
        {
            if (reporter == null)
            {
                reporter = new OperationReporter(handler: null);
            }

            return services
                .AddSingleton<AnnotationCodeGeneratorDependencies>()
                .AddSingleton<CSharpMigrationOperationGeneratorDependencies>()
                .AddSingleton<CSharpMigrationsGeneratorDependencies>()
                .AddSingleton<CSharpSnapshotGeneratorDependencies>()
                .AddSingleton<DiagnosticSource>(new DiagnosticListener(DbLoggerCategory.Name))
                .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
                .AddSingleton<ICandidateNamingService, CandidateNamingService>()
                .AddSingleton<IConstructorBindingFactory, ConstructorBindingFactory>()
                .AddSingleton<ICSharpDbContextGenerator, CSharpDbContextGenerator>()
                .AddSingleton<ICSharpEntityTypeGenerator, CSharpEntityTypeGenerator>()
                .AddSingleton<ICSharpHelper, CSharpHelper>()
                .AddSingleton<ICSharpMigrationOperationGenerator, CSharpMigrationOperationGenerator>()
                .AddSingleton<ICSharpSnapshotGenerator, CSharpSnapshotGenerator>()
                .AddSingleton<ICSharpUtilities, CSharpUtilities>()
                .AddSingleton<IDbContextLogger, NullDbContextLogger>()
                .AddSingleton(typeof(IDiagnosticsLogger<>), typeof(DiagnosticsLogger<>))
                .AddSingleton<IInterceptors, Interceptors>()
                .AddSingleton<ILoggingOptions, LoggingOptions>()
                .AddSingleton<IMemberClassifier, MemberClassifier>()
                .AddSingleton<IMigrationsCodeGenerator, CSharpMigrationsGenerator>()
                .AddSingleton<IMigrationsCodeGeneratorSelector, MigrationsCodeGeneratorSelector>()
                .AddSingleton<IModelCodeGenerator, CSharpModelGenerator>()
                .AddSingleton<IModelCodeGeneratorSelector, ModelCodeGeneratorSelector>()
                .AddSingleton<IModelRuntimeInitializer, ModelRuntimeInitializer>()
                .AddSingleton<IModelValidator, RelationalModelValidator>()
                .AddSingleton<INamedConnectionStringResolver>(
                    new DesignTimeConnectionStringResolver(applicationServiceProviderAccessor))
                .AddSingleton(reporter)
                .AddSingleton<IParameterBindingFactories, ParameterBindingFactories>()
                .AddSingleton<IPluralizer, HumanizerPluralizer>()
                .AddSingleton<IPropertyParameterBindingFactory, PropertyParameterBindingFactory>()
                .AddSingleton<IRegisteredServices>(new RegisteredServices(services.Select(s => s.ServiceType)))
                .AddSingleton<IReverseEngineerScaffolder, ReverseEngineerScaffolder>()
                .AddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>()
                .AddSingleton<IScaffoldingTypeMapper, ScaffoldingTypeMapper>()
                .AddSingleton<ITypeMappingSource>(p => p.GetRequiredService<IRelationalTypeMappingSource>())
                .AddSingleton<IValueConverterSelector, ValueConverterSelector>()
                .AddSingleton<MigrationsCodeGeneratorDependencies>()
                .AddSingleton<ModelCodeGeneratorDependencies>()
                .AddSingleton<ModelRuntimeInitializerDependencies>()
                .AddSingleton<ModelValidatorDependencies>()
                .AddSingleton<ProviderCodeGeneratorDependencies>()
                .AddSingleton<RelationalModelValidatorDependencies>()
                .AddSingleton<RelationalTypeMappingSourceDependencies>()
                .AddSingleton<RuntimeModelDependencies>()
                .AddSingleton<TypeMappingSourceDependencies>()
                .AddSingleton<ValueConverterSelectorDependencies>()
                .AddTransient<MigrationsScaffolderDependencies>()
                .AddTransient<IMigrationsScaffolder, MigrationsScaffolder>()
                .AddTransient<ISnapshotModelProcessor, SnapshotModelProcessor>()
                .AddLogging(b => b.SetMinimumLevel(LogLevel.Debug).AddProvider(new OperationLoggerProvider(reporter)));
        }

        /// <summary>
        ///     Adds services from the <see cref="DbContext" /> which are used at design time.
        /// </summary>
        /// <param name="services"> The <see cref="IServiceCollection" /> the services will be added to. </param>
        /// <param name="context"> The <see cref="DbContext" /> the services will be added from. </param>
        /// <returns> The <paramref name="services" />. This enables chaining additional method calls. </returns>
        public static IServiceCollection AddDbContextDesignTimeServices(
            this IServiceCollection services,
            DbContext context)
            => services
                .AddTransient(_ => context.GetService<ICurrentDbContext>())
                .AddTransient(_ => context.GetService<IDatabaseProvider>())
                .AddTransient(_ => context.GetService<IDbContextOptions>())
                .AddTransient(_ => context.GetService<IHistoryRepository>())
                .AddTransient(_ => context.GetService<IMigrationsAssembly>())
                .AddTransient(_ => context.GetService<IMigrationsIdGenerator>())
                .AddTransient(_ => context.GetService<IMigrationsModelDiffer>())
                .AddTransient(_ => context.GetService<IMigrator>())
                .AddTransient(_ => context.GetService<IRelationalTypeMappingSource>())
                .AddTransient(_ => context.GetService<IDesignTimeModel>().Model)
                .AddTransient(_ => context.GetService<IModelRuntimeInitializer>());
    }
}
