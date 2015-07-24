// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalModelAnnotations : IRelationalModelAnnotations
    {
        public RelationalModelAnnotations([NotNull] IModel model, [CanBeNull] string providerPrefix)
            : this(new RelationalAnnotations(model, providerPrefix))
        {
        }

        protected RelationalModelAnnotations([NotNull] RelationalAnnotations annotations)
        {
            Annotations = annotations;
        }

        protected RelationalAnnotations Annotations { get; }

        protected virtual IModel Model => (IModel)Annotations.Metadata;

        public virtual IReadOnlyList<ISequence> Sequences
        {
            get
            {
                var providerSequences = Annotations.ProviderPrefix != null
                    ? Sequence.GetSequences(Model, Annotations.ProviderPrefix).ToList()
                    : (IList<ISequence>)ImmutableList<ISequence>.Empty;

                return Sequence.GetSequences(Model, RelationalAnnotationNames.Prefix)
                    .Where(rs => !providerSequences.Any(ss => ss.Name == rs.Name && ss.Schema == rs.Schema))
                    .Concat(providerSequences)
                    .ToList();
            }
        }

        public virtual ISequence FindSequence(string name, string schema = null)
            => (Annotations.ProviderPrefix == null ? null : Sequence.FindSequence(Model, Annotations.ProviderPrefix, name, schema))
               ?? Sequence.FindSequence(Model, RelationalAnnotationNames.Prefix, name, schema);

        public virtual Sequence GetOrAddSequence([CanBeNull] string name, [CanBeNull] string schema = null)
            => new Sequence(
                (Model)Model,
                Annotations.ProviderPrefix ?? RelationalAnnotationNames.Prefix,
                Check.NotEmpty(name, nameof(name)),
                Check.NullButNotEmpty(schema, nameof(schema)));
    }
}
