// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalModelAnnotations : IRelationalModelAnnotations
    {
        protected readonly RelationalFullAnnotationNames ProviderFullAnnotationNames;

        public RelationalModelAnnotations([NotNull] IModel model, [CanBeNull] RelationalFullAnnotationNames providerFullAnnotationNames)
            : this(new RelationalAnnotations(model), providerFullAnnotationNames)
        {
        }

        protected RelationalModelAnnotations(
            [NotNull] RelationalAnnotations annotations,
            [CanBeNull] RelationalFullAnnotationNames providerFullAnnotationNames)
        {
            Annotations = annotations;
            ProviderFullAnnotationNames = providerFullAnnotationNames;
        }

        protected virtual RelationalAnnotations Annotations { get; }
        protected virtual IModel Model => (IModel)Annotations.Metadata;

        public virtual IReadOnlyList<ISequence> Sequences
        {
            get
            {
                var providerSequences = ProviderFullAnnotationNames != null
                    ? Sequence.GetSequences(Model, ProviderFullAnnotationNames.SequencePrefix).ToList()
                    : new List<ISequence>();

                return Sequence.GetSequences(Model, RelationalFullAnnotationNames.Instance.SequencePrefix)
                    .Where(rs => !providerSequences.Any(ss => (ss.Name == rs.Name) && (ss.Schema == rs.Schema)))
                    .Concat(providerSequences)
                    .ToList();
            }
        }

        public virtual ISequence FindSequence(string name, string schema = null)
            => (ProviderFullAnnotationNames == null
                   ? null
                   : Sequence.FindSequence(Model, ProviderFullAnnotationNames.SequencePrefix, name, schema))
               ?? Sequence.FindSequence(Model, RelationalFullAnnotationNames.Instance.SequencePrefix, name, schema);

        public virtual Sequence GetOrAddSequence([NotNull] string name, [CanBeNull] string schema = null)
            => Sequence.GetOrAddSequence((IMutableModel)Model,
                (ProviderFullAnnotationNames ?? RelationalFullAnnotationNames.Instance).SequencePrefix,
                name,
                schema);

        public virtual string DefaultSchema
        {
            get
            {
                return (string)Annotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.DefaultSchema,
                    ProviderFullAnnotationNames?.DefaultSchema);
            }
            [param: CanBeNull] set { SetDefaultSchema(value); }
        }

        protected virtual bool SetDefaultSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.DefaultSchema,
                ProviderFullAnnotationNames?.DefaultSchema,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string DatabaseName
        {
            get
            {
                return (string)Annotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.DatabaseName,
                    ProviderFullAnnotationNames?.DatabaseName);
            }
            [param: CanBeNull] set { SetDatabaseName(value); }
        }

        protected virtual bool SetDatabaseName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.DatabaseName,
                ProviderFullAnnotationNames?.DatabaseName,
                Check.NullButNotEmpty(value, nameof(value)));
    }
}
