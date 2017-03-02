// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class StartupInvoker
    {
        private readonly Type _startupType;
        private readonly Type _designTimeServicesType;
        private readonly string _environment;
        private readonly string _contentRootPath;
        private readonly string _startupAssemblyName;
        private readonly IOperationReporter _reporter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public StartupInvoker(
            [NotNull] IOperationReporter reporter,
            [NotNull] Assembly startupAssembly,
            [CanBeNull] string environment,
            [NotNull] string contentRootPath)
        {
            Check.NotNull(reporter, nameof(reporter));
            Check.NotNull(startupAssembly, nameof(startupAssembly));
            Check.NotEmpty(contentRootPath, nameof(contentRootPath));

            _reporter = reporter;

            _environment = !string.IsNullOrEmpty(environment)
                ? environment
                : "Development";

            _contentRootPath = contentRootPath;

            _startupAssemblyName = startupAssembly.GetName().Name;

            _startupType = startupAssembly.GetLoadableDefinedTypes().Where(t => typeof(IStartup).IsAssignableFrom(t.AsType()))
                .Concat(startupAssembly.GetLoadableDefinedTypes().Where(t => t.Name == "Startup" + _environment))
                .Concat(startupAssembly.GetLoadableDefinedTypes().Where(t => t.Name == "Startup"))
                .Select(t => t.AsType())
                .FirstOrDefault();

            _designTimeServicesType = startupAssembly.GetLoadableDefinedTypes()
                .Where(t => typeof(IDesignTimeServices).IsAssignableFrom(t.AsType())).Select(t => t.AsType())
                .FirstOrDefault()
                ?? _startupType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IServiceProvider ConfigureServices()
        {
            var services = ConfigureHostServices(new ServiceCollection());

            return Invoke(
                       _startupType,
                       new[] { "Configure" + _environment + "Services", "ConfigureServices" },
                       services) as IServiceProvider
                   ?? services.BuildServiceProvider();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IServiceCollection ConfigureDesignTimeServices([NotNull] IServiceCollection services)
            => ConfigureDesignTimeServices(_designTimeServicesType, services);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IServiceCollection ConfigureDesignTimeServices([CanBeNull] Type type, [NotNull] IServiceCollection services)
        {
            Invoke(type, new[] { "ConfigureDesignTimeServices" }, services);
            return services;
        }

        private object Invoke(Type type, string[] methodNames, IServiceCollection services)
        {
            if (type == null)
            {
                return null;
            }

            MethodInfo method = null;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < methodNames.Length; i++)
            {
                method = type.GetTypeInfo().GetDeclaredMethod(methodNames[i]);
                if (method != null)
                {
                    break;
                }
            }

            if (method == null)
            {
                return null;
            }

            try
            {
                var instance = !method.IsStatic
                    ? ActivatorUtilities.GetServiceOrCreateInstance(GetHostServices(), type)
                    : null;

                var parameters = method.GetParameters();
                var arguments = new object[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameterType = parameters[i].ParameterType;
                    arguments[i] = parameterType == typeof(IServiceCollection)
                        ? services
                        : ActivatorUtilities.GetServiceOrCreateInstance(GetHostServices(), parameterType);
                }

                return method.Invoke(instance, arguments);
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ex = ex.InnerException;
                }

                _reporter.WriteWarning(
                    DesignStrings.InvokeStartupMethodFailed(method.Name, type.ShortDisplayName(), ex.Message));
                _reporter.WriteVerbose(ex.ToString());

                return null;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IServiceCollection ConfigureHostServices([NotNull] IServiceCollection services)
        {
            services.AddSingleton<IHostingEnvironment>(
                new HostingEnvironment
                {
                    ContentRootPath = _contentRootPath,
                    EnvironmentName = _environment,
                    ApplicationName = _startupAssemblyName
                });

            services.AddLogging();
            services.AddOptions();

            return services;
        }

        private IServiceProvider GetHostServices()
            => ConfigureHostServices(new ServiceCollection()).BuildServiceProvider();
    }
}
