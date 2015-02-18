// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerSequenceValueGeneratorFactory
    {
        private readonly SqlStatementExecutor _executor;

        public SqlServerSequenceValueGeneratorFactory([NotNull] SqlStatementExecutor executor)
        {
            Check.NotNull(executor, nameof(executor));

            _executor = executor;
        }

        public virtual ValueGenerator Create(
            [NotNull] IProperty property, 
            [NotNull] SqlServerSequenceValueGeneratorState generatorState, 
            [NotNull] SqlServerConnection connection)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(generatorState, nameof(generatorState));
            Check.NotNull(connection, nameof(connection));

            if (property.PropertyType.UnwrapNullableType() == typeof(long))
            {
                return new SqlServerSequenceValueGenerator<long>(_executor, generatorState, connection);
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(int))
            {
                return new SqlServerSequenceValueGenerator<int>(_executor, generatorState, connection);
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(short))
            {
                return new SqlServerSequenceValueGenerator<short>(_executor, generatorState, connection);
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(byte))
            {
                return new SqlServerSequenceValueGenerator<byte>(_executor, generatorState, connection);
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(ulong))
            {
                return new SqlServerSequenceValueGenerator<ulong>(_executor, generatorState, connection);
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(uint))
            {
                return new SqlServerSequenceValueGenerator<uint>(_executor, generatorState, connection);
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(ushort))
            {
                return new SqlServerSequenceValueGenerator<ushort>(_executor, generatorState, connection);
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(sbyte))
            {
                return new SqlServerSequenceValueGenerator<sbyte>(_executor, generatorState, connection);
            }

            throw new ArgumentException(Internal.Strings.InvalidValueGeneratorFactoryProperty(
                nameof(SqlServerSequenceValueGeneratorFactory), property.Name, property.EntityType.SimpleName));
        }
    }
}
