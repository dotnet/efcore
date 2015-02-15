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

            return new SqlServerSequenceValueGenerator(_executor, GetSequenceName(property), GetBlockSize(property));
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
