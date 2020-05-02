// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention which ensures that all sequences in the model have unique names
    ///     within a schema when truncated to the maximum identifier length for the model.
    /// </summary>
    public class SequenceUniquificationConvention : IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SequenceUniquificationConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention. </param>
        public SequenceUniquificationConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            var model = modelBuilder.Metadata;
            var modelSequences =
                (SortedDictionary<(string Name, string Schema), Sequence>)model[RelationalAnnotationNames.Sequences];

            if (modelSequences != null)
            {
                var schemaToSequenceNameToSequences = new Dictionary<string, Dictionary<string, Sequence>>();
                var nullSchemaSequenceNameToSequences = new Dictionary<string, Sequence>();
                foreach (var sequenceTupleToSequence in modelSequences)
                {
                    var schemaName = sequenceTupleToSequence.Key.Schema;
                    if (schemaName == null)
                    {
                        nullSchemaSequenceNameToSequences.Add(
                            sequenceTupleToSequence.Key.Name, sequenceTupleToSequence.Value);
                    }
                    else
                    {
                        if (!schemaToSequenceNameToSequences.TryGetValue(schemaName, out var sequenceNameToSequences))
                        {
                            sequenceNameToSequences = new Dictionary<string, Sequence>();
                            schemaToSequenceNameToSequences[schemaName] = sequenceNameToSequences;
                        }

                        sequenceNameToSequences.Add(sequenceTupleToSequence.Key.Name, sequenceTupleToSequence.Value);
                    }
                }

                var maxLength = model.GetMaxIdentifierLength();
                UniquifySequenceNamesInSchema(
                    null, nullSchemaSequenceNameToSequences, maxLength, ref modelSequences);
                foreach (var schemaToSequenceNameToSequence in schemaToSequenceNameToSequences)
                {
                    UniquifySequenceNamesInSchema(schemaToSequenceNameToSequence.Key,
                        schemaToSequenceNameToSequence.Value, maxLength, ref modelSequences);
                }
            }
        }

        private static void UniquifySequenceNamesInSchema(
            string schemaName, Dictionary<string, Sequence> sequenceNameToSequences, int maxLength,
            ref SortedDictionary<(string Name, string Schema), Sequence> modelSequences)
        {
            var sequenceNamesInSchema = new Dictionary<string, int>();
            var sequencesToRemove = new List<(string Name, string Schema)>();
            var sequencesToAdd = new Dictionary<(string Name, string Schema), Sequence>();
            foreach (var sequenceNameToSequence in sequenceNameToSequences)
            {
                var originalSequenceName = sequenceNameToSequence.Key;
                var newSequenceName =
                    Uniquifier.Uniquify(originalSequenceName, sequenceNamesInSchema, maxLength);
                sequenceNamesInSchema.Add(newSequenceName, 0);
                if (!string.Equals(newSequenceName, originalSequenceName, StringComparison.Ordinal))
                {
                    // do not just remove the old and immediately add the new sequence
                    // here in case the new name clashes with a different old, but
                    // not yet removed sequence
                    sequencesToRemove.Add((originalSequenceName, schemaName));
                    sequencesToAdd.Add((newSequenceName, schemaName),
                        Sequence.WithNewName(sequenceNameToSequence.Value, newSequenceName));
                }
            }

            foreach (var sequenceToRemove in sequencesToRemove)
            {
                modelSequences.Remove(sequenceToRemove);
            }

            foreach (var sequenceToAdd in sequencesToAdd)
            {
                modelSequences.Add(sequenceToAdd.Key, sequenceToAdd.Value);
            }
        }
    }
}
