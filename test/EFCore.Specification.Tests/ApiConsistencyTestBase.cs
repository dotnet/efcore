// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class ApiConsistencyTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : ApiConsistencyTestBase<TFixture>.ApiConsistencyFixtureBase, new()
    {
        protected ApiConsistencyTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected const BindingFlags PublicInstance
            = BindingFlags.Instance | BindingFlags.Public;

        protected const BindingFlags AnyInstance
            = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        protected static bool IsCompilerSynthesizedMethod(MethodBase method)
            => method.Name == "op_Equality"
                || method.Name == "op_Inequality"
                || method.Name == "PrintMembers"
                // Ignore synthesized copy constructors on records
                || method is ConstructorInfo
                && method.GetParameters().Length == 1
                && method.GetParameters()[0] is var firstParam
                && firstParam.Name == "original"
                && firstParam.ParameterType == method.DeclaringType;

        protected virtual TFixture Fixture { get; }

        [ConditionalFact]
        public void Fluent_api_methods_should_not_return_void()
        {
            var voidMethods
                = (from type in GetAllTypes(Fixture.FluentApiTypes)
                   where type.IsVisible
                   from method in type.GetMethods(PublicInstance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                   where method.ReturnType == typeof(void)
                   select type.Name + "." + method.Name)
                .ToList();

            Assert.False(
                voidMethods.Count > 0,
                "\r\n-- Missing fluent returns --\r\n" + string.Join(Environment.NewLine, voidMethods));
        }

        [ConditionalFact]
        public void Generic_fluent_api_methods_should_return_generic_types()
        {
            var nonGenericMethods = new List<(Type Type, MethodInfo Method)>();
            foreach (var type in GetAllTypes(Fixture.FluentApiTypes))
            {
                if (!type.IsVisible
                    || !type.IsGenericType
                    || type.BaseType == typeof(object)
                    || type.BaseType.IsGenericType)
                {
                    continue;
                }

                foreach (var method in type.GetMethods(PublicInstance))
                {
                    if (method.ReturnType == type.BaseType
                        && !Fixture.UnmatchedMetadataMethods.Contains(method))
                    {
                        var hidingMethod = type.GetMethod(
                            method.Name,
                            method.GetGenericArguments().Length,
                            PublicInstance | BindingFlags.DeclaredOnly,
                            null,
                            method.GetParameters().Select(p => p.ParameterType).ToArray(),
                            null);
                        if (hidingMethod == null || hidingMethod == method || hidingMethod.ReturnType != type)
                        {
                            nonGenericMethods.Add((type, method));
                        }
                    }
                }
            }

            foreach (var type in GetAllTypes(Fixture.FluentApiTypes))
            {
                if (!type.IsVisible)
                {
                    continue;
                }

                // Look for extension methods
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    if (method.ReturnType != (method.GetParameters().FirstOrDefault()?.ParameterType)
                        || !Fixture.GenericFluentApiTypes.TryGetValue(method.ReturnType, out var genericType)
                        || Fixture.UnmatchedMetadataMethods.Contains(method))
                    {
                        continue;
                    }

                    var methodFound = false;
                    foreach (var hidingMethod in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    {
                        if (method.Name != hidingMethod.Name
                            || hidingMethod.GetGenericArguments().Length != genericType.GetGenericArguments().Length
                            || hidingMethod.ReturnType.GetGenericTypeDefinition() != genericType
                            || !hidingMethod.GetParameters().Skip(1).Select(p => p.ParameterType).SequenceEqual(
                                method.GetParameters().Skip(1).Select(p => p.ParameterType)))
                        {
                            continue;
                        }

                        methodFound = true;
                        break;
                    }

                    if (!methodFound)
                    {
                        nonGenericMethods.Add((type, method));
                    }
                }
            }

            Assert.False(
                nonGenericMethods.Count > 0,
                "\r\n-- Non-generic fluent returns --\r\n"
                + string.Join(
                    Environment.NewLine, nonGenericMethods.Select(
                        m =>
                            $"{m.Method.ReturnType.ShortDisplayName()} {m.Type.Name}.{m.Method.Name}({Format(m.Method.GetParameters())})")));
        }

        [ConditionalFact]
        public void Metadata_types_have_expected_structure()
        {
            var errors = Fixture.MetadataTypes.Select(ValidateMetadata)
                .Where(e => e != null)
                .ToList();

            Assert.False(
                errors.Count > 0,
                "\r\n-- Errors: --\r\n" + string.Join(Environment.NewLine, errors));
        }

        private static readonly string MetadataNamespace = typeof(IReadOnlyModel).Namespace;
        private static readonly string MetadataBuilderNamespace = typeof(IConventionModelBuilder).Namespace;

        private string ValidateMetadata(KeyValuePair<Type, (Type, Type, Type, Type)> types)
        {
            var readonlyType = types.Key;
            var (mutableType, conventionType, conventionBuilderType, runtimeType) = types.Value;

            if (!readonlyType.IsAssignableFrom(mutableType))
            {
                return $"{mutableType.Name} should derive from {readonlyType.Name}";
            }

            if (!readonlyType.IsAssignableFrom(conventionType))
            {
                return $"{mutableType.Name} should derive from {readonlyType.Name}";
            }

            if (typeof(IAnnotation) != readonlyType
                && typeof(IReadOnlyAnnotatable) != readonlyType)
            {
                if (!typeof(IReadOnlyAnnotatable).IsAssignableFrom(readonlyType))
                {
                    return $"{readonlyType.Name} should derive from IAnnotatable";
                }

                if (!typeof(IMutableAnnotatable).IsAssignableFrom(mutableType))
                {
                    return $"{mutableType.Name} should derive from IMutableAnnotatable";
                }

                if (!typeof(IConventionAnnotatable).IsAssignableFrom(conventionType))
                {
                    return $"{conventionType.Name} should derive from IConventionAnnotatable";
                }

                if (conventionBuilderType != null
                    && !typeof(IConventionAnnotatableBuilder).IsAssignableFrom(conventionBuilderType))
                {
                    return $"{conventionBuilderType.Name} should derive from IConventionAnnotatableBuilder";
                }

                if (readonlyType.Namespace != MetadataNamespace)
                {
                    return $"{readonlyType.Name} is expected to be in the {MetadataNamespace} namespace";
                }

                if (mutableType.Namespace != MetadataNamespace)
                {
                    return $"{mutableType.Name} is expected to be in the {MetadataNamespace} namespace";
                }

                if (conventionType.Namespace != MetadataNamespace)
                {
                    return $"{conventionType.Name} is expected to be in the {MetadataNamespace} namespace";
                }

                if (conventionBuilderType != null
                    && conventionBuilderType.Namespace != MetadataBuilderNamespace)
                {
                    return $"{conventionBuilderType.Name} is expected to be in the {MetadataBuilderNamespace} namespace";
                }
            }

            if (conventionBuilderType != null)
            {
                var builderProperty = conventionType.GetProperty("Builder");
                if (builderProperty == null
                    || builderProperty.PropertyType != conventionBuilderType)
                {
                    return $"{conventionType.Name} expected to have a '{conventionBuilderType.Name} Builder' property";
                }

                var metadataProperty = conventionBuilderType.GetProperty("Metadata");
                if (metadataProperty == null
                    || metadataProperty.PropertyType != conventionType)
                {
                    return $"{conventionBuilderType.Name} expected to have a '{conventionType.Name} Metadata' property";
                }
            }

            return null;
        }

        [ConditionalFact]
        public void Mutable_metadata_types_have_matching_methods()
        {
            var errors =
                Fixture.MetadataMethods.Select(
                        typeTuple =>
                            from readonlyMethod in typeTuple.ReadOnly
                            where !Fixture.UnmatchedMetadataMethods.Contains(readonlyMethod)
                            join mutableMethod in typeTuple.Mutable
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

            if (Fixture.MetadataTypes.TryGetValue(readonlyMethod.ReturnType, out var expectedReturnTypes))
            {
                if (mutableMethod == null)
                {
                    return "No IMutable equivalent of "
                        + $"{readonlyMethod.DeclaringType.Name}.{readonlyMethod.Name}({Format(readonlyMethod.GetParameters())})";
                }

                if (mutableMethod.ReturnType != expectedReturnTypes.Mutable)
                {
                    return $"{mutableMethod.DeclaringType.Name}.{mutableMethod.Name}({Format(mutableMethod.GetParameters())})"
                        + $" expected to have {expectedReturnTypes.Mutable.ShortDisplayName()} return type";
                }
            }
            else
            {
                var sequenceType = readonlyMethod.ReturnType.TryGetSequenceType();
                if (sequenceType != null
                    && Fixture.MetadataTypes.TryGetValue(sequenceType, out expectedReturnTypes))
                {
                    if (mutableMethod == null)
                    {
                        return "No IMutable equivalent of "
                            + $"{readonlyMethod.DeclaringType.Name}.{readonlyMethod.Name}({Format(readonlyMethod.GetParameters())})";
                    }

                    if (mutableMethod.ReturnType.TryGetSequenceType() != expectedReturnTypes.Mutable)
                    {
                        return $"{mutableMethod.DeclaringType.Name}.{mutableMethod.Name}({Format(mutableMethod.GetParameters())})"
                            + $" expected to have a return type that derives from IEnumerable<{expectedReturnTypes.Mutable}>.";
                    }
                }
            }

            return null;
        }

        [ConditionalFact]
        public void Convention_metadata_types_have_matching_methods()
        {
            var errors =
                Fixture.MetadataMethods.Select(
                        typeTuple =>
                            from mutableMethod in typeTuple.Mutable
                            where !Fixture.UnmatchedMetadataMethods.Contains(mutableMethod)
                            join conventionMethod in typeTuple.Convention
                                on GetConventionName(mutableMethod) equals conventionMethod.Name into conventionGroup
                            from conventionMethod in conventionGroup.DefaultIfEmpty()
                            select (mutableMethod, conventionMethod))
                    .SelectMany(m => m.Select(MatchConvention))
                    .Where(e => e != null)
                    .ToList();

            Assert.False(
                errors.Count > 0,
                "\r\n-- Mismatches: --\r\n" + string.Join(Environment.NewLine, errors));

            static string GetConventionName(MethodInfo mutableMethod)
            {
                var name = mutableMethod.Name;
                if (mutableMethod.Name.StartsWith("set_", StringComparison.Ordinal))
                {
                    name = "Set" + name[4..];
                }

                return name;
            }
        }

        private string MatchConvention((MethodInfo Mutable, MethodInfo Convention) methodTuple)
        {
            var (mutableMethod, conventionMethod) = methodTuple;

            Type expectedReturnType;
            if (mutableMethod.ReturnType == typeof(void))
            {
                if (conventionMethod == null)
                {
                    return "No IConvention equivalent of "
                        + $"{mutableMethod.DeclaringType.Name}.{mutableMethod.Name}({Format(mutableMethod.GetParameters())})";
                }
            }
            else if (Fixture.MutableMetadataTypes.TryGetValue(mutableMethod.ReturnType, out expectedReturnType))
            {
                if (conventionMethod == null)
                {
                    return "No IConvention equivalent of "
                        + $"{mutableMethod.DeclaringType.Name}.{mutableMethod.Name}({Format(mutableMethod.GetParameters())})";
                }

                if (conventionMethod.ReturnType != expectedReturnType)
                {
                    return $"{conventionMethod.DeclaringType.Name}.{conventionMethod.Name}({Format(conventionMethod.GetParameters())})"
                        + $" expected to have {expectedReturnType.ShortDisplayName()} return type";
                }
            }
            else
            {
                var sequenceType = mutableMethod.ReturnType.TryGetSequenceType();
                if (sequenceType != null
                    && Fixture.MutableMetadataTypes.TryGetValue(sequenceType, out expectedReturnType))
                {
                    if (conventionMethod == null)
                    {
                        return "No IConvention equivalent of "
                            + $"{mutableMethod.DeclaringType.Name}.{mutableMethod.Name}({Format(mutableMethod.GetParameters())})";
                    }

                    if (conventionMethod.ReturnType.TryGetSequenceType() != expectedReturnType)
                    {
                        return $"{conventionMethod.DeclaringType.Name}.{conventionMethod.Name}({Format(conventionMethod.GetParameters())})"
                            + $" expected to have a return type that derives from IEnumerable<{expectedReturnType.Name}>.";
                    }
                }
            }

            return null;
        }

        [ConditionalFact]
        public void Convention_metadata_types_have_expected_methods()
        {
            var errors =
                Fixture.MetadataMethods.Select(t => ValidateConventionMethods(t.Convention))
                    .Where(e => e != null)
                    .ToList();

            Assert.False(
                errors.Count > 0,
                "\r\n-- Errors: --\r\n" + string.Join(Environment.NewLine, errors));
        }

        private string ValidateConventionMethods(IReadOnlyList<MethodInfo> methods)
        {
            if (methods.Count == 0)
            {
                return null;
            }

            var type = methods[0].DeclaringType;
            var methodLookup = new Dictionary<string, MethodInfo>();
            foreach (var method in methods)
            {
                methodLookup[method.Name] = method;
            }

            foreach (var methodTuple in methodLookup)
            {
                if (!Fixture.UnmatchedMetadataMethods.Contains(methodTuple.Value)
                    && methodTuple.Key.StartsWith("Set", StringComparison.Ordinal))
                {
                    var expectedName = "Get" + methodTuple.Key[3..] + "ConfigurationSource";
                    if (!methodLookup.TryGetValue(expectedName, out var getAspectConfigurationSource))
                    {
                        return $"{type.Name} expected to have a {expectedName}() method";
                    }

                    if (getAspectConfigurationSource.ReturnType != typeof(ConfigurationSource?))
                    {
                        return $"{type.Name}.{getAspectConfigurationSource.Name}({Format(getAspectConfigurationSource.GetParameters())})"
                            + " expected to have ConfigurationSource? return type";
                    }
                }
            }

            return null;
        }

        [ConditionalFact]
        public void Convention_builder_types_have_expected_methods()
        {
            var errors =
                Fixture.MetadataMethods.Select(t => ValidateConventionBuilderMethods(t.ConventionBuilder))
                    .Where(e => e != null)
                    .ToList();

            Assert.False(
                errors.Count > 0,
                "\r\n-- Errors: --\r\n" + string.Join(Environment.NewLine, errors));
        }

        private string ValidateConventionBuilderMethods(IReadOnlyList<MethodInfo> methods)
        {
            if (methods == null
                || methods.Count == 0)
            {
                return null;
            }

            var declaringType = methods[0].DeclaringType;
            var builderType = methods[0].IsStatic ? methods[0].GetParameters()[0].ParameterType : declaringType;
            var methodLookup = new Dictionary<string, MethodInfo>();
            foreach (var method in methods)
            {
                methodLookup[method.Name] = method;
            }

            foreach (var method in methodLookup.Values)
            {
                if (Fixture.UnmatchedMetadataMethods.Contains(method)
                    || method.ReturnType != builderType)
                {
                    continue;
                }

                var expectedName = method.Name.StartsWith("HasNo", StringComparison.Ordinal)
                    ? "CanRemove" + method.Name[5..]
                    : "CanSet"
                    + (method.Name.StartsWith("Has", StringComparison.Ordinal)
                        || method.Name.StartsWith("Use", StringComparison.Ordinal)
                            ? method.Name[3..]
                            : method.Name.StartsWith("To", StringComparison.Ordinal)
                                ? method.Name[2..]
                                : method.Name);
                if (!methodLookup.TryGetValue(expectedName, out var canSetMethod))
                {
                    return $"{declaringType.Name} expected to have a {expectedName} method";
                }

                var parameterIndex = method.IsStatic ? 1 : 0;
                var parameters = method.GetParameters();
                if (parameters.Length > parameterIndex
                    && parameters[parameterIndex].ParameterType != canSetMethod.GetParameters()[parameterIndex].ParameterType)
                {
                    return $"{declaringType.Name}.{canSetMethod.Name}({Format(canSetMethod.GetParameters())})"
                        + $" expected to have the first parameter of type {parameters[parameterIndex].ParameterType.ShortDisplayName()}";
                }
            }

            return null;
        }

        [ConditionalFact]
        public void Runtime_metadata_types_have_matching_methods()
        {
            var errors =
                Fixture.MetadataMethods.Select(
                        typeTuple =>
                            from readOnlyMethod in typeTuple.ReadOnly
                            where !Fixture.UnmatchedMetadataMethods.Contains(readOnlyMethod)
                            join runtimeMethod in typeTuple.Runtime
                                on readOnlyMethod.Name equals runtimeMethod?.Name into runtimeGroup
                            from runtimeMethod in runtimeGroup.DefaultIfEmpty()
                            select (readOnlyMethod, runtimeMethod))
                    .SelectMany(m => m.Select(MatchRuntime))
                    .Where(e => e != null)
                    .ToList();

            Assert.False(
                errors.Count > 0,
                "\r\n-- Mismatches: --\r\n" + string.Join(Environment.NewLine, errors));
        }

        private string MatchRuntime((MethodInfo ReadOnly, MethodInfo Runtime) methodTuple)
        {
            var (readOnlyMethod, runtimeMethod) = methodTuple;

            Type expectedReturnType;
            if (readOnlyMethod.ReturnType == typeof(void))
            {
                if (runtimeMethod == null)
                {
                    return "No IRuntime equivalent of "
                        + $"{readOnlyMethod.DeclaringType.Name}.{readOnlyMethod.Name}({Format(readOnlyMethod.GetParameters())})";
                }
            }
            else if (Fixture.MutableMetadataTypes.TryGetValue(readOnlyMethod.ReturnType, out expectedReturnType))
            {
                if (runtimeMethod == null)
                {
                    return "No IRuntime equivalent of "
                        + $"{readOnlyMethod.DeclaringType.Name}.{readOnlyMethod.Name}({Format(readOnlyMethod.GetParameters())})";
                }

                if (runtimeMethod.ReturnType != expectedReturnType)
                {
                    return $"{runtimeMethod.DeclaringType.Name}.{runtimeMethod.Name}({Format(runtimeMethod.GetParameters())})"
                        + $" expected to have {expectedReturnType.ShortDisplayName()} return type";
                }
            }
            else
            {
                var sequenceType = readOnlyMethod.ReturnType.TryGetSequenceType();
                if (sequenceType != null
                    && Fixture.MutableMetadataTypes.TryGetValue(sequenceType, out expectedReturnType))
                {
                    if (runtimeMethod == null)
                    {
                        return "No IRuntime equivalent of "
                            + $"{readOnlyMethod.DeclaringType.Name}.{readOnlyMethod.Name}({Format(readOnlyMethod.GetParameters())})";
                    }

                    if (runtimeMethod.ReturnType.TryGetSequenceType() != expectedReturnType)
                    {
                        return $"{runtimeMethod.DeclaringType.Name}.{runtimeMethod.Name}({Format(runtimeMethod.GetParameters())})"
                            + $" expected to have a return type that derives from IEnumerable<{expectedReturnType.Name}>.";
                    }
                }
            }

            return null;
        }

        [ConditionalFact]
        public void Readonly_metadata_methods_have_expected_name()
        {
            var errors =
                Fixture.MetadataMethods
                    .SelectMany(m => m.ReadOnly.Select(ValidateMethodName))
                    .Where(e => e != null)
                    .ToList();

            Assert.False(
                errors.Count > 0,
                "\r\n-- Errors: --\r\n" + string.Join(Environment.NewLine, errors));
        }

        protected string ValidateMethodName(MethodInfo method)
        {
            var name = method.Name;
            if (name.StartsWith("get_", StringComparison.Ordinal))
            {
                name = name[4..];
                if (name.StartsWith("Get", StringComparison.Ordinal)
                    && !Fixture.MetadataMethodExceptions.Contains(method))
                {
                    return $"{method.DeclaringType.ShortDisplayName()}.{name}({Format(method.GetParameters())}) expected to not have "
                        + "Get prefix";
                }
            }

            return null;
        }

        [ConditionalFact]
        public void Mutable_metadata_methods_have_expected_shape()
        {
            var errors =
                Fixture.MetadataMethods
                    .SelectMany(m => m.Mutable.Select(ValidateMutableMethod))
                    .Where(e => e != null)
                    .ToList();

            Assert.False(
                errors.Count > 0,
                "\r\n-- Errors: --\r\n" + string.Join(Environment.NewLine, errors));
        }

        private string ValidateMutableMethod(MethodInfo mutableMethod)
        {
            var message = ValidateMethodName(mutableMethod);
            if (message != null)
            {
                return message;
            }

            var parameters = mutableMethod.GetParameters();
            var parameterIndex = mutableMethod.IsStatic ? 1 : 0;
            var firstParameter = parameters.Length > parameterIndex ? parameters[parameterIndex] : null;
            var name = mutableMethod.Name;
            if (firstParameter != null
                && (name.StartsWith("Add", StringComparison.Ordinal)
                    || name.StartsWith("Remove", StringComparison.Ordinal)
                    || name.StartsWith("Set", StringComparison.Ordinal))
                && !Fixture.MetadataMethodExceptions.Contains(mutableMethod)
                && mutableMethod.ReturnType != firstParameter.ParameterType
                && (firstParameter.ParameterType != typeof(Type) || mutableMethod.ReturnType != typeof(string))
                && (firstParameter.ParameterType != typeof(string) || mutableMethod.ReturnType != typeof(FieldInfo))
                && !Fixture.MutableMetadataTypes.ContainsKey(mutableMethod.ReturnType))
            {
                if (name.StartsWith("Set", StringComparison.Ordinal))
                {
                    if (mutableMethod.ReturnType != typeof(void))
                    {
                        return $"{mutableMethod.DeclaringType.Name}.{name}({Format(parameters)}) expected to have a void return type";
                    }
                }
                else
                {
                    return $"{mutableMethod.DeclaringType.Name}.{name}({Format(parameters)}) expected to have an IMutable or "
                        + $"{firstParameter.ParameterType.ShortDisplayName()} return type";
                }
            }

            return null;
        }

        [ConditionalFact]
        public void Convention_metadata_methods_have_expected_shape()
        {
            var errors =
                Fixture.MetadataMethods
                    .SelectMany(m => m.Convention.Select(ValidateConventionMethod))
                    .Where(e => e != null)
                    .ToList();

            Assert.False(
                errors.Count > 0,
                "\r\n-- Errors: --\r\n" + string.Join(Environment.NewLine, errors));
        }

        private string ValidateConventionMethod(MethodInfo conventionMethod)
        {
            var message = ValidateMethodName(conventionMethod);
            if (message != null)
            {
                return message;
            }

            var parameters = conventionMethod.GetParameters();
            var parameterIndex = conventionMethod.IsStatic ? 1 : 0;
            var firstParameter = parameters.Length > parameterIndex ? parameters[parameterIndex] : null;
            var name = conventionMethod.Name;
            if (firstParameter != null
                && (name.StartsWith("Add", StringComparison.Ordinal)
                    || name.StartsWith("Remove", StringComparison.Ordinal)
                    || name.StartsWith("Set", StringComparison.Ordinal))
                && !Fixture.MetadataMethodExceptions.Contains(conventionMethod)
                && conventionMethod.ReturnType != firstParameter.ParameterType
                && (firstParameter.ParameterType != typeof(Type) || conventionMethod.ReturnType != typeof(string))
                && (firstParameter.ParameterType != typeof(string) || conventionMethod.ReturnType != typeof(FieldInfo))
                && (parameters.Length <= parameterIndex + 2
                    || (conventionMethod.ReturnType != parameters[parameterIndex + 1].ParameterType
                        && !typeof(ITuple).IsAssignableFrom(conventionMethod.ReturnType)))
                && !Fixture.ConventionMetadataTypes.ContainsKey(conventionMethod.ReturnType))
            {
                return
                    $"{conventionMethod.DeclaringType.ShortDisplayName()}.{name}({Format(parameters)}) expected to have an IConvention or "
                    + $"{firstParameter.ParameterType.ShortDisplayName()} return type";
            }

            if (parameters.Length > parameterIndex
                && !Fixture.MetadataMethodExceptions.Contains(conventionMethod)
                && !name.StartsWith("Remove", StringComparison.Ordinal)
                && !name.StartsWith("Find", StringComparison.Ordinal)
                && !name.StartsWith("Get", StringComparison.Ordinal)
                && name != "IsOwned"
                && name != "IsIgnored")
            {
                var lastParameter = conventionMethod.GetParameters()[^1];
                if (lastParameter.Name != "fromDataAnnotation"
                    || !Equals(lastParameter.DefaultValue, false))
                {
                    return
                        $"{conventionMethod.DeclaringType.ShortDisplayName()}.{name}({Format(parameters)}) expected to have a 'bool fromDataAnnotation = false' parameter";
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
                           || it.GetConstructors()[0].GetParameters()[0].Name != "dependencies"
                           // Check that the parameter has a non-public copy constructor, identifying C# 9 records
                           || !it.GetConstructors()[0].GetParameters()[0].ParameterType
                               .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                               .Any(c => c.GetParameters() is var parameters
                                   && parameters.Length == 1
                                   && parameters[0].Name == "original"))
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
                ?? descriptor.ImplementationFactory?.GetType().GenericTypeArguments[1];

        [ConditionalFact]
        public virtual void Private_classes_should_be_sealed()
        {
            var nonSealedPrivates
                = (from type in GetAllTypes(TargetAssembly.GetTypes())
                   where type.IsNestedPrivate
                       && !type.IsSealed
                       && !type.IsAbstract
                       && !type.DeclaringType.GetNestedTypes(BindingFlags.NonPublic).Any(t => t.BaseType == type)
                   select type.FullName)
                .ToList();

            Assert.False(
                nonSealedPrivates.Count > 0,
                "\r\n-- Private class is not sealed --\r\n" + string.Join(Environment.NewLine, nonSealedPrivates));
        }

        [ConditionalFact]
        public virtual void Public_inheritable_apis_should_be_virtual()
        {
            var nonVirtualMethods
                = (from type in GetAllTypes(TargetAssembly.GetTypes())
                   where type.IsVisible
                       && !type.IsSealed
                   from method in type.GetMethods(AnyInstance)
                   where method.DeclaringType == type
                       && !Fixture.NonVirtualMethods.Contains(method)
                       && (!method.IsVirtual || method.IsFinal)
                       && !method.Name.StartsWith("add_", StringComparison.Ordinal)
                       && !method.Name.StartsWith("remove_", StringComparison.Ordinal)
                       && !method.Name.Equals("get_NodeType", StringComparison.Ordinal)
                       && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly)
                   select type.FullName + "." + method.Name)
                .ToList();

            Assert.False(
                nonVirtualMethods.Count > 0,
                "\r\n-- Missing virtual APIs --\r\n" + string.Join(Environment.NewLine, nonVirtualMethods));
        }

        [ConditionalFact]
        public virtual void Public_api_arguments_should_have_not_null_annotation()
        {
            var parametersMissingAttribute
                = (from type in GetAllTypes(TargetAssembly.GetTypes())
                   where type.IsVisible
                       && !typeof(Delegate).IsAssignableFrom(type)
                   let interfaceMappings = type.GetInterfaces().Select(i => type.GetTypeInfo().GetRuntimeInterfaceMap(i))
                   let events = type.GetEvents()
                   from method in type.GetMethods(AnyInstance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                       .Concat<MethodBase>(
                           type.GetConstructors(
                               BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static))
                   where (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly)
                       && !IsCompilerSynthesizedMethod(method)
                       && !Fixture.NotAnnotatedMethods.Contains(method)
                       && (method is ConstructorInfo || ((MethodInfo)method).GetBaseDefinition().DeclaringType == method.DeclaringType)
                       && (type.IsInterface || !interfaceMappings.Any(im => im.TargetMethods.Contains(method)))
                       && (!events.Any(e => e.AddMethod == method || e.RemoveMethod == method))
                   from parameter in method.GetParameters()
                   where !parameter.IsOut
                   let parameterType = parameter.ParameterType.IsByRef
                       ? parameter.ParameterType.GetElementType()
                       : parameter.ParameterType
                   where !parameterType.IsValueType
                       && !parameter.GetCustomAttributes()
                           .Any(
                               a => a.GetType().Namespace == "JetBrains.Annotations"
                                   && (a.GetType().Name == nameof(NotNullAttribute)
                                       || a.GetType().Name == nameof(CanBeNullAttribute)))
                   select $"{type.FullName}.{method.Name}[{parameter.Name}]")
                .ToList();

            Assert.False(
                parametersMissingAttribute.Count > 0,
                "\r\n-- Missing NotNull annotations --\r\n" + string.Join(Environment.NewLine, parametersMissingAttribute));
        }

        [ConditionalFact]
        public virtual void Public_api_arguments_should_not_have_redundant_not_null_annotation()
        {
            var parametersWithRedundantAttribute
                = (from type in GetAllTypes(TargetAssembly.GetTypes())
                   where type.IsVisible && !typeof(Delegate).IsAssignableFrom(type)
                   let interfaceMappings = type.GetInterfaces().Select(i => type.GetTypeInfo().GetRuntimeInterfaceMap(i))
                   let events = type.GetEvents()
                   from method in type.GetMethods(AnyInstance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                       .Concat<MethodBase>(type.GetConstructors())
                   where method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly
                   from parameter in method.GetParameters()
                   let parameterType = parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType() : parameter.ParameterType
                   let attributes = parameter.GetCustomAttributes(inherit: false)
                   where (!(method is ConstructorInfo || ((MethodInfo)method).GetBaseDefinition().DeclaringType == method.DeclaringType)
                           || !type.IsInterface && interfaceMappings.Any(im => im.TargetMethods.Contains(method))
                           || events.Any(e => e.AddMethod == method || e.RemoveMethod == method)
                           || parameterType.IsValueType && !parameterType.IsNullableType())
                       && attributes.Any(
                           a => a.GetType().Namespace == "JetBrains.Annotations"
                               && (a.GetType().Name == nameof(NotNullAttribute)
                                   || a.GetType().Name == nameof(CanBeNullAttribute)))
                       || parameterType.IsValueType
                       && parameterType.IsNullableType()
                       && attributes.Any(a => a.GetType().Name == nameof(CanBeNullAttribute))
                   select (Type: type, Method: method, Parameter: parameter)).ToList();

            Assert.False(
                parametersWithRedundantAttribute.Count > 0,
                "\r\n-- Redundant NotNull annotations --\r\n" + string.Join(Environment.NewLine,
                    parametersWithRedundantAttribute.Select(t => $"{t.Type.FullName}.{t.Method.Name}[{t.Parameter.Name}]")));
        }

        private static readonly HashSet<MethodInfo> _nonCancellableAsyncMethods = new();

        protected virtual HashSet<MethodInfo> NonCancellableAsyncMethods
            => _nonCancellableAsyncMethods;

        [ConditionalFact]
        public virtual void Async_methods_should_have_overload_with_cancellation_token_and_end_with_async_suffix()
        {
            var asyncMethods
                = (from type in GetAllTypes(TargetAssembly.GetTypes())
                   where type.IsVisible
                   from method in type.GetMethods(AnyInstance | BindingFlags.Static)
                   where method.DeclaringType == type
                       && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly)
                   where typeof(Task).IsAssignableFrom(method.ReturnType)
                   select method).ToList();

            var asyncMethodsWithToken
                = (from method in asyncMethods
                   where method.GetParameters().Any(pi => pi.ParameterType == typeof(CancellationToken))
                   select method).ToList();

            var asyncMethodsWithoutToken
                = (from method in asyncMethods
                   where !NonCancellableAsyncMethods.Contains(method)
                       && method.GetParameters().All(pi => pi.ParameterType != typeof(CancellationToken))
                   select method).ToList();

            var missingOverloads
                = (from methodWithoutToken in asyncMethodsWithoutToken
                   where !asyncMethodsWithToken
                           .Any(
                               methodWithToken => methodWithoutToken.Name == methodWithToken.Name
                                   && methodWithoutToken.DeclaringType == methodWithToken.DeclaringType)
                       && !Fixture.AsyncMethodExceptions.Contains(methodWithoutToken)
                   // ReSharper disable once PossibleNullReferenceException
                   select methodWithoutToken.DeclaringType.Name + "." + methodWithoutToken.Name)
                .ToList();

            Assert.False(
                missingOverloads.Count > 0,
                "\r\n-- Missing async overloads --\r\n" + string.Join(Environment.NewLine, missingOverloads));

            var missingSuffixMethods
                = asyncMethods
                    .Where(
                        method => !method.Name.EndsWith("Async", StringComparison.Ordinal)
                            && method.DeclaringType != null
                            && !Fixture.AsyncMethodExceptions.Contains(method))
                    .Select(method => method.DeclaringType.Name + "." + method.Name)
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
                        && prefixes.Any(p => parameter.Name.StartsWith(p, StringComparison.Ordinal))
                    select $"{type.FullName}.{method.Name}[{parameter.Name}]")
                .ToList();

            Assert.False(
                parameters.Count > 0,
                "\r\n-- Prefixed bool parameters --\r\n" + string.Join(Environment.NewLine, parameters));
        }

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

        private static string Format(ParameterInfo[] parameters)
            => string.Join(", ", parameters.Select(p => p.ParameterType.Name));

        public abstract class ApiConsistencyFixtureBase
        {
            protected ApiConsistencyFixtureBase()
            {
                Initialize();
            }

            public virtual HashSet<Type> FluentApiTypes { get; } = new();

            public virtual Dictionary<Type, Type> GenericFluentApiTypes { get; } = new()
            {
                { typeof(CollectionCollectionBuilder), typeof(CollectionCollectionBuilder<,>) },
                { typeof(CollectionNavigationBuilder), typeof(CollectionNavigationBuilder<,>) },
                { typeof(DataBuilder), typeof(DataBuilder<>) },
                { typeof(DiscriminatorBuilder), typeof(DiscriminatorBuilder<>) },
                { typeof(EntityTypeBuilder), typeof(EntityTypeBuilder<>) },
                { typeof(IndexBuilder), typeof(IndexBuilder<>) },
                { typeof(KeyBuilder), typeof(KeyBuilder<>) },
                { typeof(NavigationBuilder), typeof(NavigationBuilder<,>) },
                { typeof(OwnedNavigationBuilder), typeof(OwnedNavigationBuilder<,>) },
                { typeof(OwnedEntityTypeBuilder), typeof(OwnedEntityTypeBuilder<>) },
                { typeof(OwnershipBuilder), typeof(OwnershipBuilder<,>) },
                { typeof(PropertyBuilder), typeof(PropertyBuilder<>) },
                { typeof(ReferenceCollectionBuilder), typeof(ReferenceCollectionBuilder<,>) },
                { typeof(ReferenceNavigationBuilder), typeof(ReferenceNavigationBuilder<,>) },
                { typeof(ReferenceReferenceBuilder), typeof(ReferenceReferenceBuilder<,>) },
                { typeof(DbContextOptionsBuilder), typeof(DbContextOptionsBuilder<>) }
            };

            public virtual HashSet<MethodInfo> NonVirtualMethods { get; } = new();
            public virtual HashSet<MethodInfo> NotAnnotatedMethods { get; } = new();
            public virtual HashSet<MethodInfo> AsyncMethodExceptions { get; } = new();
            public virtual HashSet<MethodInfo> UnmatchedMetadataMethods { get; } = new();
            public virtual HashSet<MethodInfo> MetadataMethodExceptions { get; } = new();

            public virtual HashSet<PropertyInfo> ComputedDependencyProperties { get; }
                = new()
                {
                    typeof(ProviderConventionSetBuilderDependencies).GetProperty(
                        nameof(ProviderConventionSetBuilderDependencies.ContextType)),
                    typeof(QueryCompilationContextDependencies).GetProperty(nameof(QueryCompilationContextDependencies.ContextType)),
                    typeof(QueryCompilationContextDependencies).GetProperty(
                        nameof(QueryCompilationContextDependencies.QueryTrackingBehavior)),
                    typeof(QueryContextDependencies).GetProperty(nameof(QueryContextDependencies.StateManager)),
#pragma warning disable CS0618 // Type or member is obsolete
                    typeof(QueryContextDependencies).GetProperty(nameof(QueryContextDependencies.QueryProvider))
#pragma warning restore CS0618 // Type or member is obsolete
                };

            public Dictionary<Type, (Type Mutable, Type Convention, Type ConventionBuilder, Type Runtime)> MetadataTypes { get; }
                = new()
                {
                    {
                        typeof(IReadOnlyModel),
                        (typeof(IMutableModel),
                        typeof(IConventionModel),
                        typeof(IConventionModelBuilder),
                        typeof(IModel))
                    },
                    {
                        typeof(IReadOnlyAnnotatable),
                        (typeof(IMutableAnnotatable),
                        typeof(IConventionAnnotatable),
                        typeof(IConventionAnnotatableBuilder),
                        typeof(IAnnotatable))
                    },
                    {
                        typeof(IAnnotation),
                        (typeof(IAnnotation),
                        typeof(IConventionAnnotation),
                        null,
                        null)
                    },
                    {
                        typeof(IReadOnlyEntityType),
                        (typeof(IMutableEntityType),
                        typeof(IConventionEntityType),
                        typeof(IConventionEntityTypeBuilder),
                        typeof(IEntityType))
                    },
                    {
                        typeof(IReadOnlyTypeBase),
                        (typeof(IMutableTypeBase),
                        typeof(IConventionTypeBase),
                        null,
                        typeof(ITypeBase))
                    },
                    {
                        typeof(IReadOnlyKey),
                        (typeof(IMutableKey),
                        typeof(IConventionKey),
                        typeof(IConventionKeyBuilder),
                        typeof(IKey))
                    },
                    {
                        typeof(IReadOnlyForeignKey),
                        (typeof(IMutableForeignKey),
                        typeof(IConventionForeignKey),
                        typeof(IConventionForeignKeyBuilder),
                        typeof(IForeignKey))
                    },
                    {
                        typeof(IReadOnlyIndex),
                        (typeof(IMutableIndex),
                        typeof(IConventionIndex),
                        typeof(IConventionIndexBuilder),
                        typeof(IIndex))
                    },
                    {
                        typeof(IReadOnlyProperty),
                        (typeof(IMutableProperty),
                        typeof(IConventionProperty),
                        typeof(IConventionPropertyBuilder),
                        typeof(IProperty))
                    },
                    {
                        typeof(IReadOnlyNavigation),
                        (typeof(IMutableNavigation),
                        typeof(IConventionNavigation),
                        typeof(IConventionNavigationBuilder),
                        typeof(INavigation))
                    },
                    {
                        typeof(IReadOnlySkipNavigation),
                        (typeof(IMutableSkipNavigation),
                        typeof(IConventionSkipNavigation),
                        typeof(IConventionSkipNavigationBuilder),
                        typeof(ISkipNavigation))
                    },
                    {
                        typeof(IReadOnlyServiceProperty),
                        (typeof(IMutableServiceProperty),
                        typeof(IConventionServiceProperty),
                        typeof(IConventionServicePropertyBuilder),
                        typeof(IServiceProperty))
                    },
                    {
                        typeof(IReadOnlyNavigationBase),
                        (typeof(IMutableNavigationBase),
                        typeof(IConventionNavigationBase),
                        null,
                        typeof(INavigationBase))
                    },
                    {
                        typeof(IReadOnlyPropertyBase),
                        (typeof(IMutablePropertyBase),
                        typeof(IConventionPropertyBase),
                        null,
                        typeof(IPropertyBase))
                    }
                };

            public Dictionary<Type, Type> MutableMetadataTypes { get; } = new();
            public Dictionary<Type, Type> ConventionMetadataTypes { get; } = new();

            public virtual
                List<(Type Type,
                    Type ReadonlyExtensions,
                    Type MutableExtensions,
                    Type ConventionExtensions,
                    Type ConventionBuilderExtensions,
                    Type RuntimeExtensions)> MetadataExtensionTypes { get; }
                = new();

            public List<(IReadOnlyList<MethodInfo> ReadOnly,
                IReadOnlyList<MethodInfo> Mutable,
                IReadOnlyList<MethodInfo> Convention,
                IReadOnlyList<MethodInfo> ConventionBuilder,
                IReadOnlyList<MethodInfo> Runtime)>
                MetadataMethods { get; } = new();

            protected virtual void Initialize()
            {
                foreach (var typeTuple in MetadataTypes.Values)
                {
                    MutableMetadataTypes[typeTuple.Mutable] = typeTuple.Convention;
                    ConventionMetadataTypes[typeTuple.Convention] = typeTuple.ConventionBuilder;
                }

                foreach (var extensionTypeTuple in MetadataExtensionTypes)
                {
                    var type = extensionTypeTuple.Type;
                    var (mutableType, conventionType, conventionBuilderType, runtimeType) = MetadataTypes[type];
                    var readOnlyMethods = extensionTypeTuple.ReadonlyExtensions?.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Where(m => !IsObsolete(m) && m.GetParameters().First().ParameterType == type).ToArray()
                        ?? new MethodInfo[0];
                    var mutableMethods = extensionTypeTuple.MutableExtensions?.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Where(m => !IsObsolete(m) && m.GetParameters().First().ParameterType == mutableType).ToArray()
                        ?? new MethodInfo[0];
                    var conventionMethods = extensionTypeTuple.ConventionExtensions?.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Where(m => !IsObsolete(m) && m.GetParameters().First().ParameterType == conventionType).ToArray()
                        ?? new MethodInfo[0];
                    var conventionBuilderMethods = extensionTypeTuple.ConventionBuilderExtensions
                        ?.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Where(m => !IsObsolete(m) && m.GetParameters().First().ParameterType == conventionBuilderType).ToArray()
                        ?? new MethodInfo[0];
                    var runtimeMethods = extensionTypeTuple.RuntimeExtensions?.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Where(m => !IsObsolete(m) && m.GetParameters().First().ParameterType == runtimeType).ToArray()
                        ?? new MethodInfo[0];
                    MetadataMethods.Add((readOnlyMethods, mutableMethods, conventionMethods, conventionBuilderMethods, runtimeMethods));
                }
            }

            protected void AddInstanceMethods(Dictionary<Type, (Type Mutable, Type Convention, Type ConventionBuilder, Type Runtime)> types)
            {
                foreach (var typeTuple in types)
                {
                    var readOnlyMethods = typeTuple.Key.GetMethods(PublicInstance)
                        .Where(m => !IsObsolete(m)).ToArray() ?? new MethodInfo[0];
                    var mutableMethods = typeTuple.Value.Mutable.GetMethods(PublicInstance)
                        .Where(m => !IsObsolete(m)).ToArray() ?? new MethodInfo[0];
                    var conventionMethods = typeTuple.Value.Convention.GetMethods(PublicInstance)
                        .Where(m => !IsObsolete(m)).ToArray() ?? new MethodInfo[0];
                    var conventionBuilderMethods = typeTuple.Value.ConventionBuilder?.GetMethods(PublicInstance)
                        .Where(m => !IsObsolete(m)).ToArray() ?? new MethodInfo[0];
                    var runtimeMethods = typeTuple.Value.Runtime?.GetMethods(PublicInstance)
                        .Where(m => !IsObsolete(m)).ToArray() ?? new MethodInfo[0];
                    MetadataMethods.Add((readOnlyMethods, mutableMethods, conventionMethods, conventionBuilderMethods, runtimeMethods));
                }
            }

            protected bool IsObsolete(MethodInfo method)
                => Attribute.IsDefined(method, typeof(ObsoleteAttribute), inherit: false);
        }
    }
}
