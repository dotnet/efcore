// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.FunctionalTests
{
    public static class Extensions
    {
        public static void ValidateMessage(
            this Exception exception,
            Type resourceAssemblyType,
            string expectedResourceMethod,
            params object[] parameters)
        {
            var strings = resourceAssemblyType.GetTypeInfo().Assembly.GetType(resourceAssemblyType.Namespace + ".Strings").GetTypeInfo();
            var method = parameters.Length == 0
                ? strings.GetDeclaredProperty(expectedResourceMethod).GetGetMethod()
                : strings.GetDeclaredMethods(expectedResourceMethod).Single();
            var expectedMessage = (string)method.Invoke(null, parameters);
            Assert.Equal(expectedMessage, exception.Message);
        }

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
