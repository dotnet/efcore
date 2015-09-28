// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Update.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration.Internal
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
                return new SqlServerSequenceHiLoValueGenerator<long>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(int))
            {
                return new SqlServerSequenceHiLoValueGenerator<int>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(short))
            {
                return new SqlServerSequenceHiLoValueGenerator<short>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(byte))
            {
                return new SqlServerSequenceHiLoValueGenerator<byte>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(char))
            {
                return new SqlServerSequenceHiLoValueGenerator<char>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(ulong))
            {
                return new SqlServerSequenceHiLoValueGenerator<ulong>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(uint))
            {
                return new SqlServerSequenceHiLoValueGenerator<uint>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(ushort))
            {
                return new SqlServerSequenceHiLoValueGenerator<ushort>(_executor, _sqlGenerator, generatorState, connection);
            }

            if (property.ClrType.UnwrapNullableType() == typeof(sbyte))
            {
                return new SqlServerSequenceHiLoValueGenerator<sbyte>(_executor, _sqlGenerator, generatorState, connection);
            }

            throw new ArgumentException(CoreStrings.InvalidValueGeneratorFactoryProperty(
                nameof(SqlServerSequenceValueGeneratorFactory), property.Name, property.DeclaringEntityType.DisplayName()));
        }
    }
}
