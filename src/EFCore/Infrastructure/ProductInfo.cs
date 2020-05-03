// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class ProductInfo
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string GetVersion()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            => typeof(ProductInfo).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
