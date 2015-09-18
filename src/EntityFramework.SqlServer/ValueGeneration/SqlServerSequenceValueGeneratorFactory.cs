// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public class SqlServerSequenceValueGeneratorFactory : ISqlServerSequenceValueGeneratorFactory
    {
        private readonly ISqlServerUpdateSqlGenerator _sqlGenerator;
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;

        public SqlServerSequenceValueGeneratorFactory(
            [NotNull] ISqlServerUpdateSqlGenerator sqlGenerator,
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory)
        {
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));

            _sqlGenerator = sqlGenerator;
            _commandBuilderFactory = commandBuilderFactory;
        }

        public virtual ValueGenerator Create(IProperty property, SqlServerSequenceValueGeneratorState generatorState, ISqlServerConnection connection)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(generatorState, nameof(generatorState));
            Check.NotNull(connection, nameof(connection));

            if (property.ClrType.UnwrapNullableType() == typeof(long))
            {
                return new SqlServerSequenceValueGenerator<long>(_commandBuilderFactory, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(int))
            {
                return new SqlServerSequenceValueGenerator<int>(_commandBuilderFactory, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(short))
            {
                return new SqlServerSequenceValueGenerator<short>(_commandBuilderFactory, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(byte))
            {
                return new SqlServerSequenceValueGenerator<byte>(_commandBuilderFactory, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(char))
            {
                return new SqlServerSequenceValueGenerator<char>(_commandBuilderFactory, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(ulong))
            {
                return new SqlServerSequenceValueGenerator<ulong>(_commandBuilderFactory, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(uint))
            {
                return new SqlServerSequenceValueGenerator<uint>(_commandBuilderFactory, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(ushort))
            {
                return new SqlServerSequenceValueGenerator<ushort>(_commandBuilderFactory, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(sbyte))
            {
                return new SqlServerSequenceValueGenerator<sbyte>(_commandBuilderFactory, _sqlGenerator, generatorState, connection);
            }

            throw new ArgumentException(Entity.Internal.Strings.InvalidValueGeneratorFactoryProperty(
                nameof(SqlServerSequenceValueGeneratorFactory), property.Name, property.DeclaringEntityType.DisplayName()));
        }
    }
}
