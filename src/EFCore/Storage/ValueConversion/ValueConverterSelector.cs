// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.NetworkInformation;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     A registry of <see cref="ValueConverter" /> instances that can be used to find
///     the preferred converter to use to convert to and from a given model type
///     to a type that the database provider supports.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
///     </para>
/// </remarks>
public class ValueConverterSelector : IValueConverterSelector
{
    private readonly ConcurrentDictionary<(Type ModelClrType, Type ProviderClrType), ValueConverterInfo> _converters = new();

    private static readonly Type[] SignedPreferred = [typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(decimal)];

    private static readonly Type[] UnsignedPreferred =
    [
        typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(decimal)
    ];

    private static readonly Type[] FloatingPreferred = [typeof(float), typeof(double), typeof(decimal)];

    private static readonly Type[] CharPreferred =
    [
        typeof(char), typeof(int), typeof(ushort), typeof(uint), typeof(long), typeof(ulong), typeof(decimal)
    ];

    private static readonly Type[] Numerics =
    [
        typeof(int),
        typeof(long),
        typeof(short),
        typeof(byte),
        typeof(ulong),
        typeof(uint),
        typeof(ushort),
        typeof(sbyte),
        typeof(decimal),
        typeof(double),
        typeof(float)
    ];

    // ReSharper disable once InconsistentNaming
    private static readonly Type? _readOnlyIPAddressType = IPAddress.Loopback.GetType();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValueConverterSelector" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public ValueConverterSelector(ValueConverterSelectorDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ValueConverterSelectorDependencies Dependencies { get; }

    /// <summary>
    ///     Returns the list of <see cref="ValueConverter" /> instances that can be
    ///     used to convert the given model type. Converters nearer the front of
    ///     the list should be used in preference to converters nearer the end.
    /// </summary>
    /// <param name="modelClrType">The type for which a converter is needed.</param>
    /// <param name="providerClrType">The database provider type to target, or null for any.</param>
    /// <returns>The converters available.</returns>
    public virtual IEnumerable<ValueConverterInfo> Select(
        Type modelClrType,
        Type? providerClrType = null)
    {
        if (modelClrType.IsEnum)
        {
            foreach (var converterInfo in FindNumericConventions(
                         modelClrType,
                         providerClrType,
                         typeof(EnumToNumberConverter<,>),
                         EnumToStringOrBytes))
            {
                yield return converterInfo;
            }
        }
        else if (modelClrType == typeof(bool))
        {
            foreach (var converterInfo in FindNumericConventions(
                         typeof(bool),
                         providerClrType,
                         typeof(BoolToZeroOneConverter<>),
                         null))
            {
                yield return converterInfo;
            }

            if (providerClrType == null
                || providerClrType == typeof(string))
            {
                yield return BoolToStringConverter.DefaultInfo;
            }

            if (providerClrType == null
                || providerClrType == typeof(byte[]))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(byte[])),
                    static k => new ValueConverterInfo(
                        k.ModelClrType,
                        typeof(byte[]),
                        _ => new BoolToZeroOneConverter<byte>().ComposeWith(
                            NumberToBytesConverter<byte>.DefaultInfo.Create()),
                        new ConverterMappingHints(size: 1)));
            }
        }
        else if (modelClrType == typeof(char))
        {
            foreach (var valueConverterInfo in ForChar(typeof(char), providerClrType))
            {
                yield return valueConverterInfo;
            }
        }
        else if (modelClrType == typeof(Guid))
        {
            if (providerClrType == null
                || providerClrType == typeof(string))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(string)),
                    _ => GuidToStringConverter.DefaultInfo);
            }

            if (providerClrType == null
                || providerClrType == typeof(byte[]))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(byte[])),
                    _ => GuidToBytesConverter.DefaultInfo);
            }
        }
        else if (modelClrType == typeof(byte[]))
        {
            if (providerClrType == null
                || providerClrType == typeof(string))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(string)),
                    _ => BytesToStringConverter.DefaultInfo);
            }
        }
        else if (modelClrType == typeof(Uri))
        {
            if (providerClrType == null
                || providerClrType == typeof(string))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(string)),
                    _ => UriToStringConverter.DefaultInfo);
            }
        }
        else if (modelClrType == typeof(string))
        {
            if (providerClrType == null
                || providerClrType == typeof(byte[]))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(byte[])),
                    _ => StringToBytesConverter.DefaultInfo);
            }
            else if (providerClrType.IsEnum)
            {
                yield return _converters.GetOrAdd(
                    (typeof(string), providerClrType),
                    k => GetDefaultValueConverterInfo(typeof(StringToEnumConverter<>).MakeGenericType(k.ProviderClrType)));
            }
            else if (Numerics.Contains(providerClrType))
            {
                foreach (var converterInfo in FindNumericConventions(
                             typeof(string),
                             providerClrType,
                             typeof(StringToNumberConverter<>),
                             null))
                {
                    yield return converterInfo;
                }
            }
            else if (providerClrType == typeof(DateTime))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(DateTime)),
                    _ => StringToDateTimeConverter.DefaultInfo);
            }
            else if (providerClrType == typeof(DateTimeOffset))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(DateTimeOffset)),
                    _ => StringToDateTimeOffsetConverter.DefaultInfo);
            }
            else if (providerClrType == typeof(DateOnly))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(DateOnly)),
                    _ => StringToDateOnlyConverter.DefaultInfo);
            }
            else if (providerClrType == typeof(TimeSpan))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(TimeSpan)),
                    _ => StringToTimeSpanConverter.DefaultInfo);
            }
            else if (providerClrType == typeof(TimeOnly))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(TimeOnly)),
                    _ => StringToTimeOnlyConverter.DefaultInfo);
            }
            else if (providerClrType == typeof(Guid))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(Guid)),
                    _ => StringToGuidConverter.DefaultInfo);
            }
            else if (providerClrType == typeof(bool))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(bool)),
                    _ => StringToBoolConverter.DefaultInfo);
            }
            else if (providerClrType == typeof(char))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(char)),
                    _ => StringToCharConverter.DefaultInfo);
            }
            else if (providerClrType == typeof(Uri))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(Uri)),
                    _ => StringToUriConverter.DefaultInfo);
            }
        }
        else if (modelClrType == typeof(DateTime)
                 || modelClrType == typeof(DateTimeOffset)
                 || modelClrType == typeof(TimeSpan)
                 || modelClrType == typeof(TimeOnly))
        {
            if (providerClrType == null
                || providerClrType == typeof(string))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(string)),
                    k => k.ModelClrType == typeof(DateTime)
                        ? DateTimeToStringConverter.DefaultInfo
                        : k.ModelClrType == typeof(DateTimeOffset)
                            ? DateTimeOffsetToStringConverter.DefaultInfo
                            : k.ModelClrType == typeof(TimeSpan)
                                ? TimeSpanToStringConverter.DefaultInfo
                                : TimeOnlyToStringConverter.DefaultInfo);
            }

            if (providerClrType == null
                || providerClrType == typeof(long))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(long)),
                    k => k.ModelClrType == typeof(DateTime)
                        ? DateTimeToBinaryConverter.DefaultInfo
                        : k.ModelClrType == typeof(DateTimeOffset)
                            ? DateTimeOffsetToBinaryConverter.DefaultInfo
                            : k.ModelClrType == typeof(TimeSpan)
                                ? TimeSpanToTicksConverter.DefaultInfo
                                : TimeOnlyToTicksConverter.DefaultInfo);
            }

            if (providerClrType == null
                || providerClrType == typeof(byte[]))
            {
                yield return modelClrType == typeof(DateTimeOffset)
                    ? _converters.GetOrAdd(
                        (modelClrType, typeof(byte[])),
                        _ => DateTimeOffsetToBytesConverter.DefaultInfo)
                    : _converters.GetOrAdd(
                        (modelClrType, typeof(byte[])),
                        static k => new ValueConverterInfo(
                            k.ModelClrType,
                            typeof(byte[]),
                            i => (i.ModelClrType == typeof(DateTime)
                                    ? DateTimeToBinaryConverter.DefaultInfo.Create()
                                    : i.ModelClrType == typeof(TimeSpan)
                                        ? TimeSpanToTicksConverter.DefaultInfo.Create()
                                        : TimeOnlyToTicksConverter.DefaultInfo.Create())
                                .ComposeWith(
                                    NumberToBytesConverter<long>.DefaultInfo.Create()),
                            NumberToBytesConverter<long>.DefaultInfo.MappingHints));
            }
        }
        else if (modelClrType == typeof(DateOnly))
        {
            if (providerClrType == null
                || providerClrType == typeof(string))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(string)),
                    _ => DateOnlyToStringConverter.DefaultInfo);
            }

            if (providerClrType == null
                || providerClrType == typeof(byte[]))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(byte[])),
                    static k => new ValueConverterInfo(
                        k.ModelClrType,
                        typeof(byte[]),
                        _ => new DateOnlyToStringConverter().ComposeWith(
                            StringToBytesConverter.DefaultInfo.Create()),
                        StringToBytesConverter.DefaultInfo.MappingHints));
            }
        }
        else if (modelClrType == typeof(IPAddress) || modelClrType == _readOnlyIPAddressType)
        {
            if (providerClrType == null
                || providerClrType == typeof(string))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(string)),
                    _ => IPAddressToStringConverter.DefaultInfo);
            }

            if (providerClrType == typeof(byte[]))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(byte[])),
                    _ => IPAddressToBytesConverter.DefaultInfo);
            }
        }
        else if (modelClrType == typeof(PhysicalAddress))
        {
            if (providerClrType == null
                || providerClrType == typeof(string))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(string)),
                    _ => PhysicalAddressToStringConverter.DefaultInfo);
            }

            if (providerClrType == typeof(byte[]))
            {
                yield return _converters.GetOrAdd(
                    (modelClrType, typeof(byte[])),
                    _ => PhysicalAddressToBytesConverter.DefaultInfo);
            }
        }
        else if (Numerics.Contains(modelClrType)
                 && (providerClrType == null
                     || providerClrType == typeof(byte[])
                     || providerClrType == typeof(string)
                     || Numerics.Contains(providerClrType)))
        {
            foreach (var converterInfo in FindNumericConventions(
                         modelClrType,
                         providerClrType,
                         typeof(CastingConverter<,>),
                         NumberToStringOrBytes))
            {
                yield return converterInfo;
            }
        }
    }

    private IEnumerable<ValueConverterInfo> ForChar(
        Type underlyingModelType,
        Type? underlyingProviderType)
    {
        if (underlyingProviderType == null
            || underlyingProviderType == typeof(string))
        {
            yield return _converters.GetOrAdd(
                (underlyingModelType, typeof(string)),
                k => CharToStringConverter.DefaultInfo);
        }

        foreach (var converterInfo in FindNumericConventions(
                     underlyingModelType,
                     underlyingProviderType,
                     typeof(CastingConverter<,>),
                     CharToBytes))
        {
            yield return converterInfo;
        }
    }

    private IEnumerable<ValueConverterInfo> CharToBytes(
        Type underlyingModelType,
        Type? underlyingProviderType)
    {
        if (underlyingProviderType == null
            || underlyingProviderType == typeof(byte[]))
        {
            yield return _converters.GetOrAdd(
                (underlyingModelType, typeof(byte[])),
                _ => NumberToBytesConverter<char>.DefaultInfo);
        }
    }

    private IEnumerable<ValueConverterInfo> EnumToStringOrBytes(
        Type underlyingModelType,
        Type? underlyingProviderType)
    {
        if (underlyingProviderType == null
            || underlyingProviderType == typeof(string))
        {
            yield return _converters.GetOrAdd(
                (underlyingModelType, typeof(string)),
                k => GetDefaultValueConverterInfo(typeof(EnumToStringConverter<>).MakeGenericType(k.ModelClrType)));
        }

        if (underlyingProviderType == null
            || underlyingProviderType == typeof(byte[]))
        {
            yield return _converters.GetOrAdd(
                (underlyingModelType, typeof(byte[])),
                static k =>
                {
                    var (modelClrType, _) = k;
                    var toNumber = GetDefaultValueConverterInfo(
                        typeof(EnumToNumberConverter<,>).MakeGenericType(modelClrType, modelClrType.GetEnumUnderlyingType()));

                    var toBytes = GetDefaultValueConverterInfo(
                        typeof(NumberToBytesConverter<>).MakeGenericType(modelClrType.GetEnumUnderlyingType()));

                    return new ValueConverterInfo(
                        modelClrType,
                        typeof(byte[]),
                        _ => toNumber.Create().ComposeWith(toBytes.Create()),
                        toBytes.MappingHints);
                });
        }
    }

    private IEnumerable<ValueConverterInfo> NumberToStringOrBytes(
        Type underlyingModelType,
        Type? underlyingProviderType)
    {
        if (underlyingProviderType == null
            || underlyingProviderType == typeof(string))
        {
            yield return _converters.GetOrAdd(
                (underlyingModelType, typeof(string)),
                k => GetDefaultValueConverterInfo(typeof(NumberToStringConverter<>).MakeGenericType(k.ModelClrType)));
        }

        if (underlyingProviderType == null
            || underlyingProviderType == typeof(byte[]))
        {
            yield return _converters.GetOrAdd(
                (underlyingModelType, typeof(byte[])),
                k => GetDefaultValueConverterInfo(typeof(NumberToBytesConverter<>).MakeGenericType(k.ModelClrType)));
        }
    }

    private IEnumerable<ValueConverterInfo> FindNumericConventions(
        Type modelType,
        Type? providerType,
        Type converterType,
        Func<Type, Type?, IEnumerable<ValueConverterInfo>>? afterPreferred)
    {
        var usedTypes = new List<Type> { modelType }; // List not hash because few members
        var underlyingModelType = modelType.UnwrapEnumType();

        if (modelType.IsEnum)
        {
            foreach (var converterInfo in FindPreferredConversions(
                         [underlyingModelType], modelType, providerType, converterType))
            {
                yield return converterInfo;

                usedTypes.Add(converterInfo.ProviderClrType);
            }
        }

        foreach (var converterInfo in FindPreferredConversions(
                     SignedPreferred, modelType, providerType, converterType))
        {
            if (!usedTypes.Contains(converterInfo.ProviderClrType))
            {
                yield return converterInfo;

                usedTypes.Add(converterInfo.ProviderClrType);
            }
        }

        if (underlyingModelType == typeof(byte)
            || underlyingModelType == typeof(uint)
            || underlyingModelType == typeof(ulong)
            || underlyingModelType == typeof(ushort))
        {
            foreach (var converterInfo in FindPreferredConversions(
                         UnsignedPreferred, modelType, providerType, converterType))
            {
                if (!usedTypes.Contains(converterInfo.ProviderClrType))
                {
                    yield return converterInfo;

                    usedTypes.Add(converterInfo.ProviderClrType);
                }
            }
        }

        if (underlyingModelType == typeof(float)
            || underlyingModelType == typeof(double))
        {
            foreach (var converterInfo in FindPreferredConversions(
                         FloatingPreferred, modelType, providerType, converterType))
            {
                yield return converterInfo;

                usedTypes.Add(converterInfo.ProviderClrType);
            }
        }

        if (underlyingModelType == typeof(char))
        {
            foreach (var converterInfo in FindPreferredConversions(
                         CharPreferred, modelType, providerType, converterType))
            {
                yield return converterInfo;

                usedTypes.Add(converterInfo.ProviderClrType);
            }
        }

        if (afterPreferred != null)
        {
            foreach (var converterInfo in afterPreferred(modelType, providerType))
            {
                yield return converterInfo;

                usedTypes.Add(converterInfo.ProviderClrType);
            }
        }

        foreach (var numeric in Numerics)
        {
            if ((providerType == null
                    || providerType == numeric)
                && !usedTypes.Contains(numeric))
            {
                yield return _converters.GetOrAdd(
                    (modelType, numeric),
                    static (k, t) => GetDefaultValueConverterInfo(
                        t.GetTypeInfo().GenericTypeParameters.Length == 1
                            ? t.MakeGenericType(k.ProviderClrType)
                            : t.MakeGenericType(k.ModelClrType, k.ProviderClrType)),
                    converterType);
            }
        }
    }

    private IEnumerable<ValueConverterInfo> FindPreferredConversions(
        Type[] candidateTypes,
        Type modelType,
        Type? providerType,
        Type converterType)
    {
        var underlyingModelType = modelType.UnwrapEnumType();

        for (var i = 0; i < candidateTypes.Length; i++)
        {
            if (underlyingModelType == candidateTypes[i])
            {
                if (!modelType.IsEnum)
                {
                    i++;
                }

                for (; i < candidateTypes.Length; i++)
                {
                    if (providerType == null
                        || providerType == candidateTypes[i])
                    {
                        yield return _converters.GetOrAdd(
                            (modelType, candidateTypes[i]),
                            static (k, t) => GetDefaultValueConverterInfo(t.MakeGenericType(k.ModelClrType, k.ProviderClrType)),
                            converterType);
                    }
                }
            }
        }
    }

    private static ValueConverterInfo GetDefaultValueConverterInfo(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
        Type converterTypeInfo)
        => (ValueConverterInfo)converterTypeInfo.GetAnyProperty("DefaultInfo")!.GetValue(null)!;
}
