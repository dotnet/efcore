// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

namespace EntityFramework.SqlServer.ReverseEngineering
{
    public class InformationSchemaContext : DbContext
    {
        private readonly DbConnection _connection;

        public InformationSchemaContext(DbConnection connection)
        {
            _connection = connection;
        }

        public DbSet<InformationSchemaTable> Tables { get; set; }
        public DbSet<InformationSchemaTableColumn> TableColumns { get; set; }
        public DbSet<InformationSchemaConstraint> TableConstraints { get; set; }

        protected override void OnConfiguring(DbContextOptions options)
        {
            options.UseSqlServer(_connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InformationSchemaTable>(
                b =>
                {
                    b.ForRelational().Table("TABLES", "INFORMATION_SCHEMA");
                    b.Property(t => t.Schema).ForRelational().Column("TABLE_SCHEMA");
                    b.Property(t => t.Name).ForRelational().Column("TABLE_NAME");
                    b.Property(t => t.TableType).ForRelational().Column("TABLE_TYPE");
                    b.Key(t => new { t.Schema, t.Name });
                });
            modelBuilder.Entity<InformationSchemaTableColumn>(
                b =>
                {
                    b.ForRelational().Table("COLUMNS", "INFORMATION_SCHEMA");
                    b.Property(c => c.TableSchema).ForRelational().Column("TABLE_SCHEMA");
                    b.Property(c => c.TableName).ForRelational().Column("TABLE_NAME");
                    b.Property(c => c.Name).ForRelational().Column("COLUMN_NAME");
                    b.Property(c => c.Default).ForRelational().Column("COLUMN_DEFAULT");
                    b.Property(c => c.IsNullable).ForRelational().Column("IS_NULLABLE");
                    b.Property(c => c.DataType).ForRelational().Column("DATA_TYPE");
                    b.Property(c => c.MaxLength).ForRelational().Column("CHARACTER_MAXIMUM_LENGTH");
                    b.Property(c => c.NumericPrecision).ForRelational().Column("NUMERIC_PRECISION");
                    b.Property(c => c.Scale).ForRelational().Column("NUMERIC_SCALE");
                    b.Property(c => c.DateTimePrecision).ForRelational().Column("DATETIME_PRECISION");
                    b.Key(c => new { c.TableSchema, c.TableName, c.Name });
                    b.ManyToOne<InformationSchemaTable>(collection: t => t.Columns).ForeignKey(c => new { c.TableSchema, c.TableName });
                });
            modelBuilder.Entity<InformationSchemaConstraint>(
                b =>
                {
                    b.ForRelational().Table("TABLE_CONSTRAINTS", "INFORMATION_SCHEMA");
                    b.Property(c => c.Name).ForRelational().Column("CONSTRAINT_NAME");
                    b.Property(c => c.TableSchema).ForRelational().Column("TABLE_SCHEMA");
                    b.Property(c => c.TableName).ForRelational().Column("TABLE_NAME");
                    b.Property(c => c.ConstraintType).ForRelational().Column("CONSTRAINT_TYPE");
                    b.Key(c => c.Name);
                    b.ManyToOne<InformationSchemaTable>(collection: t => t.Constraints).ForeignKey(c => new { c.TableSchema, c.TableName });
                });
            modelBuilder.Entity<InformationSchemaKeyColumnUsage>(
                b =>
                {
                    b.ForRelational().Table("KEY_COLUMN_USAGE", "INFORMATION_SCHEMA");
                    b.Property(c => c.ConstraintName).ForRelational().Column("CONSTRAINT_NAME");
                    b.Property(c => c.TableSchema).ForRelational().Column("TABLE_SCHEMA");
                    b.Property(c => c.TableName).ForRelational().Column("TABLE_NAME");
                    b.Property(c => c.ColumnName).ForRelational().Column("COLUMN_NAME");
                    b.Property(c => c.Position).ForRelational().Column("ORDINAL_POSITION");
                    b.Key(u => new { u.ConstraintName, u.TableSchema, u.TableName, u.ColumnName });
                    b.ManyToOne<InformationSchemaConstraint>(collection: c => c.KeyColumnUsages).ForeignKey(c => c.ConstraintName);
                    b.ManyToOne(u => u.Column).ForeignKey(u => new { u.TableSchema, u.TableName, u.ColumnName });
                });
            modelBuilder.Entity<InformationSchemaReferentialConstraint>(
                b =>
                {
                    b.ForRelational().Table("REFERENTIAL_CONSTRAINTS", "INFORMATION_SCHEMA");
                    b.Property(c => c.ConstraintName).ForRelational().Column("CONSTRAINT_NAME");
                    b.Property(c => c.UniqueConstraintName).ForRelational().Column("UNIQUE_CONSTRAINT_NAME");
                    b.Property(c => c.DeleteRule).ForRelational().Column("DELETE_RULE");
                    b.Key(c => new { c.ConstraintName, c.UniqueConstraintName });
                    // TODO: Why is the second type parameter required?
                    b.OneToOne<InformationSchemaConstraint>(navigationToPrincipal: c => c.ReferentialConstraint).ForeignKey<InformationSchemaReferentialConstraint>(c => c.ConstraintName);
                    b.ManyToOne(c => c.UniqueConstraint).ForeignKey(c => c.UniqueConstraintName);
                });
        }
    }

    public class InformationSchemaTable
    {
        public string Schema { get; set; }
        public string Name { get; set; }
        public string TableType { get; set; }
        public ICollection<InformationSchemaTableColumn> Columns { get; } = new HashSet<InformationSchemaTableColumn>();
        public ICollection<InformationSchemaConstraint> Constraints { get; } = new HashSet<InformationSchemaConstraint>();
    }

    public class InformationSchemaTableColumn
    {
        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public string Name { get; set; }
        public string Default { get; set; }
        public string IsNullable { get; set; }
        public string DataType { get; set; }
        public int? MaxLength { get; set; }
        public byte? NumericPrecision { get; set; }
        public int? Scale { get; set; }
        public short? DateTimePrecision { get; set; }
    }

    public class InformationSchemaConstraint
    {
        public string Name { get; set; }
        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public string ConstraintType { get; set; }
        public ICollection<InformationSchemaKeyColumnUsage> KeyColumnUsages { get; } = new HashSet<InformationSchemaKeyColumnUsage>();
        public InformationSchemaReferentialConstraint ReferentialConstraint { get; set; }
    }

    public class InformationSchemaKeyColumnUsage
    {
        public string ConstraintName { get; set; }
        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public int Position { get; set; }
        public InformationSchemaTableColumn Column { get; set; }
    }

    public class InformationSchemaReferentialConstraint
    {
        public string ConstraintName { get; set; }
        public string UniqueConstraintName { get; set; }
        public string DeleteRule { get; set; }
        public InformationSchemaConstraint UniqueConstraint { get; set; }
    }
}
