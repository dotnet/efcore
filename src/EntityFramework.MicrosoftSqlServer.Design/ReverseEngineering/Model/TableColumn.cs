// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering.Model
{
    public class TableColumn
    {
        public const string Query =
            @"SELECT
    quotename(c.TABLE_SCHEMA) + quotename(c.TABLE_NAME) + quotename(c.COLUMN_NAME) [Id]
  , quotename(c.TABLE_SCHEMA) + quotename(c.TABLE_NAME) [ParentId]
  , c.COLUMN_NAME [Name]
  , c.ORDINAL_POSITION [Ordinal]
  , CAST( CASE c.IS_NULLABLE WHEN 'YES' THEN 1 WHEN 'NO' THEN 0 ELSE 0 END as bit) [IsNullable]
  , c.DATA_TYPE  AS [DataType]
  , c.CHARACTER_MAXIMUM_LENGTH [MaxLength]
  , CAST(c.NUMERIC_PRECISION as integer) [NumericPrecision]
  , CAST(c.DATETIME_PRECISION as integer)[DateTimePrecision]
  , CAST(c.NUMERIC_SCALE as integer) [Scale]
  , CAST(columnproperty( object_id(quotename(c.TABLE_SCHEMA) + '.' + quotename(c.TABLE_NAME)), c.COLUMN_NAME, 'IsIdentity' ) as bit) as [IsIdentity]
  , CAST(columnproperty( object_id(quotename(c.TABLE_SCHEMA) + '.' + quotename(c.TABLE_NAME)), c.COLUMN_NAME, 'IsComputed' ) as bit) as [IsStoreGenerated]
  , c.COLUMN_DEFAULT as [Default]
  FROM
  INFORMATION_SCHEMA.COLUMNS c
  INNER JOIN
  INFORMATION_SCHEMA.TABLES t
  ON c.TABLE_CATALOG = t.TABLE_CATALOG
  AND c.TABLE_SCHEMA = t.TABLE_SCHEMA
  AND c.TABLE_NAME = t.TABLE_NAME
  AND t.TABLE_TYPE = 'BASE TABLE'
  AND t.TABLE_NAME <> '" + HistoryRepository.DefaultTableName + "'";

        public virtual string Id { get;[param: CanBeNull] set; }
        public virtual string TableId { get;[param: CanBeNull] set; }
        public virtual string ColumnName { get;[param: CanBeNull] set; }
        public virtual int Ordinal { get;[param: CanBeNull] set; }
        public virtual bool IsNullable { get;[param: CanBeNull] set; }
        public virtual string DataType { get;[param: CanBeNull] set; }
        public virtual int? MaxLength { get;[param: CanBeNull] set; }
        public virtual int? NumericPrecision { get;[param: CanBeNull] set; }
        public virtual int? DateTimePrecision { get;[param: CanBeNull] set; }
        public virtual int? Scale { get;[param: CanBeNull] set; }
        public virtual bool IsIdentity { get;[param: CanBeNull] set; }
        public virtual bool IsStoreGenerated { get;[param: CanBeNull] set; }
        public virtual string DefaultValue { get;[param: CanBeNull] set; }

        public static TableColumn CreateFromReader([NotNull] SqlDataReader reader)
        {
            Check.NotNull(reader, nameof(reader));

            var tableColumn = new TableColumn();
            tableColumn.Id = reader.IsDBNull(0) ? null : reader.GetString(0);
            tableColumn.TableId = reader.IsDBNull(1) ? null : reader.GetString(1);
            tableColumn.ColumnName = reader.IsDBNull(2) ? null : reader.GetString(2);
            tableColumn.Ordinal = reader.GetInt32(3);
            tableColumn.IsNullable = reader.GetBoolean(4);
            tableColumn.DataType = reader.IsDBNull(5) ? null : reader.GetString(5);
            tableColumn.MaxLength = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6);
            tableColumn.NumericPrecision = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7);
            tableColumn.DateTimePrecision = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8);
            tableColumn.Scale = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9);
            tableColumn.IsIdentity = reader.GetBoolean(10);
            tableColumn.IsStoreGenerated = reader.GetBoolean(11);
            tableColumn.DefaultValue = reader.IsDBNull(12) ? null : reader.GetString(12);

            return tableColumn;
        }

        public override string ToString()
        {
            return "TC[Id=" + Id
                   + ", TableId=" + TableId
                   + ", ColumnName=" + ColumnName
                   + ", Ordinal=" + Ordinal
                   + ", IsNullable=" + IsNullable
                   + ", DataType=" + DataType
                   + ", MaxLength=" + MaxLength
                   + ", NumericPrecision=" + NumericPrecision
                   + ", DateTimePrecision=" + DateTimePrecision
                   + ", Scale=" + Scale
                   + ", IsIdentity=" + IsIdentity
                   + ", IsStoreGenerated=" + IsStoreGenerated
                   + ", DefaultValue=" + DefaultValue
                   + "]";
        }
    }
}
