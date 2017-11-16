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
    /// <summary>
    ///     Properties for relational-specific annotations accessed through
    ///     <see cref="RelationalMetadataExtensions.Relational(IMutableModel)" />.
    /// </summary>
    public class RelationalModelAnnotations : IRelationalModelAnnotations
    {
        /// <summary>
        ///     Constructs an instance for annotations of the given <see cref="IModel" />.
        /// </summary>
        /// <param name="model"> The <see cref="IModel" /> to use. </param>
        public RelationalModelAnnotations([NotNull] IModel model)
            : this(new RelationalAnnotations(model))
        {
        }

        /// <summary>
        ///     Constructs an instance for annotations of the <see cref="IModel" />
        ///     represented by the given annotation helper.
        /// </summary>
        /// <param name="annotations">
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IModel" /> to annotate.
        /// </param>
        protected RelationalModelAnnotations([NotNull] RelationalAnnotations annotations)
            => Annotations = annotations;

        /// <summary>
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IModel" /> to annotate.
        /// </summary>
        protected virtual RelationalAnnotations Annotations { get; }

        /// <summary>
        ///     The <see cref="IModel" /> to annotate.
        /// </summary>
        protected virtual IModel Model => (IModel)Annotations.Metadata;

        /// <summary>
        ///     All <see cref="ISequence" />s contained in the model.
        /// </summary>
        public virtual IReadOnlyList<IMutableSequence> Sequences
            => Sequence.GetSequences(Model, RelationalAnnotationNames.SequencePrefix).ToList();

        /// <summary>
        ///     Finds an <see cref="ISequence" /> with the given name.
        /// </summary>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence. </param>
        /// <returns>
        ///     The <see cref="ISequence" /> or <c>null</c> if no sequence with the given name in
        ///     the given schema was found.
        /// </returns>
        public virtual IMutableSequence FindSequence([NotNull] string name, [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var annotationName = BuildAnnotationName(RelationalAnnotationNames.SequencePrefix, name, schema);

            return Model[annotationName] == null ? null : new Sequence(Model, annotationName);
        }

        /// <summary>
        ///     Either returns the existing <see cref="IMutableSequence" /> with the given name in the given schema
        ///     or creates a new sequence with the given name and schema.
        /// </summary>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema name, or <c>null</c> to use the default schema. </param>
        /// <returns> The sequence. </returns>
        public virtual IMutableSequence GetOrAddSequence([NotNull] string name, [CanBeNull] string schema = null)
            => FindSequence(name, schema)
               ?? new Sequence((IMutableModel)Model, BuildAnnotationName(RelationalAnnotationNames.SequencePrefix, name, schema), name, schema);

        private static string BuildAnnotationName(string annotationPrefix, string name, string schema)
            => annotationPrefix + schema + "." + name;

        /// <summary>
        ///     All <see cref="IDbFunction" />s contained in the model.
        /// </summary>
        public virtual IReadOnlyList<IDbFunction> DbFunctions
            => DbFunction.GetDbFunctions(Model, RelationalAnnotationNames.DbFunction).ToList();

        /// <summary>
        ///     Finds a <see cref="IDbFunction" /> that is mapped to the method represented by the given <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="methodInfo"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The <see cref="IDbFunction" /> or <c>null</c> if the method is not mapped. </returns>
        public virtual IDbFunction FindDbFunction(MethodInfo methodInfo)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));

            return DbFunction.FindDbFunction(Model, RelationalAnnotationNames.DbFunction, methodInfo);
        }

        /// <summary>
        ///     Either returns the existing <see cref="DbFunction" /> mapped to the given method
        ///     or creates a new function mapped to the method.
        /// </summary>
        /// <param name="methodInfo"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The <see cref="DbFunction" />. </returns>
        public virtual DbFunction GetOrAddDbFunction([NotNull] MethodInfo methodInfo)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));

            return DbFunction.GetOrAddDbFunction((IMutableModel)Model, methodInfo, RelationalAnnotationNames.DbFunction);
        }

        /// <summary>
        ///     The default schema to use for the model, or <c>null</c> if none has been explicitly set.
        /// </summary>
        public virtual string DefaultSchema
        {
            get => (string)Annotations.Metadata[RelationalAnnotationNames.DefaultSchema];
            [param: CanBeNull] set => SetDefaultSchema(value);
        }

        /// <summary>
        ///     Attempts to set the <see cref="DefaultSchema" /> using the semantics of the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetDefaultSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.DefaultSchema,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     The maximum length allowed for store identifiers.
        /// </summary>
        public virtual int MaxIdentifierLength
        {
            get => (int?)Annotations.Metadata[RelationalAnnotationNames.MaxIdentifierLength] ?? short.MaxValue;
            set => SetMaxIdentifierLength(value);
        }

        /// <summary>
        ///     Attempts to set the <see cref="MaxIdentifierLength" /> using the semantics of the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetMaxIdentifierLength(int? value)
            => Annotations.SetAnnotation(RelationalAnnotationNames.MaxIdentifierLength, value);

        /// <summary>
        ///     Finds an <see cref="ISequence" /> with the given name.
        /// </summary>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence. </param>
        /// <returns>
        ///     The <see cref="ISequence" /> or <c>null</c> if no sequence with the given name in
        ///     the given schema was found.
        /// </returns>
        ISequence IRelationalModelAnnotations.FindSequence(string name, string schema) => FindSequence(name, schema);

        /// <summary>
        ///     All <see cref="ISequence" />s contained in the model.
        /// </summary>
        IReadOnlyList<ISequence> IRelationalModelAnnotations.Sequences => Sequences;
    }
}
