// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Converters
{
    /// <summary>
    ///     Contains information on an available <see cref="ValueConverter" /> including a factory to
    ///     create an instance.
    /// </summary>
    public struct ValueConverterInfo
    {
        private readonly Func<ValueConverterInfo, ValueConverter> _factory;

        /// <summary>
        ///     Creates a new <see cref="ValueConverterInfo" /> instance.
        /// </summary>
        /// <param name="modelClrType"> The CLR type used in the EF model. </param>
        /// <param name="storeClrType"> The CLR type used when reading and writing from the store. </param>
        /// <param name="factory"> A factory to create the converter, if needed. </param>
        /// <param name="mappingHints">
        ///     Hints that can be used by the type mapper to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public ValueConverterInfo(
            [NotNull] Type modelClrType,
            [NotNull] Type storeClrType,
            [NotNull] Func<ValueConverterInfo, ValueConverter> factory,
            ConverterMappingHints mappingHints = default)
        {
            _factory = factory;
            Check.NotNull(modelClrType, nameof(modelClrType));
            Check.NotNull(storeClrType, nameof(storeClrType));
            Check.NotNull(factory, nameof(factory));

            ModelClrType = modelClrType;
            StoreClrType = storeClrType;
            MappingHints = mappingHints;
        }

        /// <summary>
        ///     The CLR type used in the EF model.
        /// </summary>
        public Type ModelClrType { get; }

        /// <summary>
        ///     The CLR type used when reading and writing from the store.
        /// </summary>
        public Type StoreClrType { get; }

        /// <summary>
        ///     Hints that can be used by the type mapper to create data types with appropriate
        ///     facets for the converted data.
        /// </summary>
        public ConverterMappingHints MappingHints { get; }

        /// <summary>
        ///     Creates an instance of the <see cref="ValueConverter" />.
        /// </summary>
        public ValueConverter Create() => _factory(this);
    }
}
