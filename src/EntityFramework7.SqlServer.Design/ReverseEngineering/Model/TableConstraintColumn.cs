// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering.Model
{
    public class TableConstraintColumn
    {
        public const string Query =
            @"SELECT
    quotename(tc.CONSTRAINT_SCHEMA) + quotename(tc.CONSTRAINT_NAME) + quotename(kcu.COLUMN_NAME) [Id]
  , quotename(tc.TABLE_SCHEMA) + quotename(tc.TABLE_NAME) + quotename(kcu.COLUMN_NAME) [ColumnId]
  , quotename(tc.CONSTRAINT_SCHEMA) + quotename(tc.CONSTRAINT_NAME) [ConstraintId]
  , tc.CONSTRAINT_TYPE [ConstraintType]
  , kcu.ORDINAL_POSITION [Ordinal]
  FROM
  INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
  INNER JOIN
  INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
  ON tc.CONSTRAINT_CATALOG = kcu.CONSTRAINT_CATALOG
  AND tc.CONSTRAINT_SCHEMA = kcu.CONSTRAINT_SCHEMA
  AND tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
  AND tc.TABLE_CATALOG = kcu.TABLE_CATALOG
  AND tc.TABLE_SCHEMA = kcu.TABLE_SCHEMA
  AND tc.TABLE_NAME = kcu.TABLE_NAME
  WHERE tc.TABLE_NAME IS NOT NULL
";

        public virtual string Id { get; [param: CanBeNull] set; }
        public virtual string ColumnId { get; [param: CanBeNull] set; }
        public virtual string ConstraintId { get; [param: CanBeNull] set; }
        public virtual string ConstraintType { get; [param: CanBeNull] set; }
        public virtual int Ordinal { get; [param: CanBeNull] set; }

        public static TableConstraintColumn CreateFromReader([NotNull] SqlDataReader reader)
        {
            Check.NotNull(reader, nameof(reader));

            var tableConstraintColumn = new TableConstraintColumn();
            tableConstraintColumn.Id = reader.IsDBNull(0) ? null : reader.GetString(0);
            tableConstraintColumn.ColumnId = reader.IsDBNull(1) ? null : reader.GetString(1);
            tableConstraintColumn.ConstraintId = reader.IsDBNull(2) ? null : reader.GetString(2);
            tableConstraintColumn.ConstraintType = reader.IsDBNull(3) ? null : reader.GetString(3);
            tableConstraintColumn.Ordinal = reader.GetInt32(4);

            return tableConstraintColumn;
        }

        public override string ToString()
        {
            return "TCon[Id=" + Id
                   + ", ColumnId=" + ColumnId
                   + ", ConstraintId=" + ConstraintId
                   + ", ConstraintType=" + ConstraintType
                   + ", Ordinal=" + Ordinal
                   + "]";
        }
    }
}
