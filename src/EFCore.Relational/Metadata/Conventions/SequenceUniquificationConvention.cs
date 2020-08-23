// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            var model = modelBuilder.Metadata;
            var modelSequences =
                (SortedDictionary<(string Name, string Schema), Sequence>)model[RelationalAnnotationNames.Sequences];

            if (modelSequences != null)
            {
                var maxLength = model.GetMaxIdentifierLength();
                var toReplace = modelSequences
                    .Where(s => s.Key.Name.Length > maxLength).ToList();

                foreach (var sequence in toReplace)
                {
                    var schemaName = sequence.Key.Schema;
                    var newSequenceName = Uniquifier.Uniquify(
                        sequence.Key.Name, modelSequences,
                        sequenceName => (sequenceName, schemaName), maxLength);
                    Sequence.SetName((IMutableModel)model, sequence.Value, newSequenceName);
                }
            }
        }
    }
}
