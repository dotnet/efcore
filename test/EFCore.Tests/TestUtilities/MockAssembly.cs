// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class MockAssembly : Assembly
    {
        public static Assembly Create(params Type[] definedTypes)
            => Create(definedTypes, new MockMethodInfo(definedTypes.First()));

        public static Assembly Create(Type[] definedTypes, MethodInfo entryPoint)
        {
            var definedTypeInfos = definedTypes.Select(t => t.GetTypeInfo()).ToArray();

            return new MockAssembly(definedTypeInfos, entryPoint);
        }

        public MockAssembly(IEnumerable<TypeInfo> definedTypes, MethodInfo entryPoint)
        {
            DefinedTypes = definedTypes;
            EntryPoint = entryPoint;
        }

        public override MethodInfo EntryPoint { get; }

        public override IEnumerable<TypeInfo> DefinedTypes { get; }

        public override AssemblyName GetName()
            => new(nameof(MockAssembly));

        public override string FullName
            => nameof(MockAssembly);
    }
}
