// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.FunctionalTests
{
    public static class Extensions
    {
        public static IEnumerable<T> NullChecked<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }

        public static IServiceCollection AddTestModelSource(this IServiceCollection serviceCollection, Action<ModelBuilder> onModelCreating = null)
        {
            serviceCollection.AddSingleton(typeof(IModelSource), p => new TestModelSource(onModelCreating));

            return serviceCollection;
        }
    }
}
