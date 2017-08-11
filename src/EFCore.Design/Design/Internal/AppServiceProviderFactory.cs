// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class AppServiceProviderFactory
    {
        private readonly Assembly _startupAssembly;
        private readonly IOperationReporter _reporter;

        public AppServiceProviderFactory([NotNull] Assembly startupAssembly, [NotNull] IOperationReporter reporter)
        {
            _startupAssembly = startupAssembly;
            _reporter = reporter;
        }

        public virtual IServiceProvider Create([NotNull] string[] args)
        {
            _reporter.WriteVerbose(DesignStrings.FindingServiceProvider);

            return CreateFromBuildWebHost(args)
                   ?? CreateEmptyServiceProvider();
        }

        private IServiceProvider CreateFromBuildWebHost(string[] args)
        {
            _reporter.WriteVerbose(DesignStrings.FindingBuildWebHost);

            var programType = FindProgramClass();
            if (programType == null)
            {
                _reporter.WriteVerbose(DesignStrings.NoEntryPoint(_startupAssembly.GetName().Name));

                return null;
            }

            var buildWebHostMethod = programType.GetTypeInfo().GetDeclaredMethod("BuildWebHost");
            if (buildWebHostMethod == null)
            {
                _reporter.WriteVerbose(DesignStrings.NoBuildWebHost(programType.DisplayName()));

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
            _reporter.WriteVerbose(DesignStrings.UsingBuildWebHost(programType.ShortDisplayName()));

            try
            {
                var webHost = buildWebHostMethod.Invoke(null, new object[] { args });
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
                _reporter.WriteWarning(DesignStrings.InvokeBuildWebHostFailed(programType.ShortDisplayName(), ex.Message));

                return null;
            }
        }

        protected virtual Type FindProgramClass()
            => _startupAssembly.EntryPoint?.DeclaringType;

        private IServiceProvider CreateEmptyServiceProvider()
        {
            _reporter.WriteVerbose(DesignStrings.NoServiceProvider);

            return new ServiceCollection().BuildServiceProvider();
        }
    }
}
