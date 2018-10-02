// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Oracle.ValueGeneration.Internal
{
    public class OracleValueGeneratorCache : ValueGeneratorCache, IOracleValueGeneratorCache
    {
        private readonly ConcurrentDictionary<string, OracleSequenceValueGeneratorState> _sequenceGeneratorCache
            = new ConcurrentDictionary<string, OracleSequenceValueGeneratorState>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueGeneratorCache" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public OracleValueGeneratorCache([NotNull] ValueGeneratorCacheDependencies dependencies)
            : base(dependencies)
        {
        }

        public virtual OracleSequenceValueGeneratorState GetOrAddSequenceState(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var sequence = property.Oracle().FindHiLoSequence();

            Debug.Assert(sequence != null);

            return _sequenceGeneratorCache.GetOrAdd(
                GetSequenceName(sequence),
                _ => new OracleSequenceValueGeneratorState(sequence));
        }

        private static string GetSequenceName(ISequence sequence)
            => (sequence.Schema == null ? "" : sequence.Schema + ".") + sequence.Name;
    }
}
