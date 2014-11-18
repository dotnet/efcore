// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;

#if ASPNET50 || ASPNETCORE50
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.Runtime.Infrastructure;
#endif

namespace Microsoft.Data.Entity.Commands
{
    public class ContextTool
    {
        public static DbContext CreateContext([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            // TODO: Allow other construction patterns (See #639)
            return TryCreateContextFromStartup(type) ?? (DbContext)Activator.CreateInstance(type);
        }

        private static DbContext TryCreateContextFromStartup(Type type)
        {
#if ASPNET50 || ASPNETCORE50
            try
            {
                // TODO: Let Hosting do this the right way (See aspnet/Hosting#85)
                var hostingServiceCollection = new ServiceCollection()
                    .Import(CallContextServiceLocator.Locator.ServiceProvider)
                    .Add(HostingServices.GetDefaultServices())
                    .AddInstance<IHostingEnvironment>(new HostingEnvironment { EnvironmentName = "Development" });
                var hostingServices = hostingServiceCollection.BuildServiceProvider();
                var assembly = type.GetTypeInfo().Assembly;
                var startupType = assembly.DefinedTypes.FirstOrDefault(t => t.Name.Equals("Startup", StringComparison.Ordinal));
                var instance = ActivatorUtilities.GetServiceOrCreateInstance(hostingServices, startupType.AsType());
                var servicesMethod = startupType.GetDeclaredMethod("ConfigureServices");
                hostingServiceCollection.Add(OptionsServices.GetDefaultServices());
                servicesMethod.Invoke(instance, new[] { hostingServiceCollection });
                var applicationServices = hostingServiceCollection.BuildServiceProvider();
                return applicationServices.GetService(type) as DbContext;
            }
            catch
            {
            }
#endif

            return null;
        }

        public static IEnumerable<Type> GetContextTypes([NotNull] Assembly assembly)
        {
            Check.NotNull(assembly, "assembly");

            return assembly.GetTypes().Where(
                t => !t.GetTypeInfo().IsAbstract
                     && !t.GetTypeInfo().IsGenericType
                     && typeof(DbContext).IsAssignableFrom(t));
        }

        public static Type SelectType([NotNull] IEnumerable<Type> types, [CanBeNull] string name)
        {
            Check.NotNull(types, "contextTypes");

            Type[] candidates;

            if (string.IsNullOrEmpty(name))
            {
                candidates = types.Take(2).ToArray();
                if (candidates.Length == 0)
                {
                    throw new InvalidOperationException(Strings.NoContext);
                }
                if (candidates.Length == 1)
                {
                    return candidates[0];
                }

                throw new InvalidOperationException(Strings.MultipleContexts);
            }

            candidates = FilterTypes(types, name, ignoreCase: true).ToArray();
            if (candidates.Length == 0)
            {
                throw new InvalidOperationException(Strings.NoContextWithName(name));
            }
            if (candidates.Length == 1)
            {
                return candidates[0];
            }

            // Disambiguate using case
            candidates = FilterTypes(candidates, name).ToArray();
            if (candidates.Length == 0)
            {
                throw new InvalidOperationException(Strings.MultipleContextsWithName(name));
            }
            if (candidates.Length == 1)
            {
                return candidates[0];
            }

            // Allow selecting types in the default namespace
            candidates = candidates.Where(t => t.Namespace == null).ToArray();
            if (candidates.Length == 0)
            {
                throw new InvalidOperationException(Strings.MultipleContextsWithQualifiedName(name));
            }

            Debug.Assert(candidates.Length == 1, "candidates.Length is not 1.");

            return candidates[0];
        }

        private static IEnumerable<Type> FilterTypes(
            [NotNull] IEnumerable<Type> types,
            [NotNull] string name,
            bool ignoreCase = false)
        {
            Debug.Assert(types != null, "types is null.");
            Debug.Assert(!string.IsNullOrEmpty(name), "name is null or empty.");

            var comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return types.Where(
                t => string.Equals(t.Name, name, comparisonType)
                     || string.Equals(t.FullName, name, comparisonType)
                     || string.Equals(t.AssemblyQualifiedName, name, comparisonType));
        }
    }
}
