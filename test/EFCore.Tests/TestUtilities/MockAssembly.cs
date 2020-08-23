// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
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
            => new AssemblyName(nameof(MockAssembly));

        private class MockMethodInfo : MethodInfo
        {
            public MockMethodInfo(Type declaringType)
            {
                DeclaringType = declaringType;
            }

            public override Type DeclaringType { get; }

            public override ICustomAttributeProvider ReturnTypeCustomAttributes
                => throw new NotImplementedException();

            public override RuntimeMethodHandle MethodHandle
                => throw new NotImplementedException();

            public override MethodAttributes Attributes
                => throw new NotImplementedException();

            public override string Name
                => throw new NotImplementedException();

            public override Type ReflectedType
                => throw new NotImplementedException();

            public override MethodInfo GetBaseDefinition()
                => throw new NotImplementedException();

            public override object[] GetCustomAttributes(bool inherit)
                => throw new NotImplementedException();

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
                => throw new NotImplementedException();

            public override MethodImplAttributes GetMethodImplementationFlags()
                => throw new NotImplementedException();

            public override ParameterInfo[] GetParameters()
                => throw new NotImplementedException();

            public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
                => throw new NotImplementedException();

            public override bool IsDefined(Type attributeType, bool inherit)
                => throw new NotImplementedException();
        }
    }
}
