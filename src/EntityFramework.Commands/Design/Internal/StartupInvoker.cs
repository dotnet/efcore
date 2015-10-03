// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.DependencyInjection;

#if DNX451 || DNXCORE50
using Microsoft.AspNet.Hosting;
#endif

namespace Microsoft.Data.Entity.Design.Internal
{
    public class StartupInvoker
    {
        private readonly TypeInfo _startupType;
        private readonly string _environment;
        private readonly IServiceProvider _dnxServices;

        public StartupInvoker(
            [NotNull] string startupAssemblyName,
            [CanBeNull] string environment,
            [CanBeNull] IServiceProvider dnxServices)
        {
            Check.NotEmpty(startupAssemblyName, nameof(startupAssemblyName));

            _environment = !string.IsNullOrEmpty(environment)
                ? environment
                : "Development";

            var startupAssembly = Assembly.Load(new AssemblyName(startupAssemblyName));
            _startupType = startupAssembly.DefinedTypes.Where(t => t.Name == "Startup" + _environment)
                .Concat(startupAssembly.DefinedTypes.Where(t => t.Name == "Startup"))
                .FirstOrDefault();

            _dnxServices = dnxServices;
        }

        public virtual void ConfigureDesignTimeServices([NotNull] IServiceCollection services)
        {
            if (_startupType == null)
            {
                return;
            }

            var method = _startupType.GetDeclaredMethod("ConfigureDesignTimeServices");
            if (method == null)
            {
                return;
            }

            var instance = !method.IsStatic
                ? ActivatorUtilities.GetServiceOrCreateInstance(GetHostServices(), _startupType.AsType())
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

            method.Invoke(instance, arguments);
        }

        protected virtual IServiceProvider GetHostServices()
            => new ServiceCollection()
#if DNX451 || DNXCORE50
                .ImportDnxServices(_dnxServices)
                .AddInstance<IHostingEnvironment>(new HostingEnvironment { EnvironmentName = _environment })
#endif
                .AddLogging()
                .BuildServiceProvider();
    }
}
