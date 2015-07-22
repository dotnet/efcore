// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class SqlitePropertyAnnotations : ReadOnlySqlitePropertyAnnotations
    {
        public SqlitePropertyAnnotations([NotNull] Property property)
            : base(property)
        {
        }

        public new virtual string ColumnName
        {
            get { return base.ColumnName; }
            [param: CanBeNull] set { Property[SqliteNameAnnotation] = value; }
        }

        public new virtual string ColumnType
        {
            get { return base.ColumnType; }
            [param: CanBeNull] set { Property[SqliteColumnTypeAnnotation] = value; }
        }

        public new virtual string GeneratedValueSql
        {
            get { return base.GeneratedValueSql; }
            [param: CanBeNull] set { Property[SqliteGeneratedValueSqlAnnotation] = value; }
        }

        protected new virtual Property Property => (Property)base.Property;
    }
}
