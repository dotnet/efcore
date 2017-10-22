// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Converters
{
    /// <summary>
    ///     A registry of <see cref="ValueConverter" /> instances that can be used to find
    ///     the preferred converter to use to convert to and from a given model type
    ///     to a store type that the database provider supports.
    /// </summary>
    public class ValueConverterSelector : IValueConverterSelector
    {
        private readonly ConcurrentDictionary<(Type ModelClrType, Type StoreClrType), ValueConverterInfo> _converters
            = new ConcurrentDictionary<(Type, Type), ValueConverterInfo>();

        private static readonly Type[] _signedPreferred =
        {
            typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(decimal)
        };

        private static readonly Type[] _unsignedPreferred =
        {
            typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(decimal)
        };

        private static readonly Type[] _floatingPreferred =
        {
            typeof(float), typeof(double), typeof(decimal)
        };

        private static readonly Type[] _charPreferred =
        {
            typeof(char), typeof(int), typeof(ushort), typeof(uint), typeof(long), typeof(ulong), typeof(decimal)
        };

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
        /// <param name="modelType"> The type for which a converter is needed. </param>
        /// <param name="storeClrType"> The store type to target, or null for any. </param>
        /// <returns> The converters available. </returns>
        public virtual IEnumerable<ValueConverterInfo> ForTypes(
            Type modelType,
            Type storeClrType = null)
        {
            Check.NotNull(modelType, nameof(modelType));

            var underlyingModelType = modelType.UnwrapNullableType();
            var underlyingStoreType = storeClrType?.UnwrapNullableType();

            if (underlyingModelType.IsEnum)
            {
                foreach (var converterInfo in FindNumericConvertions(
                    underlyingModelType,
                    underlyingStoreType,
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
                    underlyingStoreType,
                    typeof(BoolToZeroOneConverter<>),
                    null))
                {
                    yield return converterInfo;
                }

                if (underlyingStoreType == null
                    || underlyingStoreType == typeof(string))
                {
                    yield return BoolToStringConverter.DefaultInfo;
                }

                if (underlyingStoreType == null
                    || underlyingStoreType == typeof(byte[]))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(byte[])),
                        k => new ValueConverterInfo(
                            underlyingModelType,
                            typeof(byte[]),
                            info => ValueConverter.Compose(
                                new BoolToZeroOneConverter<byte>(),
                                NumberToBytesConverter<byte>.DefaultInfo.Create()),
                            new ConverterMappingHints(size: 1)));
                }
            }
            else if (underlyingModelType == typeof(char))
            {
                foreach (var valueConverterInfo in ForChar(typeof(char), underlyingStoreType))
                {
                    yield return valueConverterInfo;
                }
            }
            else if (underlyingModelType == typeof(Guid))
            {
                if (underlyingStoreType == null
                    || underlyingStoreType == typeof(byte[]))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(byte[])),
                        k => GuidToBytesConverter.DefaultInfo);
                }

                if (underlyingStoreType == null
                    || underlyingStoreType == typeof(string))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(string)),
                        k => GuidToStringConverter.DefaultInfo);
                }
            }
            else if (underlyingModelType == typeof(byte[]))
            {
                if (underlyingStoreType == null
                    || underlyingStoreType == typeof(string))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(string)),
                        k => BytesToStringConverter.DefaultInfo);
                }
            }
            else if (underlyingModelType == typeof(string))
            {
                if (underlyingStoreType == null
                    || underlyingStoreType == typeof(byte[]))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(byte[])),
                        k => StringToBytesConverter.DefaultInfo);
                }
            }
            else if (underlyingModelType == typeof(DateTime)
                || underlyingModelType == typeof(DateTimeOffset)
                || underlyingModelType == typeof(TimeSpan))
            {
                if (underlyingStoreType == null
                    || underlyingStoreType == typeof(string))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(string)),
                        k => k.ModelClrType == typeof(DateTime)
                            ? DateTimeToStringConverter.DefaultInfo
                            : k.ModelClrType == typeof(DateTimeOffset)
                                ? DateTimeOffsetToStringConverter.DefaultInfo
                                : TimeSpanToStringConverter.DefaultInfo);
                }

                if (underlyingStoreType == null
                    || underlyingStoreType == typeof(long))
                {
                    yield return _converters.GetOrAdd(
                        (underlyingModelType, typeof(long)),
                        k => k.ModelClrType == typeof(DateTime)
                            ? DateTimeToBinaryConverter.DefaultInfo
                            : k.ModelClrType == typeof(DateTimeOffset)
                                ? DateTimeOffsetToBinaryConverter.DefaultInfo
                                : TimeSpanToTicksConverter.DefaultInfo);
                }

                if (underlyingStoreType == null
                    || underlyingStoreType == typeof(byte[]))
                {
                    if (underlyingModelType == typeof(DateTimeOffset))
                    {
                        yield return _converters.GetOrAdd(
                            (underlyingModelType, typeof(byte[])),
                            k => DateTimeOffsetToBytesConverter.DefaultInfo);
                    }
                    else
                    {
                        yield return _converters.GetOrAdd(
                            (underlyingModelType, typeof(byte[])),
                            k => new ValueConverterInfo(
                                underlyingModelType,
                                typeof(byte[]),
                                i =>
                                    ValueConverter.Compose(
                                        i.ModelClrType == typeof(DateTime)
                                            ? DateTimeToBinaryConverter.DefaultInfo.Create()
                                            : TimeSpanToTicksConverter.DefaultInfo.Create(),
                                        NumberToBytesConverter<long>.DefaultInfo.Create()),
                                NumberToBytesConverter<long>.DefaultInfo.MappingHints));
                    }
                }
            }
            else if (_numerics.Contains(underlyingModelType)
                     && (underlyingStoreType == null
                         || _numerics.Contains(underlyingStoreType)))
            {
                foreach (var converterInfo in FindNumericConvertions(
                    underlyingModelType,
                    underlyingStoreType,
                    typeof(CastingConverter<,>),
                    NumberToStringOrBytes))
                {
                    yield return converterInfo;
                }
            }
        }

        private IEnumerable<ValueConverterInfo> ForChar(
            Type underlyingModelType, Type underlyingStoreType)
        {
            if (underlyingStoreType == null
                || underlyingStoreType == typeof(string))
            {
                yield return _converters.GetOrAdd(
                    (underlyingModelType, typeof(string)),
                    k => CharToStringConverter.DefaultInfo);
            }

            foreach (var converterInfo in FindNumericConvertions(
                underlyingModelType,
                underlyingStoreType,
                typeof(CastingConverter<,>),
                CharToBytes))
            {
                yield return converterInfo;
            }
        }

        private IEnumerable<ValueConverterInfo> CharToBytes(
            Type underlyingModelType, Type underlyingStoreType)
        {
            if (underlyingStoreType == null
                || underlyingStoreType == typeof(byte[]))
            {
                yield return _converters.GetOrAdd(
                    (underlyingModelType, typeof(byte[])),
                    k => NumberToBytesConverter<char>.DefaultInfo);
            }
        }

        private IEnumerable<ValueConverterInfo> EnumToStringOrBytes(
            Type underlyingModelType, Type underlyingStoreType)
        {
            if (underlyingStoreType == null
                || underlyingStoreType == typeof(string))
            {
                yield return _converters.GetOrAdd(
                    (underlyingModelType, typeof(string)),
                    k => (ValueConverterInfo)typeof(EnumToStringConverter<>)
                        .MakeGenericType(k.ModelClrType)
                        .GetAnyProperty("DefaultInfo")
                        .GetValue(null));
            }

            if (underlyingStoreType == null
                || underlyingStoreType == typeof(byte[]))
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
                            i => ValueConverter.Compose(toNumber.Create(), toBytes.Create()),
                            toBytes.MappingHints);
                    });

            }
        }

        private IEnumerable<ValueConverterInfo> NumberToStringOrBytes(
            Type underlyingModelType, Type underlyingStoreType)
        {
            if (underlyingStoreType == null
                || underlyingStoreType == typeof(string))
            {
                yield return _converters.GetOrAdd(
                    (underlyingModelType, typeof(string)),
                    k => (ValueConverterInfo)typeof(NumberToStringConverter<>)
                        .MakeGenericType(k.ModelClrType)
                        .GetAnyProperty("DefaultInfo")
                        .GetValue(null));
            }

            if (underlyingStoreType == null
                || underlyingStoreType == typeof(byte[]))
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
            Type storeType,
            Type converterType,
            Func<Type, Type, IEnumerable<ValueConverterInfo>> afterPreferred)
        {
            var usedTypes = new List<Type> { modelType }; // List not hash because few members
            var underlyingModelType = modelType.UnwrapEnumType();

            if (modelType.IsEnum)
            {
                foreach (var converterInfo in FindPreferredConversions(
                    new[] { underlyingModelType }, modelType, storeType, converterType))
                {
                    yield return converterInfo;

                    usedTypes.Add(converterInfo.StoreClrType);
                }
            }

            foreach (var converterInfo in FindPreferredConversions(
                _signedPreferred, modelType, storeType, converterType))
            {
                if (!usedTypes.Contains(converterInfo.StoreClrType))
                {
                    yield return converterInfo;

                    usedTypes.Add(converterInfo.StoreClrType);
                }
            }

            if (underlyingModelType == typeof(byte)
                || underlyingModelType == typeof(uint)
                || underlyingModelType == typeof(ulong)
                || underlyingModelType == typeof(ushort))
            {
                foreach (var converterInfo in FindPreferredConversions(
                    _unsignedPreferred, modelType, storeType, converterType))
                {
                    if (!usedTypes.Contains(converterInfo.StoreClrType))
                    {
                        yield return converterInfo;

                        usedTypes.Add(converterInfo.StoreClrType);
                    }
                }
            }

            if (underlyingModelType == typeof(float)
                || underlyingModelType == typeof(double))
            {
                foreach (var converterInfo in FindPreferredConversions(
                    _floatingPreferred, modelType, storeType, converterType))
                {
                    yield return converterInfo;

                    usedTypes.Add(converterInfo.StoreClrType);
                }
            }

            if (underlyingModelType == typeof(char))
            {
                foreach (var converterInfo in FindPreferredConversions(
                    _charPreferred, modelType, storeType, converterType))
                {
                    yield return converterInfo;

                    usedTypes.Add(converterInfo.StoreClrType);
                }
            }

            if (afterPreferred != null)
            {
                foreach (var converterInfo in afterPreferred(modelType, storeType))
                {
                    yield return converterInfo;

                    usedTypes.Add(converterInfo.StoreClrType);
                }
            }

            foreach (var numeric in _numerics)
            {
                if ((storeType == null
                     || storeType == numeric)
                    && !usedTypes.Contains(numeric))
                {
                    yield return _converters.GetOrAdd(
                        (modelType, numeric),
                        k => (ValueConverterInfo)(converterType.GetTypeInfo().GenericTypeParameters.Length == 1
                                ? converterType.MakeGenericType(k.StoreClrType)
                                : converterType.MakeGenericType(k.ModelClrType, k.StoreClrType))
                            .GetAnyProperty("DefaultInfo")
                            .GetValue(null));
                }
            }
        }

        private IEnumerable<ValueConverterInfo> FindPreferredConversions(
            Type[] candidateTypes,
            Type modelType,
            Type storeType,
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
                        if (storeType == null
                            || storeType == candidateTypes[i])
                        {
                            yield return _converters.GetOrAdd(
                                (modelType, candidateTypes[i]),
                                k => (ValueConverterInfo)converterType.MakeGenericType(k.ModelClrType, k.StoreClrType)
                                    .GetAnyProperty("DefaultInfo")
                                    .GetValue(null));
                        }
                    }
                }
            }
        }
    }
}
