// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class MockMethodInfo : MethodInfo
    {
        private readonly Action<object[]> _invoke;

        public MockMethodInfo(Type declaringType, Action<object[]> invoke = null)
        {
            _invoke = invoke;
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
            => new ParameterInfo[1];

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            _invoke?.Invoke(parameters);

            return null;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
            => throw new NotImplementedException();
    }
}
