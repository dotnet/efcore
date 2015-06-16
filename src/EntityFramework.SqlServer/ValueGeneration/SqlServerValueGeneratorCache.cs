// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.SqlServer.ValueGeneration
{
    public class SqlServerValueGeneratorCache : ValueGeneratorCache, ISqlServerValueGeneratorCache
    {
        private readonly ThreadSafeDictionaryCache<string, SqlServerSequenceValueGeneratorState> _sequenceGeneratorCache
            = new ThreadSafeDictionaryCache<string, SqlServerSequenceValueGeneratorState>();

        public virtual SqlServerSequenceValueGeneratorState GetOrAddSequenceState(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return _sequenceGeneratorCache.GetOrAdd(
                GetSequenceName(property),
                sequenceName => new SqlServerSequenceValueGeneratorState(sequenceName, GetBlockSize(property), GetPoolSize(property)));
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

        // TODO: Allow configuration without creation of derived factory type
        // Issue #778
        public virtual int GetPoolSize([NotNull] IProperty property) => 5;
    }
}
