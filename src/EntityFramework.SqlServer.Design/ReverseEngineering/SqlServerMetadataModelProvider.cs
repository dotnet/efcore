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
using Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering.Model;
using Microsoft.Data.Entity.SqlServer.Design.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerMetadataModelProvider : IDatabaseMetadataModelProvider
    {
        private static readonly List<string> DataTypesForMax = new List<string>() { "varchar", "nvarchar", "varbinary" };
        private static readonly List<string> DataTypesForPrecisionAndScale = new List<string>() { "decimal", "numeric" };

        public static readonly string AnnotationPrefix = "SqlServerMetadataModelProvider:";
        public static readonly string AnnotationNameDependentEndNavPropName = AnnotationPrefix + "DependentEndNavPropName";
        public static readonly string AnnotationNamePrincipalEndNavPropName = AnnotationPrefix + "PrincipalEndNavPropName";
        public static readonly string AnnotationNamePrincipalEntityTypeError = AnnotationPrefix + "EntityTypeError";

        private ILogger _logger;

        // data loaded directly from database
        private Dictionary<string, Table> _tables;
        private Dictionary<string, TableColumn> _tableColumns;
        private Dictionary<string, ForeignKeyColumnMapping> _foreignKeyColumnMappings;

        // primary key, foreign key and unique columns information constructed from database data
        private Dictionary<string, int> _primaryKeyOrdinals = new Dictionary<string, int>();
        private Dictionary<string, Dictionary<string, int>> _foreignKeyOrdinals =
            new Dictionary<string, Dictionary<string, int>>(); // 1st string is ColumnId, 2nd is ConstraintId
        private HashSet<string> _uniqueConstraintColumns = new HashSet<string>();

        // utility data constructed as we iterate over the data
        private Dictionary<EntityType, EntityType> _relationalEntityTypeToCodeGenEntityTypeMap =
            new Dictionary<EntityType, EntityType>();
        private Dictionary<Property, Property> _relationalPropertyToCodeGenPropertyMap = new Dictionary<Property, Property>();
        private Dictionary<string, Property> _relationalColumnIdToRelationalPropertyMap = new Dictionary<string, Property>();
        private Dictionary<EntityType, Dictionary<string, List<Property>>> _relationalEntityTypeToForeignKeyConstraintsMap =
            new Dictionary<EntityType, Dictionary<string, List<Property>>>(); // string is ConstraintId

        public SqlServerMetadataModelProvider([NotNull] IServiceProvider serviceProvider)
        {
            _logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
            if (_logger == null)
            {
                throw new ArgumentException(typeof(SqlServerMetadataModelProvider).Name + " cannot find a service of type " + typeof(ILogger).Name);
            }
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
                    _foreignKeyColumnMappings = LoadData<ForeignKeyColumnMapping>(
                        conn, ForeignKeyColumnMapping.Query, ForeignKeyColumnMapping.CreateFromReader, fkcm => fkcm.Id);

                    var tableConstraintColumns = LoadData<TableConstraintColumn>(
                        conn, TableConstraintColumn.Query, TableConstraintColumn.CreateFromReader, tcc => tcc.Id);
                    CreatePrimaryForeignKeyAndUniqueMaps(tableConstraintColumns);
                }
                finally
                {
                    if (conn != null && conn.State == ConnectionState.Open)
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

            return CreateModel();
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

        public void CreatePrimaryForeignKeyAndUniqueMaps(Dictionary<string, TableConstraintColumn> tableConstraintColumns)
        {
            var uniqueConstraintToColumnsMap = new Dictionary<string, List<string>>();

            foreach (var tableConstraintColumn in tableConstraintColumns.Values)
            {
                if (tableConstraintColumn.ConstraintType == "PRIMARY KEY"
                    || tableConstraintColumn.ConstraintType == "UNIQUE")
                {
                    if (tableConstraintColumn.ConstraintType == "PRIMARY KEY")
                    {
                        _primaryKeyOrdinals.Add(tableConstraintColumn.ColumnId, tableConstraintColumn.Ordinal);
                    }

                    // add this column to the list of columns for this unique constraint
                    List<string> columnIds;
                    if (!uniqueConstraintToColumnsMap.TryGetValue(tableConstraintColumn.ConstraintId, out columnIds))
                    {
                        columnIds = new List<string>();
                        uniqueConstraintToColumnsMap[tableConstraintColumn.ConstraintId] = columnIds;
                    }

                    if (!columnIds.Contains(tableConstraintColumn.ColumnId))
                    {
                        columnIds.Add(tableConstraintColumn.ColumnId);
                    }
                }
                else if (tableConstraintColumn.ConstraintType == "FOREIGN KEY")
                {
                    // add this column to the list of columns for this foreign key constraint
                    Dictionary<string, int> constraintNameToOrdinalMap;
                    if (!_foreignKeyOrdinals.TryGetValue(tableConstraintColumn.ColumnId, out constraintNameToOrdinalMap))
                    {
                        constraintNameToOrdinalMap = new Dictionary<string, int>();
                        _foreignKeyOrdinals[tableConstraintColumn.ColumnId] = constraintNameToOrdinalMap;
                    }

                    constraintNameToOrdinalMap[tableConstraintColumn.ConstraintId] = tableConstraintColumn.Ordinal;
                }
            }

            // store the ordered list of columns for each unique constraint
            foreach (var uniqueConstraintColumnIds in uniqueConstraintToColumnsMap.Values)
            {
                var columnsCombinationId = ConstructIdForCombinationOfColumns(uniqueConstraintColumnIds);
                if (!_uniqueConstraintColumns.Contains(columnsCombinationId))
                {
                    _uniqueConstraintColumns.Add(columnsCombinationId);
                }
            }
        }

        public IModel CreateModel()
        {
            // the relationalModel is an IModel, but not the one that will be returned
            // it's just directly from the database - EntityType = table, Property = column
            // etc with no attempt to hook up foreign key columns or make the
            // names fit CSharp conventions etc.
            var relationalModel = ConstructRelationalModel();

            var nameMapper = new SqlServerNameMapper(
                relationalModel,
                entity => _tables[entity.Name].TableName,
                property => _tableColumns[property.Name].ColumnName);

            return ConstructCodeGenModel(relationalModel, nameMapper);
        }

        public IModel ConstructRelationalModel()
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
                    _logger.WriteWarning("For columnId " + tc.Id
                        + "Could not find table with TableId " + tc.TableId + ". Skipping column.");
                    continue;
                }

                // IModel will not allow Properties to be created without a Type, so map to CLR type here.
                // This means if we come across a column with a SQL Server type which we can't map we will ignore it.
                // Note: foreign key properties appear just like any other property in the relational model.
                Type clrPropertyType;
                if (!SqlServerTypeMapping._sqlTypeToClrTypeMap.TryGetValue(tc.DataType, out clrPropertyType))
                {
                    _logger.WriteWarning("For columnId: " + tc.Id
                        + " Could not find type mapping for SQL Server type " + tc.DataType + ". Skipping column.");
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

        public IModel ConstructCodeGenModel(
            IModel relationalModel, SqlServerNameMapper nameMapper)
        {
            var codeGenModel = new Microsoft.Data.Entity.Metadata.Model();
            foreach (var relationalEntityType in relationalModel.EntityTypes.Cast<EntityType>())
            {
                var codeGenEntityType = codeGenModel
                    .AddEntityType(nameMapper.EntityTypeToClassNameMap[relationalEntityType]);
                _relationalEntityTypeToCodeGenEntityTypeMap[relationalEntityType] = codeGenEntityType;
                codeGenEntityType.SqlServer().Table = _tables[relationalEntityType.Name].TableName;
                codeGenEntityType.SqlServer().Schema = _tables[relationalEntityType.Name].SchemaName;

                // Loop over relational properties constructing a matching property in the 
                // codeGenModel. Also accumulate:
                //    a) primary key properties
                //    b) constraint properties
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
                else
                {
                    codeGenEntityType.AddAnnotation(
                        AnnotationNamePrincipalEntityTypeError, "Attempt to generate EntityType " + codeGenEntityType.Name
                        + " failed. We could identify no primary key columns in the underlying SQL Server table "
                        + _tables[relationalEntityType.Name].SchemaName + "." + _tables[relationalEntityType.Name].TableName + ".");
                }
            } // end of loop over all relational EntityTypes

            AddForeignKeysToCodeGenModel(codeGenModel);

            return codeGenModel;
        }

        public void AddForeignKeysToCodeGenModel(IModel codeGenModel)
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
                                .OrderBy(relationalProperty =>
                                    _foreignKeyOrdinals[relationalProperty.Name][foreignKeyConstraintId]) // relational property's name is the columnId
                                .Select(relationalProperty =>
                                    {
                                        Property codeGenProperty;
                                        return _relationalPropertyToCodeGenPropertyMap
                                            .TryGetValue(relationalProperty, out codeGenProperty)
                                                ? codeGenProperty
                                                : null; 
                                    })
                                .ToList();

                        //TODO: SQL Server allows more than 1 foreign key on the same set of properties
                        // how should we react?
                        var codeGenForeignKey = codeGenEntityType.GetOrAddForeignKey(foreignKeyCodeGenProperties, targetPrimaryKey);

                        if (targetRelationalEntityType == fromColumnRelationalEntityType // self-referencing foreign key
                            || _uniqueConstraintColumns.Contains(
                                ConstructIdForCombinationOfColumns(
                                    foreignKeyConstraintRelationalPropertyList
                                        .Select(p => p.Name)))) // relational property's name is the columnId
                        {
                            codeGenForeignKey.IsUnique = true;
                        }

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

                    if (foreignKey.ReferencedEntityType != foreignKey.EntityType)
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

        public Property FindTargetColumn(string foreignKeyConstraintId, string fromColumnId)
        {
            ForeignKeyColumnMapping foreignKeyColumnMapping;
            if (!_foreignKeyColumnMappings.TryGetValue(
                foreignKeyConstraintId + fromColumnId, out foreignKeyColumnMapping))
            {
                _logger.WriteWarning("Could not find foreignKeyMapping for ConstraintId " + foreignKeyConstraintId
                    + " FromColumn " + fromColumnId);
                return null;
            }

            TableColumn toColumn;
            if (!_tableColumns.TryGetValue(foreignKeyColumnMapping.ToColumnId, out toColumn))
            {
                _logger.WriteWarning("Could not find toColumn with ColumnId " + foreignKeyColumnMapping.ToColumnId);
                return null;
            }

            Property toColumnRelationalProperty;
            if (!_relationalColumnIdToRelationalPropertyMap.TryGetValue(toColumn.Id, out toColumnRelationalProperty))
            {
                _logger.WriteWarning("Could not find relational property for toColumn with ColumnId " + toColumn.Id);
                return null;
            }

            return toColumnRelationalProperty;
        }

        public static string ConstructIdForCombinationOfColumns(IEnumerable<string> listOfColumnIds)
        {
            return string.Join(string.Empty, listOfColumnIds.OrderBy(columnId => columnId));
        }

        public void ApplyPropertyProperties(Property property, TableColumn tc)
        {
            if (property.Name != tc.ColumnName)
            {
                property.SqlServer().Column = tc.ColumnName;
            }

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
                if (typeof(byte) == SqlServerTypeMapping._sqlTypeToClrTypeMap[tc.DataType])
                {
                    _logger.WriteWarning("For columnId: " + tc.Id + ". The SQL Server data type is " + tc.DataType
                        + ". This will be mapped to CLR type byte which does not allow ValueGenerationStrategy Identity. "
                        + "Generating a matching Property but ignoring the Identity setting.");
                }
                else
                {
                    property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Identity;
                }
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
    }
}