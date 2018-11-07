// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     A registry of <see cref="ValueConverter" /> instances that can be used to find
    ///     the preferred converter to use to convert to and from a given model type
    ///     to a type that the database provider supports.
    /// </summary>
    public class ValueConverterSelector : IValueConverterSelector
    {
        private readonly ConcurrentDictionary<(Type ModelClrType, Type ProviderClrType), ValueConverterInfo> _converters
            = new ConcurrentDictionary<(Type, Type), ValueConverterInfo>();

        private static readonly Type[] _signedPreferred =
            { typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(decimal) };

        private static readonly Type[] _unsignedPreferred =
            { typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(decimal) };

        private static readonly Type[] _floatingPreferred =
            { typeof(float), typeof(double), typeof(decimal) };

        private static readonly Type[] _charPreferred =
            { typeof(char), typeof(int), typeof(ushort), typeof(uint), typeof(long), typeof(ulong), typeof(decimal) };

        private static readonly Type[] _numerics =
        {
            typeof(int), typeof(long), typeof(short), typeof(byte),
            typeof(ulong), typeof(uint), typeof(ushort), typeof(sbyte),
            typeof(decimal), typeof(double), typeof(float)
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

            var underlyingModelType = modelClrType.UnwrapNullableType();
            var underlyingProviderType = providerClrType?.UnwrapNullableType();

            if (underlyingModelType.IsEnum)
            {
                foreach (var converterInfo in FindNumericConvertions(
                    underlyingModelType,
                    underlyingProviderType,
                    typeof(EnumToNumberConverter<,>),
                    EnumToStringOrBytes))
                {
                    yield return converterInfo;
                }
            }
            else if (underlyingModelType == typeof(bool))
            {
                foreach (var converterInfo in FindNumericConvertions(
                    typeof(bool),
                    underlyingProviderType,
                    typeof(BoolToZeroOneConverter<>),
                    null))
                {
                    yield return converterInfo;
                }

                if (underlyingProviderType == null
                    || underlyingProviderType == typeof(string))
                {
                    yield return BoolToStringConverter.DefaultInfo;
                }

                if (underlyingProviderType == null
                    || underlyingProviderType == typeof(byte[]))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(byte[])),
                        k => new ValueConverterInfo(
                            underlyingModelType,
                            typeof(byte[]),
                            info => new BoolToZeroOneConverter<byte>().ComposeWith(
                                NumberToBytesConverter<byte>.DefaultInfo.Create()),
                            new ConverterMappingHints(size: 1)));
                }
            }
            else if (underlyingModelType == typeof(char))
            {
                foreach (var valueConverterInfo in ForChar(typeof(char), underlyingProviderType))
                {
                    yield return valueConverterInfo;
                }
            }
            else if (underlyingModelType == typeof(Guid))
            {
                if (underlyingProviderType == null
                    || underlyingProviderType == typeof(byte[]))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(byte[])),
                        k => GuidToBytesConverter.DefaultInfo);
                }

                if (underlyingProviderType == null
                    || underlyingProviderType == typeof(string))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(string)),
                        k => GuidToStringConverter.DefaultInfo);
                }
            }
            else if (underlyingModelType == typeof(byte[]))
            {
                if (underlyingProviderType == null
                    || underlyingProviderType == typeof(string))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(string)),
                        k => BytesToStringConverter.DefaultInfo);
                }
            }
            else if (underlyingModelType == typeof(string))
            {
                if (underlyingProviderType == null
                    || underlyingProviderType == typeof(byte[]))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(byte[])),
                        k => StringToBytesConverter.DefaultInfo);
                }
                else if (underlyingProviderType.IsEnum)
                {
                    yield return _converters.GetOrAdd(
                        (typeof(string), underlyingProviderType),
                        k => (ValueConverterInfo)typeof(StringToEnumConverter<>)
                            .MakeGenericType(k.ProviderClrType)
                            .GetAnyProperty("DefaultInfo")
                            .GetValue(null));
                }
                else if (_numerics.Contains(underlyingProviderType))
                {
                    foreach (var converterInfo in FindNumericConvertions(
                        typeof(string),
                        underlyingProviderType,
                        typeof(StringToNumberConverter<>),
                        null))
                    {
                        yield return converterInfo;
                    }
                }
                else if (underlyingProviderType == typeof(DateTime))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(DateTime)),
                        k => StringToDateTimeConverter.DefaultInfo);
                }
                else if (underlyingProviderType == typeof(DateTimeOffset))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(DateTimeOffset)),
                        k => StringToDateTimeOffsetConverter.DefaultInfo);
                }
                else if (underlyingProviderType == typeof(TimeSpan))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(TimeSpan)),
                        k => StringToTimeSpanConverter.DefaultInfo);
                }
                else if (underlyingProviderType == typeof(Guid))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(Guid)),
                        k => StringToGuidConverter.DefaultInfo);
                }
                else if (underlyingProviderType == typeof(bool))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(bool)),
                        k => StringToBoolConverter.DefaultInfo);
                }
                else if (underlyingProviderType == typeof(char))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(char)),
                        k => StringToCharConverter.DefaultInfo);
                }
            }
            else if (underlyingModelType == typeof(DateTime)
                     || underlyingModelType == typeof(DateTimeOffset)
                     || underlyingModelType == typeof(TimeSpan))
            {
                if (underlyingProviderType == null
                    || underlyingProviderType == typeof(string))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(string)),
                        k => k.ModelClrType == typeof(DateTime)
                            ? DateTimeToStringConverter.DefaultInfo
                            : k.ModelClrType == typeof(DateTimeOffset)
                                ? DateTimeOffsetToStringConverter.DefaultInfo
                                : TimeSpanToStringConverter.DefaultInfo);
                }

                if (underlyingProviderType == null
                    || underlyingProviderType == typeof(long))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(long)),
                        k => k.ModelClrType == typeof(DateTime)
                            ? DateTimeToBinaryConverter.DefaultInfo
                            : k.ModelClrType == typeof(DateTimeOffset)
                                ? DateTimeOffsetToBinaryConverter.DefaultInfo
                                : TimeSpanToTicksConverter.DefaultInfo);
                }

                if (underlyingProviderType == null
                    || underlyingProviderType == typeof(byte[]))
                {
                    yield return underlyingModelType == typeof(DateTimeOffset)
                        ? _converters.GetOrAdd(
                            (underlyingModelType, typeof(byte[])),
                            k => DateTimeOffsetToBytesConverter.DefaultInfo)
                        : _converters.GetOrAdd(
                            (underlyingModelType, typeof(byte[])),
                            k => new ValueConverterInfo(
                                underlyingModelType,
                                typeof(byte[]),
                                i => (i.ModelClrType == typeof(DateTime)
                                        ? DateTimeToBinaryConverter.DefaultInfo.Create()
                                        : TimeSpanToTicksConverter.DefaultInfo.Create())
                                    .ComposeWith(
                                        NumberToBytesConverter<long>.DefaultInfo.Create()),
                                NumberToBytesConverter<long>.DefaultInfo.MappingHints));
                }
            }
            else if (_numerics.Contains(underlyingModelType)
                     && (underlyingProviderType == null
                         || underlyingProviderType == typeof(byte[])
                         || underlyingProviderType == typeof(string)
                         || _numerics.Contains(underlyingProviderType)))
            {
                foreach (var converterInfo in FindNumericConvertions(
                    underlyingModelType,
                    underlyingProviderType,
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

            foreach (var converterInfo in FindNumericConvertions(
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

        private IEnumerable<ValueConverterInfo> FindNumericConvertions(
            Type modelType,
            Type providerType,
            Type converterType,
            Func<Type, Type, IEnumerable<ValueConverterInfo>> afterPreferred)
        {
            var usedTypes = new List<Type>
            {
                modelType
            }; // List not hash because few members
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
