// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class ReadOnlySqlitePropertyExtensions : ReadOnlyRelationalPropertyExtensions, ISqlitePropertyExtensions
    {
        protected const string SqliteNameAnnotation = SqliteAnnotationNames.Prefix + RelationalAnnotationNames.ColumnName;
        protected const string SqliteColumnTypeAnnotation = SqliteAnnotationNames.Prefix + RelationalAnnotationNames.ColumnType;
        protected const string SqliteDefaultExpressionAnnotation = SqliteAnnotationNames.Prefix
            + RelationalAnnotationNames.ColumnDefaultExpression;

        public ReadOnlySqlitePropertyExtensions([NotNull] IProperty property)
            : base(property)
        {
        }

        public override string Column => Property[SqliteNameAnnotation] as string ?? base.Column;
        public override string ColumnType => Property[SqliteColumnTypeAnnotation] as string ?? base.ColumnType;
        public override string DefaultExpression => Property[SqliteDefaultExpressionAnnotation] as string
            ?? base.DefaultExpression;
    }
}
