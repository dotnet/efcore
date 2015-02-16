// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerSequenceValueGeneratorFactory : ValueGeneratorFactory
    {
        private readonly SqlStatementExecutor _executor;

        public SqlServerSequenceValueGeneratorFactory([NotNull] SqlStatementExecutor executor)
        {
            Check.NotNull(executor, nameof(executor));

            _executor = executor;
        }

        public virtual int GetBlockSize([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var incrementBy = property.SqlServer().TryGetSequence().IncrementBy;

            if (incrementBy <= 0)
            {
                throw new NotSupportedException(Strings.SequenceBadBlockSize(incrementBy, GetSequenceName(property)));
            }

            return incrementBy;
        }

        public virtual string GetSequenceName([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var sequence = property.SqlServer().TryGetSequence();

            return (sequence.Schema == null ? "" : (sequence.Schema + ".")) + sequence.Name;
        }

        public override ValueGenerator Create(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            if (property.PropertyType.UnwrapNullableType() == typeof(long))
            {
                return new SqlServerSequenceValueGenerator<long>(_executor, GetSequenceName(property), GetBlockSize(property));
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(int))
            {
                return new SqlServerSequenceValueGenerator<int>(_executor, GetSequenceName(property), GetBlockSize(property));
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(short))
            {
                return new SqlServerSequenceValueGenerator<short>(_executor, GetSequenceName(property), GetBlockSize(property));
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(byte))
            {
                return new SqlServerSequenceValueGenerator<byte>(_executor, GetSequenceName(property), GetBlockSize(property));
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(ulong))
            {
                return new SqlServerSequenceValueGenerator<ulong>(_executor, GetSequenceName(property), GetBlockSize(property));
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(uint))
            {
                return new SqlServerSequenceValueGenerator<uint>(_executor, GetSequenceName(property), GetBlockSize(property));
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(ushort))
            {
                return new SqlServerSequenceValueGenerator<ushort>(_executor, GetSequenceName(property), GetBlockSize(property));
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(sbyte))
            {
                return new SqlServerSequenceValueGenerator<sbyte>(_executor, GetSequenceName(property), GetBlockSize(property));
            }

            throw new ArgumentException(Internal.Strings.InvalidValueGeneratorFactoryProperty(
                nameof(SqlServerSequenceValueGeneratorFactory), property.Name, property.EntityType.SimpleName));
        }

        public override int GetPoolSize(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            // TODO: Allow configuration without creation of derived factory type
            // Issue #778
            return 5;
        }

        public override string GetCacheKey(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return GetSequenceName(property);
        }
    }
}
