// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class ReadOnlyRelationalModelExtensions : IRelationalModelExtensions
    {
        protected const string RelationalSequenceAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.Sequence;

        private readonly IModel _model;

        public ReadOnlyRelationalModelExtensions([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            _model = model;
        }

        protected virtual IModel Model => _model;

        public virtual IReadOnlyList<Sequence> Sequences => (
            from a in _model.Annotations
            where a.Name.StartsWith(RelationalSequenceAnnotation)
            select Sequence.Deserialize((string)a.Value))
            .ToList();

        public virtual Sequence TryGetSequence(string name, string schema = null)
            => FindSequence(
                RelationalSequenceAnnotation + Check.NullButNotEmpty(schema, nameof(schema))
                + "."
                + Check.NotEmpty(name, nameof(name)));

        protected virtual Sequence FindSequence([NotNull] string annotationName)
        {
            Check.NotEmpty(annotationName, nameof(annotationName));

            var value = Model[annotationName];
            if (value == null)
            {
                return null;
            }

            var sequence = Sequence.Deserialize((string)value);
            sequence.Model = _model;
            return sequence;
        }
    }
}
