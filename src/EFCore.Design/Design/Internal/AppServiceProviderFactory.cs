// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting.WebHostBuilderFactory;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class AppServiceProviderFactory
    {
        private readonly Assembly _startupAssembly;
        private readonly IOperationReporter _reporter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public AppServiceProviderFactory([NotNull] Assembly startupAssembly, [NotNull] IOperationReporter reporter)
        {
            _startupAssembly = startupAssembly;
            _reporter = reporter;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IServiceProvider Create([NotNull] string[] args)
        {
            _reporter.WriteVerbose(DesignStrings.FindingServiceProvider);

            return CreateFromBuildWebHost(args)
                   ?? CreateEmptyServiceProvider();
        }

        private IServiceProvider CreateFromBuildWebHost(string[] args)
        {
            _reporter.WriteVerbose(DesignStrings.FindingBuildWebHost);

            var webHostFactoryResult = WebHostFactoryResolver.ResolveWebHostFactory<object, object>(_startupAssembly);
            switch (webHostFactoryResult.ResultKind)
            {
                case FactoryResolutionResultKind.Success:
                    break;
                case FactoryResolutionResultKind.NoEntryPoint:
                    _reporter.WriteVerbose(DesignStrings.NoEntryPoint(_startupAssembly.GetName().Name));
                    return null;
                case FactoryResolutionResultKind.NoCreateWebHostBuilder:
                case FactoryResolutionResultKind.NoBuildWebHost:
                    _reporter.WriteVerbose(DesignStrings.NoBuildWebHost(webHostFactoryResult.ProgramType.DisplayName()));
                    return null;
                default:
                    Debug.Fail("Unexpected value: " + webHostFactoryResult.ResultKind);
                    return null;
            }

            // TODO: Remove when dotnet/cli#6617 is fixed
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == null)
            {
                environment = "Development";
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
            }

            _reporter.WriteVerbose(DesignStrings.UsingEnvironment(environment));
            _reporter.WriteVerbose(DesignStrings.UsingBuildWebHost(webHostFactoryResult.ProgramType.ShortDisplayName()));

            try
            {
                var webHost = webHostFactoryResult.WebHostFactory(args);
                var webHostType = webHost.GetType();
                var servicesProperty = webHostType.GetTypeInfo().GetDeclaredProperty("Services");
                var services = (IServiceProvider)servicesProperty.GetValue(webHost);

                return services.CreateScope().ServiceProvider;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ex = ex.InnerException;
                }

                _reporter.WriteVerbose(ex.ToString());
                _reporter.WriteWarning(DesignStrings.InvokeBuildWebHostFailed(webHostFactoryResult.ProgramType.ShortDisplayName(), ex.Message));

                return null;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual Type FindProgramClass()
            => _startupAssembly.EntryPoint?.DeclaringType;

        private IServiceProvider CreateEmptyServiceProvider()
        {
            _reporter.WriteVerbose(DesignStrings.NoServiceProvider);

            return new ServiceCollection().BuildServiceProvider();
        }
    }
}
