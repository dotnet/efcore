// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalModelAnnotations : IRelationalModelAnnotations
    {
        public RelationalModelAnnotations([NotNull] IModel model)
            : this(new RelationalAnnotations(model))
        {
        }

        protected RelationalModelAnnotations(
            [NotNull] RelationalAnnotations annotations) => Annotations = annotations;

        protected virtual RelationalAnnotations Annotations { get; }

        protected virtual IModel Model => (IModel)Annotations.Metadata;

        public virtual IReadOnlyList<ISequence> Sequences
            => Sequence.GetSequences(Model, RelationalAnnotationNames.SequencePrefix).ToList();

        public virtual ISequence FindSequence(string name, string schema = null)
            => Sequence.FindSequence(Model, RelationalAnnotationNames.SequencePrefix, name, schema);

        public virtual Sequence GetOrAddSequence([NotNull] string name, [CanBeNull] string schema = null)
            => Sequence.GetOrAddSequence((IMutableModel)Model, RelationalAnnotationNames.SequencePrefix, name, schema);

        public virtual string DefaultSchema
        {
            get => (string)Annotations.GetAnnotation(RelationalAnnotationNames.DefaultSchema);
            [param: CanBeNull] set => SetDefaultSchema(value);
        }

        protected virtual bool SetDefaultSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.DefaultSchema,
                Check.NullButNotEmpty(value, nameof(value)));
    }
}
