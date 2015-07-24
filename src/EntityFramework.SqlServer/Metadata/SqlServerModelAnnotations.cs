// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerModelAnnotations : RelationalModelAnnotations, ISqlServerModelAnnotations
    {
        public SqlServerModelAnnotations([NotNull] IModel model)
            : base(model, SqlServerAnnotationNames.Prefix)
        {
        }

        protected SqlServerModelAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual string HiLoSequenceName
        {
            get { return (string)Annotations.GetAnnotation(SqlServerAnnotationNames.HiLoSequenceName); }
            [param: CanBeNull] set { SetHiLoSequenceName(value); }
        }

        protected virtual bool SetHiLoSequenceName([CanBeNull]string value)
            => Annotations.SetAnnotation(SqlServerAnnotationNames.HiLoSequenceName, Check.NullButNotEmpty(value, nameof(value)));

        public virtual string HiLoSequenceSchema
        {
            get { return (string)Annotations.GetAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema); }
            [param: CanBeNull] set { SetHiLoSequenceSchema(value); }
        }

        protected virtual bool SetHiLoSequenceSchema([CanBeNull]string value)
            => Annotations.SetAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema, Check.NullButNotEmpty(value, nameof(value)));

        public virtual int? HiLoSequencePoolSize
        {
            get { return (int?)Annotations.GetAnnotation(SqlServerAnnotationNames.HiLoSequencePoolSize); }
            set { SetHiLoSequencePoolSize(value); }
        }

        protected virtual bool SetHiLoSequencePoolSize(int? value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), Entity.Internal.Strings.HiLoBadPoolSize);
            }

            return Annotations.SetAnnotation(SqlServerAnnotationNames.HiLoSequencePoolSize, value);
        }

        public virtual SqlServerIdentityStrategy? IdentityStrategy
        {
            get { return (SqlServerIdentityStrategy?)Annotations.GetAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy); }
            set { SetIdentityStrategy(value); }
        }

        protected virtual bool SetIdentityStrategy(SqlServerIdentityStrategy? value)
            => Annotations.SetAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy, value);
    }
}
