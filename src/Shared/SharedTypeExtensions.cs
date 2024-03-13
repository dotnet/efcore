// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System;

[DebuggerStepThrough]
internal static class SharedTypeExtensions
{
    private static readonly Dictionary<Type, string> BuiltInTypeNames = new()
    {
        { typeof(bool), "bool" },
        { typeof(byte), "byte" },
        { typeof(char), "char" },
        { typeof(decimal), "decimal" },
        { typeof(double), "double" },
        { typeof(float), "float" },
        { typeof(int), "int" },
        { typeof(long), "long" },
        { typeof(object), "object" },
        { typeof(sbyte), "sbyte" },
        { typeof(short), "short" },
        { typeof(string), "string" },
        { typeof(uint), "uint" },
        { typeof(ulong), "ulong" },
        { typeof(ushort), "ushort" },
        { typeof(void), "void" }
    };

    public static Type UnwrapNullableType(this Type type)
        => Nullable.GetUnderlyingType(type) ?? type;

    public static bool IsNullableValueType(this Type type)
        => type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

    public static bool IsNullableType(this Type type)
        => !type.IsValueType || type.IsNullableValueType();

    public static bool IsValidEntityType(this Type type)
        => type is { IsClass: true, IsArray: false }
            && type != typeof(string);

    public static bool IsValidComplexType(this Type type)
        => !type.IsArray
            && !type.IsInterface
            && !IsScalarType(type);

    public static bool IsScalarType(this Type type)
        => type == typeof(string)
            || CommonTypeDictionary.ContainsKey(type);

    public static bool IsPropertyBagType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type type)
    {
        if (type.IsGenericTypeDefinition)
        {
            return false;
        }

        var types = GetGenericTypeImplementations(type, typeof(IDictionary<,>));
        return types.Any(
            t => t.GetGenericArguments()[0] == typeof(string)
                && t.GetGenericArguments()[1] == typeof(object));
    }

    public static Type MakeNullable(this Type type, bool nullable = true)
        => type.IsNullableType() == nullable
            ? type
            : nullable
                ? typeof(Nullable<>).MakeGenericType(type)
                : type.UnwrapNullableType();

    public static bool IsNumeric(this Type type)
    {
        type = type.UnwrapNullableType();

        return type.IsInteger()
            || type == typeof(decimal)
            || type == typeof(float)
            || type == typeof(double);
    }

    public static bool IsInteger(this Type type)
    {
        type = type.UnwrapNullableType();

        return type == typeof(int)
            || type == typeof(long)
            || type == typeof(short)
            || type == typeof(byte)
            || type == typeof(uint)
            || type == typeof(ulong)
            || type == typeof(ushort)
            || type == typeof(sbyte)
            || type == typeof(char);
    }

    public static bool IsSignedInteger(this Type type)
        => type == typeof(int)
            || type == typeof(long)
            || type == typeof(short)
            || type == typeof(sbyte);

    public static bool IsAnonymousType(this Type type)
        => type.Name.StartsWith("<>", StringComparison.Ordinal)
            && type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), inherit: false).Length > 0
            && type.Name.Contains("AnonymousType");

    public static PropertyInfo? GetAnyProperty(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
        this Type type,
        string name)
    {
        var props = type.GetRuntimeProperties().Where(p => p.Name == name).ToList();
        if (props.Count > 1)
        {
            throw new AmbiguousMatchException();
        }

        return props.SingleOrDefault();
    }

    public static bool IsInstantiable(this Type type)
        => type is { IsAbstract: false, IsInterface: false }
            && (!type.IsGenericType || !type.IsGenericTypeDefinition);

    public static Type UnwrapEnumType(this Type type)
    {
        var isNullable = type.IsNullableType();
        var underlyingNonNullableType = isNullable ? type.UnwrapNullableType() : type;
        if (!underlyingNonNullableType.IsEnum)
        {
            return type;
        }

        var underlyingEnumType = Enum.GetUnderlyingType(underlyingNonNullableType);
        return isNullable ? MakeNullable(underlyingEnumType) : underlyingEnumType;
    }

    public static Type GetSequenceType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type type)
    {
        var sequenceType = TryGetSequenceType(type);
        if (sequenceType == null)
        {
            throw new ArgumentException($"The type {type.Name} does not represent a sequence");
        }

        return sequenceType;
    }

    public static Type? TryGetSequenceType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type type)
        => type.TryGetElementType(typeof(IEnumerable<>))
            ?? type.TryGetElementType(typeof(IAsyncEnumerable<>));

    public static Type? TryGetElementType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type type,
        Type interfaceOrBaseType)
    {
        if (type.IsGenericTypeDefinition)
        {
            return null;
        }

        var types = GetGenericTypeImplementations(type, interfaceOrBaseType);

        Type? singleImplementation = null;
        foreach (var implementation in types)
        {
            if (singleImplementation == null)
            {
                singleImplementation = implementation;
            }
            else
            {
                singleImplementation = null;
                break;
            }
        }

        return singleImplementation?.GenericTypeArguments.FirstOrDefault();
    }

    public static bool IsCompatibleWith(this Type propertyType, Type fieldType)
    {
        if (propertyType.IsAssignableFrom(fieldType)
            || fieldType.IsAssignableFrom(propertyType))
        {
            return true;
        }

        var propertyElementType = propertyType.TryGetSequenceType();
        var fieldElementType = fieldType.TryGetSequenceType();

        return propertyElementType != null
            && fieldElementType != null
            && IsCompatibleWith(propertyElementType, fieldElementType);
    }

    public static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
    {
        var typeInfo = type.GetTypeInfo();
        if (!typeInfo.IsGenericTypeDefinition)
        {
            var baseTypes = interfaceOrBaseType.GetTypeInfo().IsInterface
                ? typeInfo.ImplementedInterfaces
                : type.GetBaseTypes();
            foreach (var baseType in baseTypes)
            {
                if (baseType.IsGenericType
                    && baseType.GetGenericTypeDefinition() == interfaceOrBaseType)
                {
                    yield return baseType;
                }
            }

            if (type.IsGenericType
                && type.GetGenericTypeDefinition() == interfaceOrBaseType)
            {
                yield return type;
            }
        }
    }

    public static IEnumerable<Type> GetBaseTypes(this Type type)
    {
        var currentType = type.BaseType;

        while (currentType != null)
        {
            yield return currentType;

            currentType = currentType.BaseType;
        }
    }

    public static List<Type> GetBaseTypesAndInterfacesInclusive(this Type type)
    {
        var baseTypes = new List<Type>();
        var typesToProcess = new Queue<Type>();
        typesToProcess.Enqueue(type);

        while (typesToProcess.Count > 0)
        {
            type = typesToProcess.Dequeue();
            baseTypes.Add(type);

            if (type.IsNullableValueType())
            {
                typesToProcess.Enqueue(Nullable.GetUnderlyingType(type)!);
            }

            if (type.IsConstructedGenericType)
            {
                typesToProcess.Enqueue(type.GetGenericTypeDefinition());
            }

            if (type is { IsGenericTypeDefinition: false, IsInterface: false })
            {
                if (type.BaseType != null)
                {
                    typesToProcess.Enqueue(type.BaseType);
                }

                foreach (var @interface in GetDeclaredInterfaces(type))
                {
                    typesToProcess.Enqueue(@interface);
                }
            }
        }

        return baseTypes;
    }

    public static IEnumerable<Type> GetTypesInHierarchy(this Type type)
    {
        var currentType = type;

        while (currentType != null)
        {
            yield return currentType;

            currentType = currentType.BaseType;
        }
    }

    public static IEnumerable<Type> GetDeclaredInterfaces(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type type)
    {
        var interfaces = type.GetInterfaces();
        if (type.BaseType == typeof(object)
            || type.BaseType == null)
        {
            return interfaces;
        }

        return interfaces.Except(GetInterfacesSuppressed(type.BaseType));

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "https://github.com/dotnet/linker/issues/2473")]
        static IEnumerable<Type> GetInterfacesSuppressed(Type type)
            => type.GetInterfaces();
    }

    public static ConstructorInfo? GetDeclaredConstructor(
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        this Type type,
        Type[]? types)
    {
        types ??= [];

        return type.GetTypeInfo().DeclaredConstructors
            .SingleOrDefault(
                c => !c.IsStatic
                    && c.GetParameters().Select(p => p.ParameterType).SequenceEqual(types))!;
    }

    public static IEnumerable<PropertyInfo> GetPropertiesInHierarchy(this Type type, string name)
    {
        var currentType = type;
        do
        {
            var typeInfo = currentType.GetTypeInfo();
            foreach (var propertyInfo in typeInfo.DeclaredProperties)
            {
                if (propertyInfo.Name.Equals(name, StringComparison.Ordinal)
                    && !(propertyInfo.GetMethod ?? propertyInfo.SetMethod)!.IsStatic)
                {
                    yield return propertyInfo;
                }
            }

            currentType = typeInfo.BaseType;
        }
        while (currentType != null);
    }

    // Looking up the members through the whole hierarchy allows to find inherited private members.
    public static IEnumerable<MemberInfo> GetMembersInHierarchy(this Type type)
    {
        var currentType = type;

        do
        {
            // Do the whole hierarchy for properties first since looking for fields is slower.
            foreach (var propertyInfo in currentType.GetRuntimeProperties().Where(pi => !(pi.GetMethod ?? pi.SetMethod)!.IsStatic))
            {
                yield return propertyInfo;
            }

            foreach (var fieldInfo in currentType.GetRuntimeFields().Where(f => !f.IsStatic))
            {
                yield return fieldInfo;
            }

            currentType = currentType.BaseType;
        }
        while (currentType != null);
    }

    public static IEnumerable<MemberInfo> GetMembersInHierarchy(
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.NonPublicProperties
            | DynamicallyAccessedMemberTypes.PublicFields
            | DynamicallyAccessedMemberTypes.NonPublicFields)]
        this Type type,
        string name)
        => type.GetMembersInHierarchy().Where(m => m.Name == name);

    public static MethodInfo GetGenericMethod(
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicMethods)]
        this Type type,
        string name,
        int genericParameterCount,
        BindingFlags bindingFlags,
        Func<Type[], Type[], Type[]> parameterGenerator,
        bool? @override = null)
        => type.GetMethods(bindingFlags)
            .Single(
                mi => mi.Name == name
                    && ((genericParameterCount == 0 && !mi.IsGenericMethod)
                        || (mi.IsGenericMethod && mi.GetGenericArguments().Length == genericParameterCount))
                    && mi.GetParameters().Select(e => e.ParameterType).SequenceEqual(
                        parameterGenerator(
                            type.IsGenericType ? type.GetGenericArguments() : Array.Empty<Type>(),
                            mi.IsGenericMethod ? mi.GetGenericArguments() : Array.Empty<Type>()))
                    && (!@override.HasValue || (@override.Value == (mi.GetBaseDefinition().DeclaringType != mi.DeclaringType))));

    private static readonly Dictionary<Type, object> CommonTypeDictionary = new()
    {
#pragma warning disable IDE0034 // Simplify 'default' expression - default causes default(object)
        { typeof(int), default(int) },
        { typeof(Guid), default(Guid) },
        { typeof(DateOnly), default(DateOnly) },
        { typeof(DateTime), default(DateTime) },
        { typeof(DateTimeOffset), default(DateTimeOffset) },
        { typeof(TimeOnly), default(TimeOnly) },
        { typeof(long), default(long) },
        { typeof(bool), default(bool) },
        { typeof(double), default(double) },
        { typeof(short), default(short) },
        { typeof(float), default(float) },
        { typeof(byte), default(byte) },
        { typeof(char), default(char) },
        { typeof(uint), default(uint) },
        { typeof(ushort), default(ushort) },
        { typeof(ulong), default(ulong) },
        { typeof(sbyte), default(sbyte) }
#pragma warning restore IDE0034 // Simplify 'default' expression
    };

    public static object? GetDefaultValue(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        this Type type)
    {
        if (!type.IsValueType)
        {
            return null;
        }

        // A bit of perf code to avoid calling Activator.CreateInstance for common types and
        // to avoid boxing on every call. This is about 50% faster than just calling CreateInstance
        // for all value types.
        return CommonTypeDictionary.TryGetValue(type, out var value)
            ? value
            : Activator.CreateInstance(type);
    }

    [RequiresUnreferencedCode("Gets all types from the given assembly - unsafe for trimming")]
    public static IEnumerable<TypeInfo> GetConstructibleTypes(
        this Assembly assembly, IDiagnosticsLogger<DbLoggerCategory.Model>? logger = null)
        => assembly.GetLoadableDefinedTypes(logger).Where(
            t => t is { IsAbstract: false, IsGenericTypeDefinition: false });

    [RequiresUnreferencedCode("Gets all types from the given assembly - unsafe for trimming")]
    public static IEnumerable<TypeInfo> GetLoadableDefinedTypes(
        this Assembly assembly, IDiagnosticsLogger<DbLoggerCategory.Model>? logger = null)
    {
        try
        {
            return assembly.DefinedTypes;
        }
        catch (ReflectionTypeLoadException ex)
        {
            logger?.TypeLoadingErrorWarning(assembly, ex);

            return ex.Types.Where(t => t != null).Select(IntrospectionExtensions.GetTypeInfo!);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string DisplayName(this Type type, bool fullName = true, bool compilable = false)
    {
        var stringBuilder = new StringBuilder();
        ProcessType(stringBuilder, type, fullName, compilable);
        return stringBuilder.ToString();
    }

    private static void ProcessType(StringBuilder builder, Type type, bool fullName, bool compilable)
    {
        if (type.IsGenericType)
        {
            var genericArguments = type.GetGenericArguments();
            ProcessGenericType(builder, type, genericArguments, genericArguments.Length, fullName, compilable);
        }
        else if (type.IsArray)
        {
            ProcessArrayType(builder, type, fullName, compilable);
        }
        else if (BuiltInTypeNames.TryGetValue(type, out var builtInName))
        {
            builder.Append(builtInName);
        }
        else if (!type.IsGenericParameter)
        {
            if (compilable)
            {
                if (type.IsNested)
                {
                    ProcessType(builder, type.DeclaringType!, fullName, compilable);
                    builder.Append('.');
                }
                else if (fullName)
                {
                    builder.Append(type.Namespace).Append('.');
                }

                builder.Append(type.Name);
            }
            else
            {
                builder.Append(fullName ? type.FullName : type.Name);
            }
        }
    }

    private static void ProcessArrayType(StringBuilder builder, Type type, bool fullName, bool compilable)
    {
        var innerType = type;
        while (innerType.IsArray)
        {
            innerType = innerType.GetElementType()!;
        }

        ProcessType(builder, innerType, fullName, compilable);

        while (type.IsArray)
        {
            builder.Append('[');
            builder.Append(',', type.GetArrayRank() - 1);
            builder.Append(']');
            type = type.GetElementType()!;
        }
    }

    private static void ProcessGenericType(
        StringBuilder builder,
        Type type,
        Type[] genericArguments,
        int length,
        bool fullName,
        bool compilable)
    {
        if (type.IsConstructedGenericType
            && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            ProcessType(builder, type.UnwrapNullableType(), fullName, compilable);
            builder.Append('?');
            return;
        }

        var offset = type.IsNested ? type.DeclaringType!.GetGenericArguments().Length : 0;

        if (compilable)
        {
            if (type.IsNested)
            {
                ProcessType(builder, type.DeclaringType!, fullName, compilable);
                builder.Append('.');
            }
            else if (fullName)
            {
                builder.Append(type.Namespace);
                builder.Append('.');
            }
        }
        else
        {
            if (fullName)
            {
                if (type.IsNested)
                {
                    ProcessGenericType(builder, type.DeclaringType!, genericArguments, offset, fullName, compilable);
                    builder.Append('+');
                }
                else
                {
                    builder.Append(type.Namespace);
                    builder.Append('.');
                }
            }
        }

        var genericPartIndex = type.Name.IndexOf('`');
        if (genericPartIndex <= 0)
        {
            builder.Append(type.Name);
            return;
        }

        builder.Append(type.Name, 0, genericPartIndex);
        builder.Append('<');

        for (var i = offset; i < length; i++)
        {
            ProcessType(builder, genericArguments[i], fullName, compilable);
            if (i + 1 == length)
            {
                continue;
            }

            builder.Append(',');
            if (!genericArguments[i + 1].IsGenericParameter)
            {
                builder.Append(' ');
            }
        }

        builder.Append('>');
    }

    public static IEnumerable<string> GetNamespaces(this Type type)
    {
        if (BuiltInTypeNames.ContainsKey(type))
        {
            yield break;
        }

        if (type.IsArray)
        {
            foreach (var ns in type.GetElementType()!.GetNamespaces())
            {
                yield return ns;
            }

            yield break;
        }

        if (type.IsConstructedGenericType
            && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            foreach (var ns in type.UnwrapNullableType().GetNamespaces())
            {
                yield return ns;
            }
        }
        else
        {
            yield return type.Namespace!;
        }

        if (type.IsGenericType)
        {
            foreach (var typeArgument in type.GenericTypeArguments)
            {
                foreach (var ns in typeArgument.GetNamespaces())
                {
                    yield return ns;
                }
            }
        }
    }

    public static ConstantExpression GetDefaultValueConstant(this Type type)
        => (ConstantExpression)GenerateDefaultValueConstantMethod
            .MakeGenericMethod(type).Invoke(null, [])!;

    private static readonly MethodInfo GenerateDefaultValueConstantMethod =
        typeof(SharedTypeExtensions).GetTypeInfo().GetDeclaredMethod(nameof(GenerateDefaultValueConstant))!;

    private static ConstantExpression GenerateDefaultValueConstant<TDefault>()
        => Expression.Constant(default(TDefault), typeof(TDefault));
}
