// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

#if DNX451 || DNXCORE50
using Microsoft.AspNet.Hosting;
#endif

namespace Microsoft.Data.Entity.Commands
{
    public class ContextTool
    {
        private readonly IServiceProvider _services;

        public ContextTool([CanBeNull] IServiceProvider services)
        {
            _services = services;
        }

        public virtual DbContext CreateContext([NotNull] Type type, [CanBeNull] string startupAssemblyName)
        {
            Check.NotNull(type, nameof(type));

#if DNX451 || DNXCORE50
            var context = TryCreateContextFromStartup(type, startupAssemblyName);
            if (context != null)
            {
                return context;
            }
#endif

            // TODO: Allow other construction patterns (See #639)
            return (DbContext)Activator.CreateInstance(type);
        }

#if DNX451 || DNXCORE50
        protected virtual DbContext TryCreateContextFromStartup(Type type, string startupAssemblyName)
        {
            var hostBuilder = new WebHostBuilder(_services);
            if (startupAssemblyName != null)
            {
                hostBuilder.UseStartup(startupAssemblyName);
            }

            var appServices = hostBuilder.Build().ApplicationServices;

            return (DbContext)appServices.GetService(type);
        }
#endif

        public static IEnumerable<Type> GetContextTypes([NotNull] Assembly assembly) =>
            assembly.GetTypes().Where(
                t => !t.GetTypeInfo().IsAbstract
                     && !t.GetTypeInfo().IsGenericType
                     && typeof(DbContext).IsAssignableFrom(t));

        public static Type SelectType([NotNull] IEnumerable<Type> types, [CanBeNull] string name)
        {
            Check.NotNull(types, nameof(types));

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
