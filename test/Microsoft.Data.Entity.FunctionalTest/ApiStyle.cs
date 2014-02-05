// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity
{
    public class ApiStyle
    {
        private const BindingFlags PublicInstance
            = BindingFlags.Instance | BindingFlags.Public;

        [Fact]
        public void Public_inheritable_apis_should_be_virtual()
        {
            var assembly = typeof(Metadata.Entity).Assembly;

            var nonVirtualMethods
                = from t in GetAllTypes(assembly.GetTypes())
                  where t.IsVisible
                        && !t.IsSealed
                        && t.GetConstructors(PublicInstance).Any()
                  from m in t.GetMethods(PublicInstance)
                  where m.DeclaringType != null
                        && m.DeclaringType.Assembly == assembly
                        && !m.IsVirtual
                  select t.Name + "." + m.Name;

            Assert.Equal("", string.Join("\r\n", nonVirtualMethods));
        }

        [Fact]
        public void Fluent_api_methods_should_not_return_void()
        {
            var fluentApiTypes = new[] { typeof(ModelBuilder) };

            var assembly = typeof(Metadata.Entity).Assembly;

            var voidMethods
                = from t in GetAllTypes(fluentApiTypes)
                  where t.IsVisible
                  from m in t.GetMethods(PublicInstance)
                  where m.DeclaringType != null
                        && m.DeclaringType.Assembly == assembly
                        && m.ReturnType == typeof(void)
                  select t.Name + "." + m.Name;

            Assert.Equal("", string.Join("\r\n", voidMethods));
        }

        private static IEnumerable<Type> GetAllTypes(IEnumerable<Type> types)
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
