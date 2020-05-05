// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member See issue#20837
    public static class ProductInfo
    {
        public static string GetVersion()
            => typeof(ProductInfo).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
