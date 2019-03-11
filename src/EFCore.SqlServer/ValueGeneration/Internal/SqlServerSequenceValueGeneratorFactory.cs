// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerSequenceValueGeneratorFactory : ISqlServerSequenceValueGeneratorFactory
    {
        private readonly ISqlServerUpdateSqlGenerator _sqlGenerator;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerSequenceValueGeneratorFactory(
            [NotNull] ISqlServerUpdateSqlGenerator sqlGenerator)
        {
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            _sqlGenerator = sqlGenerator;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ValueGenerator Create(
            IProperty property,
            SqlServerSequenceValueGeneratorState generatorState,
            ISqlServerConnection connection,
            IRawSqlCommandBuilder rawSqlCommandBuilder)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(generatorState, nameof(generatorState));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(rawSqlCommandBuilder, nameof(rawSqlCommandBuilder));

            var type = property.ClrType.UnwrapNullableType().UnwrapEnumType();

            if (type == typeof(long))
            {
                return new SqlServerSequenceHiLoValueGenerator<long>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(int))
            {
                return new SqlServerSequenceHiLoValueGenerator<int>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(decimal))
            {
                return new SqlServerSequenceHiLoValueGenerator<decimal>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(short))
            {
                return new SqlServerSequenceHiLoValueGenerator<short>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(byte))
            {
                return new SqlServerSequenceHiLoValueGenerator<byte>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(char))
            {
                return new SqlServerSequenceHiLoValueGenerator<char>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(ulong))
            {
                return new SqlServerSequenceHiLoValueGenerator<ulong>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(uint))
            {
                return new SqlServerSequenceHiLoValueGenerator<uint>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(ushort))
            {
                return new SqlServerSequenceHiLoValueGenerator<ushort>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(sbyte))
            {
                return new SqlServerSequenceHiLoValueGenerator<sbyte>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            throw new ArgumentException(
                CoreStrings.InvalidValueGeneratorFactoryProperty(
                    nameof(SqlServerSequenceValueGeneratorFactory), property.Name, property.DeclaringEntityType.DisplayName()));
        }
    }
}
