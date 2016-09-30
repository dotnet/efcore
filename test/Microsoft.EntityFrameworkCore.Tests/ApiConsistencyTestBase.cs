// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Xunit;

// ReSharper disable StringEndsWithIsCultureSpecific
// ReSharper disable StringStartsWithIsCultureSpecific
namespace Microsoft.EntityFrameworkCore.Tests
{
    public abstract class ApiConsistencyTestBase
    {
        protected const BindingFlags PublicInstance
            = BindingFlags.Instance | BindingFlags.Public;

        protected const BindingFlags AnyInstance
            = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        [Fact]
        public void Public_inheritable_apis_should_be_virtual()
        {
            var nonVirtualMethods
                = (from type in GetAllTypes(TargetAssembly.GetTypes())
                   where type.GetTypeInfo().IsVisible
                         && !type.GetTypeInfo().IsSealed
                         && type.GetConstructors(AnyInstance).Any(c => c.IsPublic || c.IsFamily || c.IsFamilyOrAssembly)
                         && (type.Namespace != null)
                         && !type.Namespace.EndsWith(".Compiled")
                   from method in type.GetMethods(AnyInstance)
                   where (method.DeclaringType == type)
                         && !(method.IsVirtual && !method.IsFinal)
                         && !method.Name.StartsWith("add_")
                         && !method.Name.StartsWith("remove_")
                         && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly)
                         && (method.Name != "GenerateCacheKeyCore")
                   select type.FullName + "." + method.Name)
                    .ToList();

            Assert.False(
                nonVirtualMethods.Any(),
                "\r\n-- Missing virtual APIs --\r\n" + string.Join(Environment.NewLine, nonVirtualMethods));
        }

        [Fact]
        public void Public_api_arguments_should_have_not_null_annotation()
        {
            var parametersMissingAttribute
                = (from type in GetAllTypes(TargetAssembly.GetTypes())
                   where type.GetTypeInfo().IsVisible && !typeof(Delegate).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo())
                   let interfaceMappings = type.GetInterfaces().Select(i => type.GetTypeInfo().GetRuntimeInterfaceMap(i))
                   let events = type.GetEvents()
                   from method in type.GetMethods(AnyInstance | BindingFlags.Static)
                       .Concat<MethodBase>(type.GetConstructors())
                   where (method.DeclaringType == type)
                         && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly)
                         && (method is ConstructorInfo
                             || (((MethodInfo)method).GetBaseDefinition().DeclaringType == method.DeclaringType))
                         && (method.Name != nameof(DbContext.OnConfiguring))
                         && (method.Name != nameof(DbContext.OnModelCreating))
                   where type.GetTypeInfo().IsInterface || !interfaceMappings.Any(im => im.TargetMethods.Contains(method))
                   where !events.Any(e => (e.AddMethod == method) || (e.RemoveMethod == method))
                   from parameter in method.GetParameters()
                   let parameterType = parameter.ParameterType.IsByRef
                       ? parameter.ParameterType.GetElementType()
                       : parameter.ParameterType
                   where !parameterType.GetTypeInfo().IsValueType
                         && !parameter.GetCustomAttributes()
                             .Any(
                                 a => (a.GetType().Name == nameof(NotNullAttribute))
                                      || (a.GetType().Name == nameof(CanBeNullAttribute)))
                   select type.FullName + "." + method.Name + "[" + parameter.Name + "]")
                    .ToList();

            Assert.False(
                parametersMissingAttribute.Any(),
                "\r\n-- Missing NotNull annotations --\r\n" + string.Join(Environment.NewLine, parametersMissingAttribute));
        }

        [Fact]
        public void Async_methods_should_have_overload_with_cancellation_token_and_end_with_async_suffix()
        {
            var asyncMethods
                = (from type in GetAllTypes(TargetAssembly.GetTypes())
                   where type.GetTypeInfo().IsVisible
                   from method in type.GetMethods(AnyInstance | BindingFlags.Static)
                   where (method.DeclaringType == type)
                         && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly)
                   where typeof(Task).GetTypeInfo().IsAssignableFrom(method.ReturnType.GetTypeInfo())
                   select method).ToList();

            var asyncMethodsWithToken
                = (from method in asyncMethods
                   where method.GetParameters().Any(pi => pi.ParameterType == typeof(CancellationToken))
                   select method).ToList();

            var asyncMethodsWithoutToken
                = (from method in asyncMethods
                   where method.GetParameters().All(pi => pi.ParameterType != typeof(CancellationToken))
                   select method).ToList();

            var missingOverloads
                = (from methodWithoutToken in asyncMethodsWithoutToken
                   where !asyncMethodsWithToken
                       .Any(methodWithToken => (methodWithoutToken.Name == methodWithToken.Name)
                                               && (methodWithoutToken.DeclaringType == methodWithToken.DeclaringType))
                   // ReSharper disable once PossibleNullReferenceException
                   select methodWithoutToken.DeclaringType.Name + "." + methodWithoutToken.Name)
                    .Except(GetCancellationTokenExceptions())
                    .ToList();

            Assert.False(
                missingOverloads.Any(),
                "\r\n-- Missing async overloads --\r\n" + string.Join(Environment.NewLine, missingOverloads));

            var missingSuffixMethods
                = asyncMethods
                    .Where(method => !method.Name.EndsWith("Async") && (method.DeclaringType != null))
                    .Select(method => method.DeclaringType.Name + "." + method.Name)
                    .Except(GetAsyncSuffixExceptions())
                    .ToList();

            Assert.False(
                missingSuffixMethods.Any(),
                "\r\n-- Missing async suffix --\r\n" + string.Join(Environment.NewLine, missingSuffixMethods));
        }

        [Fact]
        public void Public_api_bool_parameters_should_not_be_prefixed()
        {
            var prefixes = new[]
            {
                "is",
                "can",
                "has"
            };

            var parameters = (
                    from type in GetAllTypes(TargetAssembly.GetExportedTypes())
                    where !type.Namespace.Contains("Internal")
                    from method in type.GetTypeInfo().DeclaredMethods
                    where !method.IsPrivate
                    from parameter in method.GetParameters()
                    where (parameter.ParameterType.UnwrapNullableType() == typeof(bool))
                          && prefixes.Any(parameter.Name.StartsWith)
                    select $"{type.FullName}.{method.Name}[{parameter.Name}]")
                .ToList();

            Assert.False(
                parameters.Any(),
                "\r\n-- Prefixed bool parameteres --\r\n" + string.Join(Environment.NewLine, parameters));
        }

        protected virtual IEnumerable<string> GetCancellationTokenExceptions() => Enumerable.Empty<string>();

        protected virtual IEnumerable<string> GetAsyncSuffixExceptions() => Enumerable.Empty<string>();

        protected abstract Assembly TargetAssembly { get; }

        protected virtual IEnumerable<Type> GetAllTypes(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                yield return type;

                foreach (var nestedType in GetAllTypes(type.GetTypeInfo().DeclaredNestedTypes.Select(i => i.AsType())))
                {
                    yield return nestedType;
                }
            }
        }
    }
}
