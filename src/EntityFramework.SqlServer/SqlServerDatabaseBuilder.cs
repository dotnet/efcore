// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDatabaseBuilder : DatabaseBuilder
    {
        public SqlServerDatabaseBuilder([NotNull] SqlServerTypeMapper typeMapper)
            : base(typeMapper)
        {
        }

        public virtual new SqlServerTypeMapper TypeMapper
        {
            get { return (SqlServerTypeMapper)base.TypeMapper; }
        }

        protected override Sequence BuildSequence(IProperty property)
        {
            Check.NotNull(property, "property");

            var sequence = property.SqlServer().TryGetSequence();

            return sequence == null
                ? null
                : new Sequence(
                    new SchemaQualifiedName(sequence.Name, sequence.Schema),
                    GetSqlDataType(sequence.Type),
                    sequence.StartValue,
                    sequence.IncrementBy);
        }

        private static string GetSqlDataType(Type sequenceType)
        {
            Contract.Assert(sequenceType == typeof(long)
                            || sequenceType == typeof(int)
                            || sequenceType == typeof(short)
                            || sequenceType == typeof(byte));

            return sequenceType == typeof(long)
                ? "bigint"
                : sequenceType == typeof(int)
                    ? "int"
                    : sequenceType == typeof(short)
                        ? "smallint"
                        : "tinyint";
        }

        protected override string GetSchema(IEntityType entityType)
        {
            return entityType.SqlServer().Schema;
        }

        protected override string GetTableName(IEntityType entityType)
        {
            return entityType.SqlServer().Table;
        }

        protected override string GetIndexName(IIndex index)
        {
            return index.SqlServer().Name;
        }

        protected override string GetColumnName(IProperty property)
        {
            return property.SqlServer().Column;
        }

        protected override string GetColumnType(IProperty property)
        {
            return property.SqlServer().ColumnType;
        }

        protected override object GetColumnDefaultValue(IProperty property)
        {
            return property.SqlServer().DefaultValue;
        }

        protected override string GetColumnDefaultSql(IProperty property)
        {
            return property.SqlServer().DefaultExpression;
        }

        protected override string GetForeignKeyName(IForeignKey foreignKey)
        {
            return foreignKey.SqlServer().Name;
        }

        protected override string GetKeyName(IKey key)
        {
            return key.SqlServer().Name;
        }

        protected override bool IsKeyClustered(IKey key)
        {
            return key.SqlServer().IsClustered != false;
        }

        protected override bool IsIndexClustered(IIndex index)
        {
            return index.SqlServer().IsClustered == true;
        }
    }
}
