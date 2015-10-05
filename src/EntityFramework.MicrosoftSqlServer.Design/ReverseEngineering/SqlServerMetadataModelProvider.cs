// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering.Model;
using Microsoft.Data.Entity.SqlServer.Design.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;

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

        // primary key, foreign key and unique constraint information constructed from database data
        private readonly Dictionary<string,
            Dictionary<TableConstraint, SortedList<int, string>>> _tableToConstraintToColumnsMap =
            new Dictionary<string,
                Dictionary<TableConstraint, SortedList<int, string>>>(); // 1st string is TableId, 2nd is ColumnId

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
                    CreateConstraintMaps(tableConstraintColumns);
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

        public virtual void CreateConstraintMaps(
            [NotNull] Dictionary<string, TableConstraintColumn> tableConstraintColumns)
        {
            Check.NotNull(tableConstraintColumns, nameof(tableConstraintColumns));

            foreach (var tableConstraintColumn in tableConstraintColumns.Values)
            {
                var constraintType = GetConstraintType(tableConstraintColumn);
                if (constraintType == null)
                {
                    // not a PK, FK or unique constraint
                    continue;
                }

                var tableId = _tableColumns[tableConstraintColumn.ColumnId].TableId;
                Dictionary<TableConstraint, SortedList<int, string>> constraintToColumnsMap;
                if (!_tableToConstraintToColumnsMap.TryGetValue(tableId, out constraintToColumnsMap))
                {
                    constraintToColumnsMap = new Dictionary<TableConstraint, SortedList<int, string>>();
                    _tableToConstraintToColumnsMap[tableId] = constraintToColumnsMap; 
                }

                var constraint = new TableConstraint(
                    tableConstraintColumn.ConstraintId, constraintType.Value);
                SortedList<int, string> sortedColumns;
                if (!constraintToColumnsMap.TryGetValue(constraint, out sortedColumns))
                {
                    sortedColumns = new SortedList<int, string>();
                    constraintToColumnsMap[constraint] = sortedColumns;
                }

                sortedColumns.Add(tableConstraintColumn.Ordinal, tableConstraintColumn.ColumnId);
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
                EntityType entityType;
                if (!_tableIdToEntityType.TryGetValue(tc.TableId, out entityType))
                {
                    // table has been filtered out by user
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

            AddPrimaryAndAlternateKeysToModel();

            foreach (var entityType in relationalModel.EntityTypes)
            {
                if (entityType.FindPrimaryKey() == null)
                {
                    var errorMessage = SqlServerDesignStrings.NoPrimaryKeyColumns(
                        entityType.Relational().Schema,
                        entityType.Relational().TableName);
                    entityType.AddAnnotation(AnnotationNameEntityTypeError, errorMessage);
                    Logger.LogWarning(errorMessage);
                }
            }

            AddForeignKeysToModel();
        }


        public virtual void AddPrimaryAndAlternateKeysToModel()
        {
            foreach (var keyValuePair in _tableToConstraintToColumnsMap)
            {
                var tableId = keyValuePair.Key;
                EntityType entityType;
                if (!_tableIdToEntityType.TryGetValue(tableId, out entityType))
                {
                    // table has been filtered out by user
                    continue;
                }

                var constraintToColumnIdsMap = keyValuePair.Value;
                foreach (var entry in constraintToColumnIdsMap
                    .Where(kvp => kvp.Key.ConstraintType == ConstraintType.PrimaryKey
                        || kvp.Key.ConstraintType == ConstraintType.Unique))
                {
                    var constraint = entry.Key;
                    var sortedColumnIds = entry.Value.Values.ToList();
                    var tuple = MatchProperties(sortedColumnIds);
                    var matchingProperties = tuple.Item1;
                    var unmappedColumnIds = tuple.Item2;
                    if (ConstraintType.PrimaryKey == constraint.ConstraintType)
                    {
                        if (unmappedColumnIds.Count == 0)
                        {
                            entityType.SetPrimaryKey(matchingProperties);
                        }
                    }
                    else
                    {
                        if (unmappedColumnIds.Count == 0)
                        {
                            entityType.AddKey(matchingProperties);
                        }
                        else
                        {
                            Logger.LogWarning(
                                SqlServerDesignStrings.UnableToMatchPropertiesForUniqueKey(
                                    constraint.ConstraintId, unmappedColumnIds));
                        }
                    }
                }
            }
        }

        public virtual void AddForeignKeysToModel()
        {
            foreach (var keyValuePair in _tableToConstraintToColumnsMap)
            {
                var tableId = keyValuePair.Key;
                EntityType fromColumnsEntityType;
                if (!_tableIdToEntityType.TryGetValue(tableId, out fromColumnsEntityType))
                {
                    // table has been filtered out by user
                    continue;
                }
                var constraintToColumnIdsMap = keyValuePair.Value;
                foreach (var entry in constraintToColumnIdsMap
                    .Where(kvp => kvp.Key.ConstraintType == ConstraintType.ForeignKey))
                {
                    var constraint = entry.Key;
                    var fromColumnIds = entry.Value.Values.ToList();
                    var tuple = MatchProperties(fromColumnIds);
                    var unmappedColumnIds = tuple.Item2;
                    if (unmappedColumnIds.Count != 0)
                    {
                        Logger.LogWarning(SqlServerDesignStrings.UnableToMatchPropertiesForForeignKey(
                            constraint.ConstraintId,
                            string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumnIds)));
                        continue;
                    }

                    var fromProperties = tuple.Item1;
                    var toColumnIds = FindToColumns(constraint.ConstraintId, fromColumnIds);
                    if (toColumnIds == null)
                    {
                        continue;
                    }

                    tuple = MatchProperties(toColumnIds);
                    unmappedColumnIds = tuple.Item2;
                    if (unmappedColumnIds.Count != 0)
                    {
                        Logger.LogWarning(SqlServerDesignStrings.UnableToMatchPropertiesForForeignKey(
                            constraint.ConstraintId,
                            string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumnIds)));
                        continue;
                    }

                    var toProperties = tuple.Item1;
                    var toEntityType = toProperties[0].DeclaringEntityType;
                    var principalKey = toEntityType.FindKey(toProperties);
                    if (principalKey == null)
                    {
                        Logger.LogWarning(SqlServerDesignStrings.NoKeyForColumns(
                            constraint.ConstraintId,
                            toEntityType.Name,
                            string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator,
                                toProperties.Select(p => p.Name))));
                        continue;
                    }

                    // Note: In theory there may be more than one foreign key constraint on the
                    // exact same set of properties. Here we just conflate into one foreign key.
                    var foreignKey = fromColumnsEntityType.GetOrAddForeignKey(
                        fromProperties, principalKey, toEntityType);

                    // If there's a primary key or unique constraint on this table which
                    // has the same columns as the FK then mark the foreign key as unique.
                    if (_tableToConstraintToColumnsMap[tableId]
                        .Any(kvp =>
                            (kvp.Key.ConstraintType == ConstraintType.PrimaryKey
                            || kvp.Key.ConstraintType == ConstraintType.Unique)
                            && fromColumnIds.SequenceEqual(kvp.Value.Values)))
                    {
                        foreignKey.IsUnique = true;
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
                        SqlServerDesignStrings.DataTypeDoesNotAllowSqlServerIdentityStrategy(
                            tableColumn.Id, tableColumn.DataType));
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

        protected virtual Tuple<List<Property>, List<string>> MatchProperties(
            [NotNull] List<string> columnIds)
        {
            Check.NotEmpty(columnIds, nameof(columnIds));

            var matchingProperties = new List<Property>();
            var unmappedColumnIds = new List<string>();
            foreach (var columnId in columnIds)
            {
                Property prop;
                if (_columnIdToProperty.TryGetValue(columnId, out prop))
                {
                    matchingProperties.Add(prop);
                }
                else
                {
                    unmappedColumnIds.Add(columnId);
                }
            }

            return new Tuple<List<Property>, List<string>>(matchingProperties, unmappedColumnIds);
        }

        protected virtual List<string> FindToColumns(
            [NotNull] string foreignKeyConstraintId, [NotNull] List<string> fromColumnIds)
        {
            Check.NotEmpty(foreignKeyConstraintId, nameof(foreignKeyConstraintId));
            Check.NotEmpty(fromColumnIds, nameof(fromColumnIds));

            var toColumnIds = new List<string>();
            foreach (var fromColumnId in fromColumnIds)
            {
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
                    Logger.LogWarning(SqlServerDesignStrings.ForeignKeyTargetTableWasExcluded(
                        foreignKeyConstraintId, toTable.SchemaName, toTable.TableName));
                    return null;
                }

                toColumnIds.Add(foreignKeyColumnMapping.ToColumnId);
            }

            return toColumnIds;
        }

        protected virtual ConstraintType? GetConstraintType([
            NotNull] TableConstraintColumn tableConstraintColumn)
        {
            Check.NotNull(tableConstraintColumn, nameof(tableConstraintColumn));

            switch (tableConstraintColumn.ConstraintType)
            {
                case "PRIMARY KEY":
                    return ConstraintType.PrimaryKey;

                case "UNIQUE":
                    return ConstraintType.Unique;

                case "FOREIGN KEY":
                    return ConstraintType.ForeignKey;

                default:
                    return null;
            }
        }

        protected enum ConstraintType : byte
        {
            PrimaryKey = 0,
            Unique = 1,
            ForeignKey = 2
        }

        protected class TableConstraint
        {
            public TableConstraint(
                [NotNull] string constraintId, ConstraintType constraintType)
            {
                Check.NotEmpty(constraintId, nameof(constraintId));

                ConstraintId = constraintId;
                ConstraintType = constraintType;
            }

            public string ConstraintId { get; }
            public ConstraintType ConstraintType { get; }

            public override bool Equals(object obj)
            {
                var otherTableConstraint = obj as TableConstraint;
                if (otherTableConstraint == null)
                {
                    return false;
                }

                return ConstraintId == otherTableConstraint.ConstraintId;
            }

            public override int GetHashCode()
            {
                return ConstraintId.GetHashCode();
            }
        }
    }
}
