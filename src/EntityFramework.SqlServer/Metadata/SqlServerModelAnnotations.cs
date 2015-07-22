// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        public virtual SqlServerIdentityStrategy? IdentityStrategy
        {
            get { return (SqlServerIdentityStrategy?)GetAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy); }
            [param: CanBeNull] set { SetAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy, value); }
        }

        public virtual string HiLoSequenceName
        {
            get { return (string)GetAnnotation(SqlServerAnnotationNames.HiLoSequenceName); }
            [param: CanBeNull] set { SetAnnotation(SqlServerAnnotationNames.HiLoSequenceName, Check.NullButNotEmpty(value, nameof(value))); }
        }

        public virtual string HiLoSequenceSchema
        {
            get { return (string)GetAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema); }
            [param: CanBeNull] set { SetAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema, Check.NullButNotEmpty(value, nameof(value))); }
        }

        public virtual int? HiLoSequencePoolSize
        {
            get { return (int?)GetAnnotation(SqlServerAnnotationNames.HiLoSequencePoolSize); }
            [param: CanBeNull] set { SetAnnotation(SqlServerAnnotationNames.HiLoSequencePoolSize, value); }
        }
    }
}
