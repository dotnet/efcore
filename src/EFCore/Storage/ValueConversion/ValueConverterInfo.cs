// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Contains information on an available <see cref="ValueConverter" /> including a factory to
    ///     create an instance.
    /// </summary>
    public readonly struct ValueConverterInfo
    {
        private readonly Func<ValueConverterInfo, ValueConverter> _factory;

        /// <summary>
        ///     Creates a new <see cref="ValueConverterInfo" /> instance.
        /// </summary>
        /// <param name="modelClrType"> The CLR type used in the EF model. </param>
        /// <param name="providerClrType"> The CLR type used when reading and writing from the database provider. </param>
        /// <param name="factory"> A factory to create the converter, if needed. </param>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public ValueConverterInfo(
            [NotNull] Type modelClrType,
            [NotNull] Type providerClrType,
            [NotNull] Func<ValueConverterInfo, ValueConverter> factory,
            [CanBeNull] ConverterMappingHints mappingHints = null)
        {
            _factory = factory;
            Check.NotNull(modelClrType, nameof(modelClrType));
            Check.NotNull(providerClrType, nameof(providerClrType));
            Check.NotNull(factory, nameof(factory));

            ModelClrType = modelClrType;
            ProviderClrType = providerClrType;
            MappingHints = mappingHints;
        }

        /// <summary>
        ///     The CLR type used in the EF model.
        /// </summary>
        public Type ModelClrType { get; }

        /// <summary>
        ///     The CLR type used when reading and writing from the database provider.
        /// </summary>
        public Type ProviderClrType { get; }

        /// <summary>
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </summary>
        public ConverterMappingHints MappingHints { get; }

        /// <summary>
        ///     Creates an instance of the <see cref="ValueConverter" />.
        /// </summary>
        public ValueConverter Create() => _factory(this);
    }
}
