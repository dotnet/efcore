// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class DesignTimeProviderServicesAttribute : Attribute
    {
        public DesignTimeProviderServicesAttribute(
            [NotNull] string typeName, [NotNull] string assemblyName, [NotNull] string packageName)
        {
            Check.NotEmpty(typeName, nameof(typeName));
            Check.NotEmpty(assemblyName, nameof(assemblyName));
            Check.NotEmpty(packageName, nameof(packageName));

            TypeName = typeName;
            AssemblyName = assemblyName;
            PackageName = packageName;
        }

        public string TypeName { get; }
        public string AssemblyName { get; }
        public string PackageName { get; }
    }
}
