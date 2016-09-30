// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if !NET451
using Moq;
#endif
namespace Microsoft.EntityFrameworkCore.Tests.TestUtilities
{
    public partial class MockAssembly
    {
        public static Assembly Create(params Type[] definedTypes)
        {
            var definedTypeInfos = definedTypes.Select(t => t.GetTypeInfo()).ToArray();

#if NET451
            return new MockAssembly(definedTypeInfos);
#else
            var assembly = new Mock<Assembly>();
            assembly.SetupGet(a => a.DefinedTypes).Returns(definedTypeInfos);
            assembly.Setup(a => a.GetName()).Returns(new AssemblyName(nameof(MockAssembly)));

            return assembly.Object;
#endif
        }

#if !NET451
        public AssemblyName GetName()
            => new AssemblyName(nameof(MockAssembly));
#endif
    }

#if NET451
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
#endif
}
