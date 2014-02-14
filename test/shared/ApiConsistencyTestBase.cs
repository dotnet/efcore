// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.Data.Entity
{
    public abstract class ApiConsistencyTestBase
    {
        protected const BindingFlags PublicInstance
            = BindingFlags.Instance | BindingFlags.Public;

        [Fact]
        public void PublicInheritableApisShouldBeVirtual()
        {
            var nonVirtualMethods
                = from t in GetAllTypes(TargetAssembly.GetTypes())
                  where t.IsVisible
                        && !t.IsSealed
                        && t.GetConstructors(PublicInstance).Any()
                  from m in t.GetMethods(PublicInstance)
                  where m.DeclaringType != null
                        && m.DeclaringType.Assembly == TargetAssembly
                        && !m.IsVirtual
                  select t.Name + "." + m.Name;

            Assert.Equal("", string.Join("\r\n", nonVirtualMethods));
        }

        [Fact]
        public void PublicApiArgumentsShouldHaveNotNullAnnotation()
        {
            var parametersMissingAttribute
                = from t in GetAllTypes(TargetAssembly.GetTypes())
                  where t.IsVisible
                  from m in t.GetMethods(PublicInstance | BindingFlags.Static)
                      .Concat<MethodBase>(t.GetConstructors())
                  where m.DeclaringType != null
                        && m.DeclaringType.Assembly == TargetAssembly
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
