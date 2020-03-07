// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design
{
    public abstract class DesignTimeProviderServicesTest
    {
        protected abstract Assembly GetRuntimeAssembly();
        protected abstract Type GetDesignTimeServicesType();

        [ConditionalFact]
        public void Ensure_assembly_identity_matches()
        {
            var runtimeAssembly = GetRuntimeAssembly();
            var dtAttribute = runtimeAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>();
            var dtType = GetDesignTimeServicesType();
            Assert.NotNull(dtType);

            Assert.NotNull(dtAttribute);
            Assert.Equal(dtType.FullName, dtAttribute.TypeName);
        }
    }
}
