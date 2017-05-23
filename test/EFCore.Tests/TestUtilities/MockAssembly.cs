// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class MockAssembly : Assembly
    {
        public static Assembly Create(params Type[] definedTypes)
        {
            var definedTypeInfos = definedTypes.Select(t => t.GetTypeInfo()).ToArray();

            return new MockAssembly(definedTypeInfos);
        }

        public MockAssembly(IEnumerable<TypeInfo> definedTypes)
        {
            DefinedTypes = definedTypes;
        }

        public override IEnumerable<TypeInfo> DefinedTypes { get; }

        public override AssemblyName GetName()
            => new AssemblyName(nameof(MockAssembly));
    }
}
