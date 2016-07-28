// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests
{
    public abstract class DesignTimeProviderServicesTest
    {
        protected abstract Assembly GetRuntimeAssembly();
        protected abstract Type GetDesignTimeServicesType();

        [Fact]
        public void EnsureAssemblyIdentityMatches()
        {
            var runtimeAssembly = GetRuntimeAssembly();
            var dtAttribute = runtimeAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>();
            var dtType = GetDesignTimeServicesType();
            Assert.NotNull(dtType);

            Assert.NotNull(dtAttribute);
            Assert.Equal(dtType.GetTypeInfo().Assembly.GetName().FullName, dtAttribute.AssemblyName);
            Assert.Equal(dtType.FullName, dtAttribute.TypeName);
        }
    }
}
