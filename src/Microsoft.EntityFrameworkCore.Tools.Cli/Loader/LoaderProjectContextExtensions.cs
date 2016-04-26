// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !NET451
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.DotNet.ProjectModel.Compilation;
using Microsoft.Extensions.DependencyModel;
using JetBrains.Annotations;
using Microsoft.DotNet.ProjectModel;

namespace Microsoft.EntityFrameworkCore.Tools.Cli.Loader
{
    public static class LoaderProjectContextExtensions
    {
        public static AssemblyLoadContext CreateLoadContext(
            [NotNull] this ProjectContext context,
            [NotNull] string runtimeIdentifier,
            [NotNull] string configuration,
            [CanBeNull] string outputPath = null)
        {
            var exporter = context.CreateExporter(configuration);
            var assemblies = new Dictionary<AssemblyName, string>(AssemblyNameComparer.OrdinalIgnoreCase);
            var dllImports = new Dictionary<string, string>();

            var fallbacks = DependencyContext.Default.RuntimeGraph.FirstOrDefault(f => f.Runtime.Equals(runtimeIdentifier));
            if (fallbacks == null)
            {
                throw new InvalidOperationException($"Failed to load runtime fallback graph for: {runtimeIdentifier}");
            }

            foreach (var export in exporter.GetAllExports())
            {
                var group = SelectGroup(export.RuntimeAssemblyGroups, fallbacks);

                // TODO: Handle resource assemblies
                if (group != null)
                {
                    foreach (var asset in group.Assets)
                    {
                        // REVIEW: Should we use the following?
                        // AssemblyLoadContext.GetAssemblyName(asset.ResolvedPath);
                        var assemblyName = new AssemblyName(asset.Name);
                        assemblies[assemblyName] = asset.ResolvedPath;
                    }
                }

                group = SelectGroup(export.NativeLibraryGroups, fallbacks);

                if (group != null)
                {
                    foreach (var asset in group.Assets)
                    {
                        dllImports[asset.Name] = asset.ResolvedPath;
                    }
                }
            }

            return new DesignTimeProjectLoadContext(
                assemblies,
                dllImports,

                // Add the project's output directory path to ensure project-to-project references get located
                new[] { context.GetOutputPaths(configuration, outputPath: outputPath).CompilationOutputPath });
        }

        private static LibraryAssetGroup SelectGroup(IEnumerable<LibraryAssetGroup> groups, RuntimeFallbacks fallbacks)
        {
            foreach (var runtime in fallbacks.AllRuntimes())
            {
                var group = groups.GetRuntimeGroup(runtime);
                if (group != null)
                {
                    return group;
                }
            }
            return groups.GetDefaultGroup();
        }

        private class AssemblyNameComparer : IEqualityComparer<AssemblyName>
        {
            public static readonly IEqualityComparer<AssemblyName> OrdinalIgnoreCase = new AssemblyNameComparer();

            private AssemblyNameComparer()
            {
            }

            public bool Equals(AssemblyName x, AssemblyName y)
            {
                // Ignore case because that's what Assembly.Load does.
                return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.CultureName ?? string.Empty, y.CultureName ?? string.Empty, StringComparison.Ordinal);
            }

            public int GetHashCode(AssemblyName obj)
            {
                var hashCode = 0;
                if (obj.Name != null)
                {
                    hashCode ^= obj.Name.GetHashCode();
                }

                hashCode ^= (obj.CultureName ?? string.Empty).GetHashCode();
                return hashCode;
            }
        }
    }

    internal static class RuntimeFallbackExtensions
    {
        public static IEnumerable<string> AllRuntimes(this RuntimeFallbacks fallback) 
            => Enumerable.Concat(new[] { fallback.Runtime }, fallback.Fallbacks);
    }
}
#endif