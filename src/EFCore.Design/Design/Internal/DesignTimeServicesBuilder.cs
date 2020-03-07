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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class DesignTimeServicesBuilder
    {
        private readonly Assembly _assembly;
        private readonly Assembly _startupAssembly;
        private readonly IOperationReporter _reporter;
        private readonly string[] _args;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public DesignTimeServicesBuilder(
            [NotNull] Assembly assembly,
            [NotNull] Assembly startupAssembly,
            [NotNull] IOperationReporter reporter,
            [NotNull] string[] args)
        {
            _startupAssembly = startupAssembly;
            _reporter = reporter;
            _args = args;
            _assembly = assembly;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IServiceProvider Build([NotNull] DbContext context)
        {
            Check.NotNull(context, nameof(context));

            var services = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices(_reporter)
                .AddDbContextDesignTimeServices(context);
            var provider = context.GetService<IDatabaseProvider>().Name;
            ConfigureProviderServices(provider, services);
            ConfigureReferencedServices(services, provider);
            ConfigureUserServices(services);

            return services.BuildServiceProvider();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IServiceProvider Build([NotNull] string provider)
        {
            Check.NotEmpty(provider, nameof(provider));

            var services = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices(_reporter, GetApplicationServices);
            ConfigureProviderServices(provider, services, throwOnError: true);
            ConfigureReferencedServices(services, provider);
            ConfigureUserServices(services);

            return services.BuildServiceProvider();
        }

        private IServiceProvider GetApplicationServices()
            => new AppServiceProviderFactory(_startupAssembly, _reporter).Create(_args);

        private void ConfigureUserServices(IServiceCollection services)
        {
            _reporter.WriteVerbose(DesignStrings.FindingDesignTimeServices(_startupAssembly.GetName().Name));

            var designTimeServicesType = _startupAssembly.GetLoadableDefinedTypes()
                .Where(t => typeof(IDesignTimeServices).GetTypeInfo().IsAssignableFrom(t)).Select(t => t.AsType())
                .FirstOrDefault();
            if (designTimeServicesType == null)
            {
                _reporter.WriteVerbose(DesignStrings.NoDesignTimeServices);

                return;
            }

            _reporter.WriteVerbose(DesignStrings.UsingDesignTimeServices(designTimeServicesType.ShortDisplayName()));

            ConfigureDesignTimeServices(designTimeServicesType, services);
        }

        private void ConfigureReferencedServices(IServiceCollection services, string provider)
        {
            _reporter.WriteVerbose(DesignStrings.FindingReferencedServices(_startupAssembly.GetName().Name));

            var references = _startupAssembly.GetCustomAttributes<DesignTimeServicesReferenceAttribute>()
                .Concat(_assembly.GetCustomAttributes<DesignTimeServicesReferenceAttribute>())
                .Distinct()
                .ToList();

            if (references.Count == 0)
            {
                _reporter.WriteVerbose(DesignStrings.NoReferencedServices);

                return;
            }

            foreach (var reference in references)
            {
                if (reference.ForProvider != null
                    && !string.Equals(reference.ForProvider, provider, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var designTimeServicesType = Type.GetType(reference.TypeName, throwOnError: true);

                _reporter.WriteVerbose(
                    DesignStrings.UsingReferencedServices(designTimeServicesType.Assembly.GetName().Name));

                ConfigureDesignTimeServices(designTimeServicesType, services);
            }
        }

        private void ConfigureProviderServices(string provider, IServiceCollection services, bool throwOnError = false)
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

                    return;
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

                    return;
                }

                throw new InvalidOperationException(message);
            }

            var designTimeServicesType = providerAssembly.GetType(
                providerServicesAttribute.TypeName,
                throwOnError: true,
                ignoreCase: false);

            _reporter.WriteVerbose(DesignStrings.UsingProviderServices(provider));

            ConfigureDesignTimeServices(designTimeServicesType, services);
        }

        private static void ConfigureDesignTimeServices(
            Type designTimeServicesType,
            IServiceCollection services)
        {
            Debug.Assert(designTimeServicesType != null, "designTimeServicesType is null.");

            var designTimeServices = (IDesignTimeServices)Activator.CreateInstance(designTimeServicesType);
            designTimeServices.ConfigureDesignTimeServices(services);
        }
    }
}
