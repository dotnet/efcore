// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public static class ProductInfo
    {
        public static string GetVersion()
            => typeof(ProductInfo).GetTypeInfo().Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
