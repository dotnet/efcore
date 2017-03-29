// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if NETCOREAPP2_0
using Moq;
#endif
namespace Microsoft.EntityFrameworkCore.Tests.TestUtilities
{
    public partial class MockAssembly
    {
        public static Assembly Create(params Type[] definedTypes)
        {
            var definedTypeInfos = definedTypes.Select(t => t.GetTypeInfo()).ToArray();

#if NET46
            return new MockAssembly(definedTypeInfos);
#elif NETCOREAPP2_0
            var assembly = new Mock<Assembly>();
            assembly.SetupGet(a => a.DefinedTypes).Returns(definedTypeInfos);
            assembly.Setup(a => a.GetName()).Returns(new AssemblyName(nameof(MockAssembly)));

            return assembly.Object;
#else
#error target frameworks need to be updated.
#endif
        }

#if NETCOREAPP2_0
        public AssemblyName GetName()
            => new AssemblyName(nameof(MockAssembly));
#elif NET46
#else
#error target frameworks need to be updated.
#endif
    }

#if NET46
    public partial class MockAssembly : Assembly
    {
        public MockAssembly(IEnumerable<TypeInfo> definedTypes)
        {
            DefinedTypes = definedTypes;
        }

        public override IEnumerable<TypeInfo> DefinedTypes { get; }

        public override AssemblyName GetName()
            => new AssemblyName(nameof(MockAssembly));
    }
#elif NETCOREAPP2_0
#else
#error target frameworks need to be updated.
#endif
}
