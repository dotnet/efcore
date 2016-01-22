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
            [NotNull] string fullyQualifiedTypeName, [NotNull] string packageName)
        {
            Check.NotEmpty(fullyQualifiedTypeName, nameof(fullyQualifiedTypeName));
            Check.NotEmpty(packageName, nameof(packageName));

            FullyQualifiedTypeName = fullyQualifiedTypeName;
            PackageName = packageName;
        }

        public string FullyQualifiedTypeName { get; }
        public string PackageName { get; }
    }
}
