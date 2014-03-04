// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

// TODO: This file should be shared

namespace Microsoft.Data.SqlServer.Tests
{
    public abstract class ApiConsistencyTestBase
    {
        protected const BindingFlags PublicInstance
            = BindingFlags.Instance | BindingFlags.Public;

        [Fact]
        public void Public_inheritable_apis_should_be_virtual()
        {
            var nonVirtualMethods
                = from t in GetAllTypes(TargetAssembly.GetTypes())
                  where t.IsVisible
                        && !t.IsSealed
                        && t.GetConstructors(PublicInstance).Any()
                        && t.Namespace != null
                        && !t.Namespace.EndsWith(".Compiled")
                  from m in t.GetMethods(PublicInstance)
                  where m.DeclaringType != null
                        && m.DeclaringType.Assembly == TargetAssembly
                        && !m.IsVirtual
                  select t.Name + "." + m.Name;

            Assert.Equal("", string.Join("\r\n", nonVirtualMethods));
        }

        [Fact]
        public void Public_api_arguments_should_have_not_null_annotation()
        {
            var parametersMissingAttribute
                = from t in GetAllTypes(TargetAssembly.GetTypes())
                  where t.IsVisible
                  let ims = t.GetInterfaces().Select(t.GetInterfaceMap)
                  from m in t.GetMethods(PublicInstance | BindingFlags.Static)
                      .Concat<MethodBase>(t.GetConstructors())
                  where m.DeclaringType != null
                        && m.DeclaringType.Assembly == TargetAssembly
                  where t.IsInterface || !ims.Any(im => im.TargetMethods.Contains(m))
                  from p in m.GetParameters()
                  where !p.ParameterType.IsValueType
                        && !p.GetCustomAttributes()
                            .Any(
                                a => a.GetType().Name == "NotNullAttribute"
                                     || a.GetType().Name == "CanBeNullAttribute")
                  select t.Name + "." + m.Name + "[" + p.Name + "]";

            Assert.Equal("", string.Join("\r\n", parametersMissingAttribute));
        }

        protected abstract Assembly TargetAssembly { get; }

        protected static IEnumerable<Type> GetAllTypes(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                yield return type;

                foreach (var nestedType in GetAllTypes(type.GetNestedTypes()))
                {
                    yield return nestedType;
                }
            }
        }
    }
}
