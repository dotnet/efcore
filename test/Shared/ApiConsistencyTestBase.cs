// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Data.Entity
{
    public abstract class ApiConsistencyTestBase
    {
        protected const BindingFlags PublicInstance
            = BindingFlags.Instance | BindingFlags.Public;

        [Fact]
        public void Public_inheritable_apis_should_be_virtual()
        {
            var nonVirtualMethods
                = (from t in GetAllTypes(TargetAssembly.GetTypes())
                    where t.IsVisible
                          && !t.IsSealed
                          && t.GetConstructors(PublicInstance).Any()
                          && t.Namespace != null
                          && !t.Namespace.EndsWith(".Compiled")
                    from m in t.GetMethods(PublicInstance)
                    where m.DeclaringType != null
                          && m.DeclaringType.Assembly == TargetAssembly
                          && !(m.IsVirtual && !m.IsFinal)
                    select t.Name + "." + m.Name)
                    .ToList();

            Assert.False(
                nonVirtualMethods.Any(),
                "\r\n-- Missing virtual APIs --\r\n" + string.Join("\r\n", nonVirtualMethods));
        }

        [Fact]
        public void Public_api_arguments_should_have_not_null_annotation()
        {
            var parametersMissingAttribute
                = (from t in GetAllTypes(TargetAssembly.GetTypes())
                    where t.IsVisible && !typeof(Delegate).IsAssignableFrom(t)
                    let ims = t.GetInterfaces().Select(t.GetInterfaceMap)
                    let es = t.GetEvents()
                    from m in t.GetMethods(PublicInstance | BindingFlags.Static)
                        .Concat<MethodBase>(t.GetConstructors())
                    where m.DeclaringType != null
                          && m.DeclaringType.Assembly == TargetAssembly
                    where t.IsInterface || !ims.Any(im => im.TargetMethods.Contains(m))
                    where !es.Any(e => e.AddMethod == m || e.RemoveMethod == m)
                    from p in m.GetParameters()
                    where !p.ParameterType.IsValueType
                          && !p.GetCustomAttributes()
                              .Any(
                                  a => a.GetType().Name == "NotNullAttribute"
                                       || a.GetType().Name == "CanBeNullAttribute")
                    select t.Name + "." + m.Name + "[" + p.Name + "]")
                    .ToList();

            Assert.False(
                parametersMissingAttribute.Any(),
                "\r\n-- Missing NotNull annotations --\r\n" + string.Join("\r\n", parametersMissingAttribute));
        }

        [Fact]
        public void Async_methods_should_have_overload_with_cancellation_token_and_end_with_async_suffix()
        {
            var asyncMethodsWithToken
                = (from t in GetAllTypes(TargetAssembly.GetTypes())
                    where t.IsVisible
                    from m in t.GetMethods(PublicInstance)
                    where typeof(Task).IsAssignableFrom(m.ReturnType)
                          && m.GetParameters().Any(pi => pi.ParameterType == typeof(CancellationToken))
                    select m).ToList();

            var asyncMethodsWithoutToken
                = (from t in GetAllTypes(TargetAssembly.GetTypes())
                    where t.IsVisible
                    from m in t.GetMethods(PublicInstance)
                    where typeof(Task).IsAssignableFrom(m.ReturnType)
                          && m.GetParameters().All(pi => pi.ParameterType != typeof(CancellationToken))
                    select m).ToList();

            var missingOverloads
                = (from m1 in asyncMethodsWithoutToken
                    where !asyncMethodsWithToken
                        .Any(m2 => m1.Name == m2.Name
                                   && m1.ReflectedType == m2.ReflectedType)
                    // ReSharper disable once PossibleNullReferenceException
                    select m1.DeclaringType.Name + "." + m1.Name).ToList();

            Assert.False(
                missingOverloads.Any(),
                "\r\n-- Missing async overloads --\r\n" + string.Join("\r\n", missingOverloads));

            var missingSuffixMethods
                = asyncMethodsWithToken
                    .Where(mi => !mi.Name.EndsWith("Async"))
                    .Select(mi => mi.DeclaringType.Name + "." + mi.Name)
                    .ToList();

            Assert.False(
                missingSuffixMethods.Any(),
                "\r\n-- Missing async suffix --\r\n" + string.Join("\r\n", missingSuffixMethods));
        }

        protected abstract Assembly TargetAssembly { get; }

        protected virtual IEnumerable<Type> GetAllTypes(IEnumerable<Type> types)
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
