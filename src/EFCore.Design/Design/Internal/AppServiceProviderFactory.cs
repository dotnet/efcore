// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class AppServiceProviderFactory
    {
        private readonly Assembly _startupAssembly;

        public AppServiceProviderFactory([NotNull] Assembly startupAssembly)
            => _startupAssembly = startupAssembly;

        public virtual IServiceProvider Create([NotNull] string[] args)
            => CreateFromBuildWebHost(args)
                ?? CreateEmptyServiceProvider();

        private IServiceProvider CreateFromBuildWebHost(string[] args)
        {
            var programType = FindProgramClass();
            if (programType == null)
            {
                return null;
            }

            var buildWebHostMethod = programType.GetTypeInfo().GetDeclaredMethod("BuildWebHost");
            if (buildWebHostMethod == null)
            {
                return null;
            }

            // TODO: Remove when dotnet/cli#6617 is fixed
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == null)
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            }

            var webHost = buildWebHostMethod.Invoke(null, new object[] { args });
            var webHostType = webHost.GetType();
            var servicesProperty = webHostType.GetTypeInfo().GetDeclaredProperty("Services");
            var services = (IServiceProvider)servicesProperty.GetValue(webHost);

            return services.CreateScope().ServiceProvider;
        }

        protected virtual Type FindProgramClass()
#if NET46
            => _startupAssembly.EntryPoint?.DeclaringType;
#elif NETSTANDARD1_3
            => Enumerable.FirstOrDefault(
                from t in _startupAssembly.GetLoadableDefinedTypes()
                from m in t.DeclaredMethods
                let ps = m.GetParameters()
                where m.IsStatic
                    && (m.ReturnType == typeof(void) || m.ReturnType == typeof(int))
                    && m.Name == "Main"
                    && (ps.Length == 0 || (ps.Length == 1 && ps[0].ParameterType == typeof(string[])))
                select t.AsType());
#else
#error target frameworks need to be updated.
#endif

        private IServiceProvider CreateEmptyServiceProvider()
            => new ServiceCollection().BuildServiceProvider();
    }
}