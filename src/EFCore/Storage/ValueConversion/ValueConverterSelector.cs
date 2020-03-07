// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     <para>
    ///         A registry of <see cref="ValueConverter" /> instances that can be used to find
    ///         the preferred converter to use to convert to and from a given model type
    ///         to a type that the database provider supports.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class ValueConverterSelector : IValueConverterSelector
    {
        private readonly ConcurrentDictionary<(Type ModelClrType, Type ProviderClrType), ValueConverterInfo> _converters
            = new ConcurrentDictionary<(Type, Type), ValueConverterInfo>();

        private static readonly Type[] _signedPreferred = { typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(decimal) };

        private static readonly Type[] _unsignedPreferred =
        {
            typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(decimal)
        };

        private static readonly Type[] _floatingPreferred = { typeof(float), typeof(double), typeof(decimal) };

        private static readonly Type[] _charPreferred =
        {
            typeof(char), typeof(int), typeof(ushort), typeof(uint), typeof(long), typeof(ulong), typeof(decimal)
        };

        private static readonly Type[] _numerics =
        {
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
        };

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueConverterSelector" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public ValueConverterSelector([NotNull] ValueConverterSelectorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="ValueConverterSelector" />
        /// </summary>
        protected virtual ValueConverterSelectorDependencies Dependencies { get; }

        /// <summary>
        ///     Returns the list of <see cref="ValueConverter" /> instances that can be
        ///     used to convert the given model type. Converters nearer the front of
        ///     the list should be used in preference to converters nearer the end.
        /// </summary>
        /// <param name="modelClrType"> The type for which a converter is needed. </param>
        /// <param name="providerClrType"> The database provider type to target, or null for any. </param>
        /// <returns> The converters available. </returns>
        public virtual IEnumerable<ValueConverterInfo> Select(
            Type modelClrType,
            Type providerClrType = null)
        {
            Check.NotNull(modelClrType, nameof(modelClrType));

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
                        k => new ValueConverterInfo(
                            modelClrType,
                            typeof(byte[]),
                            info => new BoolToZeroOneConverter<byte>().ComposeWith(
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
                    || providerClrType == typeof(byte[]))
                {
                    yield return _converters.GetOrAdd(
                        (modelClrType, typeof(byte[])),
                        k => GuidToBytesConverter.DefaultInfo);
                }

                if (providerClrType == null
                    || providerClrType == typeof(string))
                {
                    yield return _converters.GetOrAdd(
                        (modelClrType, typeof(string)),
                        k => GuidToStringConverter.DefaultInfo);
                }
            }
            else if (modelClrType == typeof(byte[]))
            {
                if (providerClrType == null
                    || providerClrType == typeof(string))
                {
                    yield return _converters.GetOrAdd(
                        (modelClrType, typeof(string)),
                        k => BytesToStringConverter.DefaultInfo);
                }
            }
            else if (modelClrType == typeof(Uri))
            {
                if (providerClrType == null
                    || providerClrType == typeof(string))
                {
                    yield return _converters.GetOrAdd(
                        (modelClrType, typeof(string)),
                        k => UriToStringConverter.DefaultInfo);
                }
            }
            else if (modelClrType == typeof(string))
            {
                if (providerClrType == null
                    || providerClrType == typeof(byte[]))
                {
                    yield return _converters.GetOrAdd(
                        (modelClrType, typeof(byte[])),
                        k => StringToBytesConverter.DefaultInfo);
                }
                else if (providerClrType.IsEnum)
                {
                    yield return _converters.GetOrAdd(
                        (typeof(string), providerClrType),
                        k => (ValueConverterInfo)typeof(StringToEnumConverter<>)
                            .MakeGenericType(k.ProviderClrType)
                            .GetAnyProperty("DefaultInfo")
                            .GetValue(null));
                }
                else if (_numerics.Contains(providerClrType))
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
                        k => StringToDateTimeConverter.DefaultInfo);
                }
                else if (providerClrType == typeof(DateTimeOffset))
                {
                    yield return _converters.GetOrAdd(
                        (modelClrType, typeof(DateTimeOffset)),
                        k => StringToDateTimeOffsetConverter.DefaultInfo);
                }
                else if (providerClrType == typeof(TimeSpan))
                {
                    yield return _converters.GetOrAdd(
                        (modelClrType, typeof(TimeSpan)),
                        k => StringToTimeSpanConverter.DefaultInfo);
                }
                else if (providerClrType == typeof(Guid))
                {
                    yield return _converters.GetOrAdd(
                        (modelClrType, typeof(Guid)),
                        k => StringToGuidConverter.DefaultInfo);
                }
                else if (providerClrType == typeof(bool))
                {
                    yield return _converters.GetOrAdd(
                        (modelClrType, typeof(bool)),
                        k => StringToBoolConverter.DefaultInfo);
                }
                else if (providerClrType == typeof(char))
                {
                    yield return _converters.GetOrAdd(
                        (modelClrType, typeof(char)),
                        k => StringToCharConverter.DefaultInfo);
                }
                else if (providerClrType == typeof(Uri))
                {
                    yield return _converters.GetOrAdd(
                        (modelClrType, typeof(Uri)),
                        k => StringToUriConverter.DefaultInfo);
                }
            }
            else if (modelClrType == typeof(DateTime)
                || modelClrType == typeof(DateTimeOffset)
                || modelClrType == typeof(TimeSpan))
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
                                : TimeSpanToStringConverter.DefaultInfo);
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
                                : TimeSpanToTicksConverter.DefaultInfo);
                }

                if (providerClrType == null
                    || providerClrType == typeof(byte[]))
                {
                    yield return modelClrType == typeof(DateTimeOffset)
                        ? _converters.GetOrAdd(
                            (modelClrType, typeof(byte[])),
                            k => DateTimeOffsetToBytesConverter.DefaultInfo)
                        : _converters.GetOrAdd(
                            (modelClrType, typeof(byte[])),
                            k => new ValueConverterInfo(
                                modelClrType,
                                typeof(byte[]),
                                i => (i.ModelClrType == typeof(DateTime)
                                        ? DateTimeToBinaryConverter.DefaultInfo.Create()
                                        : TimeSpanToTicksConverter.DefaultInfo.Create())
                                    .ComposeWith(
                                        NumberToBytesConverter<long>.DefaultInfo.Create()),
                                NumberToBytesConverter<long>.DefaultInfo.MappingHints));
                }
            }
            else if (_numerics.Contains(modelClrType)
                && (providerClrType == null
                    || providerClrType == typeof(byte[])
                    || providerClrType == typeof(string)
                    || _numerics.Contains(providerClrType)))
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
            Type underlyingModelType, Type underlyingProviderType)
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
            Type underlyingModelType, Type underlyingProviderType)
        {
            if (underlyingProviderType == null
                || underlyingProviderType == typeof(byte[]))
            {
                yield return _converters.GetOrAdd(
                    (underlyingModelType, typeof(byte[])),
                    k => NumberToBytesConverter<char>.DefaultInfo);
            }
        }

        private IEnumerable<ValueConverterInfo> EnumToStringOrBytes(
            Type underlyingModelType, Type underlyingProviderType)
        {
            if (underlyingProviderType == null
                || underlyingProviderType == typeof(string))
            {
                yield return _converters.GetOrAdd(
                    (underlyingModelType, typeof(string)),
                    k => (ValueConverterInfo)typeof(EnumToStringConverter<>)
                        .MakeGenericType(k.ModelClrType)
                        .GetAnyProperty("DefaultInfo")
                        .GetValue(null));
            }

            if (underlyingProviderType == null
                || underlyingProviderType == typeof(byte[]))
            {
                yield return _converters.GetOrAdd(
                    (underlyingModelType, typeof(byte[])),
                    k =>
                    {
                        var toNumber = (ValueConverterInfo)typeof(EnumToNumberConverter<,>)
                            .MakeGenericType(k.ModelClrType, k.ModelClrType.GetEnumUnderlyingType())
                            .GetAnyProperty("DefaultInfo")
                            .GetValue(null);

                        var toBytes = (ValueConverterInfo)typeof(NumberToBytesConverter<>)
                            .MakeGenericType(k.ModelClrType.GetEnumUnderlyingType())
                            .GetAnyProperty("DefaultInfo")
                            .GetValue(null);

                        return new ValueConverterInfo(
                            underlyingModelType,
                            typeof(byte[]),
                            i => toNumber.Create().ComposeWith(toBytes.Create()),
                            toBytes.MappingHints);
                    });
            }
        }

        private IEnumerable<ValueConverterInfo> NumberToStringOrBytes(
            Type underlyingModelType, Type underlyingProviderType)
        {
            if (underlyingProviderType == null
                || underlyingProviderType == typeof(string))
            {
                yield return _converters.GetOrAdd(
                    (underlyingModelType, typeof(string)),
                    k => (ValueConverterInfo)typeof(NumberToStringConverter<>)
                        .MakeGenericType(k.ModelClrType)
                        .GetAnyProperty("DefaultInfo")
                        .GetValue(null));
            }

            if (underlyingProviderType == null
                || underlyingProviderType == typeof(byte[]))
            {
                yield return _converters.GetOrAdd(
                    (underlyingModelType, typeof(byte[])),
                    k => (ValueConverterInfo)typeof(NumberToBytesConverter<>)
                        .MakeGenericType(k.ModelClrType)
                        .GetAnyProperty("DefaultInfo")
                        .GetValue(null));
            }
        }

        private IEnumerable<ValueConverterInfo> FindNumericConventions(
            Type modelType,
            Type providerType,
            Type converterType,
            Func<Type, Type, IEnumerable<ValueConverterInfo>> afterPreferred)
        {
            var usedTypes = new List<Type> { modelType }; // List not hash because few members
            var underlyingModelType = modelType.UnwrapEnumType();

            if (modelType.IsEnum)
            {
                foreach (var converterInfo in FindPreferredConversions(
                    new[] { underlyingModelType }, modelType, providerType, converterType))
                {
                    yield return converterInfo;

                    usedTypes.Add(converterInfo.ProviderClrType);
                }
            }

            foreach (var converterInfo in FindPreferredConversions(
                _signedPreferred, modelType, providerType, converterType))
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
                    _unsignedPreferred, modelType, providerType, converterType))
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
                    _floatingPreferred, modelType, providerType, converterType))
                {
                    yield return converterInfo;

                    usedTypes.Add(converterInfo.ProviderClrType);
                }
            }

            if (underlyingModelType == typeof(char))
            {
                foreach (var converterInfo in FindPreferredConversions(
                    _charPreferred, modelType, providerType, converterType))
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

            foreach (var numeric in _numerics)
            {
                if ((providerType == null
                        || providerType == numeric)
                    && !usedTypes.Contains(numeric))
                {
                    yield return _converters.GetOrAdd(
                        (modelType, numeric),
                        k => (ValueConverterInfo)(converterType.GetTypeInfo().GenericTypeParameters.Length == 1
                                ? converterType.MakeGenericType(k.ProviderClrType)
                                : converterType.MakeGenericType(k.ModelClrType, k.ProviderClrType))
                            .GetAnyProperty("DefaultInfo")
                            .GetValue(null));
                }
            }
        }

        private IEnumerable<ValueConverterInfo> FindPreferredConversions(
            Type[] candidateTypes,
            Type modelType,
            Type providerType,
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
                                k => (ValueConverterInfo)converterType.MakeGenericType(k.ModelClrType, k.ProviderClrType)
                                    .GetAnyProperty("DefaultInfo")
                                    .GetValue(null));
                        }
                    }
                }
            }
        }
    }
}
