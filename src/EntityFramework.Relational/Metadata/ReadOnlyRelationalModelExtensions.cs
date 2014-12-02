// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            Check.NotNull(model, "model");

            _model = model;
        }

        protected virtual IModel Model
        {
            get { return _model; }
        }

        public virtual Sequence TryGetSequence(string name, string schema = null)
        {
            Check.NotEmpty(name, "name");
            Check.NullButNotEmpty(schema, "schema");

            return FindSequence(RelationalSequenceAnnotation + schema + "." + name);
        }

        protected virtual Sequence FindSequence([NotNull] string annotationName)
        {
            Check.NotEmpty(annotationName, "annotationName");

            var value = Model[annotationName];
            if (value == null)
            {
                return null;
            }

            var sequence = Sequence.Deserialize(value);
            sequence.Model = _model;
            return sequence;
        }
    }
}
