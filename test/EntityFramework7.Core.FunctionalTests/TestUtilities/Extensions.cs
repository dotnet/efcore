// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity.FunctionalTests
{
    public static class Extensions
    {
        public static IServiceCollection ServiceCollection(this EntityFrameworkServicesBuilder builder) 
            => builder.GetService();

        public static IEnumerable<T> NullChecked<T>(this IEnumerable<T> enumerable) 
            => enumerable ?? Enumerable.Empty<T>();
    }
}
