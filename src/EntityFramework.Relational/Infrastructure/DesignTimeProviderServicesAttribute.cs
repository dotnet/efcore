// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class DesignTimeProviderServicesAttribute : Attribute
    {
        public DesignTimeProviderServicesAttribute(
            [NotNull] string typeName, [NotNull] string assemblyName)
        {
            Check.NotEmpty(typeName, nameof(typeName));
            Check.NotEmpty(assemblyName, nameof(assemblyName));

            TypeName = typeName;
            AssemblyName = assemblyName;
        }

        public string TypeName { get; }
        public string AssemblyName { get; }
    }
}
