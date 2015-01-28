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
using Microsoft.Data.Entity.SqlServer.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer.ReverseEngineering
{
    public class SqlServerMetadataModelProvider : IDatabaseMetadataModelProvider
    {
        public static readonly string AnnotationNameTableId = "TableId";
        public static readonly string AnnotationNameTableIdSchemaTableSeparator = ".";
        public static readonly string AnnotationNameColumnId = "ColumnId";
        public static readonly string AnnotationNameColumnName = "ColumnName";
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

            //_logger.WriteInformation("Tables");
            //foreach (var t in tables)
            //{
            //    var table = t.Value;
            //    _logger.WriteInformation(table.ToString());
            //}

            //_logger.WriteInformation(Environment.NewLine + "Columns");
            //foreach (var tc in tableColumns)
            //{
            //    _logger.WriteInformation(tc.Value.ToString());
            //}

            //_logger.WriteInformation(Environment.NewLine + "Constraint Columns");
            //foreach (var tc in tableConstraintColumns)
            //{
            //    _logger.WriteInformation(tc.Value.ToString());
            //}

            //_logger.WriteInformation(Environment.NewLine + "Foreign Key Column Mappings");
            //foreach (var fkcm in foreignKeyColumnMappings)
            //{
            //    _logger.WriteInformation(fkcm.Value.ToString());
            //}

            Dictionary<string, int> primaryKeyOrdinals;
            Dictionary<string, Dictionary<string, int>> foreignKeyOrdinals;
            CreatePrimaryAndForeignKeyMaps(
                tableConstraintColumns, out primaryKeyOrdinals, out foreignKeyOrdinals);

            return CreateModel(tables, tableColumns,
                primaryKeyOrdinals, foreignKeyOrdinals, foreignKeyColumnMappings);
        }

        public static Dictionary<string, T> LoadData<T>(
            SqlConnection conn, string query,
            Func<SqlDataReader, T> createFromReader, Func<T, string> identifier)
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

        /// <summary>
        /// Output two Dictionaries
        /// 
        /// The first one maps (for all primary keys)
        ///   ColumnId -> Ordinal at which that column appears in the primary key for the table in which it is defined
        /// 
        /// The second one maps (for all foreign keys)
        ///   ColumnId -> a Dictionary which maps ConstraintId -> Ordinal at which that Column appears within that FK constraint
        /// 
        /// </summary>
        /// <returns></returns>
        public void CreatePrimaryAndForeignKeyMaps(
            Dictionary<string, TableConstraintColumn> tableConstraintColumns,
            out Dictionary<string, int> primaryKeyOrdinals,
            out Dictionary<string, Dictionary<string, int>> foreignKeyOrdinals)
        {
            primaryKeyOrdinals = new Dictionary<string, int>();
            foreignKeyOrdinals = new Dictionary<string, Dictionary<string, int>>();

            foreach (var tableConstraintColumn in tableConstraintColumns.Values)
            {
                if (tableConstraintColumn.ConstraintType == "PRIMARY KEY")
                {
                    primaryKeyOrdinals.Add(tableConstraintColumn.ColumnId, tableConstraintColumn.Ordinal);
                }
                else if (tableConstraintColumn.ConstraintType == "FOREIGN KEY")
                {
                    Dictionary<string, int> constraintNameToOrdinalMap;
                    if (!foreignKeyOrdinals.TryGetValue(tableConstraintColumn.ColumnId, out constraintNameToOrdinalMap))
                    {
                        constraintNameToOrdinalMap = new Dictionary<string, int>();
                        foreignKeyOrdinals[tableConstraintColumn.ColumnId] = constraintNameToOrdinalMap;
                    }

                    constraintNameToOrdinalMap[tableConstraintColumn.ConstraintId] = tableConstraintColumn.Ordinal;
                }
                else
                {
                    _logger.WriteInformation("Unknown Constraint Type for " + tableConstraintColumn);
                }
            }
        }

        public IModel CreateModel(
            Dictionary<string, Table> tables,
            Dictionary<string, TableColumn> tableColumns,
            Dictionary<string, int> primaryKeyOrdinals,
            Dictionary<string, Dictionary<string, int>> foreignKeyOrdinals,
            Dictionary<string, ForeignKeyColumnMapping> foreignKeyColumnMappings)
        {
            var columnIdToProperty = new Dictionary<string, Property>();
            var propertyToForeignKeyConstraintIds = new Dictionary<Property, List<string>>();

            var model = new Microsoft.Data.Entity.Metadata.Model();
            foreach (var table in tables.Values)
            {
                var entityTypeName =
                    table.SchemaName
                    + AnnotationNameTableIdSchemaTableSeparator
                    + table.TableName;
                var entityType = model.AddEntityType(entityTypeName);
                entityType.AddAnnotation(AnnotationNameTableId, table.Id);

                var primaryKeys = new List<Property>();
                var foreignKeys = new Dictionary<string, List<Property>>();
                foreach (var tc in tableColumns.Values.Where(col => col.TableId == table.Id))
                {
                    Type clrPropertyType;
                    if (!SqlServerTypeMapping._sqlTypeToClrTypeMap.TryGetValue(tc.DataType, out clrPropertyType))
                    {
                        // _logger.WriteInformation("Could not find type mapping for SQL Server type " + tc.DataType);
                    }
                    else
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
                        int primaryKeyOrdinal;
                        if (!primaryKeyOrdinals.TryGetValue(tc.Id, out primaryKeyOrdinal))
                        {
                            //TODO - only if _logger != null
                            // _logger.WriteInformation("Could not find PKOrdinal mapping for " + tc);
                        }
                        else
                        {
                            primaryKeys.Add(property);
                            property.AddAnnotation(AnnotationNamePrimaryKeyOrdinal, primaryKeyOrdinal.ToString());
                        }


                        // make column a foreign key if it appears in an FK constraint
                        Dictionary<string, int> constraintIdOrdinalKeyValuePairMap;
                        if (!foreignKeyOrdinals.TryGetValue(tc.Id, out constraintIdOrdinalKeyValuePairMap))
                        {
                            //TODO - only if _logger != null
                            // _logger.WriteInformation("Could not find FKOrdinal mapping for " + tc);
                        }
                        else
                        {
                            // store in which constraints this column appears
                            foreach (var keyValuePair in constraintIdOrdinalKeyValuePairMap)
                            {
                                var constraintId = keyValuePair.Key;
                                var ordinal = keyValuePair.Value;
                                List<string> constraintIds;
                                if (!propertyToForeignKeyConstraintIds.TryGetValue(property, out constraintIds))
                                {
                                    constraintIds = new List<string>();
                                    propertyToForeignKeyConstraintIds[property] = constraintIds;
                                }
                                if (!constraintIds.Contains(constraintId))
                                {
                                    constraintIds.Add(constraintId);
                                }

                                property.AddAnnotation(
                                    GetForeignKeyOrdinalPositionAnnotationName(constraintId),
                                    ordinal.ToString());
                            }
                        }

                        ApplyPropertyProperties(property, tc);
                    }
                } // end of loop over all columns for this table

                entityType.SetPrimaryKey(primaryKeys);
            } // end of loop over tables

            // loop over all properties adding TargetEntityType and TargetProperty for ForeignKeys
            // this has to be done after all EntityTypes and their Properties have been created
            foreach (var fromEntityTpe in model.EntityTypes)
            {
                foreach (var fromProperty in fromEntityTpe.Properties)
                {
                    List<string> foreignKeyConstraintIds;
                    if (propertyToForeignKeyConstraintIds.TryGetValue(fromProperty, out foreignKeyConstraintIds))
                    {
                        fromProperty.AddAnnotation(AnnotationNameForeignKeyConstraints,
                            string.Join(AnnotationFormatForeignKeyConstraintSeparator, foreignKeyConstraintIds));

                        foreach (var foreignKeyConstraintId in foreignKeyConstraintIds)
                        {
                            var columnId = fromProperty[AnnotationNameColumnId];
                            ForeignKeyColumnMapping foreignKeyMapping;
                            if (!foreignKeyColumnMappings.TryGetValue(foreignKeyConstraintId + columnId, out foreignKeyMapping))
                            {
                                _logger.WriteError("Could not find foreignKeyMapping for ConstraintId " + foreignKeyConstraintId + " FromColumn " + columnId);
                                break;
                            }

                            TableColumn toColumn;
                            if (!tableColumns.TryGetValue(foreignKeyMapping.ToColumnId, out toColumn))
                            {
                                _logger.WriteError("Could not find toColumn with ColumnId " + foreignKeyMapping.ToColumnId);
                                break;
                            }

                            Property toProperty;
                            if (!columnIdToProperty.TryGetValue(toColumn.Id, out toProperty))
                            {
                                _logger.WriteError("Could not find mapping to a Property for ColumnId " + toColumn.Id);
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

        //TODO - this works around the fact that string.Split() does not exist in ASPNETCORE50
        public static string[] SplitString(char[] delimiters, string input)
        {
            var output = new List<string>();

            var workingString = input;
            int firstIndex = -1;
            do
            {
                firstIndex = workingString.IndexOfAny(delimiters);
                if (firstIndex < 0)
                {
                    output.Add(workingString);
                }
                else
                {
                    output.Add(workingString.Substring(0, firstIndex));
                }
                workingString = workingString.Substring(firstIndex + 1);
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

        public DbContextCodeGenerator GetContextModelCodeGenerator(ReverseEngineeringGenerator generator, DbContextGeneratorModel dbContextGeneratorModel)
        {
            return new SqlServerDbContextCodeGeneratorContext(
                generator
                , dbContextGeneratorModel.MetadataModel
                , dbContextGeneratorModel.Namespace
                , dbContextGeneratorModel.ClassName
                , dbContextGeneratorModel.ConnectionString);
        }
        public EntityTypeCodeGenerator GetEntityTypeModelCodeGenerator(
            ReverseEngineeringGenerator generator, EntityTypeGeneratorModel entityTypeGeneratorModel)
        {
            return new SqlServerEntityTypeCodeGeneratorContext(
                generator
                , entityTypeGeneratorModel.EntityType
                , entityTypeGeneratorModel.Namespace);
        }
    }
}