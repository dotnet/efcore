// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.SqlServer.ReverseEngineering.Model;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer.ReverseEngineering
{
    public class SqlServerMetadataModelProvider : IDatabaseMetadataModelProvider
    {
        public static readonly Dictionary<string, Type> _sqlTypeToClrTypeMap
            = new Dictionary<string, Type>()
                {
                    // exact numerics
                    { "bigint", typeof(long) },
                    { "bit", typeof(byte) },
                    { "decimal", typeof(decimal) },
                    { "int", typeof(int) },
                    //TODO { "money", typeof(decimal) },
                    { "numeric", typeof(decimal) },
                    { "smallint", typeof(short) },
                    //TODO{ "smallmoney", typeof(decimal) },
                    { "tinyint", typeof(byte) },

                    // approximate numerics
                    { "float", typeof(float) },
                    { "real", typeof(double) },

                    // date and time
                    { "date", typeof(DateTime) },
                    { "datetime", typeof(DateTime) },
                    { "datetime2", typeof(DateTime) },
                    { "datetimeoffset", typeof(DateTimeOffset) },
                    { "smalldatetime", typeof(DateTime) },
                    { "time", typeof(DateTime) },

                    // character strings
                    { "char", typeof(string) },
                    { "text", typeof(string) },
                    { "varchar", typeof(string) },

                    // unicode character strings
                    { "nchar", typeof(string) },
                    { "ntext", typeof(string) },
                    { "nvarchar", typeof(string) },

                    // binary
                    { "binary", typeof(byte[]) },
                    { "image", typeof(byte[]) },
                    { "varbinary", typeof(byte[]) },

                    //TODO other
                    //{ "cursor", typeof(yyy) },
                    //{ "hierarchyid", typeof(yyy) },
                    //{ "sql_variant", typeof(yyy) },
                    //{ "table", typeof(yyy) },
                    //{ "timestamp", typeof(yyy) },
                    { "uniqueidentifier", typeof(Guid) },
                    //{ "xml", typeof(yyy) },

                    //TODO spatial
                };

        // annotation names
        public static readonly string AnnotationNameTableId = "TableId";
        public static readonly string AnnotationNameTableIdSchemaTableSeparator = ".";
        public static readonly string AnnotationNameColumnId = "ColumnId";
        public static readonly string AnnotationNamePrimaryKeyOrdinal = "PrimaryKeyOrdinalPosition";
        public static readonly string AnnotationNameForeignKeyConstraints = "ForeignKeyConstraints";
        public static readonly string AnnotationFormatForeignKey = "ForeignKey[{0}]{1}"; // {O} = ConstraintId, {1} = Descriptor
        public static readonly string AnnotationFormatForeignKeyConstraintSeparator = ",";
        public static readonly string AnnotationDescriptorForeignKeyOrdinal = "Ordinal";
        public static readonly string AnnotationDescriptorForeignKeyTargetEntityType = "TargetEntityType";
        public static readonly string AnnotationDescriptorForeignKeyTargetProperty = "TargetProperty";
        public static readonly string AnnotationNamePrecision = "Precision";
        public static readonly string AnnotationNameMaxLength = "MaxLength";
        public static readonly string AnnotationNameScale = "Scale";
        public static readonly string AnnotationNameIsIdentity = "IsIdentity";
        public static readonly string AnnotationNameIsNullable = "IsNullable";

        private ILogger _logger;

        public SqlServerMetadataModelProvider([NotNull] IServiceProvider serviceProvider)
        {
            _logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
        }

        public IModel GenerateMetadataModel(string connectionString, string filters)
        {
            Dictionary<string, Table> tables;
            Dictionary<string, TableColumn> tableColumns;
            Dictionary<string, TableConstraintColumn> tableConstraintColumns;
            Dictionary<string, ForeignKeyColumnMapping> foreignKeyColumnMappings;
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    tables = LoadData<Table>(conn, Table.Query, Table.CreateFromReader, t => t.Id);
                    tableColumns = LoadData<TableColumn>(conn, TableColumn.Query, TableColumn.CreateFromReader, tc => tc.Id);
                    tableConstraintColumns = LoadData<TableConstraintColumn>(
                        conn, TableConstraintColumn.Query, TableConstraintColumn.CreateFromReader, tcc => tcc.Id);
                    foreignKeyColumnMappings = LoadData<ForeignKeyColumnMapping>(
                        conn, ForeignKeyColumnMapping.Query, ForeignKeyColumnMapping.CreateFromReader, fkcm => fkcm.Id);
                }
                finally
                {
                    if (conn != null)
                    {
                        if (conn.State == ConnectionState.Open)
                        {
                            try
                            {
                                conn.Close();
                            }
                            catch (SqlException)
                            {
                                // do nothing if attempt to close connection fails
                            }
                        }
                    }
                }
            }

            //Console.WriteLine("Tables");
            //foreach (var t in tables)
            //{
            //    var table = t.Value;
            //    Console.WriteLine(table.ToString());
            //}

            //Console.WriteLine(Environment.NewLine + "Columns");
            //foreach (var tc in tableColumns)
            //{
            //    Console.WriteLine(tc.Value.ToString());
            //}

            //Console.WriteLine(Environment.NewLine + "Constraint Columns");
            //foreach (var tc in tableConstraintColumns)
            //{
            //    Console.WriteLine(tc.Value.ToString());
            //}

            //Console.WriteLine(Environment.NewLine + "Foreign Key Column Mappings");
            //foreach (var fkcm in foreignKeyColumnMappings)
            //{
            //    Console.WriteLine(fkcm.Value.ToString());
            //}

            return CreateModel(tables, tableColumns, tableConstraintColumns, foreignKeyColumnMappings);
        }

        public static Dictionary<string, T> LoadData<T>(
            SqlConnection conn, string query, Func<SqlDataReader, T> createFromReader, Func<T, string> identifier)
        {
            var data = new Dictionary<string, T>();
            var sqlCommand = new SqlCommand(query);
            sqlCommand.Connection = conn;

            using (var reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var item = createFromReader(reader);
                    data.Add(identifier(item), item);
                }
            }

            return data;
        }

        public IModel CreateModel(
            Dictionary<string, Table> tables,
            Dictionary<string, TableColumn> tableColumns,
            Dictionary<string, TableConstraintColumn> tableConstraintColumns,
            Dictionary<string, ForeignKeyColumnMapping> foreignKeyColumnMappings)
        {
            var columnIdToProperty = new Dictionary<string, Property>();
            var model = new Microsoft.Data.Entity.Metadata.Model();
            foreach (var t in tables)
            {
                var table = t.Value;
                var entityTypeName =
                    table.SchemaName
                    + AnnotationNameTableIdSchemaTableSeparator
                    + table.TableName;
                var entityType = model.AddEntityType(entityTypeName);
                entityType.AddAnnotation(AnnotationNameTableId, table.Id);

                var primaryKeys = new List<Property>();
                var foreignKeys = new Dictionary<string, List<Property>>();
                foreach (var tc in tableColumns.Values.Where(col => col.ParentId == table.Id))
                {
                    Type clrPropertyType;
                    if (_sqlTypeToClrTypeMap.TryGetValue(tc.DataType, out clrPropertyType))
                    {
                        if (tc.IsNullable)
                        {
                            clrPropertyType = clrPropertyType.MakeNullable();
                        }
                        // have to add property in shadow state as we have no CLR type representing the EntityType at this stage
                        var property = entityType.AddProperty(tc.ColumnName, clrPropertyType, true);
                        property.AddAnnotation(AnnotationNameColumnId, tc.Id);
                        columnIdToProperty.Add(tc.Id, property);

                        // make column a primary key if it appears in the PK constraint
                        var primaryKeyConstraintColumn =
                            tableConstraintColumns.Values
                            .FirstOrDefault(c => c.ParentId == table.Id && c.ColumnName == tc.ColumnName && c.ConstraintType == "PRIMARY KEY");
                        if (primaryKeyConstraintColumn != null)
                        {
                            primaryKeys.Add(property);
                            property.AddAnnotation(AnnotationNamePrimaryKeyOrdinal, primaryKeyConstraintColumn.Ordinal.ToString());
                        }

                        // make column a foreign key if it appears in an FK constraint

                        // loop over constraints in which this column appears
                        foreach (var foreignKeyConstraintColumn in
                            tableConstraintColumns.Values
                            .Where(c => c.ParentId == table.Id && c.ColumnName == tc.ColumnName && c.ConstraintType == "FOREIGN KEY"))
                        {
                            var foreignKeyConstraintsAnnotation = property.TryGetAnnotation(AnnotationNameForeignKeyConstraints);
                            if (foreignKeyConstraintsAnnotation == null)
                            {
                                foreignKeyConstraintsAnnotation = property.AddAnnotation(AnnotationNameForeignKeyConstraints, foreignKeyConstraintColumn.ConstraintId);
                            }
                            else
                            {
                                string oldForeignKeyConstraintsAnnotationValue = foreignKeyConstraintsAnnotation.Value;
                                property.RemoveAnnotation(foreignKeyConstraintsAnnotation);
                                property.AddAnnotation(AnnotationNameForeignKeyConstraints, 
                                    oldForeignKeyConstraintsAnnotationValue
                                    + AnnotationFormatForeignKeyConstraintSeparator
                                    + foreignKeyConstraintColumn.ConstraintId);
                            }
                            property.AddAnnotation(
                                GetForeignKeyOrdinalPositionAnnotationName(foreignKeyConstraintColumn.ConstraintId),
                                foreignKeyConstraintColumn.Ordinal.ToString());
                        }

                        ApplyPropertyProperties(property, tc);
                    }
                    // else skip this property
                }

                entityType.SetPrimaryKey(primaryKeys);
            }

            // loop over all properties adding TargetEntityType and TargetProperty for ForeignKeys
            // this has to be done after all EntityTypes and their Properties have been created.
            foreach (var fromEntityTpe in model.EntityTypes)
            {
                foreach (var fromProperty in fromEntityTpe.Properties)
                {
                    var foreignKeyConstraintsAnnotation = fromProperty.TryGetAnnotation(AnnotationNameForeignKeyConstraints);
                    if (foreignKeyConstraintsAnnotation != null)
                    {
                        var foreignKeyConstraintIds = SplitString(
                            AnnotationFormatForeignKeyConstraintSeparator.ToCharArray()
                            , foreignKeyConstraintsAnnotation.Value);
                        foreach(var foreignKeyConstraintId in foreignKeyConstraintIds)
                        {
                            var columnId = fromProperty[AnnotationNameColumnId];
                            var foreignKeyMapping = foreignKeyColumnMappings.Values
                                .SingleOrDefault(fkcm => fkcm.ConstraintId == foreignKeyConstraintId && fkcm.FromColumnId == columnId);
                            if (foreignKeyMapping == null)
                            {
                                _logger.WriteError("Could not find foreignKeyMapping for ConstrantId " + foreignKeyConstraintId + " FromColumn " + columnId);
                                //Console.WriteLine("Could not find foreignKeyMapping for ConstrantId " + foreignKeyConstraintId + " FromColumn " + columnId);
                                break;
                            }

                            TableColumn toColumn;
                            if (!tableColumns.TryGetValue(foreignKeyMapping.ToColumnId, out toColumn))
                            {
                                _logger.WriteError("Could not find toColumn with ColumnId " + foreignKeyMapping.ToColumnId);
                                //Console.WriteLine("Could not find toColumn with ColumnId " + foreignKeyMapping.ToColumnId);
                                break;
                            }

                            Property toProperty;
                            if (!columnIdToProperty.TryGetValue(toColumn.Id, out toProperty))
                            {
                                _logger.WriteError("Could not find mapping to a Property for ColumnId " + toColumn.Id);
                                //Console.WriteLine("Could not find mapping to a Property for ColumnId " + toColumn.Id);
                                break;
                            }

                            fromProperty.AddAnnotation(GetForeignKeyTargetPropertyAnnotationName(foreignKeyConstraintId), toProperty.Name);
                            fromProperty.AddAnnotation(GetForeignKeyTargetEntityTypeAnnotationName(foreignKeyConstraintId), toProperty.EntityType.Name);
                        }
                    }
                }
            }

            return model;
        }

        public static string[] SplitString(char[] delimiters, string input)
        {
            var output = new List<string>();

            var workingString = input;
            int firstIndex = -1;
            do {
                firstIndex = workingString.IndexOfAny(delimiters);
                if (firstIndex < 0)
                {
                    // Console.WriteLine("FirstIndex = " + firstIndex + ". Output = >>>" + workingString + "<<<");
                    output.Add(workingString);
                }
                else
                {
                    // Console.WriteLine("FirstIndex = " + firstIndex + ". Output = >>>" + workingString.Substring(0, firstIndex) + "<<<");
                    output.Add(workingString.Substring(0, firstIndex));
                }
                workingString = workingString.Substring(firstIndex + 1);
                // Console.WriteLine("workingString = >>>" + workingString + "<<<");
            }
            while (firstIndex >= 0 && !string.IsNullOrEmpty(workingString));

            return output.ToArray();
        }

        public static string GetForeignKeyOrdinalPositionAnnotationName(string foreignKeyConstraintId)
        {
            return GetForeignKeyAnnotationName(AnnotationDescriptorForeignKeyOrdinal, foreignKeyConstraintId);
        }


        public static string GetForeignKeyTargetPropertyAnnotationName(string foreignKeyConstraintId)
        {
            return GetForeignKeyAnnotationName(AnnotationDescriptorForeignKeyTargetProperty, foreignKeyConstraintId);
        }

        public static string GetForeignKeyTargetEntityTypeAnnotationName(string foreignKeyConstraintId)
        {
            return GetForeignKeyAnnotationName(AnnotationDescriptorForeignKeyTargetEntityType, foreignKeyConstraintId);
        }

        public static string GetForeignKeyAnnotationName(string descriptor, string foreignKeyConstraintId)
        {
            return string.Format(AnnotationFormatForeignKey, foreignKeyConstraintId, descriptor);
        }

        public static void ApplyPropertyProperties(Property property, TableColumn tc)
        {
            property.IsNullable = tc.IsNullable;
            property.AddAnnotation(AnnotationNameIsNullable, tc.IsNullable.ToString());
            property.MaxLength = tc.MaxLength == -1 ? null : tc.MaxLength;
            if (property.MaxLength != null)
            {
                property.AddAnnotation(AnnotationNameMaxLength, property.MaxLength.Value.ToString());
            }
            if (tc.NumericPrecision.HasValue)
            {
                property.AddAnnotation(AnnotationNamePrecision, tc.NumericPrecision.Value.ToString());
            }
            if (tc.DateTimePrecision.HasValue)
            {
                property.AddAnnotation(AnnotationNamePrecision, tc.DateTimePrecision.Value.ToString());
            }
            if (tc.Scale.HasValue)
            {
                property.AddAnnotation(AnnotationNameScale, tc.Scale.Value.ToString());
            }
            if (tc.IsIdentity)
            {
                property.AddAnnotation(AnnotationNameIsIdentity, tc.IsIdentity.ToString());
            }
            property.IsStoreComputed = tc.IsStoreGenerated;
            if (tc.DefaultValue != null)
            {
                property.UseStoreDefault = true;
            }
        }

        public DbContextCodeGeneratorContext GetContextModelCodeGenerator(ContextTemplateModel contextTemplateModel)
        {
            return new SqlServerDbContextCodeGeneratorContext(
                contextTemplateModel.MetadataModel
                , contextTemplateModel.Namespace
                , contextTemplateModel.ClassName
                , contextTemplateModel.ConnectionString);
        }
        public EntityTypeCodeGeneratorContext GetEntityTypeModelCodeGenerator(
            EntityTypeTemplateModel entityTypeTemplateModel, DbContextCodeGeneratorContext dbContextCodeGeneratorContext)
        {
            return new SqlServerEntityTypeCodeGeneratorContext(
                entityTypeTemplateModel.EntityType
                , entityTypeTemplateModel.Namespace
                , dbContextCodeGeneratorContext);
        }
    }
}