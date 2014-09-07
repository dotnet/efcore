// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests.TestUtilities
{
    public static class Extensions
    {
        public static void ValidateMessage(
            this Exception exception,
            Type resourceAssemblyType,
            string expectedResourceMethod,
            params object[] parameters)
        {
            var strings = resourceAssemblyType.GetTypeInfo().Assembly.GetType(resourceAssemblyType.Namespace + ".Strings");
            var expectedMessage = (string)strings.GetTypeInfo().GetDeclaredMethods(expectedResourceMethod).Single().Invoke(null, parameters);
            Assert.Equal(expectedMessage, exception.Message);
        }

        public static IEnumerable<T> NullChecked<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }
    }
}
