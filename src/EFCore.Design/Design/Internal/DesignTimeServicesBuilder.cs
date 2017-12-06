// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DesignTimeServicesBuilder
    {
        private readonly Assembly _startupAssembly;
        private readonly IOperationReporter _reporter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DesignTimeServicesBuilder([NotNull] Assembly startupAssembly, [NotNull] IOperationReporter reporter)
        {
            _startupAssembly = startupAssembly;
            _reporter = reporter;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IServiceProvider Build([NotNull] DbContext context)
        {
            Check.NotNull(context, nameof(context));

            var services = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices(_reporter)
                .AddDbContextDesignTimeServices(context);
            ConfigureProviderServices(context.GetService<IDatabaseProvider>().Name, services);
            ConfigureUserServices(services);

            return services.BuildServiceProvider();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IServiceProvider Build([NotNull] string provider)
        {
            Check.NotEmpty(provider, nameof(provider));

            var services = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices(_reporter);
            ConfigureProviderServices(provider, services, throwOnError: true);
            ConfigureUserServices(services);

            return services.BuildServiceProvider();
        }

        private IServiceCollection ConfigureUserServices(IServiceCollection services)
        {
            _reporter.WriteVerbose(DesignStrings.FindingDesignTimeServices(_startupAssembly.GetName().Name));

            var designTimeServicesType = _startupAssembly.GetLoadableDefinedTypes()
                .Where(t => typeof(IDesignTimeServices).GetTypeInfo().IsAssignableFrom(t)).Select(t => t.AsType())
                .FirstOrDefault();
            if (designTimeServicesType == null)
            {
                _reporter.WriteVerbose(DesignStrings.NoDesignTimeServices);

                return services;
            }

            _reporter.WriteVerbose(DesignStrings.UsingDesignTimeServices(designTimeServicesType.ShortDisplayName()));

            return ConfigureDesignTimeServices(designTimeServicesType, services);
        }

        private IServiceCollection ConfigureProviderServices(string provider, IServiceCollection services, bool throwOnError = false)
        {
            _reporter.WriteVerbose(DesignStrings.FindingProviderServices(provider));

            Assembly providerAssembly;
            try
            {
                providerAssembly = Assembly.Load(new AssemblyName(provider));
            }
            catch (Exception ex)
            {
                var message = DesignStrings.CannotFindRuntimeProviderAssembly(provider);

                if (!throwOnError)
                {
                    _reporter.WriteVerbose(message);

                    return services;
                }

                throw new OperationException(message, ex);
            }

            var providerServicesAttribute = providerAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>();
            if (providerServicesAttribute == null)
            {
                var message = DesignStrings.CannotFindDesignTimeProviderAssemblyAttribute(
                    nameof(DesignTimeProviderServicesAttribute),
                    provider);

                if (!throwOnError)
                {
                    _reporter.WriteVerbose(message);

                    return services;
                }

                throw new InvalidOperationException(message);
            }

            var designTimeServicesType = providerAssembly.GetType(
                providerServicesAttribute.TypeName,
                throwOnError: true,
                ignoreCase: false);

            _reporter.WriteVerbose(DesignStrings.UsingProviderServices(provider));

            return ConfigureDesignTimeServices(designTimeServicesType, services);
        }

        private static IServiceCollection ConfigureDesignTimeServices(
            Type designTimeServicesType,
            IServiceCollection services)
        {
            Debug.Assert(designTimeServicesType != null, "designTimeServicesType is null.");

            var designTimeServices = (IDesignTimeServices)Activator.CreateInstance(designTimeServicesType);
            designTimeServices.ConfigureDesignTimeServices(services);

            return services;
        }
    }
}
