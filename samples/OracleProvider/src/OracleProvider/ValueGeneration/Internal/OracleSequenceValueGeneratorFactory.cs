// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Oracle.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Oracle.ValueGeneration.Internal
{
    public class OracleSequenceValueGeneratorFactory : IOracleSequenceValueGeneratorFactory
    {
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
        private readonly IUpdateSqlGenerator _sqlGenerator;

        public OracleSequenceValueGeneratorFactory(
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder,
            [NotNull] IUpdateSqlGenerator sqlGenerator)
        {
            Check.NotNull(rawSqlCommandBuilder, nameof(rawSqlCommandBuilder));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            _rawSqlCommandBuilder = rawSqlCommandBuilder;
            _sqlGenerator = sqlGenerator;
        }

        public virtual ValueGenerator Create(IProperty property, OracleSequenceValueGeneratorState generatorState, IOracleConnection connection)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(generatorState, nameof(generatorState));
            Check.NotNull(connection, nameof(connection));

            var type = property.ClrType.UnwrapNullableType().UnwrapEnumType();

            if (type == typeof(long))
            {
                return new OracleSequenceHiLoValueGenerator<long>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(int))
            {
                return new OracleSequenceHiLoValueGenerator<int>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(short))
            {
                return new OracleSequenceHiLoValueGenerator<short>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(byte))
            {
                return new OracleSequenceHiLoValueGenerator<byte>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(char))
            {
                return new OracleSequenceHiLoValueGenerator<char>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(ulong))
            {
                return new OracleSequenceHiLoValueGenerator<ulong>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(uint))
            {
                return new OracleSequenceHiLoValueGenerator<uint>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(ushort))
            {
                return new OracleSequenceHiLoValueGenerator<ushort>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(sbyte))
            {
                return new OracleSequenceHiLoValueGenerator<sbyte>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            throw new ArgumentException(
                CoreStrings.InvalidValueGeneratorFactoryProperty(
                    nameof(OracleSequenceValueGeneratorFactory), property.Name, property.DeclaringEntityType.DisplayName()));
        }
    }
}
