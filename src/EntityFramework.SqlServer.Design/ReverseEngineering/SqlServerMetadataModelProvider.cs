// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering.Model;
using Microsoft.Data.Entity.SqlServer.Design.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerMetadataModelProvider : RelationalMetadataModelProvider
    {
        private static readonly List<string> DataTypesForNumericPrecisionAndScale = new List<string> { "decimal", "numeric" };
        private static readonly List<string> DataTypesForDateTimePrecisionAndScale = new List<string> { "datetime2" };

        private readonly SqlServerLiteralUtilities _sqlServerLiteralUtilities;

        // data loaded directly from database
        private Dictionary<string, Table> _tables;
        private Dictionary<string, TableColumn> _tableColumns;
        private Dictionary<string, ForeignKeyColumnMapping> _foreignKeyColumnMappings;

        // primary key, foreign key and unique columns information constructed from database data
        private readonly Dictionary<string, int> _primaryKeyOrdinals = new Dictionary<string, int>();
        private readonly Dictionary<string, Dictionary<string, int>> _foreignKeyOrdinals =
            new Dictionary<string, Dictionary<string, int>>(); // 1st string is ColumnId, 2nd is ConstraintId
        private readonly HashSet<string> _uniqueConstraintColumns = new HashSet<string>();

        // utility data constructed as we iterate over the data
        private readonly Dictionary<string, EntityType> _tableIdToEntityType = new Dictionary<string, EntityType>();
        private readonly Dictionary<string, Property> _columnIdToProperty = new Dictionary<string, Property>();

        public SqlServerMetadataModelProvider(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] ModelUtilities modelUtilities,
            [NotNull] CSharpUtilities cSharpUtilities,
            [NotNull] IRelationalAnnotationProvider extensionsProvider,
            [NotNull] SqlServerLiteralUtilities sqlServerLiteralUtilities)
            : base(loggerFactory, modelUtilities, cSharpUtilities)
        {
            Check.NotNull(extensionsProvider, nameof(extensionsProvider));
            Check.NotNull(sqlServerLiteralUtilities, nameof(sqlServerLiteralUtilities));

            ExtensionsProvider = extensionsProvider;
            _sqlServerLiteralUtilities = sqlServerLiteralUtilities;
        }

        protected override IRelationalAnnotationProvider ExtensionsProvider { get; }

        public override IModel ConstructRelationalModel([NotNull] string connectionString)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));

            LoadMetadataFromDatabase(connectionString);

            var relationalModel = new Entity.Metadata.Model();
            AddEntityTypesToModel(relationalModel);
            AddPropertiesToModel(relationalModel);
            AddKeysToModel(relationalModel);

            return relationalModel;
        }

        public virtual void LoadMetadataFromDatabase([NotNull] string connectionString)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));

            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    _tables = LoadData(conn, Table.Query, Table.CreateFromReader, t => t.Id);
                    _tableColumns = LoadData(conn, TableColumn.Query, TableColumn.CreateFromReader, tc => tc.Id);
                    _foreignKeyColumnMappings = LoadData(
                        conn, ForeignKeyColumnMapping.Query, ForeignKeyColumnMapping.CreateFromReader, fkcm => fkcm.Id);

                    var tableConstraintColumns = LoadData(
                        conn, TableConstraintColumn.Query, TableConstraintColumn.CreateFromReader, tcc => tcc.Id);
                    CreatePrimaryForeignKeyAndUniqueMaps(tableConstraintColumns);
                }
                finally
                {
                    if (conn != null
                        && conn.State == ConnectionState.Open)
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

        public virtual Dictionary<string, T> LoadData<T>(
            [NotNull] SqlConnection connection,
            [NotNull] string query,
            [NotNull] Func<SqlDataReader, T> createFromReader,
            [NotNull] Func<T, string> identifier)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotEmpty(query, nameof(query));
            Check.NotNull(createFromReader, nameof(createFromReader));
            Check.NotNull(identifier, nameof(identifier));

            var data = new Dictionary<string, T>();
            var sqlCommand = new SqlCommand(query);
            sqlCommand.Connection = connection;

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

        public virtual void CreatePrimaryForeignKeyAndUniqueMaps(
            [NotNull] Dictionary<string, TableConstraintColumn> tableConstraintColumns)
        {
            Check.NotNull(tableConstraintColumns, nameof(tableConstraintColumns));

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

        public virtual void AddEntityTypesToModel([NotNull] Entity.Metadata.Model relationalModel)
        {
            Check.NotNull(relationalModel, nameof(relationalModel));

            foreach (var table in _tables.Values)
            {
                if (!_tableSelectionSet.Allows(table.SchemaName, table.TableName))
                {
                    continue;
                }
                var entityType = relationalModel.AddEntityType(table.Id);
                _tableIdToEntityType.Add(table.Id, entityType);
                entityType.Relational().TableName = _tables[table.Id].TableName;
                entityType.Relational().Schema = _tables[table.Id].SchemaName;
            }
        }

        public virtual void AddPropertiesToModel([NotNull] Entity.Metadata.Model relationalModel)
        {
            Check.NotNull(relationalModel, nameof(relationalModel));

            foreach (var tc in _tableColumns.Values)
            {
                var table = _tables[tc.TableId];
                if (!_tableSelectionSet.Allows(table.SchemaName, table.TableName))
                {
                    continue;
                }
                EntityType entityType;
                if (!_tableIdToEntityType.TryGetValue(tc.TableId, out entityType))
                {
                    Logger.LogWarning(
                        SqlServerDesignStrings.CannotFindTableForColumn(tc.Id, tc.TableId));
                    continue;
                }

                // If we come across a column with a SQL Server type which we can't map we will ignore it.
                // Note: foreign key properties appear just like any other property in the relational model.
                Type clrPropertyType;
                if (!SqlServerTypeMapping._sqlTypeToClrTypeMap.TryGetValue(tc.DataType, out clrPropertyType))
                {
                    Logger.LogWarning(
                        SqlServerDesignStrings.CannotFindTypeMappingForColumn(tc.Id, tc.DataType));
                    continue;
                }

                if (tc.IsNullable)
                {
                    clrPropertyType = clrPropertyType.MakeNullable();
                }

                var property = entityType.AddProperty(tc.Id, clrPropertyType);
                property.Relational().ColumnName = _tableColumns[tc.Id].ColumnName;
                _columnIdToProperty[tc.Id] = property;
                AddFacetsOnProperty(property, _tableColumns[tc.Id]);
            }
        }

        public virtual void AddKeysToModel([NotNull] Entity.Metadata.Model relationalModel)
        {
            Check.NotNull(relationalModel, nameof(relationalModel));

            var entityTypeToForeignKeyConstraintsMap =
                new Dictionary<EntityType, Dictionary<string, List<Property>>>(); // string is ConstraintId

            foreach (var entityType in relationalModel.EntityTypes)
            {
                var primaryKeyProperties = new List<Property>();
                var constraints = new Dictionary<string, List<Property>>();
                entityTypeToForeignKeyConstraintsMap[entityType] = constraints;
                foreach (var property in entityType.Properties)
                {
                    int primaryKeyOrdinal;
                    if (_primaryKeyOrdinals.TryGetValue(property.Name, out primaryKeyOrdinal))
                    {
                        primaryKeyProperties.Add(property);
                    }

                    Dictionary<string, int> foreignKeyConstraintIdOrdinalMap;
                    if (_foreignKeyOrdinals.TryGetValue(property.Name, out foreignKeyConstraintIdOrdinalMap))
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
                            constraintProperties.Add(property);
                        }
                    }
                }

                if (primaryKeyProperties.Count() > 0)
                {
                    entityType.SetPrimaryKey(
                        primaryKeyProperties
                            .OrderBy(p => _primaryKeyOrdinals[p.Name]) // note: for relational property p.Name is its columnId
                            .ToList());
                }
                else
                {
                    var errorMessage = SqlServerDesignStrings.NoPrimaryKeyColumns(
                        entityType.Relational().Schema,
                        entityType.Relational().TableName);
                    entityType.AddAnnotation(AnnotationNameEntityTypeError, errorMessage);
                    Logger.LogWarning(errorMessage);
                }
            }

            AddForeignKeysToModel(relationalModel, entityTypeToForeignKeyConstraintsMap);
        }

        public virtual void AddForeignKeysToModel([NotNull] Entity.Metadata.Model relationalModel,
            [NotNull] Dictionary<EntityType, Dictionary<string, List<Property>>> entityTypeToForeignKeyConstraintsMap)
        {
            Check.NotNull(relationalModel, nameof(relationalModel));

            foreach (var keyValuePair in entityTypeToForeignKeyConstraintsMap)
            {
                var fromColumnRelationalEntityType = keyValuePair.Key;
                foreach (var foreignKeyConstraintMap in keyValuePair.Value)
                {
                    var foreignKeyConstraintId = foreignKeyConstraintMap.Key;
                    var foreignKeyConstraintRelationalPropertyList = foreignKeyConstraintMap.Value;

                    var targetRelationalProperty = FindTargetProperty(
                        foreignKeyConstraintId,
                        foreignKeyConstraintRelationalPropertyList[0].Name);
                    if (targetRelationalProperty != null)
                    {
                        var targetRelationalEntityType = targetRelationalProperty.DeclaringEntityType;
                        var targetPrimaryKey = targetRelationalEntityType.GetPrimaryKey();

                        // need the foreign key columns ordered by ordinal (i.e. in the order they appear in the target's key)
                        var foreignKeyCodeGenProperties =
                            foreignKeyConstraintRelationalPropertyList
                                .OrderBy(relationalProperty =>
                                    _foreignKeyOrdinals[relationalProperty.Name][foreignKeyConstraintId]) // relational property's name is the columnId
                                .ToList();

                        // Note: In theory there may be more than one foreign key constraint on the
                        // exact same set of properties. Here we just conflate into one foreign key.
                        var foreignKey = fromColumnRelationalEntityType.GetOrAddForeignKey(
                            foreignKeyCodeGenProperties, targetPrimaryKey, targetRelationalEntityType);

                        if (_uniqueConstraintColumns.Contains(
                            ConstructIdForCombinationOfColumns(
                                foreignKeyConstraintRelationalPropertyList
                                    .Select(p => p.Name)))) // relational property's name is the columnId
                        {
                            foreignKey.IsUnique = true;
                        }
                    }
                }
            }
        }

        public virtual void AddFacetsOnProperty(
            [NotNull] Property property, [NotNull] TableColumn tableColumn)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(tableColumn, nameof(tableColumn));

            property.IsNullable = tableColumn.IsNullable;

            if (property.Name != tableColumn.ColumnName)
            {
                property.Relational().ColumnName = tableColumn.ColumnName;
            }

            string typeName = null;
            if (DataTypesForNumericPrecisionAndScale.Contains(tableColumn.DataType))
            {
                if (tableColumn.NumericPrecision.HasValue)
                {
                    if (tableColumn.Scale.HasValue)
                    {
                        typeName = tableColumn.DataType + "(" + tableColumn.NumericPrecision.Value + ", " + tableColumn.Scale.Value + ")";
                    }
                    else
                    {
                        typeName = tableColumn.DataType + "(" + tableColumn.NumericPrecision.Value + ")";
                    }
                }
            }
            else if (DataTypesForDateTimePrecisionAndScale.Contains(tableColumn.DataType))
            {
                if (tableColumn.DateTimePrecision.HasValue)
                {
                    if (tableColumn.Scale.HasValue)
                    {
                        typeName = tableColumn.DataType + "(" + tableColumn.DateTimePrecision.Value + ", " + tableColumn.Scale.Value + ")";
                    }
                    else
                    {
                        typeName = tableColumn.DataType + "(" + tableColumn.DateTimePrecision.Value + ")";
                    }
                }
            }

            if (typeName != null)
            {
                property.Relational().ColumnType = typeName;
            }

            if (tableColumn.IsIdentity)
            {
                property.ValueGenerated = ValueGenerated.OnAdd;

                if (typeof(byte) == SqlServerTypeMapping._sqlTypeToClrTypeMap[tableColumn.DataType])
                {
                    Logger.LogWarning(
                        SqlServerDesignStrings.DataTypeDoesNotAllowSqlServerIdentityStrategy(tableColumn.Id, tableColumn.DataType));
                }
                else
                {
                    property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.IdentityColumn;
                }
            }

            if (tableColumn.IsStoreGenerated
                || tableColumn.DataType == "timestamp")
            {
                // timestamp columns should always be treated as store generated
                // (rowversion columns are stored as data type 'timestamp')
                property.ValueGenerated = ValueGenerated.OnAddOrUpdate;
            }

            if (tableColumn.DefaultValue != null)
            {
                var defaultExpressionOrValue =
                    _sqlServerLiteralUtilities
                        .ConvertSqlServerDefaultValue(
                            ((IProperty)property).ClrType, tableColumn.DefaultValue);
                if (defaultExpressionOrValue != null
                    && defaultExpressionOrValue.DefaultExpression != null)
                {
                    property.Relational().GeneratedValueSql = defaultExpressionOrValue.DefaultExpression;
                }
                else if (defaultExpressionOrValue != null
                         && defaultExpressionOrValue.DefaultValue != null)
                {
                    property.Relational().DefaultValue = defaultExpressionOrValue.DefaultValue;
                }
                else
                {
                    Logger.LogWarning(
                        SqlServerDesignStrings.UnableToConvertDefaultValue(
                            tableColumn.Id, tableColumn.DefaultValue,
                            ((IProperty)property).ClrType, property.Name, property.DeclaringEntityType.Name));
                }
            }
        }

        public virtual Property FindTargetProperty(
            [NotNull] string foreignKeyConstraintId, [NotNull] string fromColumnId)
        {
            Check.NotEmpty(foreignKeyConstraintId, nameof(foreignKeyConstraintId));
            Check.NotEmpty(fromColumnId, nameof(fromColumnId));

            ForeignKeyColumnMapping foreignKeyColumnMapping;
            if (!_foreignKeyColumnMappings.TryGetValue(
                foreignKeyConstraintId + fromColumnId, out foreignKeyColumnMapping))
            {
                Logger.LogWarning(
                    SqlServerDesignStrings.CannotFindForeignKeyMappingForConstraintId(
                        foreignKeyConstraintId, fromColumnId));
                return null;
            }

            var toTable = _tables[_tableColumns[foreignKeyColumnMapping.ToColumnId].TableId];
            if (!_tableSelectionSet.Allows(toTable.SchemaName, toTable.TableName))
            {
                // target property belongs to a table which was excluded by the TableSelectionSet
                return null;
            }

            Property toColumnRelationalProperty;
            if (!_columnIdToProperty.TryGetValue(
                foreignKeyColumnMapping.ToColumnId, out toColumnRelationalProperty))
            {
                Logger.LogWarning(
                    SqlServerDesignStrings.CannotFindRelationalPropertyForColumnId(
                        foreignKeyConstraintId, foreignKeyColumnMapping.ToColumnId));
                return null;
            }

            return toColumnRelationalProperty;
        }
    }
}
