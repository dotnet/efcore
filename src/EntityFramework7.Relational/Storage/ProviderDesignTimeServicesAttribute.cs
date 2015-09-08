// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class ProviderDesignTimeServicesAttribute : Attribute
    {
        public ProviderDesignTimeServicesAttribute([NotNull] string typeName)
            : this(typeName, null)
        {
        }

        public ProviderDesignTimeServicesAttribute(
            [NotNull] string typeName, [CanBeNull] string assemblyName)
        {
            Check.NotNull(typeName, nameof(typeName));

            TypeName = typeName;
            AssemblyName = assemblyName;
        }

        public virtual string TypeName { get; }
        public virtual string AssemblyName { get; }
    }
}
