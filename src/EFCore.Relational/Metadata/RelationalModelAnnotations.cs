// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

        public virtual IReadOnlyList<IMutableSequence> Sequences
            => Sequence.GetSequences(Model, RelationalAnnotationNames.SequencePrefix).ToList();

        public virtual IMutableSequence FindSequence([NotNull] string name, [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var annotationName = BuildAnnotationName(RelationalAnnotationNames.SequencePrefix, name, schema);

            return Model[annotationName] == null ? null : new Sequence(Model, annotationName);
        }

        public virtual IMutableSequence GetOrAddSequence([NotNull] string name, [CanBeNull] string schema = null)
            => FindSequence(name, schema)
               ?? new Sequence((IMutableModel)Model, BuildAnnotationName(RelationalAnnotationNames.SequencePrefix, name, schema), name, schema);

        private static string BuildAnnotationName(string annotationPrefix, string name, string schema)
            => annotationPrefix + schema + "." + name;

        public virtual IReadOnlyList<IDbFunction> DbFunctions
            => DbFunction.GetDbFunctions(Model, RelationalAnnotationNames.DbFunction).ToList();

        public virtual IDbFunction FindDbFunction(MethodInfo methodInfo)
            => DbFunction.FindDbFunction(Model, RelationalAnnotationNames.DbFunction, methodInfo);

        public virtual DbFunction GetOrAddDbFunction([NotNull] MethodInfo methodInfo, ConfigurationSource configurationSource, [CanBeNull] string name = null, [CanBeNull] string schema = null)
           => DbFunction.GetOrAddDbFunction((IMutableModel)Model, methodInfo, RelationalAnnotationNames.DbFunction, configurationSource, name, schema);

        public virtual string DefaultSchema
        {
            get => (string)Annotations.Metadata[RelationalAnnotationNames.DefaultSchema];
            [param: CanBeNull] set => SetDefaultSchema(value);
        }

        protected virtual bool SetDefaultSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.DefaultSchema,
                Check.NullButNotEmpty(value, nameof(value)));

        ISequence IRelationalModelAnnotations.FindSequence(string name, string schema) => FindSequence(name, schema);
        IReadOnlyList<ISequence> IRelationalModelAnnotations.Sequences => Sequences;
    }
}
