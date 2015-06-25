// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.Templating.Compilation;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering.Model;
using Microsoft.Data.Entity.SqlServer.Design.Utilities;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerMetadataModelProvider : IDatabaseMetadataModelProvider
    {
        private static readonly List<string> DataTypesForMax = new List<string> { "varchar", "nvarchar", "varbinary" };
        private static readonly List<string> DataTypesForMaxLengthNotAllowed = new List<string> { "ntext", "text", "image" };
        private static readonly List<string> DataTypesForNumericPrecisionAndScale = new List<string> { "decimal", "numeric" };
        private static readonly List<string> DataTypesForDateTimePrecisionAndScale = new List<string> { "datetime2" };

        public const string AnnotationPrefix = "SqlServerMetadataModelProvider:";
        public const string AnnotationNameDependentEndNavPropName = AnnotationPrefix + "DependentEndNavPropName";
        public const string AnnotationNamePrincipalEndNavPropName = AnnotationPrefix + "PrincipalEndNavPropName";
        public const string AnnotationNameEntityTypeError = AnnotationPrefix + "EntityTypeError";

        public const string NavigationNameUniquifyingPattern = "{0}Navigation";
        public const string SelfReferencingPrincipalEndNavigationNamePattern = "Inverse{0}";

        public static readonly string SqlServerDbContextTemplateResourceName =
            typeof(SqlServerMetadataModelProvider).GetTypeInfo().Assembly.GetName().Name
            + ".ReverseEngineering.Templates.SqlServerDbContextTemplate.cshtml";

        public static readonly string SqlServerEntityTypeTemplateResourceName =
            typeof(SqlServerMetadataModelProvider).GetTypeInfo().Assembly.GetName().Name
            + ".ReverseEngineering.Templates.SqlServerEntityTypeTemplate.cshtml";

        private readonly ILogger _logger;
        private readonly ModelUtilities _modelUtilities;
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
        private readonly Dictionary<EntityType, EntityType> _relationalEntityTypeToCodeGenEntityTypeMap =
            new Dictionary<EntityType, EntityType>();

        private readonly Dictionary<Property, Property> _relationalPropertyToCodeGenPropertyMap = new Dictionary<Property, Property>();
        private readonly Dictionary<string, Property> _relationalColumnIdToRelationalPropertyMap = new Dictionary<string, Property>();

        private readonly Dictionary<EntityType, Dictionary<string, List<Property>>> _relationalEntityTypeToForeignKeyConstraintsMap =
            new Dictionary<EntityType, Dictionary<string, List<Property>>>(); // string is ConstraintId

        public SqlServerMetadataModelProvider([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));

            _logger = serviceProvider.GetRequiredService<ILogger>();
            _modelUtilities = serviceProvider.GetRequiredService<ModelUtilities>();
            _sqlServerLiteralUtilities = new SqlServerLiteralUtilities(_logger);
        }

        public virtual IModel GenerateMetadataModel([NotNull] string connectionString)
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

            return CreateModel();
        }

        public virtual string DbContextTemplate
        {
            get
            {
                return ReadFromResource(
                    typeof(SqlServerMetadataModelProvider).GetTypeInfo().Assembly,
                    SqlServerDbContextTemplateResourceName);
            }
        }

        public virtual DbContextCodeGeneratorHelper DbContextCodeGeneratorHelper(DbContextGeneratorModel model)
        {
            return new SqlServerDbContextCodeGeneratorHelper(model);
        }

        public virtual string EntityTypeTemplate
        {
            get
            {
                return ReadFromResource(
                    typeof(SqlServerMetadataModelProvider).GetTypeInfo().Assembly,
                    SqlServerEntityTypeTemplateResourceName);
            }
        }

        public virtual EntityTypeCodeGeneratorHelper EntityTypeCodeGeneratorHelper(EntityTypeGeneratorModel model)
        {
            return new SqlServerEntityTypeCodeGeneratorHelper(model);
        }

        public virtual void AddReferencesForTemplates(MetadataReferencesProvider metadataReferencesProvider)
        {
            metadataReferencesProvider.AddReferenceFromName("EntityFramework.SqlServer.Design");
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

        public virtual IModel CreateModel()
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

        public virtual IModel ConstructRelationalModel()
        {
            var relationalModel = new Entity.Metadata.Model();
            foreach (var table in _tables.Values)
            {
                relationalModel.AddEntityType(table.Id);
            }

            foreach (var tc in _tableColumns.Values)
            {
                var entityType = relationalModel.FindEntityType(tc.TableId);
                if (entityType == null)
                {
                    _logger.LogWarning(
                        Strings.CannotFindTableForColumn(tc.Id, tc.TableId));
                    continue;
                }

                // IModel will not allow Properties to be created without a Type, so map to CLR type here.
                // This means if we come across a column with a SQL Server type which we can't map we will ignore it.
                // Note: foreign key properties appear just like any other property in the relational model.
                Type clrPropertyType;
                if (!SqlServerTypeMapping._sqlTypeToClrTypeMap.TryGetValue(tc.DataType, out clrPropertyType))
                {
                    _logger.LogWarning(
                        Strings.CannotFindTypeMappingForColumn(tc.Id, tc.DataType));
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

        public virtual IModel ConstructCodeGenModel(
            [NotNull] IModel relationalModel, [NotNull] SqlServerNameMapper nameMapper)
        {
            Check.NotNull(relationalModel, nameof(relationalModel));
            Check.NotNull(nameMapper, nameof(nameMapper));

            var codeGenModel = new Entity.Metadata.Model();
            foreach (var relationalEntityType in relationalModel.EntityTypes.Cast<EntityType>())
            {
                var codeGenEntityType = codeGenModel
                    .AddEntityType(nameMapper.EntityTypeToClassNameMap[relationalEntityType]);
                _relationalEntityTypeToCodeGenEntityTypeMap[relationalEntityType] = codeGenEntityType;
                codeGenEntityType.Relational().Table = _tables[relationalEntityType.Name].TableName;
                codeGenEntityType.Relational().Schema = _tables[relationalEntityType.Name].SchemaName;

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
                        relationalProperty.ClrType,
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
                    var errorMessage = Strings.NoPrimaryKeyColumns(
                        codeGenEntityType.Name,
                        _tables[relationalEntityType.Name].SchemaName,
                        _tables[relationalEntityType.Name].TableName);
                    codeGenEntityType.AddAnnotation(AnnotationNameEntityTypeError, errorMessage);
                    _logger.LogWarning(Strings.CannotGenerateEntityType(codeGenEntityType.Name, errorMessage));
                }
            } // end of loop over all relational EntityTypes

            AddForeignKeysToCodeGenModel(codeGenModel);

            return codeGenModel;
        }

        public virtual void AddForeignKeysToCodeGenModel([NotNull] IModel codeGenModel)
        {
            Check.NotNull(codeGenModel, nameof(codeGenModel));

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

                        // Note: SQL Server allows more than 1 foreign key on the exact same
                        // set of properties. Here we just conflate into one foreign key.
                        var codeGenForeignKey = codeGenEntityType
                            .GetOrAddForeignKey(foreignKeyCodeGenProperties, targetPrimaryKey);

                        if (_uniqueConstraintColumns.Contains(
                            ConstructIdForCombinationOfColumns(
                                foreignKeyConstraintRelationalPropertyList
                                    .Select(p => p.Name)))) // relational property's name is the columnId
                        {
                            codeGenForeignKey.IsUnique = true;
                        }
                    }
                }
            }

            AddDependentAndPrincipalNavigationPropertyAnnotations(codeGenModel);
        }

        private void AddDependentAndPrincipalNavigationPropertyAnnotations([NotNull] IModel codeGenModel)
        {
            Check.NotNull(codeGenModel, nameof(codeGenModel));

            var entityTypeToExistingIdentifiers = new Dictionary<IEntityType, List<string>>();
            foreach (var entityType in codeGenModel.EntityTypes)
            {
                var existingIdentifiers = new List<string>();
                entityTypeToExistingIdentifiers.Add(entityType, existingIdentifiers);
                existingIdentifiers.Add(entityType.Name);
                existingIdentifiers.AddRange(
                    _modelUtilities.OrderedProperties(entityType).Select(p => p.Name));
            }

            foreach (var entityType in codeGenModel.EntityTypes)
            {
                var dependentEndExistingIdentifiers = entityTypeToExistingIdentifiers[entityType];
                foreach (var foreignKey in entityType.GetForeignKeys().Cast<ForeignKey>())
                {
                    // set up the name of the navigation property on the dependent end of the foreign key
                    var dependentEndNavigationPropertyCandidateName =
                        _modelUtilities.GetDependentEndCandidateNavigationPropertyName(foreignKey);
                    var dependentEndNavigationPropertyName =
                        CSharpUtilities.Instance.GenerateCSharpIdentifier(
                            dependentEndNavigationPropertyCandidateName,
                            dependentEndExistingIdentifiers,
                            NavigationUniquifier);
                    foreignKey.AddAnnotation(
                        AnnotationNameDependentEndNavPropName,
                        dependentEndNavigationPropertyName);
                    dependentEndExistingIdentifiers.Add(dependentEndNavigationPropertyName);

                    // set up the name of the navigation property on the principal end of the foreign key
                    var principalEndExistingIdentifiers =
                        entityTypeToExistingIdentifiers[foreignKey.PrincipalEntityType];
                    var principalEndNavigationPropertyCandidateName =
                        foreignKey.IsSelfReferencing()
                            ? string.Format(
                                CultureInfo.CurrentCulture,
                                SelfReferencingPrincipalEndNavigationNamePattern,
                                dependentEndNavigationPropertyName)
                            : _modelUtilities.GetPrincipalEndCandidateNavigationPropertyName(foreignKey);
                    var principalEndNavigationPropertyName =
                        CSharpUtilities.Instance.GenerateCSharpIdentifier(
                            principalEndNavigationPropertyCandidateName,
                            principalEndExistingIdentifiers,
                            NavigationUniquifier);
                    foreignKey.AddAnnotation(
                        AnnotationNamePrincipalEndNavPropName,
                        principalEndNavigationPropertyName);
                    principalEndExistingIdentifiers.Add(principalEndNavigationPropertyName);
                }
            }
        }

        public virtual Property FindTargetColumn(
            [NotNull] string foreignKeyConstraintId, [NotNull] string fromColumnId)
        {
            Check.NotEmpty(foreignKeyConstraintId, nameof(foreignKeyConstraintId));
            Check.NotEmpty(fromColumnId, nameof(fromColumnId));

            ForeignKeyColumnMapping foreignKeyColumnMapping;
            if (!_foreignKeyColumnMappings.TryGetValue(
                foreignKeyConstraintId + fromColumnId, out foreignKeyColumnMapping))
            {
                _logger.LogWarning(
                    Strings.CannotFindForeignKeyMappingForConstraintId(foreignKeyConstraintId, fromColumnId));
                return null;
            }

            TableColumn toColumn;
            if (!_tableColumns.TryGetValue(foreignKeyColumnMapping.ToColumnId, out toColumn))
            {
                _logger.LogWarning(
                    Strings.CannotFindToColumnForConstraintId(foreignKeyConstraintId, foreignKeyColumnMapping.ToColumnId));
                return null;
            }

            Property toColumnRelationalProperty;
            if (!_relationalColumnIdToRelationalPropertyMap.TryGetValue(toColumn.Id, out toColumnRelationalProperty))
            {
                _logger.LogWarning(
                    Strings.CannotFindRelationalPropertyForColumnId(foreignKeyConstraintId, toColumn.Id));
                return null;
            }

            return toColumnRelationalProperty;
        }

        public static string ConstructIdForCombinationOfColumns([NotNull] IEnumerable<string> listOfColumnIds)
        {
            Check.NotNull(listOfColumnIds, nameof(listOfColumnIds));

            return string.Join(string.Empty, listOfColumnIds.OrderBy(columnId => columnId));
        }

        public virtual string NavigationUniquifier(
            [NotNull] string proposedIdentifier, [CanBeNull] ICollection<string> existingIdentifiers)
        {
            if (existingIdentifiers == null
                || !existingIdentifiers.Contains(proposedIdentifier))
            {
                return proposedIdentifier;
            }

            var finalIdentifier =
                string.Format(CultureInfo.CurrentCulture, NavigationNameUniquifyingPattern, proposedIdentifier);
            var suffix = 1;
            while (existingIdentifiers.Contains(finalIdentifier))
            {
                finalIdentifier = proposedIdentifier + suffix;
                suffix++;
            }

            return finalIdentifier;
        }

        public virtual void ApplyPropertyProperties(
            [NotNull] Property property, [NotNull] TableColumn tableColumn)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(tableColumn, nameof(tableColumn));

            property.IsNullable = tableColumn.IsNullable;

            if (property.Name != tableColumn.ColumnName)
            {
                property.Relational().Column = tableColumn.ColumnName;
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
                if (typeof(byte) == SqlServerTypeMapping._sqlTypeToClrTypeMap[tableColumn.DataType])
                {
                    _logger.LogWarning(
                        Strings.DataTypeDoesNotAllowIdentityStrategy(tableColumn.Id, tableColumn.DataType));
                }
                else
                {
                    property.SqlServer().IdentityStrategy = SqlServerIdentityStrategy.IdentityColumn;
                }
            }

            if (tableColumn.IsStoreGenerated
                || tableColumn.DataType == "timestamp")
            {
                // timestamp columns should always be treated as store generated
                property.StoreGeneratedPattern = StoreGeneratedPattern.Computed;
            }

            if (tableColumn.DefaultValue != null)
            {
                var defaultExpressionOrValue =
                    _sqlServerLiteralUtilities
                        .ConvertSqlServerDefaultValue(
                            property.ClrType, tableColumn.DefaultValue);
                if (defaultExpressionOrValue != null
                    && defaultExpressionOrValue.DefaultExpression != null)
                {
                    property.Relational().DefaultValueSql = defaultExpressionOrValue.DefaultExpression;
                }
                else if (defaultExpressionOrValue != null
                         && defaultExpressionOrValue.DefaultValue != null)
                {
                    property.Relational().DefaultValue = defaultExpressionOrValue.DefaultValue;
                }
                else
                {
                    _logger.LogWarning(
                        Strings.UnableToConvertDefaultValue(
                            tableColumn.Id, tableColumn.DefaultValue,
                            property.ClrType, property.Name, property.EntityType.Name));
                }
            }
        }

        public virtual string ReadFromResource([NotNull] Assembly assembly, [NotNull] string resourceName)
        {
            Check.NotNull(assembly, nameof(assembly));
            Check.NotEmpty(resourceName, nameof(resourceName));

            using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var streamReader = new StreamReader(resourceStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}
