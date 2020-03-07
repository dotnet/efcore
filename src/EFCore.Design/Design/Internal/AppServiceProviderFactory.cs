// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class AppServiceProviderFactory
    {
        private readonly Assembly _startupAssembly;
        private readonly IOperationReporter _reporter;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public AppServiceProviderFactory([NotNull] Assembly startupAssembly, [NotNull] IOperationReporter reporter)
        {
            _startupAssembly = startupAssembly;
            _reporter = reporter;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IServiceProvider Create([NotNull] string[] args)
        {
            _reporter.WriteVerbose(DesignStrings.FindingServiceProvider);

            return CreateFromHosting(args)
                   ?? CreateEmptyServiceProvider();
        }

        private IServiceProvider CreateFromHosting(string[] args)
        {
            _reporter.WriteVerbose(DesignStrings.FindingHostingServices);

            var serviceProviderFactory = HostFactoryResolver.ResolveServiceProviderFactory(_startupAssembly);
            if (serviceProviderFactory == null)
            {
                _reporter.WriteVerbose(DesignStrings.NoCreateHostBuilder);

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

            try
            {
                var services = serviceProviderFactory(args);
                if (services == null)
                {
                    _reporter.WriteWarning(DesignStrings.MalformedCreateHostBuilder);

                    return null;
                }

                _reporter.WriteVerbose(DesignStrings.UsingHostingServices);

                return services.CreateScope().ServiceProvider;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ex = ex.InnerException;
                }

                _reporter.WriteVerbose(ex.ToString());
                _reporter.WriteWarning(DesignStrings.InvokeCreateHostBuilderFailed(ex.Message));

                return null;
            }
        }

        private IServiceProvider CreateEmptyServiceProvider()
        {
            _reporter.WriteVerbose(DesignStrings.NoServiceProvider);

            return new ServiceCollection().BuildServiceProvider();
        }
    }
}
