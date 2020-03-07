// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class ApiConsistencyTestBase
    {
        private readonly Dictionary<Type, Type> _mutableMetadataTypes = new Dictionary<Type, Type>();

        public ApiConsistencyTestBase()
        {
            foreach (var typeTuple in MetadataTypes)
            {
                _mutableMetadataTypes[typeTuple.Value.Mutable] = typeTuple.Value.Convention;
            }
        }

        protected const BindingFlags PublicInstance
            = BindingFlags.Instance | BindingFlags.Public;

        protected const BindingFlags AnyInstance
            = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        protected virtual IEnumerable<Type> FluentApiTypes
            => Enumerable.Empty<Type>();

        [ConditionalFact]
        public void Fluent_api_methods_should_not_return_void()
        {
            var voidMethods
                = (from type in GetAllTypes(FluentApiTypes)
                   where type.GetTypeInfo().IsVisible
                   from method in type.GetMethods(PublicInstance | BindingFlags.Static)
                   where method.ReturnType == typeof(void)
                   select type.Name + "." + method.Name)
                .ToList();

            Assert.False(
                voidMethods.Count > 0,
                "\r\n-- Missing fluent returns --\r\n" + string.Join(Environment.NewLine, voidMethods));
        }

        protected virtual Dictionary<Type, (Type Mutable, Type Convention)> MetadataTypes
            => new Dictionary<Type, (Type, Type)>();

        private static readonly HashSet<Type> _ignoredMetadataReturnTypes = new HashSet<Type>
        {
            typeof(bool),
            typeof(bool?),
            typeof(int),
            typeof(int?),
            typeof(string),
            typeof(object),
            typeof(Type),
            typeof(DeleteBehavior),
            typeof(ValueGenerated),
            typeof(PropertyAccessMode),
            typeof(PropertySaveBehavior),
            typeof(ChangeTrackingStrategy),
            typeof(ValueComparer),
            typeof(ValueConverter),
            typeof(Func<IProperty, IEntityType, ValueGenerator>),
            typeof(IClrCollectionAccessor),
            typeof(IClrPropertyGetter),
            typeof(IClrPropertySetter),
            typeof(LambdaExpression),
            typeof(ServiceParameterBinding),
            typeof(PropertyInfo),
            typeof(FieldInfo),
            typeof(MemberInfo),
            typeof(CoreTypeMapping),
            typeof(IAnnotation),
            typeof(IEnumerable<IAnnotation>),
            typeof(IEnumerable<IDictionary<string, object>>)
        };

        protected virtual HashSet<Type> IgnoredMetadataReturnTypes => _ignoredMetadataReturnTypes;

        [ConditionalFact]
        public void Mutable_metadata_types_have_matching_methods()
        {
            var errors =
                MetadataTypes.Select(
                        typeTuple =>
                            from readonlyMethod in typeTuple.Key.GetMethods(PublicInstance | BindingFlags.Static)
                            where readonlyMethod.Name != "get_Item"
                                && readonlyMethod.Name != "FindRuntimeEntityType"
                                && readonlyMethod.Name != "GetConcreteDerivedTypesInclusive"
                                && readonlyMethod.Name != "GetClosestCommonParent"
                                && readonlyMethod.Name != "LeastDerivedType"
                                && readonlyMethod.Name != "GetAllBaseTypesInclusive"
                                && readonlyMethod.Name != "GetAllBaseTypesInclusiveAscending"
                            join mutableMethod in typeTuple.Value.Mutable.GetMethods(PublicInstance | BindingFlags.Static)
                                on readonlyMethod.Name equals mutableMethod.Name into mutableGroup
                            from mutableMethod in mutableGroup.DefaultIfEmpty()
                            select (readonlyMethod, mutableMethod))
                    .SelectMany(m => m.Select(MatchMutable))
                    .Where(e => e != null)
                    .ToList();

            Assert.False(
                errors.Count > 0,
                "\r\n-- Mismatches: --\r\n" + string.Join(Environment.NewLine, errors));
        }

        private string MatchMutable((MethodInfo Readonly, MethodInfo Mutable) methodTuple)
        {
            var (readonlyMethod, mutableMethod) = methodTuple;

            if (IgnoredMetadataReturnTypes.Contains(readonlyMethod.ReturnType))
            {
                return null;
            }

            if (mutableMethod == null)
            {
                return $"No IMutable equivalent of {readonlyMethod.DeclaringType.Name}.{readonlyMethod.Name}";
            }

            if (readonlyMethod.ReturnType != null)
            {
                (Type Mutable, Type Convention) expectedReturnTypes;
                if (MetadataTypes.TryGetValue(readonlyMethod.ReturnType, out expectedReturnTypes))
                {
                    if (mutableMethod.ReturnType != expectedReturnTypes.Mutable)
                    {
                        return
                            $"{mutableMethod.DeclaringType.Name}.{mutableMethod.Name} expected to have {expectedReturnTypes.Mutable} return type";
                    }
                }
                else
                {
                    var sequenceType = readonlyMethod.ReturnType.TryGetSequenceType();
                    if (sequenceType != null
                        && MetadataTypes.TryGetValue(sequenceType, out expectedReturnTypes))
                    {
                        if (mutableMethod.ReturnType.TryGetSequenceType() != expectedReturnTypes.Mutable)
                        {
                            return
                                $"{mutableMethod.DeclaringType.Name}.{mutableMethod.Name} expected to have a return type that derives from IEnumerable<{expectedReturnTypes.Mutable}>.";
                        }
                    }
                }
            }

            return null;
        }

        [ConditionalFact]
        public void Convention_metadata_types_have_matching_methods()
        {
            var errors =
                MetadataTypes.Select(
                        typeTuple =>
                            from mutableMethod in typeTuple.Value.Mutable.GetMethods(PublicInstance | BindingFlags.Static)
                            where !mutableMethod.Name.StartsWith("set_")
                                && mutableMethod.Name != "get_Item"
                                && mutableMethod.Name != "RemoveIgnored"
                                && mutableMethod.Name != "GetContainingPrimaryKey"
                            join conventionMethod in typeTuple.Value.Convention.GetMethods(PublicInstance | BindingFlags.Static)
                                on mutableMethod.Name equals conventionMethod.Name into conventionGroup
                            from conventionMethod in conventionGroup.DefaultIfEmpty()
                            select (mutableMethod, conventionMethod))
                    .SelectMany(m => m.Select(MatchConvention))
                    .Where(e => e != null)
                    .ToList();

            Assert.False(
                errors.Count > 0,
                "\r\n-- Mismatches: --\r\n" + string.Join(Environment.NewLine, errors));
        }

        private string MatchConvention((MethodInfo Mutable, MethodInfo Convention) methodTuple)
        {
            var (mutableMethod, conventionMethod) = methodTuple;

            if (IgnoredMetadataReturnTypes.Contains(mutableMethod.ReturnType))
            {
                return null;
            }

            if (conventionMethod == null)
            {
                return $"No IConvention equivalent of {mutableMethod.DeclaringType.Name}.{mutableMethod.Name}";
            }

            if (mutableMethod.ReturnType != null)
            {
                Type expectedReturnType;
                if (_mutableMetadataTypes.TryGetValue(mutableMethod.ReturnType, out expectedReturnType))
                {
                    if (conventionMethod.ReturnType != expectedReturnType)
                    {
                        return
                            $"{conventionMethod.DeclaringType.Name}.{conventionMethod.Name} expected to have {expectedReturnType} return type";
                    }
                }
                else
                {
                    var sequenceType = mutableMethod.ReturnType.TryGetSequenceType();
                    if (sequenceType != null
                        && _mutableMetadataTypes.TryGetValue(sequenceType, out expectedReturnType))
                    {
                        if (conventionMethod.ReturnType.TryGetSequenceType() != expectedReturnType)
                        {
                            return
                                $"{conventionMethod.DeclaringType.Name}.{conventionMethod.Name} expected to have a return type that derives from IEnumerable<{expectedReturnType}>.";
                        }
                    }
                }
            }

            return null;
        }

        [ConditionalFact]
        public virtual void Service_implementations_should_use_dependencies_parameter_object()
        {
            var serviceCollection = new ServiceCollection();

            AddServices(serviceCollection);

            var badServiceTypes
                = (from sd in serviceCollection
                   where sd.ServiceType.Namespace.StartsWith("Microsoft.Entity", StringComparison.Ordinal)
                       && sd.ServiceType != typeof(IDiagnosticsLogger<>)
                       && sd.ServiceType != typeof(LoggingDefinitions)
                   let it = TryGetImplementationType(sd)
                   where !it.IsInterface
                   let ns = it.Namespace
                   where ns.StartsWith("Microsoft.Entity", StringComparison.Ordinal)
                       && !ns.EndsWith(".Internal", StringComparison.Ordinal)
                       && !it.Name.EndsWith("Dependencies", StringComparison.Ordinal)
                       && (it.GetConstructors().Length != 1
                           || it.GetConstructors()[0].GetParameters().Length == 0
                           || it.GetConstructors()[0].GetParameters()[0].Name != "dependencies")
                   select it)
                .ToList();

            Assert.False(
                badServiceTypes.Count > 0,
                "\r\n-- Missing or bad dependencies parameter object --\r\n" + string.Join(Environment.NewLine, badServiceTypes));
        }

        protected abstract void AddServices(ServiceCollection serviceCollection);

        private static Type TryGetImplementationType(ServiceDescriptor descriptor)
            => descriptor.ImplementationType
                ?? descriptor.ImplementationInstance?.GetType()
                ?? descriptor.ImplementationFactory?.GetType().GetTypeInfo().GenericTypeArguments[1];

        [ConditionalFact]
        public virtual void Public_inheritable_apis_should_be_virtual()
        {
            var nonVirtualMethods
                = (from type in GetAllTypes(TargetAssembly.GetTypes())
                   where type.GetTypeInfo().IsVisible
                       && !type.GetTypeInfo().IsSealed
                       && type.GetConstructors(AnyInstance).Any(c => c.IsPublic || c.IsFamily || c.IsFamilyOrAssembly)
                       && type.Namespace?.EndsWith(".Compiled", StringComparison.Ordinal) == false
                       && ShouldHaveVirtualMethods(type)
                   from method in type.GetMethods(AnyInstance)
                   where method.DeclaringType == type
                       && !(method.IsVirtual && !method.IsFinal)
                       && !method.Name.StartsWith("add_", StringComparison.Ordinal)
                       && !method.Name.StartsWith("remove_", StringComparison.Ordinal)
                       && !method.Name.Equals("get_NodeType", StringComparison.Ordinal)
                       && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly)
                       && method.Name != "GenerateCacheKeyCore"
                   select type.FullName + "." + method.Name)
                .ToList();

            Assert.False(
                nonVirtualMethods.Count > 0,
                "\r\n-- Missing virtual APIs --\r\n" + string.Join(Environment.NewLine, nonVirtualMethods));
        }

        protected virtual bool ShouldHaveVirtualMethods(Type type)
            => true;

        [ConditionalFact]
        public virtual void Public_api_arguments_should_have_not_null_annotation()
        {
            var parametersMissingAttribute
                = (from type in GetAllTypes(TargetAssembly.GetTypes())
                   where type.GetTypeInfo().IsVisible
                       && !typeof(Delegate).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo())
                       && !type.Namespace.Contains("Internal", StringComparison.Ordinal)
                   let interfaceMappings = type.GetInterfaces().Select(i => type.GetTypeInfo().GetRuntimeInterfaceMap(i))
                   let events = type.GetEvents()
                   from method in type.GetMethods(AnyInstance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                       .Concat<MethodBase>(type.GetConstructors())
                   where (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly)
                       && ShouldHaveNotNullAnnotation(method, type)
                       && !method.DeclaringType.Namespace.Contains("Query", StringComparison.Ordinal)
                   where type.GetTypeInfo().IsInterface || !interfaceMappings.Any(im => im.TargetMethods.Contains(method))
                   where !events.Any(e => e.AddMethod == method || e.RemoveMethod == method)
                   from parameter in method.GetParameters()
                   where !parameter.IsOut
                   let parameterType = parameter.ParameterType.IsByRef
                       ? parameter.ParameterType.GetElementType()
                       : parameter.ParameterType
                   where !parameterType.GetTypeInfo().IsValueType
                       && !parameter.GetCustomAttributes()
                           .Any(
                               a => a.GetType().Name == nameof(NotNullAttribute)
                                   || a.GetType().Name == nameof(CanBeNullAttribute))
                   select type.FullName + "." + method.Name + "[" + parameter.Name + "]")
                .ToList();

            Assert.False(
                parametersMissingAttribute.Count > 0,
                "\r\n-- Missing NotNull annotations --\r\n" + string.Join(Environment.NewLine, parametersMissingAttribute));
        }

        protected virtual bool ShouldHaveNotNullAnnotation(MethodBase method, Type type)
            => method is ConstructorInfo || ((MethodInfo)method).GetBaseDefinition().DeclaringType == method.DeclaringType;

        [ConditionalFact]
        public virtual void Public_api_arguments_should_not_have_redundant_not_null_annotation()
        {
            var parametersWithRedundantAttribute
                = (from type in GetAllTypes(TargetAssembly.GetTypes())
                   where type.GetTypeInfo().IsVisible && !typeof(Delegate).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo())
                   let interfaceMappings = type.GetInterfaces().Select(i => type.GetTypeInfo().GetRuntimeInterfaceMap(i))
                   let events = type.GetEvents()
                   from method in type.GetMethods(AnyInstance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                       .Concat<MethodBase>(type.GetConstructors())
                   where method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly
                   from parameter in method.GetParameters()
                   let parameterType = parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType() : parameter.ParameterType
                   let attributes = parameter.GetCustomAttributes(inherit: false)
                   where (!ShouldHaveNotNullAnnotation(method, type)
                           || !type.GetTypeInfo().IsInterface && interfaceMappings.Any(im => im.TargetMethods.Contains(method))
                           || events.Any(e => e.AddMethod == method || e.RemoveMethod == method)
                           || parameterType.GetTypeInfo().IsValueType && !parameterType.GetTypeInfo().IsNullableType())
                       && attributes.Any(
                           a => a.GetType().Name == nameof(NotNullAttribute) || a.GetType().Name == nameof(CanBeNullAttribute))
                       || parameterType.GetTypeInfo().IsValueType
                       && parameterType.GetTypeInfo().IsNullableType()
                       && attributes.Any(a => a.GetType().Name == nameof(CanBeNullAttribute))
                   select type.FullName + "." + method.Name + "[" + parameter.Name + "]").ToList();

            Assert.False(
                parametersWithRedundantAttribute.Count > 0,
                "\r\n-- Redundant NotNull annotations --\r\n" + string.Join(Environment.NewLine, parametersWithRedundantAttribute));
        }

        [ConditionalFact]
        public virtual void Async_methods_should_have_overload_with_cancellation_token_and_end_with_async_suffix()
        {
            var withoutCancellationToken = new HashSet<string>
            {
                "RelationalDatabaseFacadeExtensions.CloseConnectionAsync",
                "IRelationalConnection.CloseAsync",
                "RelationalConnection.CloseAsync",
                "DbConnectionInterceptor.ConnectionClosingAsync",
                "DbConnectionInterceptor.ConnectionClosedAsync",
                "IDbConnectionInterceptor.ConnectionClosingAsync",
                "IDbConnectionInterceptor.ConnectionClosedAsync",
                "RelationalLoggerExtensions.ConnectionClosingAsync",
                "RelationalLoggerExtensions.ConnectionClosedAsync"
            };

            var asyncMethods
                = (from type in GetAllTypes(TargetAssembly.GetTypes())
                   where type.GetTypeInfo().IsVisible
                   from method in type.GetMethods(AnyInstance | BindingFlags.Static)
                   where method.DeclaringType == type
                       && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly)
                   where typeof(Task).GetTypeInfo().IsAssignableFrom(method.ReturnType.GetTypeInfo())
                   select method).ToList();

            var asyncMethodsWithToken
                = (from method in asyncMethods
                   where method.GetParameters().Any(pi => pi.ParameterType == typeof(CancellationToken))
                   select method).ToList();

            var asyncMethodsWithoutToken
                = (from method in asyncMethods
                   where !withoutCancellationToken.Contains(method.DeclaringType.Name + "." + method.Name)
                       && method.GetParameters().All(pi => pi.ParameterType != typeof(CancellationToken))
                   select method).ToList();

            var missingOverloads
                = (from methodWithoutToken in asyncMethodsWithoutToken
                   where !asyncMethodsWithToken
                       .Any(
                           methodWithToken => methodWithoutToken.Name == methodWithToken.Name
                               && methodWithoutToken.DeclaringType == methodWithToken.DeclaringType)
                   // ReSharper disable once PossibleNullReferenceException
                   select methodWithoutToken.DeclaringType.Name + "." + methodWithoutToken.Name)
                .Except(GetCancellationTokenExceptions())
                .ToList();

            Assert.False(
                missingOverloads.Count > 0,
                "\r\n-- Missing async overloads --\r\n" + string.Join(Environment.NewLine, missingOverloads));

            var missingSuffixMethods
                = asyncMethods
                    .Where(method => !method.Name.EndsWith("Async", StringComparison.Ordinal) && method.DeclaringType != null)
                    .Select(method => method.DeclaringType.Name + "." + method.Name)
                    .Except(GetAsyncSuffixExceptions())
                    .ToList();

            Assert.False(
                missingSuffixMethods.Count > 0,
                "\r\n-- Missing async suffix --\r\n" + string.Join(Environment.NewLine, missingSuffixMethods));
        }

        [ConditionalFact]
        public virtual void Public_api_bool_parameters_should_not_be_prefixed()
        {
            var prefixes = new[] { "is", "can", "has" };

            var parameters = (
                    from type in GetAllTypes(TargetAssembly.GetExportedTypes())
                    where !type.Namespace.Contains("Internal", StringComparison.Ordinal)
                    from method in type.GetTypeInfo().DeclaredMethods
                    where !method.IsPrivate
                    from parameter in method.GetParameters()
                    where parameter.ParameterType.UnwrapNullableType() == typeof(bool)
                        && prefixes.Any(parameter.Name.StartsWith)
                    select $"{type.FullName}.{method.Name}[{parameter.Name}]")
                .ToList();

            Assert.False(
                parameters.Count > 0,
                "\r\n-- Prefixed bool parameters --\r\n" + string.Join(Environment.NewLine, parameters));
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
