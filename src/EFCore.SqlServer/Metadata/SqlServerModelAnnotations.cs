// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class SqlServerModelAnnotations : RelationalModelAnnotations, ISqlServerModelAnnotations
    {
        public const string DefaultHiLoSequenceName = "EntityFrameworkHiLoSequence";

        public SqlServerModelAnnotations([NotNull] IModel model)
            : base(model, SqlServerFullAnnotationNames.Instance)
        {
        }

        protected SqlServerModelAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations, SqlServerFullAnnotationNames.Instance)
        {
        }

        public virtual string HiLoSequenceName
        {
            get { return (string)Annotations.GetAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceName, null); }
            [param: CanBeNull] set { SetHiLoSequenceName(value); }
        }

        protected virtual bool SetHiLoSequenceName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                SqlServerFullAnnotationNames.Instance.HiLoSequenceName,
                null,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string HiLoSequenceSchema
        {
            get { return (string)Annotations.GetAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceSchema, null); }
            [param: CanBeNull] set { SetHiLoSequenceSchema(value); }
        }

        protected virtual bool SetHiLoSequenceSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                SqlServerFullAnnotationNames.Instance.HiLoSequenceSchema,
                null,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual SqlServerValueGenerationStrategy? ValueGenerationStrategy
        {
            get
            {
                return (SqlServerValueGenerationStrategy?)Annotations.GetAnnotation(
                    SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy,
                    null);
            }
            set { SetValueGenerationStrategy(value); }
        }

        protected virtual bool SetValueGenerationStrategy(SqlServerValueGenerationStrategy? value)
            => Annotations.SetAnnotation(SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy,
                null,
                value);
    }
}
