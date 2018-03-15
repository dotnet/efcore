// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Properties for relational-specific annotations accessed through
    ///     <see cref="SqlServerMetadataExtensions.SqlServer(IMutableModel)" />.
    /// </summary>
    public class SqlServerModelAnnotations : RelationalModelAnnotations, ISqlServerModelAnnotations
    {
        /// <summary>
        ///     The default name for the sequence used
        ///     with <see cref="SqlServerPropertyBuilderExtensions.ForSqlServerUseSequenceHiLo" />
        /// </summary>
        public const string DefaultHiLoSequenceName = "EntityFrameworkHiLoSequence";

        /// <summary>
        ///     Constructs an instance for annotations of the given <see cref="IModel" />.
        /// </summary>
        /// <param name="model"> The <see cref="IModel" /> to use. </param>
        public SqlServerModelAnnotations([NotNull] IModel model)
            : base(model)
        {
        }

        /// <summary>
        ///     Constructs an instance for annotations of the <see cref="IModel" />
        ///     represented by the given annotation helper.
        /// </summary>
        /// <param name="annotations">
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IModel" /> to annotate.
        /// </param>
        protected SqlServerModelAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        /// <summary>
        ///     Gets or sets the sequence name to use with
        ///     <see cref="SqlServerPropertyBuilderExtensions.ForSqlServerUseSequenceHiLo" />
        /// </summary>
        public virtual string HiLoSequenceName
        {
            get => (string)Annotations.Metadata[SqlServerAnnotationNames.HiLoSequenceName];
            [param: CanBeNull] set => SetHiLoSequenceName(value);
        }

        /// <summary>
        ///     Attempts to set the sequence name to use with
        ///     <see cref="SqlServerPropertyBuilderExtensions.ForSqlServerUseSequenceHiLo" />
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetHiLoSequenceName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                SqlServerAnnotationNames.HiLoSequenceName,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     Gets or sets the schema for the sequence to use with
        ///     <see cref="SqlServerPropertyBuilderExtensions.ForSqlServerUseSequenceHiLo" />
        /// </summary>
        public virtual string HiLoSequenceSchema
        {
            get => (string)Annotations.Metadata[SqlServerAnnotationNames.HiLoSequenceSchema];
            [param: CanBeNull] set => SetHiLoSequenceSchema(value);
        }

        /// <summary>
        ///     Attempts to set the schema for the sequence to use with
        ///     <see cref="SqlServerPropertyBuilderExtensions.ForSqlServerUseSequenceHiLo" />
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetHiLoSequenceSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                SqlServerAnnotationNames.HiLoSequenceSchema,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     The <see cref="SqlServerValueGenerationStrategy" /> to use for properties
        ///     of keys in the model, unless the property has a different strategy explicitly set.
        /// </summary>
        public virtual SqlServerValueGenerationStrategy? ValueGenerationStrategy
        {
            get => (SqlServerValueGenerationStrategy?)Annotations.Metadata[SqlServerAnnotationNames.ValueGenerationStrategy];

            set => SetValueGenerationStrategy(value);
        }

        /// <summary>
        ///     Attempts to set the <see cref="SqlServerValueGenerationStrategy" /> to use for properties
        ///     of keys in the model.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetValueGenerationStrategy(SqlServerValueGenerationStrategy? value)
            => Annotations.SetAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy, value);
    }
}
