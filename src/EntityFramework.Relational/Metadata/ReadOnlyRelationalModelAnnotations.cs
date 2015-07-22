// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ReadOnlyRelationalModelAnnotations : IRelationalModelAnnotations
    {
        private readonly IModel _model;

        public ReadOnlyRelationalModelAnnotations([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            _model = model;
        }

        protected virtual IModel Model => _model;

        public virtual IReadOnlyList<ISequence> Sequences
            => Sequence.GetSequences(_model, RelationalAnnotationNames.Prefix).ToList();

        public virtual ISequence FindSequence(string name, string schema = null)
            => Sequence.FindSequence(_model, RelationalAnnotationNames.Prefix, name, schema);
    }
}
