// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Data.Entity.Relational.Migrations.Utilities
{
    internal static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetAccessibleTypes(this Assembly assembly)
        {
            try
            {
                return assembly.DefinedTypes.Select(t => t.AsType());
            }
            catch (ReflectionTypeLoadException ex)
            {
                // The exception is thrown if some types cannot be loaded in partial trust.
                // For our purposes we just want to get the types that are loaded, which are
                // provided in the Types property of the exception.
                return ex.Types.Where(t => t != null);
            }
        }

        public static string GetInformationalVersion(this Assembly assembly)
        {
            return assembly
                .GetCustomAttributes<AssemblyInformationalVersionAttribute>()
                .Single()
                .InformationalVersion;
        }
    }
}
