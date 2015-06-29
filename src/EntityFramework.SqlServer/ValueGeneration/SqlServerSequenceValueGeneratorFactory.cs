// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.SqlServer.ValueGeneration
{
    public class SqlServerSequenceValueGeneratorFactory : ISqlServerSequenceValueGeneratorFactory
    {
        private readonly ISqlStatementExecutor _executor;
        private readonly ISqlServerUpdateSqlGenerator _sqlGenerator;

        public SqlServerSequenceValueGeneratorFactory(
            [NotNull] ISqlStatementExecutor executor,
            [NotNull] ISqlServerUpdateSqlGenerator sqlGenerator)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            _executor = executor;
            _sqlGenerator = sqlGenerator;
        }

        public virtual ValueGenerator Create(IProperty property, SqlServerSequenceValueGeneratorState generatorState, ISqlServerConnection connection)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(generatorState, nameof(generatorState));
            Check.NotNull(connection, nameof(connection));

            if (property.ClrType.UnwrapNullableType() == typeof(long))
            {
                return new SqlServerSequenceValueGenerator<long>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(int))
            {
                return new SqlServerSequenceValueGenerator<int>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(short))
            {
                return new SqlServerSequenceValueGenerator<short>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(byte))
            {
                return new SqlServerSequenceValueGenerator<byte>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(char))
            {
                return new SqlServerSequenceValueGenerator<char>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(ulong))
            {
                return new SqlServerSequenceValueGenerator<ulong>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(uint))
            {
                return new SqlServerSequenceValueGenerator<uint>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(ushort))
            {
                return new SqlServerSequenceValueGenerator<ushort>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(sbyte))
            {
                return new SqlServerSequenceValueGenerator<sbyte>(_executor, _sqlGenerator, generatorState, connection);
            }

            throw new ArgumentException(Internal.Strings.InvalidValueGeneratorFactoryProperty(
                nameof(SqlServerSequenceValueGeneratorFactory), property.Name, property.EntityType.DisplayName()));
        }
    }
}
