// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public class SqlServerValueGeneratorCache : ValueGeneratorCache, ISqlServerValueGeneratorCache
    {
        private readonly ThreadSafeDictionaryCache<string, SqlServerSequenceValueGeneratorState> _sequenceGeneratorCache
            = new ThreadSafeDictionaryCache<string, SqlServerSequenceValueGeneratorState>();

        public virtual SqlServerSequenceValueGeneratorState GetOrAddSequenceState(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var sequence = property.SqlServer().FindHiLoSequence();

            Debug.Assert(sequence != null);

            return _sequenceGeneratorCache.GetOrAdd(
                GetSequenceName(sequence),
                sequenceName => new SqlServerSequenceValueGeneratorState(sequence, GetPoolSize(property)));
        }

        private static string GetSequenceName(ISequence sequence) 
            => (sequence.Schema == null ? "" : (sequence.Schema + ".")) + sequence.Name;

        public virtual int GetPoolSize([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.SqlServer().HiLoSequencePoolSize
                   ?? property.DeclaringEntityType.Model.SqlServer().HiLoSequencePoolSize
                   ?? 1;
        }
    }
}
