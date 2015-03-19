// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

#if DNX451 || DNXCORE50
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FeatureModel;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Server;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime.Infrastructure;
#endif

namespace Microsoft.Data.Entity.Commands
{
    public class ContextTool
    {
        public static DbContext CreateContext([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            // TODO: Allow other construction patterns (See #639)
            return TryCreateContextFromStartup(type) ?? (DbContext)Activator.CreateInstance(type);
        }

        private static DbContext TryCreateContextFromStartup(Type type)
        {
#if DNX451 || DNXCORE50
            try
            {
                var context = new HostingContext
                {
                    ServerFactory = new ServerFactory(),
                };
                var instance = new HostingEngine().Start(context);
                return context.ApplicationServices.GetService(type) as DbContext;
            }
            catch
            {
            }
#endif

            return null;
        }

#if DNX451 || DNXCORE50
        private class ServerFactory : IServerFactory {
            public IServerInformation Initialize(IConfiguration configuration)
            {
                return null;
            }

            public IDisposable Start(IServerInformation serverInformation, Func<IFeatureCollection, Task> application)
            {
                return new StartInstance(application);
            }

            private class StartInstance : IDisposable
            {
                private readonly Func<IFeatureCollection, Task> _application;

                public StartInstance(Func<IFeatureCollection, Task> application)
                {
                    _application = application;
                }

                public void Dispose()
                {
                }
            }
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
