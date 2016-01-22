// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class StartupInvoker
    {
        private readonly Type _startupType;
        private readonly string _environment;

        public StartupInvoker(
            [NotNull] string startupAssemblyName,
            [CanBeNull] string environment)
        {
            Check.NotEmpty(startupAssemblyName, nameof(startupAssemblyName));

            _environment = !string.IsNullOrEmpty(environment)
                ? environment
                : "Development";

            var startupAssembly = Assembly.Load(new AssemblyName(startupAssemblyName));
            _startupType = startupAssembly.DefinedTypes.Where(t => t.Name == "Startup" + _environment)
                .Concat(startupAssembly.DefinedTypes.Where(t => t.Name == "Startup"))
                .Concat(startupAssembly.DefinedTypes.Where(t => t.Name == "Program"))
                .Concat(startupAssembly.DefinedTypes.Where(t => t.Name == "App"))
                .Select(t => t.AsType())
                .FirstOrDefault();
        }

        public virtual IServiceProvider ConfigureServices()
        {
            var services = ConfigureHostServices(new ServiceCollection());

            return Invoke(
                    _startupType,
                    new[] { "ConfigureServices", "Configure" + _environment + "Services" },
                    services) as IServiceProvider
                ?? services.BuildServiceProvider();
        }

        public virtual void ConfigureDesignTimeServices([NotNull] IServiceCollection services)
            => ConfigureDesignTimeServices(_startupType, services);

        public virtual void ConfigureDesignTimeServices([CanBeNull] Type type, [NotNull] IServiceCollection services)
            => Invoke(type, new[] { "ConfigureDesignTimeServices" }, services);

        private object Invoke(Type type, string[] methodNames, IServiceCollection services)
        {
            if (type == null)
            {
                return null;
            }

            MethodInfo method = null;
            for (int i = 0; i < methodNames.Length; i++)
            {
                method = type.GetTypeInfo().GetDeclaredMethod(methodNames[i]);
                if (method != null)
                {
                    break;
                }
                else if (i == methodNames.Length - 1)
                {
                    return null;
                }
            }

            var instance = !method.IsStatic
                ? ActivatorUtilities.GetServiceOrCreateInstance(GetHostServices(), type)
                : null;

            var parameters = method.GetParameters();
            var arguments = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;
                arguments[i] = parameterType == typeof(IServiceCollection)
                    ? services
                    : ActivatorUtilities.GetServiceOrCreateInstance(GetHostServices(), parameterType);
            }

            return method.Invoke(instance, arguments);
        }

        protected virtual IServiceCollection ConfigureHostServices([NotNull] IServiceCollection services)
            => services.AddLogging();

        private IServiceProvider GetHostServices()
            => ConfigureHostServices(new ServiceCollection()).BuildServiceProvider();
    }
}
