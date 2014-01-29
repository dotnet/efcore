// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Data.Core.Metadata;
    using Xunit;

    public class ApiStyle
    {
        private const BindingFlags PublicInstance
            = BindingFlags.Instance | BindingFlags.Public;

        [Fact]
        public void Public_inheritable_apis_should_be_virtual()
        {
            var assembly = typeof(Entity).Assembly;

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
        public void Public_api_arguments_should_have_not_null_annotation()
        {
            var assembly = typeof(Entity).Assembly;

            var parametersMissingAttribute
                = from t in GetAllTypes(assembly.GetTypes())
                  where t.IsVisible
                  from m in t.GetMethods(PublicInstance)
                  where m.DeclaringType != null
                        && m.DeclaringType.Assembly == assembly
                  from p in m.GetParameters()
                  where !p.ParameterType.IsValueType
                        && p.GetCustomAttributes().All(a => a.GetType().Name != "NotNullAttribute")
                  select t.Name + "." + m.Name + "[" + p.Name + "]";

            Assert.Equal("", string.Join("\r\n", parametersMissingAttribute));
        }

        [Fact]
        public void Fluent_api_methods_should_not_return_void()
        {
            var fluentApiTypes = new[] { typeof(ModelBuilder) };

            var assembly = typeof(Entity).Assembly;

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
