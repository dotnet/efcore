// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.ReverseEngineering.Model;
using Microsoft.Data.Entity.SqlServer.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer.ReverseEngineering
{
    public class SqlServerMetadataModelProvider : IDatabaseMetadataModelProvider
    {
        private List<string> DataTypesForMax = new List<string>() { "varchar", "nvarchar", "varbinary" };
        private List<string> DataTypesForPrecisionAndScale = new List<string>() { "decimal", "numeric" };

        public static readonly string AnnotationPrefix = "SqlServerMetadataModelProvider:";
        public static readonly string AnnotationNameDependentEndNavPropName = AnnotationPrefix + "DependentEndNavPropName";
        public static readonly string AnnotationNamePrincipalEndNavPropName = AnnotationPrefix + "PrincipalEndNavPropName";

        private ILogger _logger;

        // data loaded from database
        private Dictionary<string, Table> _tables;
        private Dictionary<string, TableColumn> _tableColumns;
        private Dictionary<string, TableConstraintColumn> _tableConstraintColumns;
        private Dictionary<string, ForeignKeyColumnMapping> _foreignKeyColumnMappings;

        // dictionaries constructed from database data
        private Dictionary<string, int> _primaryKeyOrdinals = new Dictionary<string, int>();
        private Dictionary<string, Dictionary<string, int>> _foreignKeyOrdinals =
            new Dictionary<string, Dictionary<string, int>>();
        private Dictionary<IEntityType, string> _entityTypeToClassNameMap = new Dictionary<IEntityType, string>();
        private Dictionary<IProperty, string> _propertyToPropertyNameMap = new Dictionary<IProperty, string>();
        private Dictionary<EntityType, EntityType> _relationalEntityTypeToCodeGenEntityTypeMap =
            new Dictionary<EntityType, EntityType>();
        private Dictionary<string, Property> _relationalColumnIdToRelationalPropertyMap = new Dictionary<string, Property>();
        private Dictionary<Property, Property> _relationalPropertyToCodeGenPropertyMap = new Dictionary<Property, Property>();
        private Dictionary<EntityType, Dictionary<string, List<Property>>> _relationalEntityTypeToForeignKeyConstraintsMap =
            new Dictionary<EntityType, Dictionary<string, List<Property>>>(); // string is ConstraintId

        public SqlServerMetadataModelProvider([NotNull] IServiceProvider serviceProvider)
        {
            _logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
        }

        public IModel GenerateMetadataModel(string connectionString, string filters)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    _tables = LoadData<Table>(conn, Table.Query, Table.CreateFromReader, t => t.Id);
                    _tableColumns = LoadData<TableColumn>(conn, TableColumn.Query, TableColumn.CreateFromReader, tc => tc.Id);
                    _tableConstraintColumns = LoadData<TableConstraintColumn>(
                        conn, TableConstraintColumn.Query, TableConstraintColumn.CreateFromReader, tcc => tcc.Id);
                    _foreignKeyColumnMappings = LoadData<ForeignKeyColumnMapping>(
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

            CreatePrimaryAndForeignKeyMaps();

            return CreateModel();
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
        public void CreatePrimaryAndForeignKeyMaps()
        {
            foreach (var tableConstraintColumn in _tableConstraintColumns.Values)
            {
                if (tableConstraintColumn.ConstraintType == "PRIMARY KEY")
                {
                    _primaryKeyOrdinals.Add(tableConstraintColumn.ColumnId, tableConstraintColumn.Ordinal);
                }
                else if (tableConstraintColumn.ConstraintType == "FOREIGN KEY")
                {
                    Dictionary<string, int> constraintNameToOrdinalMap;
                    if (!_foreignKeyOrdinals.TryGetValue(tableConstraintColumn.ColumnId, out constraintNameToOrdinalMap))
                    {
                        constraintNameToOrdinalMap = new Dictionary<string, int>();
                        _foreignKeyOrdinals[tableConstraintColumn.ColumnId] = constraintNameToOrdinalMap;
                    }

                    constraintNameToOrdinalMap[tableConstraintColumn.ConstraintId] = tableConstraintColumn.Ordinal;
                }
                else
                {
                    _logger.WriteInformation("Unknown Constraint Type for " + tableConstraintColumn);
                }
            }
        }

        public IModel CreateModel()
        {
            // the relationalModel is an IModel, but not the one that will be returned
            // it's just directly from the database - EntiyType = table, Property = column
            // etc with no attempt to hook up foreign key columns or make the
            // names fit CSharp conventions etc.
            var relationalModel = ConstructRelationalModel();

            // construct maps mapping of relationalModel's IEntityTypes to the names they will have in the CodeGen Model
            var nameMapper = new SqlServerNameMapper(
                relationalModel,
                entity => _tables[entity.Name].TableName,
                property => _tableColumns[property.Name].ColumnName);

            // create all codeGenModel EntityTypes and Properties
            return ConstructCodeGenModel(relationalModel, nameMapper);
        }

        private IModel ConstructRelationalModel()
        {
            var relationalModel = new Microsoft.Data.Entity.Metadata.Model();
            foreach (var table in _tables.Values)
            {
                relationalModel.AddEntityType(table.Id);
            }

            foreach (var tc in _tableColumns.Values)
            {
                var entityType = relationalModel.TryGetEntityType(tc.TableId);
                if (entityType == null)
                {
                    _logger.WriteError("Could not find table with TableId " + tc.TableId);
                    continue;
                }

                // IModel will not allow Properties to be created without a Type, so map to CLR type here.
                // This means if we come across a column with a SQL Server type which we can't map we will ignore it.
                // Note: foreign key properties appear just like any other property in the relational model.
                Type clrPropertyType;
                if (!SqlServerTypeMapping._sqlTypeToClrTypeMap.TryGetValue(tc.DataType, out clrPropertyType))
                {
                    // _logger.WriteError("Could not find type mapping for SQL Server type " + tc.DataType);
                    continue;
                }

                if (tc.IsNullable)
                {
                    clrPropertyType = clrPropertyType.MakeNullable();
                }

                var relationalProperty = entityType.AddProperty(tc.Id, clrPropertyType, shadowProperty: true);
                _relationalColumnIdToRelationalPropertyMap[tc.Id] = relationalProperty;
            }

            return relationalModel;
        }

        private IModel ConstructCodeGenModel(
            IModel relationalModel, SqlServerNameMapper nameMapper)
        {
            var codeGenModel = new Microsoft.Data.Entity.Metadata.Model();
            foreach (var relationalEntityType in relationalModel.EntityTypes.Cast<EntityType>())
            {
                var codeGenEntityType = codeGenModel.AddEntityType(nameMapper.EntityTypeToClassNameMap[relationalEntityType]);
                _relationalEntityTypeToCodeGenEntityTypeMap[relationalEntityType] = codeGenEntityType;
                codeGenEntityType.SqlServer().Table = _tables[relationalEntityType.Name].TableName;
                codeGenEntityType.SqlServer().Schema = _tables[relationalEntityType.Name].SchemaName;

                // Loop over properties in each relational EntityType.
                // If property is part of a foreign key (and not part of a primary key)
                // add to list of constraints to be added in later.
                // Otherwise construct matching property.
                var primaryKeyProperties = new List<Property>();
                var constraints = new Dictionary<string, List<Property>>();
                _relationalEntityTypeToForeignKeyConstraintsMap[relationalEntityType] = constraints;
                foreach (var relationalProperty in relationalEntityType.Properties)
                {
                    int primaryKeyOrdinal;
                    if (_primaryKeyOrdinals.TryGetValue(relationalProperty.Name, out primaryKeyOrdinal))
                    {
                        // add _relational_ property so we can order on the ordinal later
                        primaryKeyProperties.Add(relationalProperty);
                    }

                    Dictionary<string, int> foreignKeyConstraintIdOrdinalMap;
                    if (_foreignKeyOrdinals.TryGetValue(relationalProperty.Name, out foreignKeyConstraintIdOrdinalMap))
                    {
                        // relationalProperty represents (part of) a foreign key 
                        foreach (var constraintId in foreignKeyConstraintIdOrdinalMap.Keys)
                        {
                            List<Property> constraintProperties;
                            if (!constraints.TryGetValue(constraintId, out constraintProperties))
                            {
                                constraintProperties = new List<Property>();
                                constraints.Add(constraintId, constraintProperties);
                            }
                            constraintProperties.Add(relationalProperty);
                        }
                    }

                    var codeGenProperty = codeGenEntityType.AddProperty(
                        nameMapper.PropertyToPropertyNameMap[relationalProperty],
                        relationalProperty.PropertyType,
                        shadowProperty: true);
                    _relationalPropertyToCodeGenPropertyMap[relationalProperty] = codeGenProperty;
                    ApplyPropertyProperties(codeGenProperty, _tableColumns[relationalProperty.Name]);
                } // end of loop over all relational properties for given EntityType

                if (primaryKeyProperties.Count() > 0)
                {
                    // order the relational properties by their primaryKeyOrdinal, then return a list
                    // of the codeGen properties mapped to each relational property in that order
                    codeGenEntityType.SetPrimaryKey(
                        primaryKeyProperties
                        .OrderBy(p => _primaryKeyOrdinals[p.Name]) // note: for relational property p.Name is its columnId
                        .Select(p => _relationalPropertyToCodeGenPropertyMap[p])
                        .ToList());
                }
            } // end of loop over all relational EntityTypes

            AddForeignKeysToCodeGenModel(codeGenModel);

            return codeGenModel;
        }

        private void AddForeignKeysToCodeGenModel(IModel codeGenModel)
        {
            foreach (var keyValuePair in _relationalEntityTypeToForeignKeyConstraintsMap)
            {
                var fromColumnRelationalEntityType = keyValuePair.Key;
                var codeGenEntityType = _relationalEntityTypeToCodeGenEntityTypeMap[fromColumnRelationalEntityType];
                foreach (var foreignKeyConstraintMap in keyValuePair.Value)
                {
                    var foreignKeyConstraintId = foreignKeyConstraintMap.Key;
                    var foreignKeyConstraintRelationalPropertyList = foreignKeyConstraintMap.Value;

                    var targetRelationalProperty = FindTargetColumn(
                        _foreignKeyColumnMappings,
                        _tableColumns,
                        _relationalColumnIdToRelationalPropertyMap,
                        foreignKeyConstraintId,
                        foreignKeyConstraintRelationalPropertyList[0].Name);
                    if (targetRelationalProperty != null)
                    {
                        var targetRelationalEntityType = targetRelationalProperty.EntityType;
                        var targetCodeGenEntityType = _relationalEntityTypeToCodeGenEntityTypeMap[targetRelationalEntityType];
                        var targetPrimaryKey = targetCodeGenEntityType.GetPrimaryKey();

                        // to construct foreign key need the properties representing the foreign key columns in the codeGen model
                        // in the order they appear in the target's key
                        var foreignKeyCodeGenProperties =
                            foreignKeyConstraintRelationalPropertyList
                                .OrderBy(p => _foreignKeyOrdinals[p.Name][foreignKeyConstraintId]) // relational property's name is the columnId
                                .Select(p =>
                                    {
                                        Property codeGenProperty;
                                        return _relationalPropertyToCodeGenPropertyMap.TryGetValue(p, out codeGenProperty)
                                            ? codeGenProperty : null; 
                                    })
                                .ToList();

                        //TODO: SQL Server allows more than 1 foreign key on the same set of properties
                        // how should we react?
                        var codeGenForeignKey = codeGenEntityType.GetOrAddForeignKey(foreignKeyCodeGenProperties, targetPrimaryKey);
                        //TODO: make ForeignKey unique based on constraints
                        //TODO: what if multiple Navs to same target?
                    }
                }
            }

            AddDependentAndPrincipalNavigationPropertyAnnotations(codeGenModel);
        }

        private void AddDependentAndPrincipalNavigationPropertyAnnotations(IModel codeGenModel)
        {
            var entityTypeToExistingIdentifiers = new Dictionary<IEntityType, List<string>>();
            foreach (var entityType in codeGenModel.EntityTypes)
            {
                var existingIdentifiers = new List<string>();
                entityTypeToExistingIdentifiers.Add(entityType, existingIdentifiers);
                existingIdentifiers.Add(entityType.Name);
                existingIdentifiers.AddRange(entityType.Properties.Select(p => p.Name));
            }

            foreach (var entityType in codeGenModel.EntityTypes)
            {
                var dependentEndExistingIdentifiers = entityTypeToExistingIdentifiers[entityType];
                foreach (var foreignKey in entityType.ForeignKeys.Cast<ForeignKey>())
                {
                    // set up the name of the navigation property on the dependent end of the foreign key
                    var dependentEndNavigationPropertyName = CSharpUtilities.Instance.GenerateCSharpIdentifier(
                        foreignKey.ReferencedEntityType.Name, dependentEndExistingIdentifiers);
                    foreignKey.AddAnnotation(AnnotationNameDependentEndNavPropName, dependentEndNavigationPropertyName);
                    dependentEndExistingIdentifiers.Add(dependentEndNavigationPropertyName);

                    if (foreignKey.ReferencedEntityType != foreignKey.EntityType) // if not self-referencing
                    {
                        // set up the name of the navigation property on the principal end of the foreign key
                        var principalEndExistingIdentifiers = entityTypeToExistingIdentifiers[foreignKey.ReferencedEntityType];
                        var principalEndNavigationPropertyName = CSharpUtilities.Instance.GenerateCSharpIdentifier(
                            entityType.Name, principalEndExistingIdentifiers);
                        foreignKey.AddAnnotation(AnnotationNamePrincipalEndNavPropName, principalEndNavigationPropertyName);
                        principalEndExistingIdentifiers.Add(principalEndNavigationPropertyName);
                    }
                }
            }
        }

        private Property FindTargetColumn(
            Dictionary<string, ForeignKeyColumnMapping> foreignKeyColumnMappings,
            Dictionary<string, TableColumn> tableColumns,
            Dictionary<string, Property> relationalColumnIdToRelationalPropertyMap,
            string foreignKeyConstraintId,
            string fromColumnId)
        {
            ForeignKeyColumnMapping foreignKeyColumnMapping;
            if (!foreignKeyColumnMappings.TryGetValue(
                foreignKeyConstraintId + fromColumnId, out foreignKeyColumnMapping))
            {
                //_logger.WriteError("Could not find foreignKeyMapping for ConstraintId " + foreignKeyConstraintId
                //    + " FromColumn " + fromColumnId);
                return null;
            }

            TableColumn toColumn;
            if (!tableColumns.TryGetValue(foreignKeyColumnMapping.ToColumnId, out toColumn))
            {
                //_logger.WriteError("Could not find toColumn with ColumnId " + foreignKeyColumnMapping.ToColumnId);
                return null;
            }

            Property toColumnRelationalProperty;
            if (!relationalColumnIdToRelationalPropertyMap.TryGetValue(toColumn.Id, out toColumnRelationalProperty))
            {
                //_logger.WriteError("Could not find relational property for toColumn with ColumnId " + toColumn.Id);
                return null;
            }

            return toColumnRelationalProperty;
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

        public void ApplyPropertyProperties(Property property, TableColumn tc)
        {
            if (property.Name != tc.ColumnName)
            {
                property.SqlServer().Column = tc.ColumnName;
            }

            //TODO - only apply attribute when necessary
            string columnType = tc.DataType;
            if (tc.MaxLength.HasValue)
            {
                if (tc.MaxLength > 0)
                {
                    property.MaxLength = tc.MaxLength;
                }

                if (tc.MaxLength.Value >= Int32.MaxValue / 2
                    && DataTypesForMax.Contains(columnType))
                {
                    columnType += "(max)";
                }
                else
                {
                    columnType += "(" + tc.MaxLength + ")";
                }
            }
            else if (DataTypesForPrecisionAndScale.Contains(tc.DataType))
            {
                if (tc.NumericPrecision.HasValue)
                {
                    if (tc.Scale.HasValue)
                    {
                        columnType += "(" + tc.NumericPrecision.Value + ", " + tc.Scale.Value + ")";
                    }
                    else
                    {
                        columnType += "(" + tc.NumericPrecision.Value + ")";
                    }
                }
            }

            property.SqlServer().ColumnType = columnType;

            if (tc.IsIdentity)
            {
                property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Identity;
            }
            if (tc.IsStoreGenerated)
            {
                property.IsStoreComputed = tc.IsStoreGenerated;
            }
            if (tc.DefaultValue != null)
            {
                property.SqlServer().DefaultValue = tc.DefaultValue;
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