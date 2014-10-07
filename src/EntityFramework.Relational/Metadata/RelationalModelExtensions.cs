// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class RelationalModelExtensions : ReadOnlyRelationalModelExtensions
    {
        public RelationalModelExtensions([NotNull] Entity.Metadata.Model model)
            : base(model)
        {
        }

        public virtual Sequence AddOrReplaceSequence([NotNull] Sequence sequence)
        {
            Check.NotNull(sequence, "sequence");

            var model = (Entity.Metadata.Model)Model;
            sequence.Model = model;
            model[RelationalSequenceAnnotation + sequence.Schema + "." + sequence.Name] = sequence.Serialize();

            return sequence;
        }

        public virtual Sequence GetOrAddSequence([CanBeNull] string name = null, [CanBeNull] string schema = null)
        {
            Check.NullButNotEmpty(name, "name");
            Check.NullButNotEmpty(schema, "schema");

            name = name ?? Sequence.DefaultName;

            return ((Entity.Metadata.Model)Model).Relational().TryGetSequence(name, schema) 
                   ?? AddOrReplaceSequence(new Sequence(name, schema));
        }
    }
}
